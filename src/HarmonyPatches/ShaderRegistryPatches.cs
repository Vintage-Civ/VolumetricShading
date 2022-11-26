using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace VolumetricShading
{
    [HarmonyPatch(typeof(ShaderRegistry))]
    internal class ShaderRegistryPatches
    {
        [HarmonyPatch("LoadShader")]
        [HarmonyPostfix]
        public static void LoadShaderPostfix(ShaderProgram program, EnumShaderType shaderType)
        {
            VolumetricShadingMod.Instance.ShaderInjector.OnShaderLoaded(program, shaderType);
        }

        private static readonly MethodInfo HandleIncludesMethod = typeof(ShaderRegistry)
            .GetMethod("HandleIncludes", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo LoadShaderCallsiteMethod =
            typeof(ShaderRegistryPatches).GetMethod("LoadShaderCallsite");

        [HarmonyPatch("HandleIncludes")]
        [HarmonyReversePatch]
        public static string HandleIncludes(ShaderProgram program, string code, HashSet<string> filenames = null)
        {
            throw new InvalidOperationException("Stub, replaced by Harmony");
        }
        [HarmonyPatch("LoadShader")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LoadShaderTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(HandleIncludesMethod))
                {
                    found = true;
                    // current eval stack: ShaderProgram, string (shader code), null
                    
                    // pop null from eval stack
                    yield return new CodeInstruction(OpCodes.Pop);
                    
                    // push EnumShaderType to eval stack
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    
                    // call our own instead of original, signature needs to match stack!
                    yield return new CodeInstruction(OpCodes.Call, LoadShaderCallsiteMethod);
                }
                else
                {
                    yield return instruction;
                }
            }

            if (!found)
            {
                throw new Exception("Could not transpile LoadShader");
            }
        }

        public static string LoadShaderCallsite(ShaderProgram shader, string code, EnumShaderType type)
        {
            var ext = ".unknown";
            switch (type)
            {
                case EnumShaderType.FragmentShader:
                    ext = ".fsh";
                    break;
                case EnumShaderType.VertexShader:
                    ext = ".vsh";
                    break;
                case EnumShaderType.GeometryShader:
                    ext = ".gsh";
                    break;
            }
            var filename = shader.PassName + ext;



            code = VolumetricShadingMod.Instance.ShaderPatcher.Patch(filename, code);
            code = HandleIncludes(shader, code);

            if (filename == "chunkopaque.fsh")
            {

            }

            return code;
        }

        private static readonly FieldInfo IncludesField = typeof(ShaderRegistry)
            .GetField("includes", BindingFlags.Static | BindingFlags.NonPublic);
        
        private static readonly MethodInfo LoadRegisteredCallsiteMethod =
            typeof(ShaderRegistryPatches).GetMethod("LoadRegisteredCallsite");

        [HarmonyPatch("loadRegisteredShaderPrograms")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LoadRegisteredShaderProgramsTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            var generated = false;
            foreach (var instruction in instructions)
            {
                if (found && !generated)
                {
                    generated = true; 
                    yield return new CodeInstruction(OpCodes.Ldsfld, IncludesField)
                        .WithLabels(instruction.labels);
                    yield return new CodeInstruction(OpCodes.Call, LoadRegisteredCallsiteMethod);
                    
                    instruction.labels.Clear();
                }
                
                yield return instruction;
                if (instruction.opcode != OpCodes.Endfinally) continue;
                
                found = true;
            }

            if (!found)
            {
                throw new Exception("Could not patch loadRegisteredShaderPrograms");
            }
        }

        public static void LoadRegisteredCallsite(Dictionary<string, string> includes)
        {
            VolumetricShadingMod.Instance.ShaderPatcher.Reload();
            
            foreach (var entry in includes.ToList())
            {
                var value = VolumetricShadingMod.Instance.ShaderPatcher
                    .Patch(entry.Key, entry.Value, true);
                includes[entry.Key] = value;
            }
        }
        
        private static readonly MethodInfo RegisterShaderProgramCallsiteMethod =
            typeof(ShaderRegistryPatches).GetMethod("RegisterShaderProgramCallsite");


        [HarmonyPatch("RegisterShaderProgram")]
        [HarmonyPatch(new[] {typeof(string), typeof(ShaderProgram)})]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RegisterShaderProgramTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var set_Item_method = AccessTools.Method(typeof(Dictionary<string, int>), "set_Item");

            var found = false;
            var generated = false;
            foreach (var instruction in instructions)
            {
                if (found && !generated)
                {
                    generated = true;
                    yield return new CodeInstruction(OpCodes.Ldsfld, IncludesField)
                        .WithLabels(instruction.labels);
                    yield return new CodeInstruction(OpCodes.Call, RegisterShaderProgramCallsiteMethod);

                    instruction.labels.Clear();
                }

                yield return instruction;

                if (instruction.opcode == OpCodes.Callvirt && (instruction.operand as MethodInfo) == set_Item_method)
                {
                    found = true;
                }
            }

            if (!found)
            {
                throw new Exception("Could not patch RegisterShaderProgram");
            }
        }
        
        public static void RegisterShaderProgramCallsite(Dictionary<string, string> includes)
        {
            foreach (var entry in includes.ToList())
            {
                var value = VolumetricShadingMod.Instance.ShaderPatcher
                    .Patch(entry.Key, entry.Value, true);
                includes[entry.Key] = value;
            }
        }
    }
}
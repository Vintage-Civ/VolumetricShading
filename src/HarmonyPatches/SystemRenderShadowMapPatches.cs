using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.Client.NoObf;

namespace VolumetricShading
{
    [HarmonyPatch(typeof(SystemRenderShadowMap))]
    internal class SystemRenderShadowMapPatches
    {
        private static readonly MethodInfo OnRenderShadowNearBaseWidthCallsiteMethod =
            typeof(SystemRenderShadowMapPatches).GetMethod("OnRenderShadowNearBaseWidthCallsite");
        
        [HarmonyPatch("OnRenderShadowNear")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnRenderShadowNearBaseWidthTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            bool found = false;
            bool done = false;

            foreach (var instruction in instructions)
            {
                if (!found && instruction.opcode == OpCodes.Ret)
                {
                    found = true;
                    yield return instruction;
                }

                if (found && !done)
                {
                    done = true;
                    // replace constant offset
                    yield return new CodeInstruction(OpCodes.Call, OnRenderShadowNearBaseWidthCallsiteMethod);
                }
                else
                {
                    yield return instruction;                    
                }
            }
        }
        
        public static int OnRenderShadowNearBaseWidthCallsite()
        {
            return VolumetricShadingMod.Instance.ShadowTweaks.NearShadowBaseWidth;
        }

        private static readonly MethodInfo PrepareForShadowRenderingMethod = typeof(SystemRenderShadowMap)
            .GetMethod("PrepareForShadowRendering", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPatch("OnRenderShadowNear")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnRenderShadowNearZExtend(IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            CodeInstruction previousInstruction = null;
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(PrepareForShadowRenderingMethod))
                {
                    found = true;
                    
                    // fixes some shadow glitches by increasing the extra culling range for shadows
                    yield return new CodeInstruction(OpCodes.Ldc_R4, (float) 32);
                }
                else if (previousInstruction != null)
                {
                    yield return previousInstruction;
                }
                
                previousInstruction = instruction;
            }
            
            yield return previousInstruction;

            if (!found)
            {
                throw new Exception("Could not patch OnRenderShadowNear for further Z extension");
            }
        }
    }
}
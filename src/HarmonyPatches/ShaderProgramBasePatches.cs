using HarmonyLib;
using Vintagestory.Client.NoObf;

namespace VolumetricShading
{
    [HarmonyPatch(typeof(ShaderProgramBase))]
    internal class ShaderProgramBasePatches
    {
        [HarmonyPatch("Use")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void UsePostfix(ShaderProgramBase __instance)
        {
            VolumetricShadingMod.Instance.Events.EmitPostUseShader(__instance);
        }
    }
}
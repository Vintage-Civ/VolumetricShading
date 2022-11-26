using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace VolumetricShading
{
    [HarmonyPatch]
    internal class IceAndGlassPatches
    {
        [HarmonyPatch(typeof(CubeTesselator), "Tesselate")][HarmonyPrefix]
        public static void CubeTesselator(ref TCTCache vars) => Tesselate(ref vars);

        [HarmonyPatch(typeof(LiquidTesselator), "Tesselate")][HarmonyPrefix]
        public static void LiquidTesselator(ref TCTCache vars) => Tesselate(ref vars);

        [HarmonyPatch(typeof(TopsoilTesselator), "Tesselate")][HarmonyPrefix]
        public static void TopsoilTesselator(ref TCTCache vars) => Tesselate(ref vars);

        [HarmonyPatch(typeof(JsonTesselator), "Tesselate")][HarmonyPrefix]
        public static void JsonTesselator(ref TCTCache vars) => Tesselate(ref vars);

        [HarmonyPatch(typeof(JsonAndSnowLayerTesselator), "Tesselate")][HarmonyPrefix]
        public static void JsonAndSnowLayerTesselator(ref TCTCache vars) => Tesselate(ref vars);

        [HarmonyPatch(typeof(JsonAndLiquidTesselator), "Tesselate")][HarmonyPrefix]
        public static void JsonAndLiquidTesselator(ref TCTCache vars) => Tesselate(ref vars);

        public static void Tesselate(ref TCTCache vars)
        {
            if (vars.block.BlockMaterial == EnumBlockMaterial.Ice ||
                vars.block.BlockMaterial == EnumBlockMaterial.Glass)
            {
                vars.VertexFlags |= VertexFlags.ReflectiveBitMask;
            }
        }
    }
}
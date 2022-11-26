using HarmonyLib;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;
using Vintagestory.Common;

namespace VolumetricShading
{

    [HarmonyPatch(typeof(AmbientManager))]
    internal class AmbientManagerPatches
    {
        [HarmonyPatch("OnPlayerSightBeingChangedByWater")]
        [HarmonyPostfix]
        public static void OnPlayerSightBeingChangedByWaterPostfix()
        {
            VolumetricShadingMod.Instance.Events.EmitPostWaterChangeSight();
        }
    }
}
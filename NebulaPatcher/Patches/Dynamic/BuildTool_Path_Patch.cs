using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(BuildTool_Path))]
    class BuildTool_Path_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("CreatePrebuilds")]
        public static void CreatePrebuilds_Postfix()
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient && FactoryManager.EventFromServer && FactoryManager.IsHumanInput)
            {
                FactoryManager.IsHumanInput = false;
            }
            else if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient && (FactoryManager.IsHumanInput || FactoryManager.IsFromClient))
            {
                FactoryManager.IsFromClient = false;
                FactoryManager.IsHumanInput = false;
            }
        }
    }
}

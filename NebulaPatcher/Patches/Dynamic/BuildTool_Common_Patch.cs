using HarmonyLib;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch]
    class BuildTool_Common_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CreatePrebuilds))]
        [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CreatePrebuilds))]
        [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.CreatePrebuilds))]
        public static bool CreatePrebuilds_Prefix(BuildTool __instance)
        {
            return BuildToolManager.CreatePrebuilds(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CheckBuildConditions))]
        [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CheckBuildConditions))]
        [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.CheckBuildConditions))]
        public static bool CheckBuildConditions(ref bool __result)
        {
            if (FactoryManager.IgnoreBasicBuildConditionChecks)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}

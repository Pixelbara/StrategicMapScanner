using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SmartScannerReveal
{
    [StaticConstructorOnStartup]
    public static class SmartRevealHarmonyLoader
    {
        static SmartRevealHarmonyLoader()
        {
            var harmony = new Harmony("pixelbara.smartscannerreveal");
            harmony.PatchAll();
        }
    }

   
    [HarmonyPatch(typeof(WorkGiver_Refuel), "HasJobOnThing")]
    public static class Patch_HideRefuelOnRightClick
    {
        public static bool Prefix(Thing t, ref bool __result)
        {
            
            if (t != null && t.def.defName == "StrategicMapScanner")
            {
                
                if (SmartRevealMod.settings != null && SmartRevealMod.settings.noUraniumRequired)
                {
                    __result = false; 
                    return false;     
                }
            }
            return true; 
        }
    }
}
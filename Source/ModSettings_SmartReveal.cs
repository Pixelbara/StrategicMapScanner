using RimWorld;
using UnityEngine;
using Verse;

namespace SmartScannerReveal
{
    public class SmartRevealSettings : ModSettings
    {
        public float speedMultiplier = 1.0f;
        
        public bool noUraniumRequired = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref speedMultiplier, "speedMultiplier", 1.0f);
            Scribe_Values.Look(ref noUraniumRequired, "noUraniumRequired", false);
        }
    }

    public class SmartRevealMod : Mod
    {
        public static SmartRevealSettings settings;

        public SmartRevealMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<SmartRevealSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            
            listingStandard.Label($"Speed Multiplier: {settings.speedMultiplier:F1}x");
            settings.speedMultiplier = listingStandard.Slider(settings.speedMultiplier, 0.1f, 100f);

            listingStandard.Gap();

            
            listingStandard.CheckboxLabeled("No Uranium Required", ref settings.noUraniumRequired, "If enabled, the scanner will function and complete without requiring or consuming any Uranium.");

            listingStandard.Gap();

            
            if (listingStandard.ButtonText("Reset to default"))
            {
                settings.speedMultiplier = 1.0f;
                settings.noUraniumRequired = false;
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Strategic Map Scanner";
        }
    }
}
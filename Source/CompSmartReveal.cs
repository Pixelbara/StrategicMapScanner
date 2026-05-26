using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace SmartScannerReveal
{
    public class CompProperties_SmartReveal : CompProperties
    {
        public CompProperties_SmartReveal()
        {
            compClass = typeof(CompSmartReveal);
        }
    }

    public class CompSmartReveal : ThingComp
    {
        public bool isInRevealMode = false;
        public float workDone = 0f;
        public const float WorkRequired = 360000f;

        public CompRefuelable Refuelable => parent.GetComp<CompRefuelable>();
        public CompFlickable Flickable => parent.GetComp<CompFlickable>();

        public override void CompTick()
        {
            base.CompTick();

            if (Flickable != null && !Flickable.SwitchIsOn)
            {
                isInRevealMode = false;
            }

            bool bypassFuel = SmartRevealMod.settings != null && SmartRevealMod.settings.noUraniumRequired;

            
            if (bypassFuel && Refuelable != null && Refuelable.Fuel > 0f)
            {
                ExtractUranium(quiet: true);
            }

            
            if (isInRevealMode && Find.TickManager.TicksGame % 90 == 0 && parent.Map != null)
            {
                Vector3 centerPos = parent.TrueCenter();
                FleckMaker.ThrowLightningGlow(centerPos, parent.Map, 2.0f);
            }
        }

        public void FinishReveal()
        {
            isInRevealMode = false;
            workDone = 0f;

            if (SmartRevealMod.settings != null && SmartRevealMod.settings.noUraniumRequired)
            {
                Messages.Message("The entire map has been fully revealed!", MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Refuelable?.ConsumeFuel(100f);
                Messages.Message("The entire map has been fully revealed! 100 Uranium consumed.", MessageTypeDefOf.PositiveEvent);
            }

            parent.Map?.fogGrid?.ClearAllFog();

            Find.LetterStack.ReceiveLetter(
                "Map Fully Revealed",
                "The Strategic Map Scanner has successfully revealed the whole area.",
                LetterDefOf.PositiveEvent);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            
            foreach (var g in base.CompGetGizmosExtra())
                yield return g;

            bool bypassFuel = SmartRevealMod.settings != null && SmartRevealMod.settings.noUraniumRequired;
            bool hasEnoughFuel = bypassFuel || (Refuelable != null && Refuelable.Fuel >= 100f);

            var toggle = new Command_Toggle
            {
                defaultLabel = isInRevealMode ? "<color=#00ff00>Reveal Mode: ON</color>" : "<color=#ff0000>Reveal Mode: OFF</color>",
                defaultDesc = bypassFuel
                    ? "Colonists will gradually reveal the entire map. (Uranium requirement disabled in settings)"
                    : "Colonists will gradually reveal the entire map. Requires 100 Uranium.",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/RevealMap", true),
                isActive = () => isInRevealMode,
                toggleAction = ToggleRevealMode
            };

            if (!hasEnoughFuel && !isInRevealMode)
            {
                toggle.disabledReason = "Not enough Uranium (100 required).";
            }

            yield return toggle;

            
            if (!bypassFuel && Refuelable != null && Refuelable.Fuel > 0f)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Extract Uranium",
                    defaultDesc = "Safely extract all loaded Uranium from the scanner back into items.",
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true),
                    action = () => ExtractUranium(quiet: false)
                };
            }
        }

        private void ToggleRevealMode()
        {
            if (Flickable != null && !Flickable.SwitchIsOn)
            {
                Messages.Message("Turn the power switch ON first!", MessageTypeDefOf.RejectInput);
                return;
            }

            bool bypassFuel = SmartRevealMod.settings != null && SmartRevealMod.settings.noUraniumRequired;
            if (!bypassFuel && Refuelable?.Fuel < 100f && !isInRevealMode)
            {
                Messages.Message("Load 100 Uranium first!", MessageTypeDefOf.RejectInput);
                return;
            }

            isInRevealMode = !isInRevealMode;

            if (isInRevealMode)
                workDone = 0f;
        }

        private void ExtractUranium(bool quiet)
        {
            if (Refuelable == null || Refuelable.Fuel <= 0f) return;

            int amountToSpawn = Mathf.FloorToInt(Refuelable.Fuel);
            Refuelable.ConsumeFuel(Refuelable.Fuel);

            if (amountToSpawn > 0)
            {
                Thing uranium = ThingMaker.MakeThing(ThingDefOf.Uranium);
                uranium.stackCount = amountToSpawn;

                IntVec3 spawnCell = parent.Faction != null ? parent.InteractionCell : parent.Position;
                GenPlace.TryPlaceThing(uranium, spawnCell, parent.Map, ThingPlaceMode.Near);

                if (!quiet)
                {
                    Messages.Message($"Successfully extracted {amountToSpawn} Uranium.", MessageTypeDefOf.PositiveEvent);
                }
            }

            if (isInRevealMode)
            {
                isInRevealMode = false;
                workDone = 0f;
            }
        }

        public override string CompInspectStringExtra()
        {
            bool bypassFuel = SmartRevealMod.settings != null && SmartRevealMod.settings.noUraniumRequired;

            if (!isInRevealMode)
            {
                return bypassFuel ? "Reveal mode: OFF" : "Reveal mode: OFF\nRequires 100 Uranium";
            }

            float percent = Mathf.Clamp01(workDone / WorkRequired) * 100f;
            return $"Reveal progress: {percent:F1}%";
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isInRevealMode, "isInRevealMode", false);
            Scribe_Values.Look(ref workDone, "workDone", 0f);
        }
    }
}
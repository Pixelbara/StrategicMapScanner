using RimWorld;
using Verse;
using Verse.AI;

namespace SmartScannerReveal
{
    public class WorkGiver_SmartReveal : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(DefDatabase<ThingDef>.GetNamed("StrategicMapScanner", true));

        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is not Building building || !building.Spawned || building.IsForbidden(pawn))
                return false;

            
            if (pawn.skills != null)
            {
                SkillRecord intellectualSkill = pawn.skills.GetSkill(SkillDefOf.Intellectual);
                if (intellectualSkill != null && intellectualSkill.Level < 6)
                {
                    JobFailReason.Is("Too ignorant (requires Intellectual 6).");
                    return false;
                }
            }

            var comp = building.GetComp<CompSmartReveal>();
            if (comp == null || !comp.isInRevealMode)
                return false;

            bool bypassFuel = SmartRevealMod.settings != null && SmartRevealMod.settings.noUraniumRequired;
            if (!bypassFuel)
            {
                if (comp.Refuelable != null && comp.Refuelable.Fuel < 100f)
                    return false;
            }

            if (building.IsBurning())
                return false;

            return pawn.CanReserve(t, 1, -1, null, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("SmartRevealMap"), t);
        }
    }
}
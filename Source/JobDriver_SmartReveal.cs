using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SmartScannerReveal
{
    public class JobDriver_SmartReveal : JobDriver
    {
        private CompSmartReveal Comp => job.targetA.Thing.TryGetComp<CompSmartReveal>();

        public override bool TryMakePreToilReservations(bool errorOnFailed = false)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil workToil = ToilMaker.MakeToil("Work");
            workToil.initAction = () => workToil.actor.pather.StopDead();

            workToil.tickAction = () =>
            {
                var comp = Comp;
                if (comp == null || !comp.isInRevealMode)
                {
                    EndJobWith(JobCondition.Succeeded);
                    return;
                }

                bool bypassFuel = SmartRevealMod.settings != null && SmartRevealMod.settings.noUraniumRequired;
                if (!bypassFuel)
                {
                    if (comp.Refuelable != null && comp.Refuelable.Fuel < 1f)
                    {
                        Messages.Message("Scanner ran out of Uranium!", MessageTypeDefOf.NegativeEvent);
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }
                }

                Pawn actor = workToil.actor;
                actor.skills.Learn(SkillDefOf.Intellectual, 0.085f);

                float speedMod = 1.0f;
                if (SmartRevealMod.settings != null)
                {
                    
                    speedMod = SmartRevealMod.settings.speedMultiplier;
                }

                float workAmount = actor.GetStatValue(StatDefOf.ResearchSpeed) * 0.85f * speedMod;
                comp.workDone += workAmount;

                if (comp.workDone >= CompSmartReveal.WorkRequired)
                {
                    comp.FinishReveal();
                    EndJobWith(JobCondition.Succeeded);
                }
            };

            workToil.defaultCompleteMode = ToilCompleteMode.Never;

            workToil.WithProgressBar(TargetIndex.A, () =>
            {
                var comp = Comp;
                return comp != null ? Mathf.Clamp01(comp.workDone / CompSmartReveal.WorkRequired) : 0f;
            });

            yield return workToil;
        }
    }
}
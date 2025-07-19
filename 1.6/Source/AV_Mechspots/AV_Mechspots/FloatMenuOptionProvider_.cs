using RimWorld;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.XR;
using Verse;
using Verse.AI;
using static RimWorld.MechClusterSketch;


namespace AV_Mechspots
{

    public class FloatMenuOptionProvider_CarryToSpot : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool MechanoidCanDo => true;

        private bool skip = true;


        public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
        {
            Pawn pawn = context.FirstSelectedPawn;

            if (clickedThing is Pawn p && p.IsColonyMech)
            {
                skip = false;
            }

            if (!skip)
            {
                foreach (Pawn mech in context.ClickedPawns)
                {
                    if (mech.ownership.AssignedMeditationSpot == null)
                    {
                        yield return new FloatMenuOption("AV_Float_CarryToSpot_NotPossible".Translate(mech.LabelShort).CapitalizeFirst(), null);
                        continue;
                    }
                    else if (mech.ownership.AssignedMeditationSpot != null)
                    {
                        if (!pawn.CanReach(mech, PathEndMode.OnCell, Danger.Deadly) || !pawn.CanReach(mech.ownership.AssignedMeditationSpot, PathEndMode.OnCell, Danger.Deadly))
                        {
                            yield return new FloatMenuOption("AV_Float_CarryToSpot_NotPossible_2".Translate(mech.LabelShort).CapitalizeFirst() + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                            continue;
                        }
                        else
                        {
                            yield return new FloatMenuOption("AV_Float_CarryToSpot_Possible".Translate(mech.LabelShort).CapitalizeFirst(), delegate
                            {
                                Job job = JobMaker.MakeJob(MechSpotDefOfs.CarryNearMechSpot, mech, mech.ownership.AssignedMeditationSpot);
                                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            });
                        }
                    }
                }
            }

            skip = true;    //resetting bool? Might not be needed
        }

    }

}




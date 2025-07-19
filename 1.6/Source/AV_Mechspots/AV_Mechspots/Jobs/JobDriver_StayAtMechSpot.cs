using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.AI;
using Verse;
using UnityEngine.UIElements;
using System.CodeDom.Compiler;
using Verse.Noise;
using static UnityEngine.GridBrushBase;
using static AV_Mechspots.CompAssignableToMech;
using AV_Framework;

namespace AV_Mechspots
{
    public class JobDriver_StayAtMechSpot : JobDriver
    {
        private const TargetIndex MechSocket = TargetIndex.A;
        private const int ConnectDuration_Work = 2500;              // 2500 tick = every hour, also the ticks after which it checks for other work
        private const int ConnectDuration_Guarding = 180;           // 180 ticks = 3 sec, also the ticks after which to check for enemies
        private const int ConnectDuration_Charging = 2500;          // 2500 tick = every hour
        private bool UseProgressbar = false;
        private bool RandomChoosen = false;

        public override bool TryMakePreToilReservations(bool errorOnFailed)     //reserve and check if progressbar should be shown
        {
            if (this.pawn.TryGetComp<Comp_SelectableSpawner>() != null)
            {
                Building socketBuilding = this.pawn.ownership.AssignedMeditationSpot;
                CompAssignableToMech socketComp = socketBuilding.TryGetComp<CompAssignableToMech>();

                if (socketComp != null)
                {

                    if (socketComp.Props.ShowProgressbar)
                    {
                        UseProgressbar = true;
                    }
                }
            }
            return this.pawn.Reserve(this.job.GetTarget(MechSocket), this.job, 1, -1, null);    //reserve the Socket, for rare situation so mechs can not "share" the same Socket
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(MechSocket);
            IntVec3 TargetVector = pawn.ownership.AssignedMeditationSpot.Position;
            /*
            if (pawn.Position != TargetVector)  //if not on targetposition? Would fix the short not showing of the progressbar, since mech is technicly moving, not a FIX!!!!!!!!!!!!
            {
                yield return Toils_Goto.GotoThing(MechSocket, PathEndMode.OnCell);
            }
            */

            //has to be not in a if statement, Toilcount always needs to be the same! Otherwise we get an error on reload
            yield return Toils_Goto.GotoThing(MechSocket, PathEndMode.OnCell);

            CompAssignableToMech CompAssignable = pawn.ownership.AssignedMeditationSpot.TryGetComp<CompAssignableToMech>();

            if (CompAssignable != null)
            {
                Toil changeRot = ToilMaker.MakeToil("MakeToilChangeRotationOnMechSpot");
                changeRot.initAction = delegate
                {
                    if (CompAssignable.AllowedRotation == (int)AllowedRotationForAssignment.any)
                    {
                        DecideToResetRandom();
                        // do nothing (let mech decide, means it depends on the way they go to the spot)
                    }
                    else if (CompAssignable.AllowedRotation == (int)AllowedRotationForAssignment.random)
                    {
                        if (RandomChoosen == false)
                        {
                            RandomChoosen = true;
                            pawn.Rotation = Rot4.Random;
                        }
                    }
                    else if (CompAssignable.AllowedRotation == (int)AllowedRotationForAssignment.north)
                    {
                        DecideToResetRandom();
                        pawn.Rotation = Rot4.North;
                    }
                    else if (CompAssignable.AllowedRotation == (int)AllowedRotationForAssignment.east)
                    {
                        DecideToResetRandom();
                        pawn.Rotation = Rot4.East;
                    }
                    else if (CompAssignable.AllowedRotation == (int)AllowedRotationForAssignment.west)
                    {
                        DecideToResetRandom();
                        pawn.Rotation = Rot4.West;
                    }
                    else if (CompAssignable.AllowedRotation == (int)AllowedRotationForAssignment.south)
                    {
                        DecideToResetRandom();
                        pawn.Rotation = Rot4.South;
                    }
                };
                changeRot.defaultCompleteMode = ToilCompleteMode.Instant;
                yield return changeRot;
            }

            if (pawn.ownership.AssignedMeditationSpot.TryGetComp<Comp_ChargeSpot>() != null)
            {
                yield return Toils_General.Wait(ConnectDuration_Charging);
            }
            else if (UseProgressbar)
            {
                yield return Toils_General.Wait(ConnectDuration_Work);  //UseProgressbar is set in TryMakePreToilReservations
            }
            else
            {
                yield return Toils_General.Wait(ConnectDuration_Guarding);
            }     
        }


        private void DecideToResetRandom()
        {
            if(RandomChoosen)
            {
                RandomChoosen = !RandomChoosen;
            }
        }


    }
}
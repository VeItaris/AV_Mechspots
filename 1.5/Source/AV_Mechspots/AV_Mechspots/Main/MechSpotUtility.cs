using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using static AV_Mechspots.CompAssignableToMech;

namespace AV_Mechspots
{
    [StaticConstructorOnStartup]
    public static class MechSpotUtility
    {

        public static bool CanPaste = false;

        public static int AssignmentAllowCounter = -1;
        public static int AllowedRotation = -1;
        public static bool ForceStayAtSpot;    //used in JobGiver to decide beheivior
        public static bool UseAsRecharger;    //used in JobGiver_GetEnergy_Charger_TryGiveJob_Patch to self recharge when low
        public static int powerlevel = -1;  //for charger

        public static void CopyFromSpot(CompAssignableToMech comp)
        {
            //AssignmentAllowCounter = comp.AssignmentAllowCounter; // no longer needed
            AllowedRotation = comp.AllowedRotation;
            ForceStayAtSpot = comp.ForceStayAtSpot;
            UseAsRecharger = comp.UseAsRecharger;

            Comp_ChargeSpot chargeSpot = comp.parent.TryGetComp<Comp_ChargeSpot>();

            if (chargeSpot != null)
            {
                powerlevel = chargeSpot.powerlevel;
            }
            else
            {
                powerlevel = -1;
            }

            if(MechspotsSettings.DebugLogging) { Log.Message("copy successful"); }
            
            CanPaste = true;

            // no longer needed
            /*
            if (AssignmentAllowCounter == (int)AllowedMechsForAssignment.disallowAll)
            {
                Messages.Message("AV_Message_Copy_NoAssignmentAllowed".Translate().CapitalizeFirst(), MessageTypeDefOf.NegativeEvent);
            }
            */
        }

        public static void PasteToSpot(CompAssignableToMech comp)
        {
            //comp.AssignmentAllowCounter = AssignmentAllowCounter; //no longer needed or used!
            comp.AllowedRotation = AllowedRotation;
            comp.ForceStayAtSpot = ForceStayAtSpot;
            comp.UseAsRecharger = UseAsRecharger;

            if (powerlevel != -1)
            {
                Comp_ChargeSpot chargeSpot = comp.parent.TryGetComp<Comp_ChargeSpot>();
                if (chargeSpot != null)
                {
                    chargeSpot.powerlevel = powerlevel;
                }
            }
            if (MechspotsSettings.DebugLogging) { Log.Message("paste successful"); }
        }

        public static float RealMechEnergyPercentage(Pawn pawn)
        {
            float maxEnergy = pawn.RaceProps.maxMechEnergy;
            float currentEnergy = pawn.needs.energy.CurLevel;

            float percentageEnergy = currentEnergy / maxEnergy; //* 100; not times 100 as mechenergy is 100 and not 1.0f

            return percentageEnergy;
        }

        public static bool ShouldSelfShutDownOnSpot(Pawn pawn, float percentageNeeded = 0.02f)
        {
            if (RealMechEnergyPercentage(pawn) > percentageNeeded)  //more than 2% => allowed
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        

        public static bool ShouldSelfRechargeOnSpot(Pawn pawn, float offset = 0.05f)    
        {
            float num = pawn.RaceProps.maxMechEnergy * pawn.GetMechControlGroup().mechRechargeThresholds.min / 100;

            if (RealMechEnergyPercentage(pawn) < num + offset)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public static bool ShouldAutoRecharge(Pawn pawn)    //from JobGiver_GetEnergy.ShouldAutoRecharge // this is not using float percentage
        {
            Need_MechEnergy energy = pawn.needs.energy;
            if (energy == null)
            {
                return false;
            }

            return energy.CurLevel + 0.1f < (float)JobGiver_GetEnergy.GetMinAutorechargeThreshold(pawn);

        }


        public static void ReclaimOldSpotAfterRebirth(Pawn mech)
        {
            //bugfix - spot looses assignment after death of its mech
            //spots loose their mech after the mech dies, the game is saved and loaded again.
            //the spot cant reassign the mech as it is dead and no longer in the cache
            //mechs still remember their previous spot after rebirth

            if (mech.ownership.AssignedMeditationSpot != null && mech.ownership.AssignedMeditationSpot.GetAssignedPawn() == null)
            {
                if(mech.ownership.AssignedMeditationSpot.HasComp<CompAssignableToMech>())
                {
                    Log.Warning("[AV]Mechanoid Spot - bugfix - Reassigning mech to its old spot after rebirth");

                    CompAssignableToMech comp = mech.ownership.AssignedMeditationSpot.GetComp<CompAssignableToMech>();
                    comp.TryAssignPawn(mech);
                }
            }

            

        }

        public static bool ForgetDestroyedOldSpot(Pawn mech)
        {
            //bugfix - spot looses assignment after death of its mech - part 2
            //spots loose their mech after the mech dies, the game is saved and loaded again.
            //the spot cant reassign the mech as it is dead and no longer in the cache
            //mechs still remember their previous spot after rebirth
            //if the spot is destroyed mechs still show reassign in the assignment menu even though they don't have a valid spot


            if (mech.ownership.AssignedMeditationSpot != null && mech.ownership.AssignedMeditationSpot.Destroyed)
            {
                Log.Warning("[AV]Mechanoid Spot - bugfix - Removing assigned but now destroyed spot from mech");

                mech.ownership.UnclaimMeditationSpot();
                return true;
            }
            return false;
        }
    }
}

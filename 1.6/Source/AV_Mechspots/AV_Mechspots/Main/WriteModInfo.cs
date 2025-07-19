using AV_Framework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace AV_Mechspots
{
    [StaticConstructorOnStartup]
    public static class WriteModInfo
    {
        public static List<ThinkTreeDef> PatchedThinkTreeDefs = DefDatabase<ThinkTreeDef>.AllDefsListForReading.FindAll((ThinkTreeDef tree) => tree.modExtensions != null && CheckModExtensions.IsMechspotCompatibleThinkTree(tree));

        static WriteModInfo()
        {
            //Log.Message("[AV]Mechspots: Mod loaded");
            LogMechSpot();

            
        }


        public static readonly List<PawnKindDef> mechList_working = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll((PawnKindDef mech) => mech.RaceProps != null && mech.RaceProps.IsMechanoid && PatchedThinkTreeDefs.Contains(mech.RaceProps.thinkTreeMain) && mech.race.GetCompProperties<CompProperties_MechPowerCell>() == null);
       
        public static readonly List<PawnKindDef> notSupportedMechList = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll((PawnKindDef mech) => mech.RaceProps != null && mech.RaceProps.IsMechanoid && (!PatchedThinkTreeDefs.Contains(mech.RaceProps.thinkTreeMain) || mech.race.GetCompProperties<CompProperties_MechPowerCell>() != null));

        public static readonly List<PawnKindDef> PowerCellMechList = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll((PawnKindDef mech) => mech.RaceProps != null && mech.RaceProps.IsMechanoid && mech.race.GetCompProperties<CompProperties_MechPowerCell>() != null);

        public static readonly List<PawnKindDef> PatchedButNotSupportedList = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll((PawnKindDef mech) => mech.RaceProps != null && mech.RaceProps.IsMechanoid && mech.RaceProps.thinkTreeMain.modExtensions != null && !CheckModExtensions.IsMechspotCompatibleThinkTree(mech.RaceProps.thinkTreeMain));


        public static readonly List<PawnKindDef> AllMechsList = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll((PawnKindDef mech) => mech.RaceProps != null && mech.RaceProps.IsMechanoid);

        public static void LogMechSpot()
        {
            int supported_mechs = 0;
            int unsupported_mechs = 0;
            int ignored_mechs = 0;
            string ignoredMechsStringList = "not useable mechs (limited power cells or not in need for a mechanitor):";
            string unsupportedMechsStringList = "not useable mechs:";
            string supportedMechsStringList = "supported mechs:";

 
            foreach (PawnKindDef aPart in mechList_working)
            {
                supported_mechs++;
                supportedMechsStringList += "\n- " + (aPart.ToString());
            }

            foreach (PawnKindDef aPart in AllMechsList)
            {
                if (!mechList_working.Contains(aPart))
                {
                    if (aPart.race.GetCompProperties<CompProperties_MechPowerCell>() != null 
                        || (aPart.RaceProps.thinkTreeMain.modExtensions != null && !CheckModExtensions.IsMechspotCompatibleThinkTree(aPart.RaceProps.thinkTreeMain))) 
                    {
                        ignored_mechs++;
                        ignoredMechsStringList += "\n- " + aPart.ToString();
                    }
                    else
                    {
                        unsupported_mechs++;
                        unsupportedMechsStringList += "\n- " + (aPart.ToString()) + " ( Thinktree: " + aPart.RaceProps.thinkTreeMain.defName + ")";
                    }
                }
            }

            if (unsupported_mechs != 0)
            {
                string label = "[AV]Mechspots: " + mechList_working.Count.ToString() + " mechs for mechanoid spots found. " + unsupported_mechs + " not usable mechs found!\nNot usable mechs will not be in the assignment list and will ignore the spots completly.\n" + unsupportedMechsStringList + "\n" + supportedMechsStringList + "\n" + ignoredMechsStringList + "\n";
                label += "\n " + Thinktreelog();
                Log.Warning(label);
                //Log.Warning("[AV]Mechtech: " + unsupported_mechs + " out of " + mechList_working.Count.ToString() + " mechs are not supported for guarding spot :(" + unsupportedMechsStringList);
            }
            else
            {
                string label = "[AV]Mechspots: " + mechList_working.Count.ToString() + " mechs for mechanoid spots found. No conflicts found. \n" + supportedMechsStringList + "\n " + ignoredMechsStringList + "\n";
                label += "\n " + Thinktreelog();
                Log.Message(label);
                //Log.Message("[AV]Mechtech: " + (supported_mechs + ignored_mechs) + " supported mechs for guard spot found. All mechs are compatible <3 \n" + supportedMechsStringList);
            }
        }

        private static string Thinktreelog()
        {
            string thinktreelog = "Patched ThinktreeDefs: ";

            foreach (ThinkTreeDef x in PatchedThinkTreeDefs)
            {
                thinktreelog += "\n - " + x.defName;
            }

            return thinktreelog;
        }

    }

    
        

}

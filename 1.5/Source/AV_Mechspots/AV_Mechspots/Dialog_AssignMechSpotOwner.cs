using AV_Framework;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Verse;
using Verse.Sound;
using static AV_Framework.SettingsHelper;
using static HarmonyLib.Code;


namespace AV_Mechspots
{
    public class Dialog_AssignMechSpotOwner : Window
    {
        private CompAssignableToMech assignable;

        private Vector2 scrollPosition;

        private const float EntryHeight = 35f;

        private const int AssignButtonWidth = 165;

        private const int SeparatorHeight = 7;


        private static List<Pawn> filteredMechlist = new List<Pawn>(16);
        private List<Pawn> tmpFilteredMechlist = new List<Pawn>(16);

        private List<Pawn> Mechanitorlist = new List<Pawn>(16);
        private Pawn CurrentlySelectedMechanitor = null;

        private bool SortByMeditationSpot = true;

        public override Vector2 InitialSize => new Vector2(620f, 500f);

        private Texture2D onlyWorkMechs = ContentFinder<Texture2D>.Get("UI/Gizmos/AV_Only_Workmechs");
        private Texture2D onlyCombatMechs = ContentFinder<Texture2D>.Get("UI/Gizmos/AV_Only_CombatMechs");
        private Texture2D allMechs = ContentFinder<Texture2D>.Get("UI/Gizmos/AV_All_Mechs");

        private Texture2D sortListName = ContentFinder<Texture2D>.Get("UI/Gizmos/AV_SortListName");
        private Texture2D sortListMechtype = ContentFinder<Texture2D>.Get("UI/Gizmos/AV_SortListMechtype");

        private Texture2D SelectOverseer = ContentFinder<Texture2D>.Get("UI/Icons/SelectOverseer");
        
        public Dialog_AssignMechSpotOwner(CompAssignableToMech assignable)
        {
            this.assignable = assignable;
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        private bool AllowedMechtypeAll = true;
        private bool AllowedMechtypeOnlyWork = false;
        private bool AllowedMechtypeOnlyCombat = false;

        private Texture2D GetSpotTexture()
        {
            if (assignable.parent.def.uiIcon != null)
            {
                return assignable.parent.def.uiIcon;
            }
            return ContentFinder<Texture2D>.Get("UI/Buttons/AV_hexagon");
        }
        private int hexagon_offset = 3;
        private Vector2 scrollPositionSelector;
        private Vector2 scrollPositionInfoView;

        public QuickSearchWidget quickSearchWidget = new QuickSearchWidget();

        private static List<Map> tmpMaps = new List<Map>();
        private static List<Pawn> tmpPawns = new List<Pawn>();
        private static List<Caravan> tmpCaravans = new List<Caravan>();
        public void UpdateMechanitorlist()
        {
            Mechanitorlist.Clear();
            Mechanitorlist.Add(null);   //null = all mechanitors

            //Find.ColonistBar.GetColonistsInOrder
            //add mechanitors here!

            tmpMaps.Clear();
            tmpMaps.AddRange(Find.Maps);
            for (int i = 0; i < tmpMaps.Count; i++)
            {
                tmpPawns.Clear();
                tmpPawns.AddRange(tmpMaps[i].mapPawns.FreeColonists);
                tmpPawns.AddRange(tmpMaps[i].mapPawns.ColonyMutantsPlayerControlled);     // errors out for someone?! - probably because their rimworld version is pirated and not the current version?
                List<Thing> list = tmpMaps[i].listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
                for (int j = 0; j < list.Count; j++)
                {
                    if (!list[j].IsDessicated())
                    {
                        Pawn innerPawn = ((Corpse)list[j]).InnerPawn;
                        if (innerPawn != null && innerPawn.IsColonist)
                        {
                            tmpPawns.Add(innerPawn);
                        }
                    }
                }
                IReadOnlyList<Pawn> allPawnsSpawned = tmpMaps[i].mapPawns.AllPawnsSpawned;
                for (int k = 0; k < allPawnsSpawned.Count; k++)
                {
                    if (allPawnsSpawned[k].carryTracker.CarriedThing is Corpse corpse && !corpse.IsDessicated() && corpse.InnerPawn.IsColonist)
                    {
                        tmpPawns.Add(corpse.InnerPawn);
                    }
                }
            }
            tmpCaravans.Clear();
            tmpCaravans.AddRange(Find.WorldObjects.Caravans);
            tmpCaravans.SortBy((Caravan x) => x.ID);
            for (int l = 0; l < tmpCaravans.Count; l++)
            {
                if (!tmpCaravans[l].IsPlayerControlled)
                {
                    continue;
                }
                //tmpPawns.Clear();
                tmpPawns.AddRange(tmpCaravans[l].PawnsListForReading);
            }

            foreach (Pawn p in tmpPawns)
            {
                if ((p.IsColonist || p.IsColonyMutantPlayerControlled) && MechanitorUtility.IsMechanitor(p)) 
                {
                    Mechanitorlist.Add(p);
                }
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect buttonRect = inRect;
            buttonRect.x = 10f;
            buttonRect.y = 10f;
            buttonRect.width = 250f;
            buttonRect.height = 29f;

            //infobutton
            Widgets.InfoCardButton(110f + SeparatorHeight, inRect.y + 5f, assignable.parent);

            //draw header
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(140f + SeparatorHeight, inRect.y, inRect.width, 35f), assignable.parent.def.label.CapitalizeFirst());
            Text.Font = GameFont.Small;

            //draw spot 
            Rect textureRect = new Rect(inRect.x + 8.5f, inRect.y + 1, 88f, 88f);
            DrawImageNotStretched(textureRect.ContractedBy(1f), GetSpotTexture());

            #region mechanitor selection
            //thing selector rect
            Rect rect = inRect;
            rect.x = 1f;
            rect.width = 112f + 2 * hexagon_offset + 10f; // 32 + 32 + 32 + 16 + 2 * hexagon_offset + 10f (so it looks like its in the middle)
            rect.y = 100f;  //icon + header
            rect.height = inRect.height - rect.y;

            //thing selector view rect
            Rect viewRect = rect;
            viewRect.width -= 16f;
            //30 needs to be changed!
            float scrollselectorheight = Mathf.CeilToInt(30 / 3 * 42) + 42 + 21;     //(float)AllProductsThingDefs().Count * 64f + 64f;  // should be  Mathf.CeilToInt(AllProductsThingDefs().Count * / imagesPerRow) * spaceRequirement_y + spaceRequirement_y / 2
            viewRect.height = (scrollselectorheight > rect.height) ? scrollselectorheight : rect.height;  //picks bigger number 


            //Widgets.BeginScrollView(rect, ref scrollPositionSelector, viewRect);
            UpdateMechanitorlist();
            float mechanitorlistheight = 0;
            MechanitorSelector(viewRect, out mechanitorlistheight);
            //Widgets.EndScrollView();
            #endregion

            //assignedRect
            Rect assignedRect = inRect;
            assignedRect.xMin += 110f + SeparatorHeight;  //140f
            assignedRect.yMin += 40f; 
            assignedRect.height = 100f;
            assignedRect.width += -16f;  // 16 = scroll width

            //draw assigned mech
            float y = assignedRect.y;
            if (assignable.AssignedPawns.Any())
            {
                DrawAssignedRow(assignable.AssignedPawns.First(), ref y, assignedRect, 1);
            }
            else
            {
                y += EntryHeight;
            }

            using (new TextBlock(Widgets.SeparatorLineColor))
            {
                Widgets.DrawLineHorizontal(assignedRect.x, y, assignedRect.width);
            }
            y += SeparatorHeight;


            #region //draw filter


            Rect filterButtonRect = assignedRect;
            filterButtonRect.x = assignedRect.x + 3f;
            filterButtonRect.width = 32f;
            filterButtonRect.y = y;
            filterButtonRect.height = filterButtonRect.width;

            DrawSelectionHighlight(AllowedMechtypeAll, filterButtonRect);
            Widgets.DrawLightHighlight(filterButtonRect);
            if (Widgets.ButtonImage(filterButtonRect, allMechs, true, "AV_MechAssignment_Filtering_Any".Translate()))
            {
                AllowedMechtypeAll = true;
                AllowedMechtypeOnlyWork = false;
                AllowedMechtypeOnlyCombat = false;
            }
            filterButtonRect.x += filterButtonRect.width + SeparatorHeight;

            DrawSelectionHighlight(AllowedMechtypeOnlyWork, filterButtonRect);
            Widgets.DrawLightHighlight(filterButtonRect);
            if (Widgets.ButtonImage(filterButtonRect, onlyWorkMechs, true, "AV_MechAssignment_Filtering_Work".Translate()))
            {
                AllowedMechtypeAll = false;
                AllowedMechtypeOnlyWork = true;
                AllowedMechtypeOnlyCombat = false;
            }
            filterButtonRect.x += filterButtonRect.width + SeparatorHeight;

            DrawSelectionHighlight(AllowedMechtypeOnlyCombat, filterButtonRect);
            Widgets.DrawLightHighlight(filterButtonRect);
            if (Widgets.ButtonImage(filterButtonRect, onlyCombatMechs, true, "AV_MechAssignment_Filtering_Combat".Translate()))
            {
                AllowedMechtypeAll = false;
                AllowedMechtypeOnlyWork = false;
                AllowedMechtypeOnlyCombat = true;
            }
            filterButtonRect.x += filterButtonRect.width + SeparatorHeight;

            DrawSelectionHighlight(SortByMeditationSpot, filterButtonRect);
            Widgets.DrawLightHighlight(filterButtonRect);
            if (Widgets.ButtonImage(filterButtonRect, sortListMechtype, true, "AV_MechAssignment_Sorting_Assignment".Translate()))
            {
                SortByMeditationSpot = true;
            }
            filterButtonRect.x += filterButtonRect.width + SeparatorHeight;

            DrawSelectionHighlight(!SortByMeditationSpot, filterButtonRect);
            Widgets.DrawLightHighlight(filterButtonRect);

            if (Widgets.ButtonImage(filterButtonRect, sortListName, true, "AV_MechAssignment_Sorting_Name".Translate()))
            {
                SortByMeditationSpot = false;
            }
            filterButtonRect.x += filterButtonRect.width + SeparatorHeight;


            Rect searchRect = filterButtonRect;
            searchRect.x += 3f;
            searchRect.width = inRect.width - searchRect.x - 16f;
            quickSearchWidget.OnGUI(searchRect, SortFilteredMechList);


            y += filterButtonRect.height + SeparatorHeight;


            #endregion


            using (new TextBlock(Widgets.SeparatorLineColor))
            {
                Widgets.DrawLineHorizontal(assignedRect.x, y, assignedRect.width);
            }


            #region scroll view


            //info rect - this is the view
            Rect inforect = inRect;
            inforect.xMin += assignedRect.xMin;
            inforect.yMin = y;
            inforect.height = 280f;

            //content rect - this is the content, should be bigger than view rect
            Rect viewInfoRect = inforect;
            viewInfoRect.width += -16f;  // 16 = scroll width
            viewInfoRect.height = filteredMechlist.Count() * EntryHeight;
            y = inforect.y;

            Widgets.BeginScrollView(inforect, ref scrollPositionSelector, viewInfoRect);

            SortFilteredMechList();
            if (!filteredMechlist.NullOrEmpty())
            {
                int counter = 0;
                foreach (Pawn pawn2 in filteredMechlist)
                {
                    DrawUnassignedRow(pawn2, ref y, viewInfoRect, counter);
                    counter++;
                }
            }
            Widgets.EndScrollView();

            //left side
            using (new TextBlock(Widgets.SeparatorLineColor))
            {
                Widgets.DrawLineVertical(inforect.x, inforect.y, Math.Min(viewInfoRect.height, inforect.height));
            }
            //bottom
            using (new TextBlock(Widgets.SeparatorLineColor))
            {
                Widgets.DrawLineHorizontal(assignedRect.x,  Math.Min(y, 401f), assignedRect.width);
            }

            #endregion

            Rect MechanitorLabelRect = new Rect();
            MechanitorLabelRect.x = 1f;
            MechanitorLabelRect.y = 1f;
            MechanitorLabelRect.width = 200f;
            MechanitorLabelRect.height = 200f;


            //Widgets.Label(MechanitorLabelRect, ref whatIsThis, "--- Mechanitor ---", TipSignal tip = default(TipSignal))


            //draw filter feedback

            Rect feedbackRect = new Rect(inRect.width - 200f, inRect.height - EntryHeight + 4f, inRect.width, EntryHeight);


            int maxCount = assignable.AssigningCandidates.Count() - assignable.AssignedPawns.Count();

            if (assignable.AssignedPawns.Any() && assignable.AssignedPawns.First().Dead)
            {
                maxCount += 1;
            }
            Widgets.Label(feedbackRect, filteredMechlist.Count() + " / " + maxCount + " " + "AV_MechAssignment_number".Translate());
        }

        private bool AllowedMechanitor(Pawn mech)
        {
            if (CurrentlySelectedMechanitor != null) //show all if null
            {
                if(mech.GetOverseer() == CurrentlySelectedMechanitor)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private void SortFilteredMechList()
        {
            tmpFilteredMechlist.Clear();

            //add mechs to list
            if (AllowedMechtypeAll)
            {
                foreach (Pawn p in assignable.AssigningCandidates)
                {
                    if (p.IsColonyMech && AllowedMechanitor(p))
                    {
                        tmpFilteredMechlist.Add(p);
                    }
                }
                //tmpFilteredMechlist.AddRange(assignable.AssigningCandidates);
            }
            else if (AllowedMechtypeOnlyWork)
            {
                foreach (Pawn p in assignable.AssigningCandidates)
                {
                    if (p.IsColonyMech && p.RaceProps.IsWorkMech && AllowedMechanitor(p))
                    {
                        tmpFilteredMechlist.Add(p);
                    }
                }
            }
            else if (AllowedMechtypeOnlyCombat)
            {
                foreach (Pawn p in assignable.AssigningCandidates)
                {
                    if (p.IsColonyMech && !p.RaceProps.IsWorkMech && AllowedMechanitor(p))
                    {
                        tmpFilteredMechlist.Add(p);
                    }
                }
            }
            
            // remove assigned pawns
            foreach (Pawn p in assignable.AssignedPawns)
            {
                if (tmpFilteredMechlist.Contains(p))
                {
                    tmpFilteredMechlist.Remove(p);
                }
                
            }

            filteredMechlist = tmpFilteredMechlist.Where((Pawn mech) => quickSearchWidget.filter.Matches(mech.Label)).ToList();

            if (SortByMeditationSpot)
            {
                filteredMechlist.SortBy((Pawn x) => x.ownership.AssignedMeditationSpot != null, (Pawn x) => x.def.label, (Pawn x) => x.LabelShort);   // named would be x.LabelShort
            }
            else
            {
                filteredMechlist.SortBy((Pawn x) => x.def.label, (Pawn x) => x.LabelShort, (Pawn x) => x.ownership.AssignedMeditationSpot != null);
            }



            //filteredMechlist = tmpFilteredMechlist;


        }

        private void DrawSelectionHighlight(bool condition, Rect rect)
        {
            if (condition)
            {
                using (new TextBlock(Widgets.MouseoverOptionColor)) //Widgets.SeparatorLineColor
                {
                    Widgets.DrawBox(rect.ExpandedBy(1f));
                }
            }
        }

        private void DrawAssignedRow(Pawn pawn, ref float y, Rect viewRect, int i)
        {
            Rect rect = new Rect(viewRect.x, y, viewRect.width, EntryHeight);
            y += EntryHeight;

            if (i % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect);
            }
            Rect rect2 = rect;
            rect2.width = rect.height;
            Widgets.ThingIcon(rect2, pawn);
            Rect rect3 = rect;
            rect3.xMin = rect.xMax - AssignButtonWidth - 10f;
            rect3 = rect3.ContractedBy(2f);
            if (Widgets.ButtonText(rect3, "BuildingUnassign".Translate()))
            {
                assignable.TryUnassignPawn(pawn);
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            Rect rect4 = rect;
            rect4.xMin = rect2.xMax + 10f;
            rect4.xMax = rect3.xMin - 10f;

            string mechLabel;

            if(pawn.Dead)
            {
                mechLabel = "(" + "AV_Destroyed".Translate() + ") " + pawn.LabelCap;
            }
            else
            {
                mechLabel = pawn.LabelCap;
            }

            using (new TextBlock(TextAnchor.MiddleLeft))
            {
                Widgets.LabelEllipses(rect4, mechLabel);
            }
        }

        private void DrawUnassignedRow(Pawn pawn, ref float y, Rect viewRect, int i)
        {
            if (assignable.AssignedPawnsForReading.Contains(pawn))
            {
                return;
            }
            AcceptanceReport acceptanceReport = assignable.CanAssignTo(pawn);
            bool accepted = acceptanceReport.Accepted;
            Rect rect = new Rect(viewRect.x, y, viewRect.width, EntryHeight);
            y += 35f;
            if (i % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect);
            }
            if (!accepted)
            {
                GUI.color = Color.gray;
            }
            Rect rect2 = rect;
            rect2.width = rect.height;
            Widgets.ThingIcon(rect2, pawn);
            Rect rect3 = rect;
            rect3.xMin = rect.xMax - AssignButtonWidth - 10f;
            rect3 = rect3.ContractedBy(2f);
            if (!Find.IdeoManager.classicMode && accepted && assignable.IdeoligionForbids(pawn))
            {
                Rect rect4 = rect;
                rect4.width = rect.height;
                rect4.x = rect.xMax - rect.height;
                rect4 = rect4.ContractedBy(2f);
                using (new TextBlock(TextAnchor.MiddleLeft))
                {
                    Widgets.Label(rect3, "IdeoligionForbids".Translate());
                }
                IdeoUIUtility.DoIdeoIcon(rect4, pawn.ideo.Ideo, doTooltip: true, delegate
                {
                    IdeoUIUtility.OpenIdeoInfo(pawn.ideo.Ideo);
                    Close();
                });
            }
            else if (accepted)
            {
                TaggedString taggedString = (assignable.AssignedAnything(pawn) ? "BuildingReassign".Translate() : "BuildingAssign".Translate());
                if (Widgets.ButtonText(rect3, taggedString))
                {
                    assignable.TryAssignPawn(pawn);
                    if (assignable.MaxAssignedPawnsCount == 1)
                    {
                        Close();
                    }
                    else
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                }
            }
            Rect rect5 = rect;
            rect5.xMin = rect2.xMax + 10f;
            rect5.xMax = rect3.xMin - 10f;
            string label = pawn.LabelCap + (accepted ? "" : (" (" + acceptanceReport.Reason.StripTags() + ")"));
            using (new TextBlock(TextAnchor.MiddleLeft))
            {
                Widgets.LabelEllipses(rect5, label);
            }
        }

        private Vector2 PawnTextureSize = new Vector2(ColonistBar.BaseSize.x - 2f, 75f);    //same as colonist bar

        public void MechanitorSelector(Rect rect, out float totalHeight)
        {
            Rect mechanitorRect = rect;
            mechanitorRect.height = EntryHeight;
            totalHeight = 0;

            Rect iconRect = mechanitorRect;
            iconRect.width = iconRect.height;

            Rect labelRect = mechanitorRect;
            labelRect.x += iconRect.width + 3f;
            labelRect.y += 6f;
            labelRect.width = mechanitorRect.width - 3f - iconRect.width;        

            foreach (Pawn mechanitor in Mechanitorlist)
            {
                DrawSelectionHighlight(CurrentlySelectedMechanitor == mechanitor, mechanitorRect);

                if (Widgets.ButtonInvisible(mechanitorRect))
                {
                    CurrentlySelectedMechanitor = mechanitor;
                }

                if (mechanitor == null)
                {
                    GUI.DrawTexture(iconRect, SelectOverseer);

                    Widgets.LabelEllipses(labelRect, "AV_Any".Translate().CapitalizeFirst());
                }
                else
                {
                    GUI.DrawTexture(iconRect, PortraitsCache.Get(mechanitor, PawnTextureSize, Rot4.South));

                    //Widgets.ThingIcon(labelRect, mechanitor);
                    Widgets.LabelEllipses(labelRect, mechanitor.LabelShort);
                }

                //Widgets.DrawBox(rect, thickness);
                //GUI.DrawTexture(rect, DeadColonistTex);
                //GUI.DrawTexture(GetPawnTextureRect(rect.position), PortraitsCache.Get(colonist, PawnTextureSize, Rot4.South, PawnTextureCameraOffset, 1.28205f));

                mechanitorRect.y += EntryHeight;
                iconRect.y += EntryHeight;
                labelRect.y += EntryHeight;

                totalHeight += EntryHeight;
            }
        }
    }
}
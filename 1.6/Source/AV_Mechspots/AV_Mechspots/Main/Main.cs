using AV_Framework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.Noise;
using static AV_Framework.SettingsHelper;


namespace AV_Mechspots
{
    public class MechspotsSettings : ModSettings
    {
        //apply default values for settings on first start with this mod

        public static bool ShowAssignedOnMechSpots      = true;
        public static bool AllowDeadColoredAssignmentLabels = true;
        public static bool AllowMechaitorColoredAssignmentLabels = false;
        public static bool ShowUnassignedOnMechSpots    = true;
        public static float ChargingSocketPulseInterval   = 2f;
        public static bool DefaultAllowRechargeOnSpot = true;
        public static bool CenteredSpots = false;
        public static float ChargingSocketEfficiency = 1f;
        public static bool AllowUsageUnpoweredSpots = false;
        public static bool ChargingSocketProducesWaste = false;
        public static int ChargingSocketWasteStorageSpace = 5;
        
        public static bool DebugLogging                 = false;

        //not for change in settings
        public static float ChargeSpotPowerUsage = 50f;
        public static float ChargeSpotPowerUsageAddition = 150f;


        // The part that writes our settings to file. Note that saving is by ref.
        public override void ExposeData()
        {
            Scribe_Values.Look(ref ShowAssignedOnMechSpots, "ShowAssignedOnMechSpots");
            Scribe_Values.Look(ref AllowDeadColoredAssignmentLabels, "AllowDeadColoredAssignmentLabels");
            Scribe_Values.Look(ref AllowMechaitorColoredAssignmentLabels, "AllowMechaitorColoredAssignmentLabels"); 
            Scribe_Values.Look(ref ShowUnassignedOnMechSpots, "ShowUnassignedOnMechSpots");
            Scribe_Values.Look(ref ChargingSocketPulseInterval, "ChargingSocketPulseInterval");
            Scribe_Values.Look(ref DefaultAllowRechargeOnSpot, "DefaultAllowRechargeOnSpot");
            Scribe_Values.Look(ref CenteredSpots, "CenteredSpots", false, true);
            Scribe_Values.Look(ref ChargingSocketEfficiency, "ChargingSocketEfficiency");
            Scribe_Values.Look(ref AllowUsageUnpoweredSpots, "AllowUsageUnpoweredSpots");
            Scribe_Values.Look(ref ChargingSocketProducesWaste, "ChargingSocketProducesWaste");
            Scribe_Values.Look(ref ChargingSocketWasteStorageSpace, "ChargingSocketWasteStorageSpace");


            Scribe_Values.Look(ref DebugLogging, "DebugLogging");

            base.ExposeData();

        }
    }

    public class MechSpotsSettings_Window : Mod
    {
        #pragma warning disable IDE0052 // Remove unread private members
        readonly MechspotsSettings settings;
        #pragma warning restore IDE0052 // Remove unread private members

        // A mandatory constructor which resolves the reference to our settings.
        public MechSpotsSettings_Window(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<MechspotsSettings>();
        }

        private int CurrentPage = (int)AV_SettingsFolder.Left;

        private Texture2D Steve => ContentFinder<Texture2D>.Get("UI/Widgets/AV_centipede");
        private Texture2D Steve_bugs => ContentFinder<Texture2D>.Get("UI/Widgets/AV_bouble_bugs");
        private Texture2D Steve_nodev => ContentFinder<Texture2D>.Get("UI/Widgets/AV_bouble_nodev");

        public override void DoSettingsWindowContents(Rect inRect)
        {
            int Folderchanged = DrawFolderStructure(inRect, CurrentPage, "General-Settings", "Spot-Settings", "DEV-Settings");

            if (Folderchanged != 0)
            {
                CurrentPage = Folderchanged;
            }
           // CorrectCurrentFolder(CurrentPage, 2);

            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(inRect);

            Foldergap(listingStandard);
            if (CurrentPage == (int)AV_SettingsFolder.Right)
            {
                DrawDEVSettings(listingStandard, inRect);
            }
            else if (CurrentPage == (int)AV_SettingsFolder.Center)
            {
                DrawSpotSettings(listingStandard, inRect);
            }
            else if (CurrentPage == (int)AV_SettingsFolder.Left)
            {
                DrawGeneralSettings(listingStandard, inRect);
            }

            listingStandard.End();


            Rect nameRect = DrawLogo(Steve, inRect);
            nameRect.y -= 5f;
            if (CurrentPage == (int)AV_SettingsFolder.Right)
            {
                if (Prefs.DevMode)
                {
                    DrawCenteredLabelAdjusted(nameRect, "Steve", x_offset: 2f);
                    DrawLogo(Steve_bugs, inRect, 160, 150);
                }
                else
                {
                    DrawCenteredLabelAdjusted(nameRect, "Steve", true, "_____", x_offset: 2f, x_offset_drafted: 1f);
                    DrawLogo(Steve_nodev, inRect, 140, 150);
                }
            }
            else
            {
                DrawCenteredLabelAdjusted(nameRect, "Steve", x_offset: 2f);
            }


            base.DoSettingsWindowContents(inRect);
        }

        private string label;

        private int CalcChargePower()
        {
            return (int)MechspotsSettings.ChargeSpotPowerUsage + (((int)MechspotsSettings.ChargingSocketPulseInterval - 1) * (int)MechspotsSettings.ChargeSpotPowerUsageAddition);
        }

        public void DrawGeneralSettings(Listing_Standard listingStandard, Rect inRect)
        {
            listingStandard.CheckboxLabeled("AV_SettingsBoxLabel_AssignMechOverlay".Translate().CapitalizeFirst(), ref MechspotsSettings.ShowAssignedOnMechSpots, "AV_SettingsBoxDesc_AssignMechOverlay".Translate().CapitalizeFirst());
            listingStandard.CheckboxLabeled("└ " + "AV_SettingsBoxLabel_ColorAssignMechOverlay".Translate().CapitalizeFirst(), ref MechspotsSettings.AllowDeadColoredAssignmentLabels, "AV_SettingsBoxDesc_ColorAssignMechOverlay".Translate().CapitalizeFirst());
 
            CheckBoxConditional(listingStandard, inRect, !FrameworkSettings.UseColoredMechlinkRange, "└ " + "AV_SettingsBoxLabel_ColorFromFramework".Translate().CapitalizeFirst(), "AV_SettingsBoxDesc_ColorFromFramework".Translate().CapitalizeFirst(), ref MechspotsSettings.AllowMechaitorColoredAssignmentLabels, "AV_SettingsBoxReason_ColorFromFramework".Translate());
            
            listingStandard.CheckboxLabeled("AV_SettingsBoxLabel_UnassignMechOverlay".Translate().CapitalizeFirst(), ref MechspotsSettings.ShowUnassignedOnMechSpots, "AV_SettingsBoxDesc_UnassignMechOverlay".Translate().CapitalizeFirst());
            
            listingStandard.GapLine();

            listingStandard.CheckboxLabeled("AV_SettingsBoxLabel_PsychicDisturber".Translate().CapitalizeFirst(), ref FrameworkSettings.UsePlaceworker, "AV_SettingsBoxDesc_PsychicDisturber".Translate().CapitalizeFirst());
            listingStandard.CheckboxLabeled("AV_SettingsBoxLabel_CenteredSpots".Translate().CapitalizeFirst(), ref MechspotsSettings.CenteredSpots, "AV_SettingsBoxDesc_CenteredSpots".Translate().CapitalizeFirst());

            listingStandard.GapLine();

            float centeredSocketOffset = 0f;
            if (MechspotsSettings.CenteredSpots)
            {
                centeredSocketOffset = 40f;
            }

            float offset = 50f;
            if(!FrameworkSettings.UsePlaceworker)
            {
                offset = 5f;
            }
            float more_offset = 150f;

            Rect socketRect = new Rect();
            socketRect.y = 300f;
            if (MechspotsSettings.CenteredSpots)
            {
                socketRect.y -= centeredSocketOffset;
            }
            socketRect.width = 128f;
            socketRect.height = socketRect.width * 2;
            socketRect.x = (inRect.width / 2) - socketRect.width - offset - more_offset;

            if (MechspotsSettings.CenteredSpots)
            {
                GUI.DrawTexture(socketRect, Centered_socket);
            }
            else
            {
                GUI.DrawTexture(socketRect, Socket);
            }

            if(MechspotsSettings.ShowAssignedOnMechSpots)
            {
                label = "Steve";
            }
            else
            {
                label = "";
            }


            if(MechspotsSettings.AllowMechaitorColoredAssignmentLabels)
            {
                DrawCenteredLabelColored(socketRect, label, Color.cyan, socketRect.height - 100f + centeredSocketOffset);
            }
            else
            {
                DrawCenteredLabel(socketRect, label, socketRect.height - 100f + centeredSocketOffset);
            }
            


            socketRect.x = (inRect.width / 2) + offset - more_offset;

            if (MechspotsSettings.CenteredSpots)
            {
                GUI.DrawTexture(socketRect, Centered_socket);
            }
            else
            {
                GUI.DrawTexture(socketRect, Socket);
            }

            if (MechspotsSettings.ShowUnassignedOnMechSpots)
            {
                label = "Unowned".Translate();
            }
            else
            {
                label = "";
            }
            
            DrawCenteredLabel(socketRect, label, socketRect.height - 100f + centeredSocketOffset);
        }
        
        public void DrawSpotSettings(Listing_Standard listingStandard, Rect inRect)
        {
            DrawAllSpotSettings(listingStandard, inRect);

            DrawRechargerSettings(listingStandard, inRect);

        }
        public void DrawAllSpotSettings(Listing_Standard listingStandard, Rect inRect)
        {

            DrawCenteredLabel(listingStandard, new Rect(0f, listingStandard.CurHeight, inRect.width, inRect.height), "AV_Settingslabel_AllSpots".Translate());

            listingStandard.GapLine();

            Rect image_rect = GetSpotImageRect(listingStandard);
            Rect settings_rect = GetSpotSettingRect(listingStandard, inRect);


            GUI.DrawTexture(image_rect, Centered_spot);

            Listing_Standard listingStandard_new = new Listing_Standard();

            listingStandard_new.Begin(settings_rect);

            listingStandard_new.Gap(28f);   //for only two options

            listingStandard_new.CheckboxLabeled("AV_SettingsBoxLabel_SolarFlare".Translate().CapitalizeFirst(), ref MechspotsSettings.AllowUsageUnpoweredSpots, "AV_SettingsBoxDesc_SolarFlare".Translate().CapitalizeFirst());

            listingStandard_new.Gap(18f);   //for only two options

            listingStandard_new.CheckboxLabeled("AV_SettingsBoxLabel_DefaultDormantRecharge".Translate().CapitalizeFirst(), ref MechspotsSettings.DefaultAllowRechargeOnSpot, "AV_SettingsBoxDesc_DefaultDormantRecharge".Translate().CapitalizeFirst());


            listingStandard_new.End();

            listingStandard.Gap(settings_rect.height);

            listingStandard.GapLine();
        }
       
        public void DrawRechargerSettings(Listing_Standard listingStandard, Rect inRect)
        {
            DrawCenteredLabel(listingStandard, new Rect(0f, listingStandard.CurHeight, inRect.width, inRect.height), "AV_Settingslabel_ChargingSocket".Translate());

            listingStandard.GapLine();

            Rect image_rect = GetSpotImageRect(listingStandard);
            Rect settings_rect = GetSpotSettingRect(listingStandard, inRect);

            GUI.DrawTexture(image_rect, Centered_charger);


            Listing_Standard listingStandard_new = new Listing_Standard();

            listingStandard_new.Begin(settings_rect);

            listingStandard_new.Gap(6f);

            listingStandard_new.CheckboxLabeled("AV_SettingsBoxLabel_ChargingSocketProducesWaste".Translate().CapitalizeFirst(), ref MechspotsSettings.ChargingSocketProducesWaste, "AV_SettingsBoxDesc_ChargingSocketProducesWaste".Translate().CapitalizeFirst());

            string label = "└ " + "AV_SliderLabel_ChargingSocketSpace".Translate().CapitalizeFirst() + ": " + MechspotsSettings.ChargingSocketWasteStorageSpace;
            MechspotsSettings.ChargingSocketWasteStorageSpace = (int)listingStandard_new.SliderLabeled(label, MechspotsSettings.ChargingSocketWasteStorageSpace, 1, 15, tooltip: "AV_SliderDesc_ChargingSocketSpace".Translate().CapitalizeFirst());


            label = "AV_Slider_DefaultCharger".Translate().CapitalizeFirst() + " " + "AV_Gizmo_Powerlvl".Translate() + ": ";

            if (MechspotsSettings.ChargingSocketPulseInterval == 1)
            {
                label += "AV_Gizmo_Powerlvl_low".Translate();
            }
            else if (MechspotsSettings.ChargingSocketPulseInterval == 2)
            {
                label += "AV_Gizmo_Powerlvl_medium".Translate();
            }
            else if (MechspotsSettings.ChargingSocketPulseInterval == 3)
            {
                label += "AV_Gizmo_Powerlvl_high".Translate();
            }
            else
            {
                label += "AV_Gizmo_Powerlvl_extreme".Translate();
            }
            label += " (" + CalcChargePower().ToString() + "W)";

            float ChargingSocketPulseInterval_notRounded = listingStandard_new.SliderLabeled(label, MechspotsSettings.ChargingSocketPulseInterval, 1f, 4f);
            MechspotsSettings.ChargingSocketPulseInterval = (int)Math.Round(ChargingSocketPulseInterval_notRounded);


            label = "AV_Slider_ChargingSocketEfficiency".Translate().CapitalizeFirst() + ": " + MechspotsSettings.ChargingSocketEfficiency + "x";

            float ChargingSocketEfficiency_notRounded = listingStandard_new.SliderLabeled(label, MechspotsSettings.ChargingSocketEfficiency, 0.2f, 5f);
            MechspotsSettings.ChargingSocketEfficiency = (float)Math.Round(ChargingSocketEfficiency_notRounded, 1);

 
            listingStandard_new.End();

            listingStandard.Gap(settings_rect.height);

            listingStandard.GapLine();
        }

        private readonly float SpotImageOffset = 8f;
        private Rect GetSpotImageRect(Listing_Standard listingStandard)
        {
            Rect image_rect = new Rect();
            image_rect.x = 16f;
            image_rect.width = 96f;
            image_rect.height = image_rect.width * 2;
            image_rect.y = listingStandard.CurHeight - image_rect.width / 2 + SpotImageOffset;

            return image_rect;
        }
        private Rect GetSpotSettingRect(Listing_Standard listingStandard, Rect inRect, float offset = 6f)
        {
            Rect image_rect = GetSpotImageRect(listingStandard);

            Rect settings_rect = new Rect();
            settings_rect.x = image_rect.width + image_rect.x * 2;
            settings_rect.width = inRect.width - settings_rect.x;
            settings_rect.height = image_rect.width + SpotImageOffset * 2 + offset + offset;    //yes width, its a 1x2 sprite
            settings_rect.y = listingStandard.CurHeight - offset / 2;

            return settings_rect;
        }

        public void DrawDEVSettings(Listing_Standard listingStandard, Rect inRect)
        {
            DrawFeedbackString(listingStandard);

            listingStandard.GapLine();

            if (Prefs.DevMode)
            {
                listingStandard.CheckboxLabeled("AV_SettingsBoxLabel_Mechspots_DevLogging".Translate().CapitalizeFirst(), ref MechspotsSettings.DebugLogging);

                Rect buttonRect = inRect;
                buttonRect.x = 10f;
                buttonRect.y = listingStandard.CurHeight + 10f;
                buttonRect.width = 250f;
                buttonRect.height = 29f;

                if (Widgets.ButtonText(buttonRect, "AV_SettingsButtonLabel_LogMechSpot".Translate().CapitalizeFirst()))
                {
                    WriteModInfo.LogMechSpot();
                }
            }
        }

        private void DrawFeedbackString(Listing_Standard listingStandard)
        {

            string label = "AV_InfoString_OutOf".Translate().CapitalizeFirst() + " " + WriteModInfo.AllMechsList.Count + " " + "AV_InfoString_usablemechs".Translate() + " " + WriteModInfo.mechList_working.Count + " " + "AV_InfoString_usespots".Translate();
            listingStandard.Label(label);

            label = "AV_InfoString_OutOf".Translate().CapitalizeFirst() + " " + WriteModInfo.notSupportedMechList.Count + " " + "AV_InfoString_unusablemechs".Translate() + " " + WriteModInfo.PowerCellMechList.Count + WriteModInfo.PatchedButNotSupportedList.Count + " " + "AV_InfoString_limitedpower".Translate();

            listingStandard.Label(label);
            listingStandard.Label("AV_InfoString_limitedpower_neverwork".Translate());

        }

        private Texture2D Socket => ContentFinder<Texture2D>.Get("Things/Building/MechSockets/AV_MechWorkSocket");

        private Texture2D Centered_socket => ContentFinder<Texture2D>.Get("Things/Building/CenteredMechSockets/AV_MechWorkSocket");

        private Texture2D Centered_charger => ContentFinder<Texture2D>.Get("Things/Building/CenteredMechSockets/AV_MechChargingSocket");

        private Texture2D Centered_spot => ContentFinder<Texture2D>.Get("Things/Building/MechSockets/AV_MechSpot");


        // Gets called on closing the settings and actually applies the settings
        public override void WriteSettings()
        {
            FrameworkSettingsUtility.ForceSaveSettings();

            base.WriteSettings();
        }

        // Override SettingsCategory to show up in the list of settings.
        public override string SettingsCategory()
        {
            return "[AV] Mechanoid Spots";
        }
       
    }

    

}

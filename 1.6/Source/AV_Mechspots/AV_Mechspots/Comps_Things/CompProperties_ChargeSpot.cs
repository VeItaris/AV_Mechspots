using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using UnityEngine;
using Verse;

namespace AV_Mechspots
{
    public class CompProperties_ChargeSpot : CompProperties
    {
        //public int maxpowerlevel = 5;
        public float basepowerpercharge = 0.006f;
        public int powerdrain = (int)MechspotsSettings.ChargeSpotPowerUsageAddition;
        public int basechargingtime = 12000;    // every 4,8 hours
        public bool writeTimeForCharge = true;

        public HediffDef standinghediff;
        public HediffDef superchargedhediff;

        public CompProperties_ChargeSpot()
        {
            compClass = typeof(Comp_ChargeSpot);
        }
    }
}

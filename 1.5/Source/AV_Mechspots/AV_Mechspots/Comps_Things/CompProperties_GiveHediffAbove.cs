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
    public class CompProperties_GiveHediffAbove : CompProperties //CompProperties_CauseHediff_AoE
    {        
        public HediffDef hediff;

        public BodyPartDef part;

        public float range = 0.5f;

        public bool onlyTargetMechs;

        public int checkInterval = 10;

        public SoundDef activeSound;

        public bool drawLine = false;


        public bool onlyassigned = false;

        public bool onlyabove = false;

        public float severity = 1f;

        public CompProperties_GiveHediffAbove()
        {
            compClass = typeof(Comp_GiveHediffAbove);
        }
    }

}

using AV_Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AV_Mechspots
{
    public class IsMechspotCompatibleThinkTree : DefModExtension
    {
        public bool IsMechspotCompatible = false;
    }

    public static class CheckModExtensions
    {
        public static bool IsMechspotCompatibleThinkTree(Def defToCheck)
        {
            if (defToCheck.HasModExtension<IsMechspotCompatibleThinkTree>())
            {
                return defToCheck.GetModExtension<IsMechspotCompatibleThinkTree>().IsMechspotCompatible;
            }
            return false;
        }
    }

}

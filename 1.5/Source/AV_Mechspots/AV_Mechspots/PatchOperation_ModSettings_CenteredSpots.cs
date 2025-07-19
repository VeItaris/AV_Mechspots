using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace AV_Mechspots
{
    public class PatchOperation_ModSettings_CenteredSpots : PatchOperation
    {
#pragma warning disable CS0649 // Field 'PatchOperation_ModSettings_CenteredSpots.match' is never assigned to, and will always have its default value null
        private readonly PatchOperation match;
#pragma warning restore CS0649 // Field 'PatchOperation_ModSettings_CenteredSpots.match' is never assigned to, and will always have its default value null

#pragma warning disable CS0649 // Field 'PatchOperation_ModSettings_CenteredSpots.nomatch' is never assigned to, and will always have its default value null
        private readonly PatchOperation nomatch;
#pragma warning restore CS0649 // Field 'PatchOperation_ModSettings_CenteredSpots.nomatch' is never assigned to, and will always have its default value null

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (MechspotsSettings.CenteredSpots)
            {
                if (match != null)
                {
                    return match.Apply(xml);
                }
            }
            else if (nomatch != null)
            {
                return nomatch.Apply(xml);
            }
            return true;   //this gives feedback on if the loading of xml was successful, on false it will not load the xml and cause an error!
        }
    }
}

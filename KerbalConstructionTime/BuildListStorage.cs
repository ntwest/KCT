using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalConstructionTime
{
    public class BuildListStorage:ConfigNodeStorage
    {
        [Persistent]
        List<BuildListItem> VABBuildList = new List<BuildListItem>();
        [Persistent]
        List<BuildListItem> SPHBuildList = new List<BuildListItem>();
        [Persistent]
        List<BuildListItem> VABWarehouse = new List<BuildListItem>();
        [Persistent]
        List<BuildListItem> SPHWarehouse = new List<BuildListItem>();

        [Persistent]Recon_Rollout LPRecon = new Recon_Rollout();


        public override void OnDecodeFromConfigNode()
        {
            GameStates.ActiveKSC.VABList.Clear();
            GameStates.ActiveKSC.SPHList.Clear();
            GameStates.ActiveKSC.VABWarehouse.Clear();
            GameStates.ActiveKSC.SPHWarehouse.Clear();
            GameStates.ActiveKSC.Recon_Rollout.Clear();

            foreach (BuildListItem b in VABBuildList)
            {
                BuildListVessel blv = b.ToBuildListVessel();
                //if (ListContains(blv, KCT_GameStates.VABList) < 0)
                GameStates.ActiveKSC.VABList.Add(blv);
            }
            foreach (BuildListItem b in SPHBuildList)
            {
                BuildListVessel blv = b.ToBuildListVessel();
                //if (ListContains(blv, KCT_GameStates.SPHList) < 0)
                GameStates.ActiveKSC.SPHList.Add(blv);
            }
            foreach (BuildListItem b in VABWarehouse)
            {
                BuildListVessel blv = b.ToBuildListVessel();
               // if (ListContains(blv, KCT_GameStates.VABWarehouse) < 0)
                GameStates.ActiveKSC.VABWarehouse.Add(blv);
            }
            foreach (BuildListItem b in SPHWarehouse)
            {
                BuildListVessel blv = b.ToBuildListVessel();
               // if (ListContains(blv, KCT_GameStates.SPHWarehouse) < 0)
                GameStates.ActiveKSC.SPHWarehouse.Add(blv);
            }
            GameStates.ActiveKSC.Recon_Rollout.Add(LPRecon);
        }

        public override void OnEncodeToConfigNode()
        {
            VABBuildList.Clear();
            SPHBuildList.Clear();
            VABWarehouse.Clear();
            SPHWarehouse.Clear();
           /* foreach (KCT_BuildListVessel b in KCT_GameStates.VABList)
            {
                if (b.shipNode == null)
                {
                    Debug.LogError("[KCT] WARNING! DATA LOSS EVENT ON " + b.shipName + " IN VABList");
                    continue;
                }
                BuildListItem bls = new BuildListItem();
                bls.FromBuildListVessel(b);
                VABBuildList.Add(bls);
            }
            foreach (KCT_BuildListVessel b in KCT_GameStates.SPHList)
            {
                if (b.shipNode == null)
                {
                    Debug.LogError("[KCT] WARNING! DATA LOSS EVENT ON " + b.shipName + " IN SPHList");
                    continue;
                }
                BuildListItem bls = new BuildListItem();
                bls.FromBuildListVessel(b);
                SPHBuildList.Add(bls);
            }
            foreach (KCT_BuildListVessel b in KCT_GameStates.VABWarehouse)
            {
                if (b.shipNode == null)
                {
                    Debug.LogError("[KCT] WARNING! DATA LOSS EVENT ON " + b.shipName + " IN VABWarehouse");
                    continue;
                }
                BuildListItem bls = new BuildListItem();
                bls.FromBuildListVessel(b);
                VABWarehouse.Add(bls);
            }
            foreach (KCT_BuildListVessel b in KCT_GameStates.SPHWarehouse)
            {
                if (b.shipNode == null)
                {
                    Debug.LogError("[KCT] WARNING! DATA LOSS EVENT ON " + b.shipName + " IN SPHWarehouse");
                    continue;
                }
                BuildListItem bls = new BuildListItem();
                bls.FromBuildListVessel(b);
                SPHWarehouse.Add(bls);
            }
            LPRecon = KCT_GameStates.LaunchPadReconditioning;*/
        }

        public class BuildListItem
        {
            [Persistent]
            string shipName, shipID;
            [Persistent]
            double progress, buildTime;
            [Persistent]
            String launchSite, flag;
            //[Persistent]
            //List<string> InventoryParts;
            [Persistent]
            bool cannotEarnScience;
            [Persistent]
            float cost = 0, mass = 0, kscDistance = 0;
            [Persistent]
            int rushBuildClicks = 0;
            [Persistent]
            int EditorFacility = 0, LaunchPadID = -1;
            [Persistent]
            List<string> desiredManifest = new List<string>();

            public BuildListVessel ToBuildListVessel()
            {
                BuildListVessel ret = new BuildListVessel(shipName, launchSite, buildTime, flag, cost, EditorFacility);
                ret.progress = progress;
                ret.id = new Guid(shipID);
                ret.cannotEarnScience = cannotEarnScience;
                ret.TotalMass = mass;
                ret.DistanceFromKSC = kscDistance;
                ret.rushBuildClicks = rushBuildClicks;
                ret.launchSiteID = LaunchPadID;
                ret.DesiredManifest = desiredManifest;
                return ret;
            }

            public BuildListItem FromBuildListVessel(BuildListVessel blv)
            {
                this.progress = blv.progress;
                this.buildTime = blv.buildPoints;
                this.launchSite = blv.launchSite;
                this.flag = blv.flag;
                //this.shipURL = blv.shipURL;
                this.shipName = blv.shipName;
                this.shipID = blv.id.ToString();
                this.cannotEarnScience = blv.cannotEarnScience;
                this.cost = blv.cost;
                this.rushBuildClicks = blv.rushBuildClicks;
                this.mass = blv.TotalMass;
                this.kscDistance = blv.DistanceFromKSC;
                this.EditorFacility = (int)blv.GetEditorFacility();
                this.LaunchPadID = blv.launchSiteID;
                this.desiredManifest = blv.DesiredManifest;
                return this;

            }
        }
    }
}

/*
The MIT License(MIT)

Portions Copyright(c) 2018 Michael Marvin
Portions Copyright(c) 2018 Nate West

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

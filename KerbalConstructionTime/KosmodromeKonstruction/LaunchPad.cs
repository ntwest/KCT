using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;

namespace KerbalConstructionTime
{
    public class LaunchPad : ConfigNodeStorage
    {
        [Persistent] public int level = 0;
        [Persistent] public string name = "LaunchPad";
        public ConfigNode DestructionNode = new ConfigNode("DestructionState");
        public bool upgradeRepair = false;
        
        public bool destroyed
        {
            get
            {
                string nodeStr = level == 2 ? "SpaceCenter/LaunchPad/Facility/LaunchPadMedium/ksp_pad_launchPad" : "SpaceCenter/LaunchPad/Facility/building";
                ConfigNode mainNode = DestructionNode.GetNode(nodeStr);
                if (mainNode == null)
                    return false;
                else
                    return !bool.Parse(mainNode.GetValue("intact"));
            }
        }

        private string LPID = "SpaceCenter/LaunchPad";

        public LaunchPad(string LPName, int lvl=0)
        {
            name = LPName;
            level = lvl;
        }

        public void Upgrade(int lvl)
        {
            //sets the new level, assumes completely repaired
            level = lvl;

            GameStates.UpdateLaunchpadDestructionState = true;
            upgradeRepair = true;
        }

        public void Rename(string newName)
        {
            //find everything that references this launchpad by name and update the name reference
            foreach (SpaceCenterConstruction ksc in GameStates.KSCs)
            {
                if (ksc.LaunchPads.Contains(this))
                {
                    if (ksc.LaunchPads.Exists(lp => lp.name == newName))
                        return; //can't name it something that already is named that

                    foreach (Recon_Rollout rr in ksc.Recon_Rollout)
                    {
                        if (rr.launchPadID == name)
                        {
                            rr.launchPadID = newName;
                        }
                    }
                    foreach (UpgradingBuilding up in ksc.KSCTech)
                    {
                        if (up.isLaunchpad && up.launchpadID == ksc.LaunchPads.IndexOf(this))
                        {
                            up.commonName = newName;
                        }
                    }
                    /*foreach (KCT_BuildListVessel blv in ksc.VABWarehouse)
                    {
                        if (blv.la)
                    }*/ //I think also done by index and not by name
                    break;
                }
            }
            name = newName;
        }

        public void SetActive()
        {
            try
            {
                Log.Trace("Switching to LaunchPad: "+name+ " lvl: "+level+" destroyed? "+destroyed);
                GameStates.ActiveKSC.ActiveLaunchPadID = GameStates.ActiveKSC.LaunchPads.IndexOf(this);

                //set the level to this level
                if (KCT_Utilities.CurrentGameIsCareer())
                {
                    foreach (Upgradeables.UpgradeableFacility facility in GetUpgradeableFacilityReferences())
                    {
                        KCTEvents.allowedToUpgrade = true;
                        facility.SetLevel(level);
                    }
                }

                //set the destroyed state to this destroyed state
                //might need to do this one frame later?
             //   RefreshDesctructibleState();
                GameStates.UpdateLaunchpadDestructionState = true;
                upgradeRepair = false;
            }
            catch (Exception e)
            {
                Log.Trace("Error while calling SetActive: " + e.Message + e.StackTrace);
            }
        }

        public void SetDestructibleStateFromNode()
        {
            foreach (DestructibleBuilding facility in GetDestructibleFacilityReferences())
            {
                /*ConfigNode aNode = new ConfigNode();
                facility.Save(aNode);
                aNode.SetValue("intact", (!destroyed).ToString());*/
                ConfigNode aNode = DestructionNode.GetNode(facility.id);
                if (aNode != null)
                    facility.Load(aNode);
            }
        }

        public void RefreshDestructionNode()
        {
            DestructionNode = new ConfigNode("DestructionState");
            foreach (DestructibleBuilding facility in GetDestructibleFacilityReferences())
            {
                ConfigNode aNode = new ConfigNode(facility.id);
                facility.Save(aNode);
                DestructionNode.AddNode(aNode);
            }
        }

        public void CompletelyRepairNode()
        {
            foreach (ConfigNode node in DestructionNode.GetNodes())
            {
                if (node.HasValue("intact"))
                    node.SetValue("intact", "True");
            }
        }

        List<Upgradeables.UpgradeableFacility> GetUpgradeableFacilityReferences()
        {
            return ScenarioUpgradeableFacilities.protoUpgradeables[LPID].facilityRefs;
        }

        List<DestructibleBuilding> GetDestructibleFacilityReferences()
        {

            List<DestructibleBuilding> destructibles = new List<DestructibleBuilding>();
            foreach (KeyValuePair<string, ScenarioDestructibles.ProtoDestructible> kvp in ScenarioDestructibles.protoDestructibles)
            {
                if (kvp.Key.Contains("LaunchPad"))
                {
                    destructibles.AddRange(kvp.Value.dBuildingRefs);
                }
            }
            return destructibles;
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

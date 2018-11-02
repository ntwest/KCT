using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalConstructionTime
{
    public class UpgradingBuilding : IKCTBuildItem
    {
        [Persistent]
        public int upgradeLevel, currentLevel, launchpadID = 0;
        [Persistent]
        public string id, commonName;
        [Persistent]
        public double progress = 0, BP = 0, cost = 0;
        [Persistent]
        public bool UpgradeProcessed = false, isLaunchpad = false;
        //public bool allowUpgrade = false;
        private SpaceCenterConstruction _KSC = null;
        public UpgradingBuilding(string facilityID, int newLevel, int oldLevel, string name)
        {
            id = facilityID;
            upgradeLevel = newLevel;
            currentLevel = oldLevel;
            commonName = name;

            Log.Trace(string.Format("Upgrade of {0} requested from {1} to {2}", name, oldLevel, newLevel));
        }

        public UpgradingBuilding()
        {

        }

        public void Downgrade()
        {
            Log.Trace("Downgrading " + commonName + " to level " + currentLevel);
            if (isLaunchpad)
            {
                KSC.LaunchPads[launchpadID].level = currentLevel;
                if (GameStates.activeKSCName != KSC.KSCName || GameStates.ActiveKSC.ActiveLaunchPadID != launchpadID)
                {
                    return;
                }
            }
            foreach (Upgradeables.UpgradeableFacility facility in GetFacilityReferences())
            {
                KCTEvents.allowedToUpgrade = true;
                facility.SetLevel(currentLevel);
            }
            //KCT_Events.allowedToUpgrade = false;
        }

        public void Upgrade()
        {
            Log.Trace("Upgrading " + commonName + " to level " + upgradeLevel);
            if (isLaunchpad)
            {
                KSC.LaunchPads[launchpadID].level = upgradeLevel;
                KSC.LaunchPads[launchpadID].DestructionNode = new ConfigNode("DestructionState");
                if (GameStates.activeKSCName != KSC.KSCName || GameStates.ActiveKSC.ActiveLaunchPadID != launchpadID)
                {
                    UpgradeProcessed = true;
                    return;
                }
                KSC.LaunchPads[launchpadID].Upgrade(upgradeLevel);
            }
            KCTEvents.allowedToUpgrade = true;
            foreach (Upgradeables.UpgradeableFacility facility in GetFacilityReferences())
            {
                facility.SetLevel(upgradeLevel);
            }
            int newLvl = KCT_Utilities.BuildingUpgradeLevel(id);
            UpgradeProcessed = (newLvl == upgradeLevel);

            Log.Trace($"Upgrade processed: {UpgradeProcessed} Current: {newLvl} Desired: {upgradeLevel}");

            //KCT_Events.allowedToUpgrade = false;
        }

        List<Upgradeables.UpgradeableFacility> GetFacilityReferences()
        {
            return ScenarioUpgradeableFacilities.protoUpgradeables[id].facilityRefs;
        }

        public void SetBP(double cost)
        {
            // BP = Math.Sqrt(cost) * 2000 * KCT_GameStates.timeSettings.OverallMultiplier;
            BP = KCT_MathParsing.GetStandardFormulaValue("KSCUpgrade", new Dictionary<string, string>() { { "C", cost.ToString() }, { "O", KCT_PresetManager.Instance.ActivePreset.timeSettings.OverallMultiplier.ToString() } });
            if (BP <= 0) { BP = 1; }
        }

        public bool AlreadyInProgress()
        {
            return (KSC != null);
        }

        public SpaceCenterConstruction KSC
        {
            get
            {
                if (_KSC == null)
                {
                    if (!isLaunchpad)
                        _KSC = GameStates.KSCs.Find(ksc => ksc.KSCTech.Find(ub => ub.id == this.id) != null);
                    else
                        _KSC = GameStates.KSCs.Find(ksc => ksc.KSCTech.Find(ub => ub.id == this.id && ub.isLaunchpad && ub.launchpadID == this.launchpadID) != null);
                }
                return _KSC;
            }
        }

        public string GetItemName()
        {
            return commonName;
        }
        public double GetBuildRate()
        {
            double rateTotal = 0;
            if (KSC != null)
            {
                foreach (double rate in KCT_Utilities.BuildRatesSPH(KSC))
                    rateTotal += rate;
                foreach (double rate in KCT_Utilities.BuildRatesVAB(KSC))
                    rateTotal += rate;
            }
            return rateTotal;
        }
        public double GetTimeLeft()
        {
            return (BP - progress) / ((IKCTBuildItem)this).GetBuildRate();
        }
        public bool IsComplete()
        {
            return progress >= BP;
        }
        public BuildListVessel.ListType GetListType()
        {
            return BuildListVessel.ListType.KSC;
        }
        public IKCTBuildItem AsIKCTBuildItem()
        {
            return this;
        }
        public void AddProgress(double amt)
        {
            progress += amt;
            if (progress > BP) progress = BP;
        }
    }
}

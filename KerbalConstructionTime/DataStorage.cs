using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalConstructionTime
{
    public abstract class ConfigNodeStorage : IPersistenceLoad, IPersistenceSave
    {
        public ConfigNodeStorage() { }

        void IPersistenceLoad.PersistenceLoad()
        {
            OnDecodeFromConfigNode();
        }

        void IPersistenceSave.PersistenceSave()
        {
            OnEncodeToConfigNode();
        }

        public virtual void OnDecodeFromConfigNode() { }
        public virtual void OnEncodeToConfigNode() { }

        public ConfigNode AsConfigNode()
        {
            try
            {
                //Create a new Empty Node with the class name
                ConfigNode cnTemp = new ConfigNode(this.GetType().Name);
                //Load the current object in there
                cnTemp = ConfigNode.CreateConfigFromObject(this, cnTemp);
                return cnTemp;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                //Logging and return value?                    
                return new ConfigNode(this.GetType().Name);
            }
        }
    }

    public class FakePart : ConfigNodeStorage
    {
        [Persistent] public string part = "";
    }

    public class FakeTechNode : ConfigNodeStorage
    {
        [Persistent] public string id = "";
        [Persistent] public string state = "";

        public ProtoTechNode ToProtoTechNode()
        {
            ProtoTechNode ret = new ProtoTechNode();
            ret.techID = id;
            if (state == "Available")
                ret.state = RDTech.State.Available;
            else
                ret.state = RDTech.State.Unavailable;
            return ret;
        }

        public FakeTechNode FromProtoTechNode(ProtoTechNode node)
        {
            this.id = node.techID;
            this.state = node.state.ToString();
            return this;
        }
    }
    public class DataStorage : ConfigNodeStorage
    {
        [Persistent] bool enabledForSave = (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX
            || (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX));




        //[Persistent] bool firstStart = true;
        [Persistent] List<int> VABUpgrades = new List<int>() {0};
        [Persistent] List<int> SPHUpgrades = new List<int>() {0};
        [Persistent] List<int> RDUpgrades = new List<int>() {0,0};
        [Persistent] List<int> PurchasedUpgrades = new List<int>() {0,0};
        [Persistent] List<String> PartTracker = new List<String>();
        [Persistent] List<String> PartInventory = new List<String>();
        [Persistent] string activeKSC = "";
        [Persistent] float SalesFigures = 0;
        [Persistent] int UpgradesResetCounter = 0, TechUpgrades = 0, SavedUpgradePointsPreAPI = 0;


        public override void OnDecodeFromConfigNode()
        {
            //KCT_GameStates.PartTracker = ListToDict(PartTracker);
            //KCT_GameStates.PartInventory = ListToDict(PartInventory);
          /*  KCT_GameStates.ActiveKSC.VABUpgrades = VABUpgrades;
            KCT_GameStates.ActiveKSC.SPHUpgrades = SPHUpgrades;
            KCT_GameStates.ActiveKSC.RDUpgrades = RDUpgrades;*/
            GameStates.PurchasedUpgrades = PurchasedUpgrades;
            GameStates.activeKSCName = activeKSC;
            //KCT_GameStates.InventorySalesFigures = SalesFigures;
            //KCT_GameStates.InventorySaleUpgrades = (float)KCT_MathParsing.GetStandardFormulaValue("InventorySales", new Dictionary<string, string> { { "V", "0" }, { "P", SalesFigures.ToString() } });
            GameStates.UpgradesResetCounter = UpgradesResetCounter;
            GameStates.TechUpgradesTotal = TechUpgrades;
            GameStates.PermanentModAddedUpgradesButReallyWaitForTheAPI = SavedUpgradePointsPreAPI;

            SetSettings();
            //KCT_GameStates.firstStart = firstStart;
        }

        public override void OnEncodeToConfigNode()
        {
            //PartTracker = DictToList(KCT_GameStates.PartTracker);
            //PartInventory = DictToList(KCT_GameStates.PartInventory);
           // enabledForSave = KCT_GameStates.settings.enabledForSave;
            /*VABUpgrades = KCT_GameStates.VABUpgrades;
            SPHUpgrades = KCT_GameStates.SPHUpgrades;
            RDUpgrades = KCT_GameStates.RDUpgrades;*/
            TechUpgrades = GameStates.TechUpgradesTotal;
            PurchasedUpgrades = GameStates.PurchasedUpgrades;
            //firstStart = KCT_GameStates.firstStart;
            activeKSC = GameStates.ActiveKSC.KSCName;
            SalesFigures = GameStates.InventorySalesFigures;
            UpgradesResetCounter = GameStates.UpgradesResetCounter;
            SavedUpgradePointsPreAPI = GameStates.PermanentModAddedUpgradesButReallyWaitForTheAPI;

            GetSettings();
        }

        private void SetSettings()
        {
            //KCT_GameStates.settings.enabledForSave = enabledForSave;
            /*KCT_GameStates.settings.RecoveryModifier = RecoveryModifier;
            KCT_GameStates.settings.DisableBuildTime = DisableBuildTime;
            KCT_GameStates.settings.InstantTechUnlock = InstantTechUnlock;
            KCT_GameStates.settings.EnableAllBodies = EnableAllBodies;
            KCT_GameStates.settings.Reconditioning = Reconditioning;*/
        }

        private void GetSettings()
        {
           // enabledForSave = KCT_GameStates.settings.enabledForSave;
            /*RecoveryModifier = KCT_GameStates.settings.RecoveryModifier;
            DisableBuildTime = KCT_GameStates.settings.DisableBuildTime;
            InstantTechUnlock = KCT_GameStates.settings.InstantTechUnlock;
            EnableAllBodies = KCT_GameStates.settings.EnableAllBodies;
            Reconditioning = KCT_GameStates.settings.Reconditioning;*/
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

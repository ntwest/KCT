using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalConstructionTime
{
    public class SpaceCenterConstruction
    {
        public string KSCName;
        public List<BuildListVessel> VABList = new List<BuildListVessel>();
        public List<BuildListVessel> VABWarehouse = new List<BuildListVessel>();
        public List<BuildListVessel> SPHList = new List<BuildListVessel>();
        public List<BuildListVessel> SPHWarehouse = new List<BuildListVessel>();
        public List<UpgradingBuilding> KSCTech = new List<UpgradingBuilding>();
        //public List<KCT_TechItem> TechList = new List<KCT_TechItem>();
        public List<int> VABUpgrades = new List<int>() { 0 };
        public List<int> SPHUpgrades = new List<int>() { 0 };
        public List<int> RDUpgrades = new List<int>() { 0, 0 }; //research/development
        public List<Recon_Rollout> Recon_Rollout = new List<Recon_Rollout>();
        public List<double> VABRates = new List<double>(), SPHRates = new List<double>();
        public List<double> UpVABRates = new List<double>(), UpSPHRates = new List<double>();

        public List<LaunchPad> LaunchPads = new List<LaunchPad>();
        public int ActiveLaunchPadID = 0;

        public SpaceCenterConstruction(string name)
        {
            const string logBlockName = nameof( SpaceCenterConstruction ) + "..ctor";
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                var sls = PSystemSetup.Instance.StockLaunchSites.ToList();
                var nsls = PSystemSetup.Instance.NonStockLaunchSites.ToList();

                sls.ForEach( x =>
                 {
                     Log.Info( "Looking at Stock Launch Sites" );
                     Log.Info( x.name );
                     Log.Info( x.launchSiteName );
                     Log.Info( x.nodeType.ToString() );
                     Log.Info( x.pqsName );
                 } );
                nsls.ForEach( x =>
                {
                    Log.Info( "Looking at NonStock Launch Sites" );
                    Log.Info( x.name );
                    Log.Info( x.launchSiteName );                    
                    Log.Info( x.nodeType.ToString() );
                    Log.Info( x.pqsName );
                } );


                KSCName = name;
                //We propogate the tech list and upgrades throughout each KSC, since it doesn't make sense for each one to have its own tech.
                RDUpgrades[1] = GameStates.TechUpgradesTotal;
                //TechList = KCT_GameStates.ActiveKSC.TechList;
                LaunchPads.Add( new LaunchPad( "LaunchPad", KCT_Utilities.BuildingUpgradeLevel( SpaceCenterFacility.LaunchPad ) ) );
            }
        }

        public LaunchPad ActiveLPInstance
        {
            get
            {
                const string logBlockName = nameof( SpaceCenterConstruction ) + "." + nameof ( ActiveLPInstance ) + ".get()";
                using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
                {
                    return LaunchPads.Count > ActiveLaunchPadID ? LaunchPads[ActiveLaunchPadID] : null;
                }
            }
        }

        public int LaunchPadCount
        {
            get
            {
                int count = 0;
                foreach (LaunchPad lp in LaunchPads)
                    if (lp.level >= 0) count++;
                return count;
            }
        }

        public Recon_Rollout GetReconditioning(string launchSite = "LaunchPad")
        {
            const string logBlockName = nameof( SpaceCenterConstruction ) + "." + nameof( GetReconditioning );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                return Recon_Rollout.FirstOrDefault( r => r.launchPadID == launchSite && ((IKCTBuildItem)r).GetItemName() == "LaunchPad Reconditioning" );
            }
        }

        public Recon_Rollout GetReconRollout(Recon_Rollout.RolloutReconType type, string launchSite = "LaunchPad")
        {
            const string logBlockName = nameof( SpaceCenterConstruction ) + "." + nameof( GetReconRollout );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                return Recon_Rollout.FirstOrDefault( r => r.RRType == type && r.launchPadID == launchSite );
            }
        }

        public void RecalculateBuildRates()
        {
            const string logBlockName = nameof( SpaceCenterConstruction ) + "." + nameof( RecalculateBuildRates );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                VABRates.Clear();
                SPHRates.Clear();
                double rate = 0.1;
                int index = 0;
                while (rate > 0)
                {
                    rate = KCT_MathParsing.ParseBuildRateFormula( BuildListVessel.ListType.VAB, index, this );
                    if (rate >= 0)
                        VABRates.Add( rate );
                    index++;
                }
                rate = 0.1;
                index = 0;
                while (rate > 0)
                {
                    rate = KCT_MathParsing.ParseBuildRateFormula( BuildListVessel.ListType.SPH, index, this );
                    if (rate >= 0)
                        SPHRates.Add( rate );
                    index++;
                }

                Log.Trace( "VAB Rates:" );
                foreach (double v in VABRates)
                {
                    Log.Trace( v.ToString() );
                }

                Log.Trace( "SPH Rates:" );
                foreach (double v in SPHRates)
                {
                    Log.Trace( v.ToString() );
                }
            }
        }

        public void RecalculateUpgradedBuildRates()
        {
            const string logBlockName = nameof( SpaceCenterConstruction ) + "." + nameof( RecalculateUpgradedBuildRates );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                UpVABRates.Clear();
                UpSPHRates.Clear();
                double rate = 0.1;
                int index = 0;
                while (rate > 0)
                {
                    rate = KCT_MathParsing.ParseBuildRateFormula( BuildListVessel.ListType.VAB, index, this, true );
                    if (rate >= 0 && (index == 0 || VABRates[index - 1] > 0))
                        UpVABRates.Add( rate );
                    else
                        break;
                    index++;
                }
                rate = 0.1;
                index = 0;
                while (rate > 0)
                {
                    rate = KCT_MathParsing.ParseBuildRateFormula( BuildListVessel.ListType.SPH, index, this, true );
                    if (rate >= 0 && (index == 0 || SPHRates[index - 1] > 0))
                        UpSPHRates.Add( rate );
                    else
                        break;
                    index++;
                }
            }
        }

        public void SwitchLaunchPad(int LP_ID, bool updateDestrNode = true)
        {
            const string logBlockName = nameof( SpaceCenterConstruction ) + "." + nameof( SwitchLaunchPad );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                Log.Trace( "LP_ID = ".MemoizedConcat(LP_ID.MemoizedToString()));
                Log.Trace( "updateDestrNode = ".MemoizedConcat( updateDestrNode.MemoizedToString() ) );

                //set the active LP's new state
                //activate new pad

                //LaunchPads[ActiveLaunchPadID].level = KCT_Utilities.BuildingUpgradeLevel(SpaceCenterFacility.LaunchPad);
                //LaunchPads[ActiveLaunchPadID].destroyed = !KCT_Utilities.LaunchFacilityIntact(KCT_BuildListVessel.ListType.VAB); //Might want to remove this as well
                if (updateDestrNode)
                    ActiveLPInstance.RefreshDestructionNode();

                try
                {
                    LaunchPads[LP_ID].SetActive();
                }
                catch 
            }
        }

        /// <summary>
        /// Finds the highest level LaunchPad on the KSC
        /// </summary>
        /// <returns>The instance of the highest level LaunchPad</returns>
        public LaunchPad GetHighestLevelLaunchPad()
        {
            const string logBlockName = nameof( SpaceCenterConstruction ) + "." + nameof( GetHighestLevelLaunchPad );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                LaunchPad highest = LaunchPads[0];

                foreach (LaunchPad pad in LaunchPads)
                {
                    if (pad.level > highest.level)
                    {
                        highest = pad;
                    }
                }
                return highest;
            }
        }

        public ConfigNode AsConfigNode()
        {
            const string logBlockName = nameof( SpaceCenterConstruction ) + "." + nameof( AsConfigNode );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                Log.Trace( "Saving KSC " + KSCName );
                ConfigNode node = new ConfigNode( "KSC" );
                node.AddValue( "KSCName", KSCName );
                node.AddValue( "ActiveLPID", ActiveLaunchPadID );

                ConfigNode vabup = new ConfigNode( "VABUpgrades" );
                foreach (int upgrade in VABUpgrades)
                {
                    vabup.AddValue( "Upgrade", upgrade.ToString() );
                }
                node.AddNode( vabup );

                ConfigNode sphup = new ConfigNode( "SPHUpgrades" );
                foreach (int upgrade in SPHUpgrades)
                {
                    sphup.AddValue( "Upgrade", upgrade.ToString() );
                }
                node.AddNode( sphup );

                ConfigNode rdup = new ConfigNode( "RDUpgrades" );
                foreach (int upgrade in RDUpgrades)
                {
                    rdup.AddValue( "Upgrade", upgrade.ToString() );
                }
                node.AddNode( rdup );

                ConfigNode vabl = new ConfigNode( "VABList" );
                foreach (BuildListVessel blv in VABList)
                {
                    BuildListStorage.BuildListItem ship = new BuildListStorage.BuildListItem();
                    ship.FromBuildListVessel( blv );
                    ConfigNode cnTemp = new ConfigNode( "KCTVessel" );
                    cnTemp = ConfigNode.CreateConfigFromObject( ship, cnTemp );
                    ConfigNode shipNode = new ConfigNode( "ShipNode" );
                    blv.shipNode.CopyTo( shipNode );
                    cnTemp.AddNode( shipNode );
                    vabl.AddNode( cnTemp );
                }
                node.AddNode( vabl );

                ConfigNode sphl = new ConfigNode( "SPHList" );
                foreach (BuildListVessel blv in SPHList)
                {
                    BuildListStorage.BuildListItem ship = new BuildListStorage.BuildListItem();
                    ship.FromBuildListVessel( blv );
                    ConfigNode cnTemp = new ConfigNode( "KCTVessel" );
                    cnTemp = ConfigNode.CreateConfigFromObject( ship, cnTemp );
                    ConfigNode shipNode = new ConfigNode( "ShipNode" );
                    blv.shipNode.CopyTo( shipNode );
                    cnTemp.AddNode( shipNode );
                    sphl.AddNode( cnTemp );
                }
                node.AddNode( sphl );

                ConfigNode vabwh = new ConfigNode( "VABWarehouse" );
                foreach (BuildListVessel blv in VABWarehouse)
                {
                    BuildListStorage.BuildListItem ship = new BuildListStorage.BuildListItem();
                    ship.FromBuildListVessel( blv );
                    ConfigNode cnTemp = new ConfigNode( "KCTVessel" );
                    cnTemp = ConfigNode.CreateConfigFromObject( ship, cnTemp );
                    ConfigNode shipNode = new ConfigNode( "ShipNode" );
                    blv.shipNode.CopyTo( shipNode );
                    cnTemp.AddNode( shipNode );
                    vabwh.AddNode( cnTemp );
                }
                node.AddNode( vabwh );

                ConfigNode sphwh = new ConfigNode( "SPHWarehouse" );
                foreach (BuildListVessel blv in SPHWarehouse)
                {
                    BuildListStorage.BuildListItem ship = new BuildListStorage.BuildListItem();
                    ship.FromBuildListVessel( blv );
                    ConfigNode cnTemp = new ConfigNode( "KCTVessel" );
                    cnTemp = ConfigNode.CreateConfigFromObject( ship, cnTemp );
                    ConfigNode shipNode = new ConfigNode( "ShipNode" );
                    blv.shipNode.CopyTo( shipNode );
                    cnTemp.AddNode( shipNode );
                    sphwh.AddNode( cnTemp );
                }
                node.AddNode( sphwh );

                ConfigNode upgradeables = new ConfigNode( "KSCTech" );
                foreach (UpgradingBuilding buildingTech in KSCTech)
                {
                    ConfigNode bT = new ConfigNode( "UpgradingBuilding" );
                    bT = ConfigNode.CreateConfigFromObject( buildingTech, bT );
                    upgradeables.AddNode( bT );
                }
                node.AddNode( upgradeables );

                /*ConfigNode tech = new ConfigNode("TechList");
                foreach (KCT_TechItem techItem in TechList)
                {
                    KCT_TechStorageItem techNode = new KCT_TechStorageItem();
                    techNode.FromTechItem(techItem);
                    ConfigNode cnTemp = new ConfigNode("Tech");
                    cnTemp = ConfigNode.CreateConfigFromObject(techNode, cnTemp);
                    ConfigNode protoNode = new ConfigNode("ProtoNode");
                    techItem.protoNode.Save(protoNode);
                    cnTemp.AddNode(protoNode);
                    tech.AddNode(cnTemp);
                }
                node.AddNode(tech);*/

                ConfigNode RRCN = new ConfigNode( "Recon_Rollout" );
                foreach (Recon_Rollout rr in Recon_Rollout)
                {
                    ConfigNode rrCN = new ConfigNode( "Recon_Rollout_Item" );
                    rrCN = ConfigNode.CreateConfigFromObject( rr, rrCN );
                    RRCN.AddNode( rrCN );
                }
                node.AddNode( RRCN );

                ConfigNode LPs = new ConfigNode( "LaunchPads" );
                foreach (LaunchPad lp in LaunchPads)
                {
                    ConfigNode lpCN = lp.AsConfigNode();
                    lpCN.AddNode( lp.DestructionNode );
                    LPs.AddNode( lpCN );
                }
                node.AddNode( LPs );

                //Cache the regular rates
                ConfigNode CachedVABRates = new ConfigNode( "VABRateCache" );
                foreach (double rate in VABRates)
                {
                    CachedVABRates.AddValue( "rate", rate );
                }
                node.AddNode( CachedVABRates );

                ConfigNode CachedSPHRates = new ConfigNode( "SPHRateCache" );
                foreach (double rate in SPHRates)
                {
                    CachedSPHRates.AddValue( "rate", rate );
                }
                node.AddNode( CachedSPHRates );
                return node;
            }
        }

        public SpaceCenterConstruction FromConfigNode(ConfigNode node)
        {
            const string logBlockName = nameof( SpaceCenterConstruction ) + "." + nameof( AsConfigNode );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                VABUpgrades.Clear();
                SPHUpgrades.Clear();
                RDUpgrades.Clear();
                VABList.Clear();
                VABWarehouse.Clear();
                SPHList.Clear();
                SPHWarehouse.Clear();
                KSCTech.Clear();
                //TechList.Clear();
                Recon_Rollout.Clear();
                VABRates.Clear();
                SPHRates.Clear();



                this.KSCName = node.GetValue( "KSCName" );
                if (!int.TryParse( node.GetValue( "ActiveLPID" ), out this.ActiveLaunchPadID ))
                    this.ActiveLaunchPadID = 0;
                ConfigNode vabup = node.GetNode( "VABUpgrades" );
                foreach (string upgrade in vabup.GetValues( "Upgrade" ))
                {
                    this.VABUpgrades.Add( int.Parse( upgrade ) );
                }
                ConfigNode sphup = node.GetNode( "SPHUpgrades" );
                foreach (string upgrade in sphup.GetValues( "Upgrade" ))
                {
                    this.SPHUpgrades.Add( int.Parse( upgrade ) );
                }
                ConfigNode rdup = node.GetNode( "RDUpgrades" );
                foreach (string upgrade in rdup.GetValues( "Upgrade" ))
                {
                    this.RDUpgrades.Add( int.Parse( upgrade ) );
                }

                ConfigNode tmp = node.GetNode( "VABList" );
                foreach (ConfigNode vessel in tmp.GetNodes( "KCTVessel" ))
                {
                    BuildListStorage.BuildListItem listItem = new BuildListStorage.BuildListItem();
                    ConfigNode.LoadObjectFromConfig( listItem, vessel );
                    BuildListVessel blv = listItem.ToBuildListVessel();
                    blv.shipNode = vessel.GetNode( "ShipNode" );
                    blv.KSC = this;
                    this.VABList.Add( blv );
                }

                tmp = node.GetNode( "SPHList" );
                foreach (ConfigNode vessel in tmp.GetNodes( "KCTVessel" ))
                {
                    BuildListStorage.BuildListItem listItem = new BuildListStorage.BuildListItem();
                    ConfigNode.LoadObjectFromConfig( listItem, vessel );
                    BuildListVessel blv = listItem.ToBuildListVessel();
                    blv.shipNode = vessel.GetNode( "ShipNode" );
                    blv.KSC = this;
                    this.SPHList.Add( blv );
                }

                tmp = node.GetNode( "VABWarehouse" );
                foreach (ConfigNode vessel in tmp.GetNodes( "KCTVessel" ))
                {
                    BuildListStorage.BuildListItem listItem = new BuildListStorage.BuildListItem();
                    ConfigNode.LoadObjectFromConfig( listItem, vessel );
                    BuildListVessel blv = listItem.ToBuildListVessel();
                    blv.shipNode = vessel.GetNode( "ShipNode" );
                    blv.KSC = this;
                    this.VABWarehouse.Add( blv );
                }

                tmp = node.GetNode( "SPHWarehouse" );
                foreach (ConfigNode vessel in tmp.GetNodes( "KCTVessel" ))
                {
                    BuildListStorage.BuildListItem listItem = new BuildListStorage.BuildListItem();
                    ConfigNode.LoadObjectFromConfig( listItem, vessel );
                    BuildListVessel blv = listItem.ToBuildListVessel();
                    blv.shipNode = vessel.GetNode( "ShipNode" );
                    blv.KSC = this;
                    this.SPHWarehouse.Add( blv );
                }

                /* tmp = node.GetNode("TechList");
                 foreach (ConfigNode techNode in tmp.GetNodes("Tech"))
                 {
                     KCT_TechStorageItem techStorageItem = new KCT_TechStorageItem();
                     ConfigNode.LoadObjectFromConfig(techStorageItem, techNode);
                     KCT_TechItem techItem = techStorageItem.ToTechItem();
                     techItem.protoNode = new ProtoTechNode(techNode.GetNode("ProtoNode"));
                     this.TechList.Add(techItem);
                 }*/

                tmp = node.GetNode( "Recon_Rollout" );
                foreach (ConfigNode RRCN in tmp.GetNodes( "Recon_Rollout_Item" ))
                {
                    Recon_Rollout tempRR = new Recon_Rollout();
                    ConfigNode.LoadObjectFromConfig( tempRR, RRCN );
                    Recon_Rollout.Add( tempRR );
                }

                if (node.HasNode( "KSCTech" ))
                {
                    tmp = node.GetNode( "KSCTech" );
                    foreach (ConfigNode upBuild in tmp.GetNodes( "UpgradingBuilding" ))
                    {
                        UpgradingBuilding tempUP = new UpgradingBuilding();
                        ConfigNode.LoadObjectFromConfig( tempUP, upBuild );
                        KSCTech.Add( tempUP );
                    }
                }

                if (node.HasNode( "LaunchPads" ))
                {
                    LaunchPads.Clear();
                    tmp = node.GetNode( "LaunchPads" );
                    foreach (ConfigNode LP in tmp.GetNodes( "KCT_LaunchPad" ))
                    {
                        LaunchPad tempLP = new LaunchPad( "LP0" );
                        ConfigNode.LoadObjectFromConfig( tempLP, LP );
                        tempLP.DestructionNode = LP.GetNode( "DestructionState" );
                        LaunchPads.Add( tempLP );
                    }
                }

                if (node.HasNode( "VABRateCache" ))
                {
                    foreach (string rate in node.GetNode( "VABRateCache" ).GetValues( "rate" ))
                    {
                        double r;
                        if (double.TryParse( rate, out r ))
                        {
                            VABRates.Add( r );
                        }
                    }
                }

                if (node.HasNode( "SPHRateCache" ))
                {
                    foreach (string rate in node.GetNode( "SPHRateCache" ).GetValues( "rate" ))
                    {
                        double r;
                        if (double.TryParse( rate, out r ))
                        {
                            SPHRates.Add( r );
                        }
                    }
                }

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

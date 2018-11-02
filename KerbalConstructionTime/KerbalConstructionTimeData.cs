using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalConstructionTime
{

    [KSPScenario( ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION } )]
    public class KerbalConstructionTimeData : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            if (KCT_Utilities.CurrentGameIsMission())
            {
                return;
            }
            // Boolean error = false;
            Log.Trace( "Writing to persistence." );
            base.OnSave( node );
            DataStorage kctVS = new DataStorage();
            node.AddNode( kctVS.AsConfigNode() );
            foreach (SpaceCenterConstruction KSC in GameStates.KSCs)
            {
                if (KSC != null && KSC.KSCName != null && KSC.KSCName.Length > 0)
                    node.AddNode( KSC.AsConfigNode() );
            }
            ConfigNode tech = new ConfigNode( "TechList" );
            foreach (KCT_TechItem techItem in GameStates.TechList)
            {
                KCT_TechStorageItem techNode = new KCT_TechStorageItem();
                techNode.FromTechItem( techItem );
                ConfigNode cnTemp = new ConfigNode( "Tech" );
                cnTemp = ConfigNode.CreateConfigFromObject( techNode, cnTemp );
                ConfigNode protoNode = new ConfigNode( "ProtoNode" );
                techItem.protoNode.Save( protoNode );
                cnTemp.AddNode( protoNode );
                tech.AddNode( cnTemp );
            }
            node.AddNode( tech );
        }
        public override void OnLoad(ConfigNode node)
        {

            base.OnLoad( node );
            if (KCT_Utilities.CurrentGameIsMission())
            {
                return;
            }
            Log.Trace( "Reading from persistence." );
            GameStates.KSCs.Clear();
            GameStates.ActiveKSC = null;
            //KCT_Utilities.SetActiveKSC("Stock");
            GameStates.TechList.Clear();
            GameStates.TechUpgradesTotal = 0;

            DataStorage kctVS = new DataStorage();
            ConfigNode CN = node.GetNode( kctVS.GetType().Name );
            if (CN != null)
                ConfigNode.LoadObjectFromConfig( kctVS, CN );

            foreach (ConfigNode ksc in node.GetNodes( "KSC" ))
            {
                string name = ksc.GetValue( "KSCName" );
                SpaceCenterConstruction loaded_KSC = new SpaceCenterConstruction( name );
                loaded_KSC.FromConfigNode( ksc );
                if (loaded_KSC != null && loaded_KSC.KSCName != null && loaded_KSC.KSCName.Length > 0)
                {
                    loaded_KSC.RDUpgrades[1] = GameStates.TechUpgradesTotal;
                    if (GameStates.KSCs.Find( k => k.KSCName == loaded_KSC.KSCName ) == null)
                        GameStates.KSCs.Add( loaded_KSC );
                }
            }
            KCT_Utilities.SetActiveKSCToRSS();


            ConfigNode tmp = node.GetNode( "TechList" );
            if (tmp != null)
            {
                foreach (ConfigNode techNode in tmp.GetNodes( "Tech" ))
                {
                    KCT_TechStorageItem techStorageItem = new KCT_TechStorageItem();
                    ConfigNode.LoadObjectFromConfig( techStorageItem, techNode );
                    KCT_TechItem techItem = techStorageItem.ToTechItem();
                    techItem.protoNode = new ProtoTechNode( techNode.GetNode( "ProtoNode" ) );
                    GameStates.TechList.Add( techItem );
                }
            }

            KCT_GUI.CheckToolbar();
            GameStates.erroredDuringOnLoad.OnLoadFinish();
            //KerbalConstructionTime.DelayedStart();
        }
    }

}

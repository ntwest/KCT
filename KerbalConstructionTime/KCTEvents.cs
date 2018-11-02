using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using KSP.UI.Screens;

namespace KerbalConstructionTime
{
    class KCTEvents
    {
        public static KCTEvents instance = new KCTEvents();
        public bool eventAdded;

        public KCTEvents()
        {
            const string logBlockName = nameof( KCTEvents ) + ".CTOR";
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                eventAdded = false;
            }
        }

        public void addEvents()
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof (addEvents ) ;
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                GameEvents.onGUILaunchScreenSpawn.Add( launchScreenOpenEvent );
                GameEvents.onVesselRecovered.Add( vesselRecoverEvent );

                //GameEvents.onLaunch.Add(vesselSituationChange);
                GameEvents.onVesselSituationChange.Add( vesselSituationChange );
                GameEvents.onGameSceneLoadRequested.Add( gameSceneEvent );
                GameEvents.OnTechnologyResearched.Add( TechUnlockEvent );
                //if (!ToolbarManager.ToolbarAvailable || !KCT_GameStates.settings.PreferBlizzyToolbar)
                GameEvents.onGUIApplicationLauncherReady.Add( OnGUIAppLauncherReady );
                GameEvents.onEditorShipModified.Add( ShipModifiedEvent );
                GameEvents.OnPartPurchased.Add( PartPurchasedEvent );
                //GameEvents.OnVesselRecoveryRequested.Add(RecoveryRequested);
                GameEvents.onGUIRnDComplexSpawn.Add( TechEnableEvent );
                GameEvents.onGUIRnDComplexDespawn.Add( TechDisableEvent );
                GameEvents.OnKSCFacilityUpgraded.Add( FacilityUpgradedEvent );
                GameEvents.onGameStateLoad.Add( PersistenceLoadEvent );

                GameEvents.OnKSCStructureRepaired.Add( FaciliyRepaired );
                GameEvents.OnKSCStructureCollapsed.Add( FacilityDestroyed );

                GameEvents.FindEvent<EventVoid>( "OnSYInventoryAppliedToVessel" )?.Add( SYInventoryApplied );
                GameEvents.FindEvent<EventVoid>( "OnSYReady" )?.Add( SYReady );
                GameEvents.FindEvent<EventData<Part>>( "OnSYInventoryAppliedToPart" )?.Add( (p) => { KerbalConstructionTime.instance.editorRecalcuationRequired = true; } );
                //     GameEvents.OnKSCStructureRepairing.Add(FacilityRepairingEvent);
                //  GameEvents.onLevelWasLoaded.Add(LevelLoadedEvent);

                /*  GameEvents.OnCrewmemberHired.Add((ProtoCrewMember m, int i) =>
                  {
                      foreach (KCT_KSC ksc in KCT_GameStates.KSCs)
                      {
                          ksc.RecalculateBuildRates();
                          ksc.RecalculateUpgradedBuildRates();
                      }
                  });
                  GameEvents.OnCrewmemberSacked.Add((ProtoCrewMember m, int i) =>
                  {
                      foreach (KCT_KSC ksc in KCT_GameStates.KSCs)
                      {
                          ksc.RecalculateBuildRates();
                          ksc.RecalculateUpgradedBuildRates();
                      }
                  });*/

                GameEvents.onGUIAdministrationFacilitySpawn.Add( HideAllGUIs );
                GameEvents.onGUIAstronautComplexSpawn.Add( HideAllGUIs );
                GameEvents.onGUIMissionControlSpawn.Add( HideAllGUIs );
                GameEvents.onGUIRnDComplexSpawn.Add( HideAllGUIs );
                GameEvents.onGUIKSPediaSpawn.Add( HideAllGUIs );
                GameEvents.onEditorStarted.Add( () => { KCT_Utilities.HandleEditorButton(); } );

                GameEvents.onFacilityContextMenuSpawn.Add( FacilityContextMenuSpawn );

                eventAdded = true;
            }
        }

        public void HideAllGUIs()
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( HideAllGUIs );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                //KCT_GUI.hideAll();
                KCT_GUI.ClickOff();
            }
        }

        public void PersistenceLoadEvent(ConfigNode node)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( PersistenceLoadEvent );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                //KCT_GameStates.erroredDuringOnLoad.OnLoadStart();
                Log.Trace( "Looking for tech nodes." );
                ConfigNode rnd = node.GetNodes( "SCENARIO" ).FirstOrDefault( n => n.GetValue( "name" ) == "ResearchAndDevelopment" );
                if (rnd != null)
                {
                    GameStates.LastKnownTechCount = rnd.GetNodes( "Tech" ).Length;
                    Log.Trace( "Counting " + GameStates.LastKnownTechCount + " tech nodes." );
                }
                GameStates.PersistenceLoaded = true;
            }
        }

        //private static int lastLvl = -1;
        public static bool allowedToUpgrade = false;
        public void FacilityUpgradedEvent(Upgradeables.UpgradeableFacility facility, int lvl)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( FacilityUpgradedEvent );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (KCT_GUI.PrimarilyDisabled)
                {
                    bool isLaunchpad = facility.id.ToLower().Contains( "launchpad" );
                    if (!isLaunchpad)
                        return;

                    //is a launch pad
                    GameStates.ActiveKSC.ActiveLPInstance.Upgrade( lvl );

                }


                //if (!(allowedToUpgrade || !KCT_PresetManager.Instance.ActivePreset.generalSettings.KSCUpgradeTimes))
                //{
                //    KCT_UpgradingBuilding upgrading = new KCT_UpgradingBuilding(facility.id, lvl, lvl - 1, facility.id.Split('/').Last());

                //    upgrading.isLaunchpad = facility.id.ToLower().Contains("launchpad");
                //    if (upgrading.isLaunchpad)
                //    {
                //        upgrading.launchpadID = KCT_GameStates.ActiveKSC.ActiveLaunchPadID;
                //        if (upgrading.launchpadID > 0)
                //            upgrading.commonName += KCT_GameStates.ActiveKSC.ActiveLPInstance.name;//" " + (upgrading.launchpadID+1);
                //    }

                //    if (!upgrading.AlreadyInProgress())
                //    {
                //        KCT_GameStates.ActiveKSC.KSCTech.Add(upgrading);
                //        upgrading.Downgrade();
                //        double cost = facility.GetUpgradeCost();
                //        upgrading.SetBP(cost);
                //        upgrading.cost = cost;

                //        ScreenMessages.PostScreenMessage("Facility upgrade requested!", 4.0f, ScreenMessageStyle.UPPER_CENTER);
                //        Log.Trace("Facility " + facility.id + " upgrade requested to lvl " + lvl + " for " + cost + " funds, resulting in a BP of " + upgrading.BP);
                //    }
                //    else if (lvl != upgrading.currentLevel)
                //    {
                //        //
                //        KCT_UpgradingBuilding listBuilding = upgrading.KSC.KSCTech.Find(b => b.id == upgrading.id);
                //        if (upgrading.isLaunchpad)
                //            listBuilding = upgrading.KSC.KSCTech.Find(b => b.isLaunchpad && b.launchpadID == upgrading.launchpadID);
                //        listBuilding.Downgrade();
                //        KCT_Utilities.AddFunds(listBuilding.cost, TransactionReasons.None);
                //        ScreenMessages.PostScreenMessage("Facility is already being upgraded!", 4.0f, ScreenMessageStyle.UPPER_CENTER);
                //        Log.Trace("Facility " + facility.id + " tried to upgrade to lvl " + lvl + " but already in list!");
                //    }
                //}
                //else
                //{
                Log.Trace( "Facility " + facility.id + " upgraded to lvl " + lvl );
                if (facility.id.ToLower().Contains( "launchpad" ))
                {
                    if (!allowedToUpgrade)
                        GameStates.ActiveKSC.ActiveLPInstance.Upgrade( lvl ); //also repairs the launchpad
                    else
                        GameStates.ActiveKSC.ActiveLPInstance.level = lvl;
                }
                allowedToUpgrade = false;
                foreach (SpaceCenterConstruction ksc in GameStates.KSCs)
                {
                    ksc.RecalculateBuildRates();
                    ksc.RecalculateUpgradedBuildRates();
                }
                foreach (KCT_TechItem tech in GameStates.TechList)
                {
                    tech.UpdateBuildRate( GameStates.TechList.IndexOf( tech ) );
                }
                //}
                /* if (lvl <= lastLvl)
                 {
                     lastLvl = -1;
                     return;
                 }
                 facility.SetLevel(lvl - 1);
                 lastLvl = lvl;
                 double cost = facility.GetUpgradeCost();
                 double BP = Math.Sqrt(cost) * 2000 * KCT_GameStates.timeSettings.OverallMultiplier;*/

                // Log.Trace(facility.GetNormLevel());
            }
        }

        public void FacilityRepairingEvent(DestructibleBuilding facility)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( FacilityRepairingEvent );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (KCT_GUI.PrimarilyDisabled)
                    return;
                double cost = facility.RepairCost;
                double BP = Math.Sqrt( cost ) * 2000 * KCT_PresetManager.Instance.ActivePreset.timeSettings.OverallMultiplier;
                Log.Info( "Facility being repaired for " + cost + " funds, resulting in a BP of " + BP );                
                //facility.StopCoroutine("Repair");
            }
        }

        public void FaciliyRepaired(DestructibleBuilding facility)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( FaciliyRepaired );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (facility.id.Contains( "LaunchPad" ))
                {
                    Log.Info( "LaunchPad was repaired." );

                    //KCT_GameStates.ActiveKSC.LaunchPads[KCT_GameStates.ActiveKSC.ActiveLaunchPadID].destroyed = false;
                    GameStates.ActiveKSC.ActiveLPInstance.RefreshDestructionNode();
                    GameStates.ActiveKSC.ActiveLPInstance.CompletelyRepairNode();
                }
            }
        }

        public void FacilityDestroyed(DestructibleBuilding facility)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( FaciliyRepaired );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (facility.id.Contains( "LaunchPad" ))
                {
                    Log.Info( "LaunchPad was damaged." );

                    //KCT_GameStates.ActiveKSC.LaunchPads[KCT_GameStates.ActiveKSC.ActiveLaunchPadID].destroyed = !KCT_Utilities.LaunchFacilityIntact(KCT_BuildListVessel.ListType.VAB);
                    GameStates.ActiveKSC.ActiveLPInstance.RefreshDestructionNode();
                }
            }
        }

        public void RecoveryRequested(Vessel v)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( RecoveryRequested );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                //ShipBackup backup = ShipAssembly.MakeVesselBackup(v);
                //string tempFile = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/Ships/temp2.craft";
                //backup.SaveShip(tempFile);

                // KCT_GameStates.recoveryRequestVessel = backup; //ConfigNode.Load(tempFile);
            }
        }

        public void FacilityContextMenuSpawn(KSCFacilityContextMenu menu)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( FacilityContextMenuSpawn );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                KerbalConstructionTime.instance.FacilityContextMenuSpawn( menu );
            }
        }

        private void SYInventoryApplied()
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( SYInventoryApplied );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                Log.Info( "Inventory was applied. Recalculating." );
                if (HighLogic.LoadedSceneIsEditor)
                {
                    KerbalConstructionTime.instance.editorRecalcuationRequired = true;
                }
            }
        }

        private void SYReady()
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( SYReady );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (HighLogic.LoadedSceneIsEditor && GameStates.EditorShipEditingMode && GameStates.editedVessel != null)
                {
                    Log.Info( "Removing SY tracking of this vessel." );
                    string id = ScrapYardWrapper.GetPartID( GameStates.editedVessel.ExtractedPartNodes[0] );
                    ScrapYardWrapper.SetProcessedStatus( id, false );

                    Log.Info( "Adding parts back to inventory for editing..." );
                    foreach (ConfigNode partNode in GameStates.editedVessel.ExtractedPartNodes)
                    {
                        if (ScrapYardWrapper.PartIsFromInventory( partNode ))
                        {
                            ScrapYardWrapper.AddPartToInventory( partNode, false );
                        }
                    }
                }
            }
        }

        private void ShipModifiedEvent(ShipConstruct vessel)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( ShipModifiedEvent );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                KerbalConstructionTime.instance.editorRecalcuationRequired = true;
            }
        }

        public ApplicationLauncherButton KCTButtonStock = null;
        public void OnGUIAppLauncherReady()
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( OnGUIAppLauncherReady );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                bool vis;
                if (ToolbarManager.ToolbarAvailable && GameStates.settings.PreferBlizzyToolbar)
                    return;

                if (ApplicationLauncher.Ready && (KCTButtonStock == null || !ApplicationLauncher.Instance.Contains( KCTButtonStock, out vis ))) //Add Stock button
                {
                    string texturePath = PluginAssemblyUtilities.iconKCTOn;
                    KCTEvents.instance.KCTButtonStock = ApplicationLauncher.Instance.AddModApplication(
                        KCT_GUI.ClickOn,
                        KCT_GUI.ClickOff,
                        KCT_GUI.onHoverOn,
                        KCT_GUI.onHoverOff,
                        KCTEvents.instance.DummyVoid,
                        KCTEvents.instance.DummyVoid,
                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.VAB,
                        GameDatabase.Instance.GetTexture( texturePath, false ) );

                    ApplicationLauncher.Instance.EnableMutuallyExclusive( KCTEvents.instance.KCTButtonStock );

                    /*  if (HighLogic.LoadedScene == GameScenes.SPACECENTER && KCT_GameStates.showWindows[0])
                      {
                          KCTButtonStock.SetTrue(true);
                          KCT_GUI.clicked = true;
                      }*/
                }
            }
        }
        public void DummyVoid() { }

        public void PartPurchasedEvent(AvailablePart part)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( PartPurchasedEvent );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch)
                    return;
                KCT_TechItem tech = GameStates.TechList.Find( t => t.techID == part.TechRequired );
                if (tech != null && tech.isInList())
                {
                    ScreenMessages.PostScreenMessage( "[KCT] You must wait until the node is fully researched to purchase parts!", 4.0f, ScreenMessageStyle.UPPER_LEFT );
                    KCT_Utilities.AddFunds( part.entryCost, TransactionReasons.RnDPartPurchase );
                    tech.protoNode.partsPurchased.Remove( part );
                    tech.DisableTech();
                }
            }
        }

        public void TechUnlockEvent(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> ev)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( TechUnlockEvent );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                //TODO: Check if any of the parts are experimental, if so, do the normal KCT stuff and then set them experimental again
                if (!KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled) return;
                if (ev.target == RDTech.OperationResult.Successful)
                {
                    KCT_TechItem tech = new KCT_TechItem();
                    if (ev.host != null)
                        tech = new KCT_TechItem( ev.host );

                    foreach (AvailablePart expt in ev.host.partsPurchased)
                    {
                        if (ResearchAndDevelopment.IsExperimentalPart( expt ))
                            GameStates.ExperimentalParts.Add( expt );
                    }

                    //if (!KCT_GameStates.settings.InstantTechUnlock && !KCT_GameStates.settings.DisableBuildTime) tech.DisableTech();
                    if (!tech.isInList())
                    {
                        if (KCT_PresetManager.Instance.ActivePreset.generalSettings.TechUpgrades)
                            ScreenMessages.PostScreenMessage( "[KCT] Upgrade Point Added!", 4.0f, ScreenMessageStyle.UPPER_LEFT );

                        if (KCT_PresetManager.Instance.ActivePreset.generalSettings.TechUnlockTimes && KCT_PresetManager.Instance.ActivePreset.generalSettings.BuildTimes)
                        {
                            GameStates.TechList.Add( tech );
                            foreach (KCT_TechItem techItem in GameStates.TechList)
                                techItem.UpdateBuildRate( GameStates.TechList.IndexOf( techItem ) );
                            double timeLeft = tech.BuildRate > 0 ? tech.TimeLeft : tech.EstimatedTimeLeft;
                            ScreenMessages.PostScreenMessage( "[KCT] Node will unlock in " + MagiCore.Utilities.GetFormattedTime( timeLeft ), 4.0f, ScreenMessageStyle.UPPER_LEFT );
                        }
                    }
                    else
                    {
                        ResearchAndDevelopment.Instance.AddScience( tech.scienceCost, TransactionReasons.RnDTechResearch );
                        ScreenMessages.PostScreenMessage( "[KCT] This node is already being researched!", 4.0f, ScreenMessageStyle.UPPER_LEFT );
                        ScreenMessages.PostScreenMessage( "[KCT] It will unlock in " + MagiCore.Utilities.GetFormattedTime( (GameStates.TechList.First( t => t.techID == ev.host.techID )).TimeLeft ), 4.0f, ScreenMessageStyle.UPPER_LEFT );
                    }
                }
            }
        }

        public void TechDisableEvent()
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( TechDisableEvent );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                TechDisableEventFinal( true );
            }
        }

        public void TechEnableEvent()
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( TechEnableEvent );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (KCT_PresetManager.Instance.ActivePreset.generalSettings.TechUnlockTimes && KCT_PresetManager.Instance.ActivePreset.generalSettings.BuildTimes)
                {
                    foreach (KCT_TechItem techItem in GameStates.TechList)
                        techItem.EnableTech();
                }
            }
        }

        public void TechDisableEventFinal(bool save=false)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( TechDisableEventFinal );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (KCT_PresetManager.Instance != null && KCT_PresetManager.Instance.ActivePreset != null)
                {
                    if (KCT_PresetManager.Instance.ActivePreset.generalSettings.TechUnlockTimes && KCT_PresetManager.Instance.ActivePreset.generalSettings.BuildTimes)
                    {
                        foreach (KCT_TechItem tech in GameStates.TechList)
                        {
                            /* foreach (String partName in tech.UnlockedParts)
                             {
                                 AvailablePart expt = KCT_Utilities.GetAvailablePartByName(partName);
                                 if (expt != null && ResearchAndDevelopment.IsExperimentalPart(expt))
                                     if (!KCT_GameStates.ExperimentalParts.Contains(expt))
                                         KCT_GameStates.ExperimentalParts.Add(expt);
                             }*/
                            //ResearchAndDevelopment.AddExperimentalPart()
                            tech.DisableTech();
                        }
                        /*    foreach (AvailablePart expt in KCT_GameStates.ExperimentalParts)
                                ResearchAndDevelopment.AddExperimentalPart(expt);*/
                        //Need to somehow update the R&D instance
                        if (save)
                        {
                            GamePersistence.SaveGame( "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE );
                        }
                    }
                }
            }
        }

        public void gameSceneEvent(GameScenes scene)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( gameSceneEvent );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (scene == GameScenes.MAINMENU)
                {
                    GameStates.reset();
                    GameStates.firstStart = false;
                    InputLockManager.RemoveControlLock( "KCTLaunchLock" );
                    GameStates.activeKSCName = "Stock";
                    GameStates.ActiveKSC = new SpaceCenterConstruction( "Stock" );
                    GameStates.KSCs = new List<SpaceCenterConstruction>() { GameStates.ActiveKSC };
                    GameStates.LastKnownTechCount = 0;

                    GameStates.PermanentModAddedUpgradesButReallyWaitForTheAPI = 0;
                    GameStates.TemporaryModAddedUpgradesButReallyWaitForTheAPI = 0;

                    if (KCT_PresetManager.Instance != null)
                    {
                        KCT_PresetManager.Instance.ClearPresets();
                        KCT_PresetManager.Instance = null;
                    }

                    return;
                }

                GameStates.MiscellaneousTempUpgrades = 0;

                /*if (HighLogic.LoadedScene == GameScenes.MAINMENU)
                {
                    if (scene == GameScenes.SPACECENTER)
                    {
                        KCT_PresetManager.Instance.FindPresetFiles();
                        KCT_PresetManager.Instance.LoadPresets();
                    }
                }*/

                if (KCT_PresetManager.PresetLoaded() && !KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled) return;
                List<GameScenes> validScenes = new List<GameScenes> { GameScenes.SPACECENTER, GameScenes.TRACKSTATION, GameScenes.EDITOR };
                if (validScenes.Contains( scene ))
                {
                    TechDisableEventFinal();
                }

                if (HighLogic.LoadedScene == scene && scene == GameScenes.EDITOR) //Fix for null reference when using new or load buttons in editor
                {
                    GamePersistence.SaveGame( "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE );
                }

                if (HighLogic.LoadedSceneIsEditor)
                {
                    EditorLogic.fetch.Unlock( "KCTEditorMouseLock" );
                }
            }
        }

        public void launchScreenOpenEvent(GameEvents.VesselSpawnInfo v)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( launchScreenOpenEvent );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (!KCT_GUI.PrimarilyDisabled)
                {
                    // PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "Warning!", "To launch vessels you must first build them in the VAB or SPH, then launch them through the main KCT window in the Space Center!", "Ok", false, HighLogic.UISkin);
                    //open the build list to the right page
                    string selection = (v.craftSubfolder.Contains( "SPH" )) ? "SPH" : "VAB";
                    KCT_GUI.ClickOn();
                    KCT_GUI.SelectList( "" );
                    KCT_GUI.SelectList( selection );
                    Log.Trace( "Opening the GUI to the " + selection );
                }
            }
        }

        public void vesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> ev)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( vesselSituationChange );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (ev.from == Vessel.Situations.PRELAUNCH && ev.host == FlightGlobals.ActiveVessel)
                {
                    if (KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled &&
                        KCT_PresetManager.Instance.ActivePreset.generalSettings.ReconditioningTimes)
                    {
                        //KCT_Recon_Rollout reconditioning = KCT_GameStates.ActiveKSC.Recon_Rollout.FirstOrDefault(r => ((IKCTBuildItem)r).GetItemName() == "LaunchPad Reconditioning");
                        //if (reconditioning == null)
                        if (HighLogic.CurrentGame.editorFacility == EditorFacility.VAB)
                        {
                            string launchSite = FlightDriver.LaunchSiteName;
                            if (launchSite == "LaunchPad") launchSite = GameStates.ActiveKSC.ActiveLPInstance.name;
                            GameStates.ActiveKSC.Recon_Rollout.Add( new Recon_Rollout( ev.host, Recon_Rollout.RolloutReconType.Reconditioning, ev.host.id.ToString(), launchSite ) );

                        }
                    }
                }
            }
        }

        public void vesselRecoverEvent(ProtoVessel v, bool unknownAsOfNow)
        {
            const string logBlockName = nameof( KCTEvents ) + "." + nameof( vesselRecoverEvent );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (!KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled) return;
                if (!v.vesselRef.isEVA)
                {
                    // if (KCT_GameStates.settings.Debug && HighLogic.LoadedScene != GameScenes.TRACKSTATION && (v.wasControllable || v.protoPartSnapshots.Find(p => p.modules.Find(m => m.moduleName.ToLower() == "modulecommand") != null) != null))
                    if (GameStates.recoveredVessel != null && v.vesselName == GameStates.recoveredVessel.shipName)
                    {
                        //KCT_GameStates.recoveredVessel = new KCT_BuildListVessel(v);
                        //rebuy the ship if ScrapYard isn't overriding funds
                        if (!ScrapYardWrapper.OverrideFunds)
                        {
                            KCT_Utilities.SpendFunds( GameStates.recoveredVessel.cost, TransactionReasons.VesselRollout ); //pay for the ship again
                        }

                        //pull all of the parts out of the inventory
                        //This is a bit funky since we grab the part id from our part, grab the inventory part out, then try to reapply that ontop of our part
                        if (ScrapYardWrapper.Available)
                        {
                            foreach (ConfigNode partNode in GameStates.recoveredVessel.ExtractedPartNodes)
                            {
                                string id = ScrapYardWrapper.GetPartID( partNode );
                                ConfigNode inventoryVersion = ScrapYardWrapper.FindInventoryPart( id );
                                if (inventoryVersion != null)
                                {
                                    //apply it to our copy of the part
                                    ConfigNode ourTracker = partNode.GetNodes( "MODULE" ).FirstOrDefault( n => string.Equals( n.GetValue( "name" ), "ModuleSYPartTracker", StringComparison.Ordinal ) );
                                    if (ourTracker != null)
                                    {
                                        ourTracker.SetValue( "TimesRecovered", inventoryVersion.GetValue( "_timesRecovered" ) );
                                        ourTracker.SetValue( "Inventoried", inventoryVersion.GetValue( "_inventoried" ) );
                                    }
                                }
                            }


                            //process the vessel in ScrapYard
                            ScrapYardWrapper.ProcessVessel( GameStates.recoveredVessel.ExtractedPartNodes );

                            //reset the BP
                            GameStates.recoveredVessel.buildPoints = KCT_Utilities.GetBuildTime( GameStates.recoveredVessel.ExtractedPartNodes );
                        }
                        if (GameStates.recoveredVessel.type == BuildListVessel.ListType.VAB)
                        {
                            GameStates.ActiveKSC.VABWarehouse.Add( GameStates.recoveredVessel );
                        }
                        else
                        {
                            GameStates.ActiveKSC.SPHWarehouse.Add( GameStates.recoveredVessel );
                        }

                        GameStates.ActiveKSC.Recon_Rollout.Add( new Recon_Rollout( GameStates.recoveredVessel, Recon_Rollout.RolloutReconType.Recovery, GameStates.recoveredVessel.id.ToString() ) );
                        GameStates.recoveredVessel = null;
                    }
                }
            }
        }


        //private float GetResourceMass(List<ProtoPartResourceSnapshot> resources)
        //{
        //    double mass = 0;
        //    foreach (ProtoPartResourceSnapshot resource in resources)
        //    {
        //        double amount = resource.amount;
        //        PartResourceDefinition RD = PartResourceLibrary.Instance.GetDefinition(resource.resourceName);
        //        mass += amount * RD.density;
        //    }
        //    return (float)mass;
        //}
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

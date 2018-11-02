using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;
using System.Collections;
using KSP.UI.Screens;
using KSP.UI;

namespace KerbalConstructionTime
{
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class KCT_Tracking_Station : KerbalConstructionTime
    {

    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KCT_Flight : KerbalConstructionTime
    {

    }

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class KCT_SpaceCenter : KerbalConstructionTime
    {

    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class KCT_Editor : KerbalConstructionTime
    {

    }

    //[KSPAddon(KSPAddon.Startup.EditorAny | KSPAddon.Startup.Flight | KSPAddon.Startup.SpaceCentre | KSPAddon.Startup.TrackingStation, false)]
    public class KerbalConstructionTime : MonoBehaviour
    {
        internal void FacilityContextMenuSpawn(KSCFacilityContextMenu menu)
        {
            const string logBlockName = nameof( KerbalConstructionTime ) + "." + nameof( FacilityContextMenuSpawn );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                KCT_KSCContextMenuOverrider overrider = new KCT_KSCContextMenuOverrider( menu );
                StartCoroutine( overrider.OnContextMenuSpawn() );
            }
        }

        public bool editorRecalcuationRequired;
        public int updateRateThrottle;

        public static KerbalConstructionTime instance;

        public void OnDestroy()//more toolbar stuff
        {
            const string logBlockName = nameof( KerbalConstructionTime ) + "." + nameof( OnDestroy );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (GameStates.kctToolbarButton != null)
                {
                    GameStates.kctToolbarButton.Destroy();
                }
                if (KCTEvents.instance.KCTButtonStock != null)
                {
                    KSP.UI.Screens.ApplicationLauncher.Instance.RemoveModApplication( KCTEvents.instance.KCTButtonStock );
                }

                KCT_GUI.guiDataSaver.Save();
            }
        }

        private bool GUIExceptionThrown = false;
        private DateTime GUIExceptionTime;

        private void OnGUI()
        {
            if (KCT_Utilities.CurrentGameIsMission())
            {
                return;
            }


            try
            {
                if (!GUIExceptionThrown)
                {
                    KCT_GUI.SetGUIPositions();
                }
                else
                {
                    DateTime CurrentTime = DateTime.Now;
                    if (CurrentTime > GUIExceptionTime.AddSeconds(10))
                    {
                        GUIExceptionThrown = false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error( e.Message );
                Log.Error( e.StackTrace );
                GUIExceptionThrown = true;
                GUIExceptionTime = DateTime.Now;
            }
        }

        public void Awake()
        {
            const string logBlockName = nameof( KerbalConstructionTime ) + "." + nameof( Awake );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (KCT_Utilities.CurrentGameIsMission())
                {
                    return;
                }
                GameStates.erroredDuringOnLoad.OnLoadStart();
                GameStates.PersistenceLoaded = false;

                instance = this;

                GameStates.settings.Load(); //Load the settings file, if it exists

                string SavedFile = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/KCT_Settings.cfg";
                if (!System.IO.File.Exists( SavedFile ))
                {
                    GameStates.firstStart = true;
                }

                if (KCT_PresetManager.Instance == null)
                {
                    KCT_PresetManager.Instance = new KCT_PresetManager();
                }
                KCT_PresetManager.Instance.SetActiveFromSaveData();


                //Add the toolbar button
                if (ToolbarManager.ToolbarAvailable && ToolbarManager.Instance != null && GameStates.settings.PreferBlizzyToolbar)
                {
                    Log.Trace( "Adding Toolbar Button" );
                    GameStates.kctToolbarButton = ToolbarManager.Instance.add( "Kerbal_Construction_Time", "MainButton" );
                    if (GameStates.kctToolbarButton != null)
                    {
                        if (KCT_PresetManager.PresetLoaded() && !KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled) GameStates.kctToolbarButton.Visibility = new GameScenesVisibility( GameScenes.SPACECENTER );
                        else GameStates.kctToolbarButton.Visibility = new GameScenesVisibility( new GameScenes[] { GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.EDITOR } );
                        GameStates.kctToolbarButton.TexturePath = KCT_Utilities.GetButtonTexture();
                        GameStates.kctToolbarButton.ToolTip = "Kerbal Construction Time";
                        GameStates.kctToolbarButton.OnClick += ((e) =>
                        {
                            KCT_GUI.ClickToggle();
                        });
                    }
                }
            }
        }

        public void Start()
        {
            const string logBlockName = nameof( KerbalConstructionTime ) + "." + nameof( Start );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (KCT_Utilities.CurrentGameIsMission())
                {
                    return;
                }

                Log.Trace( "Start called" );

                //add the events
                if (!KCTEvents.instance.eventAdded)
                {
                    KCTEvents.instance.addEvents();
                }

                GameStates.settings.Save(); //Save the settings file, with defaults if it doesn't exist
                KCT_PresetManager.Instance.SaveActiveToSaveData();

                // Ghetto event queue
                if (HighLogic.LoadedScene == GameScenes.EDITOR)
                {
                    InvokeRepeating( "EditorRecalculation", 1, 1 );

                    KCT_GUI.buildRateForDisplay = null;
                    if (!KCT_GUI.PrimarilyDisabled)
                    {
                        KCT_Utilities.RecalculateEditorBuildTime( EditorLogic.fetch.ship );
                    }
                }

                if (KCT_GUI.PrimarilyDisabled)
                {
                    if (InputLockManager.GetControlLock( "KCTLaunchLock" ) == ControlTypes.EDITOR_LAUNCH)
                        InputLockManager.RemoveControlLock( "KCTLaunchLock" );
                }

                KACWrapper.InitKACWrapper();

                if (!KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled)
                {
                    if (InputLockManager.GetControlLock( "KCTKSCLock" ) == ControlTypes.KSC_FACILITIES)
                        InputLockManager.RemoveControlLock( "KCTKSCLock" );
                    return;
                }

                //Begin primary mod functions

                GameStates.UT = Planetarium.GetUniversalTime();

                KCT_GUI.guiDataSaver.Load();

                if (HighLogic.LoadedSceneIsEditor)
                {
                    KCT_GUI.hideAll();
                    if (!KCT_GUI.PrimarilyDisabled)
                    {
                        KCT_GUI.showEditorGUI = GameStates.showWindows[1];
                        if (KCT_GUI.showEditorGUI)
                            KCT_GUI.ClickOn();
                        else
                            KCT_GUI.ClickOff();
                    }
                }
                else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    bool shouldStart = KCT_GUI.showFirstRun;
                    KCT_GUI.hideAll();
                    if (!shouldStart)
                    {
                        KCT_GUI.showBuildList = GameStates.showWindows[0];
                        if (KCT_GUI.showBuildList)
                            KCT_GUI.ClickOn();
                        else
                            KCT_GUI.ClickOff();
                    }
                    KCT_GUI.showFirstRun = shouldStart;
                }

                if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
                {
                    if (FlightGlobals.ActiveVessel.GetCrewCount() == 0 && GameStates.launchedCrew.Count > 0)
                    {
                        KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;

                        for (int i = 0; i < FlightGlobals.ActiveVessel.parts.Count; i++)
                        {
                            Part p = FlightGlobals.ActiveVessel.parts[i];
                            //Log.Trace("craft: " + p.craftID);
                            {
                                CrewedPart cP = GameStates.launchedCrew.Find( part => part.partID == p.craftID );
                                if (cP == null) continue;
                                List<ProtoCrewMember> crewList = cP.crewList;
                                foreach (ProtoCrewMember crewMember in crewList)
                                {
                                    if (crewMember != null)
                                    {
                                        ProtoCrewMember finalCrewMember = crewMember;
                                        if (crewMember.type == ProtoCrewMember.KerbalType.Crew)
                                        {
                                            finalCrewMember = roster.Crew.FirstOrDefault( c => c.name == crewMember.name );
                                        }
                                        else if (crewMember.type == ProtoCrewMember.KerbalType.Tourist)
                                        {
                                            finalCrewMember = roster.Tourist.FirstOrDefault( c => c.name == crewMember.name );
                                        }
                                        if (finalCrewMember == null)
                                        {
                                            Debug.LogError( "Error when assigning " + crewMember.name + " to " + p.partInfo.name + ". Cannot find Kerbal in list." );
                                            continue;
                                        }
                                        try
                                        {
                                            Log.Trace( "Assigning " + finalCrewMember.name + " to " + p.partInfo.name );
                                            if (p.AddCrewmember( finalCrewMember ))//p.AddCrewmemberAt(finalCrewMember, crewList.IndexOf(crewMember)))
                                            {
                                                finalCrewMember.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
                                                if (finalCrewMember.seat != null)
                                                    finalCrewMember.seat.SpawnCrew();
                                            }
                                            else
                                            {
                                                Debug.LogError( "Error when assigning " + crewMember.name + " to " + p.partInfo.name );
                                                finalCrewMember.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                                                continue;
                                            }
                                        }
                                        catch
                                        {
                                            Debug.LogError( "Error when assigning " + crewMember.name + " to " + p.partInfo.name );
                                            finalCrewMember.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        GameStates.launchedCrew.Clear();
                    }
                }
                if (HighLogic.LoadedSceneIsFlight)
                {
                    KCT_GUI.hideAll();
                    if (GameStates.launchedVessel != null && FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
                    {
                        GameStates.launchedVessel.KSC = null; //it's invalid now
                        Log.Trace( "Attempting to remove launched vessel from build list" );
                        bool removed = GameStates.launchedVessel.RemoveFromBuildList();
                        if (removed) //Only do these when the vessel is first removed from the list
                        {
                            //Add the cost of the ship to the funds so it can be removed again by KSP
                            KCT_Utilities.AddFunds( GameStates.launchedVessel.cost, TransactionReasons.VesselRollout );
                            FlightGlobals.ActiveVessel.vesselName = GameStates.launchedVessel.shipName;
                        }
                        Recon_Rollout rollout = GameStates.ActiveKSC.Recon_Rollout.FirstOrDefault( r => r.associatedID == GameStates.launchedVessel.id.ToString() );
                        if (rollout != null)
                            GameStates.ActiveKSC.Recon_Rollout.Remove( rollout );
                    }
                }
                ratesUpdated = false;
                DelayedStart();
            }
        }

        //private void EditorRecalculation()
        //{
        //    const string logBlockName = nameof( KerbalConstructionTime ) + "." + nameof( EditorRecalculation );
        //    using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
        //    {
        //        if (editorRecalcuationRequired && !KCT_GUI.PrimarilyDisabled)
        //        {
        //            KCT_Utilities.RecalculateEditorBuildTime( EditorLogic.fetch.ship );
        //            editorRecalcuationRequired = false;
        //        }
        //    }
        //}


        private static int lvlCheckTimer = 0;
        private static bool ratesUpdated = false;
        public void FixedUpdate()
        {
            if (KCT_Utilities.CurrentGameIsMission())
            {
                return;
            }

            double lastUT = GameStates.UT > 0 ? GameStates.UT : Planetarium.GetUniversalTime();
            GameStates.UT = Planetarium.GetUniversalTime();
            try
            {
                if (KCTEvents.instance != null && KCTEvents.instance.KCTButtonStock != null)
                    if (KCT_GUI.clicked)
                        KCTEvents.instance.KCTButtonStock.SetTrue(false);
                    else
                        KCTEvents.instance.KCTButtonStock.SetFalse(false);

                if (!KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled)
                    return;

                if (!GameStates.erroredDuringOnLoad.AlertFired && GameStates.erroredDuringOnLoad.HasErrored())
                {
                    GameStates.erroredDuringOnLoad.FireAlert();
                }

                if (GameStates.UpdateLaunchpadDestructionState)
                {
                    Log.Trace("Updating launchpad destruction state.");
                    GameStates.UpdateLaunchpadDestructionState = false;
                    GameStates.ActiveKSC.ActiveLPInstance.SetDestructibleStateFromNode();
                    if (GameStates.ActiveKSC.ActiveLPInstance.upgradeRepair)
                    {
                        //repair everything, then update the node
                        GameStates.ActiveKSC.ActiveLPInstance.RefreshDestructionNode();
                        GameStates.ActiveKSC.ActiveLPInstance.CompletelyRepairNode();
                        GameStates.ActiveKSC.ActiveLPInstance.SetDestructibleStateFromNode();
                    }

                }

                if (!ratesUpdated)
                {
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER) 
                    {
                        if (ScenarioUpgradeableFacilities.GetFacilityLevelCount(SpaceCenterFacility.VehicleAssemblyBuilding) >= 0)
                        {
                            ratesUpdated = true;
                            Log.Trace("Updating build rates");
                            foreach (SpaceCenterConstruction KSC in GameStates.KSCs)
                            {
                                KSC?.RecalculateBuildRates();
                                KSC?.RecalculateUpgradedBuildRates();
                            }

                            Log.Trace("Rates updated");

                            foreach (SpaceCenterFacility facility in Enum.GetValues(typeof(SpaceCenterFacility)))
                            {
                                GameStates.BuildingMaxLevelCache[facility.ToString()] = ScenarioUpgradeableFacilities.GetFacilityLevelCount(facility);
                                Log.Trace("Cached " + facility.ToString() + " max at " + GameStates.BuildingMaxLevelCache[facility.ToString()]);
                            }
                        }
                    }
                    else
                    {
                        ratesUpdated = true;
                    }
                }

                if (GameStates.ActiveKSC?.ActiveLPInstance != null && HighLogic.LoadedScene == GameScenes.SPACECENTER && KCT_Utilities.CurrentGameIsCareer())
                {
                    if (lvlCheckTimer++ > 30)
                    {
                        lvlCheckTimer = 0;
                        if (KCT_Utilities.BuildingUpgradeLevel(SpaceCenterFacility.LaunchPad) != GameStates.ActiveKSC.ActiveLPInstance.level)
                        {
                            GameStates.ActiveKSC.SwitchLaunchPad(GameStates.ActiveKSC.ActiveLaunchPadID, false);
                            GameStates.UpdateLaunchpadDestructionState = true;
                        }
                    }
                }
                //Warp code
                if (!KCT_GUI.PrimarilyDisabled && (HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION))
                {
                    IKCTBuildItem ikctItem = KCT_Utilities.NextThingToFinish();
                    if (GameStates.targetedItem == null && ikctItem != null) GameStates.targetedItem = ikctItem;
                    double remaining = ikctItem != null ? ikctItem.GetTimeLeft() : -1;
                    double dT = TimeWarp.CurrentRate / (GameStates.UT - lastUT);
                    if (dT >= 20)
                        dT = 0.1;
                    //Log.Trace("dt: " + dT);
                    int nBuffers = 1;
                    if (GameStates.canWarp && ikctItem != null && !ikctItem.IsComplete())
                    {
                        int warpRate = TimeWarp.CurrentRateIndex;
                        if (warpRate < GameStates.lastWarpRate) //if something else changes the warp rate then release control to them, such as Kerbal Alarm Clock
                        {
                            GameStates.canWarp = false;
                            GameStates.lastWarpRate = 0;
                        }
                        else
                        {
                            if (ikctItem == GameStates.targetedItem && warpRate > 0 && TimeWarp.fetch.warpRates[warpRate] * dT * nBuffers > Math.Max(remaining, 0))
                            {
                                int newRate = warpRate;
                                //find the first rate that is lower than the current rate
                                while (newRate > 0)
                                {
                                    if (TimeWarp.fetch.warpRates[newRate] * dT * nBuffers < remaining)
                                    break;
                                newRate--;
                                }
                                Log.Trace("Warping down to " + newRate + " (delta: " + (TimeWarp.fetch.warpRates[newRate] * dT) + ")");
                                TimeWarp.SetRate(newRate, true); //hopefully a faster warp down than before
                                warpRate = newRate;
                            }
                            else if (warpRate == 0 && GameStates.warpInitiated)
                            {
                                GameStates.canWarp = false;
                                GameStates.warpInitiated = false;
                                GameStates.targetedItem = null;

                            }
                            GameStates.lastWarpRate = warpRate;
                        }

                    }
                    else if (ikctItem != null && ikctItem == GameStates.targetedItem && (GameStates.warpInitiated || GameStates.settings.ForceStopWarp) && TimeWarp.CurrentRateIndex > 0 && (remaining < 1) && (!ikctItem.IsComplete())) //Still warp down even if we don't control the clock
                    {
                        TimeWarp.SetRate(0, true);
                        GameStates.warpInitiated = false;
                        GameStates.targetedItem = null;
                    }
                }

                if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                {
                    KCT_Utilities.SetActiveKSCToRSS();
                }

                if (!KCT_GUI.PrimarilyDisabled && HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    if (KSP.UI.Screens.VesselSpawnDialog.Instance != null && KSP.UI.Screens.VesselSpawnDialog.Instance.Visible)
                    {
                        KSP.UI.Screens.VesselSpawnDialog.Instance.ButtonClose();
                        Log.Trace("Attempting to close spawn dialog!");
                    }
                }

                if (!KCT_GUI.PrimarilyDisabled)
                    KCT_Utilities.ProgressBuildTime();

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void LateUpdate()
        {
            if (KCT_Utilities.CurrentGameIsMission())
            {
                return;
            }
            // FIXME really should run this only once, and then again on techlist change.
            // For now, spam per frame
            if (KSP.UI.Screens.RDController.Instance != null)
            {
                for (int i = KSP.UI.Screens.RDController.Instance.nodes.Count; i-- > 0;)
                {
                    KSP.UI.Screens.RDNode node = KSP.UI.Screens.RDController.Instance.nodes[i];
                    if (node?.tech != null)
                    {
                        if (HasTechInList(node.tech.techID))
                        {
                            node.graphics?.SetIconColor(XKCDColors.KSPNotSoGoodOrange);
                        }
                        // else reset? Bleh, why bother.
                    }
                }
            }
        }

        protected bool HasTechInList(string id)
        {
            for (int i = GameStates.TechList.Count; i-- > 0;)
                if (GameStates.TechList[i].techID == id)
                    return true;

            return false;
        }

        private static void CheckVesselsForMissingParts()
        {
            const string logBlockName = nameof( KerbalConstructionTime ) + "." + nameof( DelayedStart );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                //check that all parts are valid in all ships. If not, warn the user and disable that vessel (once that code is written)
                if (!GameStates.vesselErrorAlerted)
                {
                    List<BuildListVessel> erroredVessels = new List<BuildListVessel>();
                    foreach (SpaceCenterConstruction KSC in GameStates.KSCs) //this is faster on subsequent scene changes
                    {
                        foreach (BuildListVessel blv in KSC.VABList)
                        {
                            if (!blv.allPartsValid)
                            {
                                //error!
                                Log.Trace( blv.shipName + " contains invalid parts!" );
                                erroredVessels.Add( blv );
                            }
                        }
                        foreach (BuildListVessel blv in KSC.VABWarehouse)
                        {
                            if (!blv.allPartsValid)
                            {
                                //error!
                                Log.Trace( blv.shipName + " contains invalid parts!" );
                                erroredVessels.Add( blv );
                            }
                        }
                        foreach (BuildListVessel blv in KSC.SPHList)
                        {
                            if (!blv.allPartsValid)
                            {
                                //error!
                                Log.Trace( blv.shipName + " contains invalid parts!" );
                                erroredVessels.Add( blv );
                            }
                        }
                        foreach (BuildListVessel blv in KSC.SPHWarehouse)
                        {
                            if (!blv.allPartsValid)
                            {
                                //error!
                                Log.Trace( blv.shipName + " contains invalid parts!" );
                                erroredVessels.Add( blv );
                            }
                        }
                    }
                    if (erroredVessels.Count > 0)
                        PopUpVesselError( erroredVessels );
                    GameStates.vesselErrorAlerted = true;
                }
            }
        }

        public static void DelayedStart()
        {
            const string logBlockName = nameof( KerbalConstructionTime ) + "." + nameof( DelayedStart );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (KCT_Utilities.CurrentGameIsMission())
                {
                    return;
                }
                Log.Trace( "DelayedStart start" );
                if (KCT_PresetManager.Instance?.ActivePreset == null || !KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled)
                    return;

                if (KCT_GUI.PrimarilyDisabled) return;

                //The following should only be executed when fully enabled for the save

                if (GameStates.ActiveKSC == null)
                {
                    KCT_Utilities.SetActiveKSCToRSS();
                }

                CheckVesselsForMissingParts();

                if (HighLogic.LoadedSceneIsEditor)
                {
                    if (GameStates.EditorShipEditingMode)
                    {
                        Log.Trace( "Editing " + GameStates.editedVessel.shipName );
                        EditorLogic.fetch.shipNameField.text = GameStates.editedVessel.shipName;
                    }
                }

                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    Log.Trace( "SP Start" );
                    if (!KCT_GUI.PrimarilyDisabled)
                    {
                        if (ToolbarManager.ToolbarAvailable && GameStates.settings.PreferBlizzyToolbar)
                            if (GameStates.showWindows[0])
                                KCT_GUI.ClickOn();
                            else
                            {
                                if (KCTEvents.instance != null && KCTEvents.instance.KCTButtonStock != null)
                                {
                                    if (GameStates.showWindows[0])
                                        KCT_GUI.ClickOn();
                                }
                            }
                        KCT_GUI.ResetBLWindow();
                    }
                    else
                    {
                        KCT_GUI.showBuildList = false;
                        GameStates.showWindows[0] = false;
                    }
                    Log.Trace( "SP UI done" );

                    if (GameStates.firstStart)
                    {
                        Log.Trace( "Showing first start." );
                        GameStates.firstStart = false;
                        KCT_GUI.showFirstRun = true;

                        //initialize the proper launchpad
                        GameStates.ActiveKSC.ActiveLPInstance.level = KCT_Utilities.BuildingUpgradeLevel( SpaceCenterFacility.LaunchPad );

                    }

                    Log.Trace( "SP switch starting" );
                    GameStates.ActiveKSC.SwitchLaunchPad( GameStates.ActiveKSC.ActiveLaunchPadID );
                    Log.Trace( "SP switch done" );

                    foreach (SpaceCenterConstruction ksc in GameStates.KSCs)
                    {
                        //foreach (KCT_Recon_Rollout rr in ksc.Recon_Rollout)
                        for (int i = 0; i < ksc.Recon_Rollout.Count; i++)
                        {
                            Recon_Rollout rr = ksc.Recon_Rollout[i];
                            if (rr.RRType != Recon_Rollout.RolloutReconType.Reconditioning && KCT_Utilities.FindBLVesselByID( new Guid( rr.associatedID ) ) == null)
                            {
                                Log.Trace( "Invalid Recon_Rollout at " + ksc.KSCName + ". ID " + rr.associatedID + " not found." );
                                ksc.Recon_Rollout.Remove( rr );
                                i--;
                            }
                        }
                    }
                    Log.Trace( "SP done" );
                }
            }
        }

        public static void PopUpVesselError(List<BuildListVessel> errored)
        {
            DialogGUIBase[] options = new DialogGUIBase[2];
            options[0] = new DialogGUIButton("Understood", () => { });
           // new DialogGUIBase("Understood", () => { }); //do nothing and close the window
            options[1] = new DialogGUIButton("Delete Vessels", () =>
            {
                foreach (BuildListVessel blv in errored)
                {
                    blv.RemoveFromBuildList();
                    KCT_Utilities.AddFunds(blv.cost, TransactionReasons.VesselRollout);
                    //remove any associated recon_rollout
                }
            });

            string txt = "The following KCT vessels contain missing or invalid parts and have been quarantined. Either add the missing parts back into your game or delete the vessels. A file containing the ship names and missing parts has been added to your save folder.\n";
            string txtToWrite = "";
            foreach (BuildListVessel blv in errored)
            {
                txt += blv.shipName + "\n";
                txtToWrite += blv.shipName+"\n";
                txtToWrite += String.Join("\n", blv.MissingParts().ToArray());
                txtToWrite += "\n\n";
            }

            //HighLogic.SaveFolder
            //make new file for missing ships
            string filename = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/missingParts.txt";
            System.IO.File.WriteAllText(filename, txtToWrite);


            //remove all rollout and recon items since they're invalid without the ships
            foreach (BuildListVessel blv in errored)
            {
                //remove any associated recon_rollout
                foreach (SpaceCenterConstruction ksc in GameStates.KSCs)
                {
                    for (int i = 0; i < ksc.Recon_Rollout.Count; i++)
                    {
                        Recon_Rollout rr = ksc.Recon_Rollout[i];
                        if (rr.associatedID == blv.id.ToString())
                        {
                            ksc.Recon_Rollout.Remove(rr);
                            i--;
                        }
                    }
                }
            }


            MultiOptionDialog diag = new MultiOptionDialog("missingPartsPopup", txt, "Vessels Contain Missing Parts", null, options);
            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), diag, false, HighLogic.UISkin);
            //PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "Vessel Contains Missing Parts", "The KCT vessel " + errored.shipName + " contains missing or invalid parts. You will not be able to do anything with the vessel until the parts are available again.", "Understood", false, HighLogic.UISkin);
        }

        public static void ShowLaunchAlert(string launchSite)
        {
            Log.Trace("Showing Launch Alert");
            if (KCT_GUI.PrimarilyDisabled)
            {
                EditorLogic.fetch.launchVessel();
            }
            else
            {
                KCT_Utilities.AddVesselToBuildList(launchSite);
                KCT_Utilities.RecalculateEditorBuildTime(EditorLogic.fetch.ship);
            }
        }
    }
}
/*
Copyright (C) 2018  Michael Marvin, Zachary Eck

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

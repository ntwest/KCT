using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalConstructionTime
{
    [KSPAddon( KSPAddon.Startup.MainMenu, true )]
    internal class IMGUIController : MonoBehaviour
    {
        private class IMGUIWindow
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public bool IsDisplayed { get; set; }
            public Rect Position { get; set; }
            public GUI.WindowFunction GUIWindowFunction { get; set; }
            public GUIStyle Style { get; set; }

            public void Draw()
            {
                if (IsDisplayed)
                {
                    Position = GUILayout.Window( Id, Position, GUIWindowFunction, Text, Style );
                }
            }
        }

        #region Singleton
        internal static IMGUIController Instance { get; private set; }
        public static bool IsReady { get; private set; } = false;
        #endregion Singleton

        #region Private Fields
        private List<GameScenes> validScenes = new List<GameScenes> { GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.SPACECENTER, GameScenes.TRACKSTATION };

        private List<IMGUIWindow> guiWindows = new List<IMGUIWindow>();

        private Rect defaultIconPosition = new Rect( Screen.width / 4, Screen.height - 30, 50, 30 );//110
        private Rect defaultMainWindowPosition = new Rect( Screen.width / 3.5f, Screen.height / 3.5f, 350, 200 );
        private Rect defaultSOIAlertPosition = new Rect( Screen.width / 3, Screen.height / 3, 250, 100 );
        private Rect defaultCentralWindowPosition = new Rect( (Screen.width - 150) / 2, (Screen.height - 50) / 2, 150, 50 );
        private Rect launchAlertPosition = new Rect((Screen.width-75)/2, (Screen.height-100)/2, 150, 100);
        private Rect defaultTimeRemainingPosition = new Rect( (Screen.width - 90) / 4, Screen.height - 85, 90, 55 );
        private Rect defaultCrewListWindowPosition = new Rect( (Screen.width - 360) / 2, (Screen.height / 4), 360, 1 );
        private Rect defaultSettingsPosition = new Rect( (3 * Screen.width / 8), (Screen.height / 4), 300, 1 );
        private Rect defaultUpgradePosition = new Rect( (Screen.width - 260) / 2, (Screen.height / 4), 260, 1 );
        private Rect defaultBLPlusPosition = new Rect( Screen.width - 500, 40, 100, 1 );

        // Hide/Show GUI Windows
        private List<string> guiHidden = new List<string>();
        #endregion

        #region Internal Properties
        internal bool GUIIsHidden { get => guiHidden.Count != 0; }
        #endregion

        #region Internal Methods
        internal void HideGUI(string lockedBy)
        {
            guiHidden.Add( lockedBy );
        }

        internal void UnHideGUI(string lockedBy)
        {
            guiHidden.RemoveAll( x => x == lockedBy );
        }

        internal void UnHideGUI()
        {
            guiHidden.Clear();
        }
        #endregion

        #region MonoBehavior and related private methods

        #region One-Time
        private void Awake()
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( Awake );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                UnityEngine.Object.DontDestroyOnLoad( this ); //Don't go away on scene changes
                Instance = this;
            }
        }

        private void Start()
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( Start );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                // Hide / Show UI on these events

                global::GameEvents.onGameSceneLoadRequested.Add( this.onGameSceneLoadRequested );
                global::GameEvents.onLevelWasLoaded.Add( this.onLevelWasLoaded );
                global::GameEvents.onHideUI.Add( this.onHideUI );
                global::GameEvents.onShowUI.Add( this.onShowUI );
                global::GameEvents.onGamePause.Add( this.onGamePause );
                global::GameEvents.onGameUnpause.Add( this.onGameUnpause );
                //global::GameEvents.OnGameSettingsApplied.Add( this.OnGameSettingsApplied );

                //defaultDTFormatter = KSPUtil.dateTimeFormatter;

                StartCoroutine( Configure() );
            }
        }
        private void OnDestroy()
        {
            //OnTimeControlGlobalSettingsLoadedEvent?.Remove( OnTimeControlGlobalSettingsLoaded );
        }

        private void OnSARGlobalSettingsLoaded(bool b)
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( OnSARGlobalSettingsLoaded );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                LoadSettings();
            }
        }

        private void SaveSettings()
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( SaveSettings );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
            }
        }

        private void LoadSettings()
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( LoadSettings );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
            }
        }

        private void SetDefaults()
        {
            defaultIconPosition = new Rect( Screen.width / 4, Screen.height - 30, 50, 30 );//110
            defaultMainWindowPosition = new Rect( Screen.width / 3.5f, Screen.height / 3.5f, 350, 200 );
            defaultSOIAlertPosition = new Rect( Screen.width / 3, Screen.height / 3, 250, 100 );
            defaultCentralWindowPosition = new Rect( (Screen.width - 150) / 2, (Screen.height - 50) / 2, 150, 50 );
            //defaultLaunchAlertPosition = new Rect( (Screen.width - 75) / 2, (Screen.height - 100) / 2, 150, 100 );
            defaultTimeRemainingPosition = new Rect( (Screen.width - 90) / 4, Screen.height - 85, 90, 55 );
            defaultCrewListWindowPosition = new Rect( (Screen.width - 360) / 2, (Screen.height / 4), 360, 1 );
            defaultSettingsPosition = new Rect( (3 * Screen.width / 8), (Screen.height / 4), 300, 1 );
            defaultUpgradePosition = new Rect( (Screen.width - 260) / 2, (Screen.height / 4), 260, 1 );
            defaultBLPlusPosition = new Rect( Screen.width - 500, 40, 100, 1 );
        }   

        private void ConfigureWindows()
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( Configure );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                guiWindows.Clear();

                Rect buildListWindowPosition = new Rect( Screen.width - 400, 40, 400, 1 );

                // Editor Window
                guiWindows.Add( new IMGUIWindow()
                {
                    Id = 8953,
                    Position = new Rect( Screen.width / 3.5f, Screen.height / 3.5f, 275, 135 ),
                    Text = "Kerbal Construction Time",
                    IsDisplayed = false,
                    Style = HighLogic.Skin.window,
                    GUIWindowFunction = KCT_GUI.DrawEditorGUI
                });

                // Build List Window
                guiWindows.Add( new IMGUIWindow()
                {
                    Id = 8950,
                    Position = new Rect( Screen.width / 3.5f, Screen.height / 3.5f, 275, 135 ),
                    Text = "Kerbal Construction Time",
                    IsDisplayed = false,
                    Style = HighLogic.Skin.window,
                    GUIWindowFunction = KCT_GUI.DrawBuildListWindow
                } );


                /*

                //if (showMainGUI)
                //    mainWindowPosition = GUILayout.Window(8950, mainWindowPosition, KCT_GUI.DrawMainGUI, "Kerbal Construction Time", HighLogic.Skin.window);
                if (showEditorGUI)
                    editorWindowPosition = GUILayout.Window( 8953, editorWindowPosition, KCT_GUI.DrawEditorGUI, "Kerbal Construction Time", HighLogic.Skin.window );
                //if (showSOIAlert)
                //    SOIAlertPosition = GUILayout.Window(8951, SOIAlertPosition, KCT_GUI.DrawSOIAlertWindow, "SOI Change", HighLogic.Skin.window);
                //if (showLaunchAlert)
                //    centralWindowPosition = GUILayout.Window(8951, centralWindowPosition, KCT_GUI.DrawLaunchAlert, "KCT", HighLogic.Skin.window);
                if (showBuildList)
                    buildListWindowPosition = GUILayout.Window( 8950, buildListWindowPosition, KCT_GUI.DrawBuildListWindow, "Build List", HighLogic.Skin.window );
                if (showClearLaunch)
                    centralWindowPosition = GUILayout.Window( 8952, centralWindowPosition, KCT_GUI.DrawClearLaunch, "Launch site not clear!", HighLogic.Skin.window );
                if (showShipRoster)
                    crewListWindowPosition = GUILayout.Window( 8955, crewListWindowPosition, KCT_GUI.DrawShipRoster, "Select Crew", HighLogic.Skin.window );
                if (showCrewSelect)
                    crewListWindowPosition = GUILayout.Window( 8954, crewListWindowPosition, KCT_GUI.DrawCrewSelect, "Select Crew", HighLogic.Skin.window );
                if (showUpgradeWindow)
                    upgradePosition = GUILayout.Window( 8952, upgradePosition, KCT_GUI.DrawUpgradeWindow, "Upgrades", HighLogic.Skin.window );
                if (showBLPlus)
                    bLPlusPosition = GUILayout.Window( 8953, bLPlusPosition, KCT_GUI.DrawBLPlusWindow, "Options", HighLogic.Skin.window );
                if (showRename)
                    centralWindowPosition = GUILayout.Window( 8954, centralWindowPosition, KCT_GUI.DrawRenameWindow, "Rename", HighLogic.Skin.window );
                if (showFirstRun)
                    centralWindowPosition = GUILayout.Window( 8954, centralWindowPosition, KCT_GUI.DrawFirstRun, "Kerbal Construction Time", HighLogic.Skin.window );
                if (showPresetSaver)
                    presetNamingWindowPosition = GUILayout.Window( 8952, presetNamingWindowPosition, KCT_GUI.DrawPresetSaveWindow, "Save as New Preset", HighLogic.Skin.window );
                if (showLaunchSiteSelector)
                    centralWindowPosition = GUILayout.Window( 8952, centralWindowPosition, DrawLaunchSiteChooser, "Select Site", HighLogic.Skin.window );






                if (showEditorGUI)
                    editorWindowPosition = GUILayout.Window( 8953, editorWindowPosition, KCT_GUI.DrawEditorGUI, "Kerbal Construction Time", HighLogic.Skin.window );

    */

            }
        }

        /// <summary>
        /// Configures the GUI
        /// </summary>
        private IEnumerator Configure()
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( Configure );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                // Wait for the necessary objects to be ready for use

                //while (!TimeController.IsReady || !RailsWarpController.IsReady || !SlowMoController.IsReady || !HyperWarpController.IsReady || !GlobalSettings.IsReady)
                //{
                //    yield return null;
                //}

                // Define shared styles
                // redText.normal.textColor = Color.red;
                // yellowText.normal.textColor = Color.yellow;
                // greenText.normal.textColor = Color.green;
                // yellowButton.normal.textColor = Color.yellow;
                // yellowButton.hover.textColor = Color.yellow;
                // yellowButton.active.textColor = Color.yellow;
                // redButton.normal.textColor = Color.red;
                // redButton.hover.textColor = Color.red;
                // redButton.active.textColor = Color.red;
                // greenButton.normal.textColor = Color.green;
                // greenButton.hover.textColor = Color.green;
                // greenButton.active.textColor = Color.green;


                // Define Windows
                ConfigureWindows();

                //OnTimeControlGlobalSettingsLoadedEvent = GameEvents.FindEvent<EventData<bool>>( nameof( TimeControlEvents.OnTimeControlGlobalSettingsChanged ) );
                //OnTimeControlGlobalSettingsLoadedEvent?.Add( OnTimeControlGlobalSettingsLoaded );

                // Create GUI control objects

                //railsWarpToGUI = new RailsWarpToIMGUI();
                //railsEditorGUI = new RailsEditorIMGUI();
                //slomoGUI = new SlowMoIMGUI();
                //hyperGUI = new HyperIMGUI();
                //detailsGUI = new DetailsIMGUI();
                //keyBindingsGUI = new KeyBindingsEditorIMGUI();
                //quickWarpToGUI = new QuickWarpToIMGUI();


                Log.Info( nameof( IMGUIController ) + ".Instance is Ready", logBlockName );
                IsReady = true;
            }
            yield break;
        }
        #endregion

        #region Event Handlers
        private void onGameSceneLoadRequested(GameScenes gs)
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( onGameSceneLoadRequested );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                onHideUI();
            }
        }

        private void onLevelWasLoaded(GameScenes gs)
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( onLevelWasLoaded );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {

                }
                if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                {

                }
                if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                {

                }
                if (HighLogic.LoadedScene == GameScenes.EDITOR)
                {

                }
                UnHideGUI();
            }
        }

        private void onGamePause()
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( onGamePause );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                Log.Info( "Hiding GUI for KSP Pause", logBlockName );
                HideGUI( "GamePaused" );
            }
        }

        private void onGameUnpause()
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( onGameUnpause );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                Log.Info( "Unhiding GUI for KSP Pause", logBlockName );
                UnHideGUI( "GamePaused" );
            }
        }

        private void onHideUI()
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( onHideUI );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                Log.Info( "Hiding GUI for Game Event", logBlockName );
                HideGUI( "GameEventsUI" );
            }
        }

        private void onShowUI()
        {
            const string logBlockName = nameof( IMGUIController ) + "." + nameof( onShowUI );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                Log.Info( "Unhiding GUI for Game Event", logBlockName );
                UnHideGUI( "GameEventsUI" );
            }
        }
        #endregion
        #region GUI Methods
        private void OnGUI()
        {
            // Skip if the GUI is hidden, or not yet configured.
            if (GUIIsHidden || !IsReady || HighLogic.CurrentGame == null)
            {
                return;
            }

            // Skip while in "Mission" Mode
            if (HighLogic.CurrentGame.Mode == Game.Modes.MISSION || HighLogic.CurrentGame.Mode == Game.Modes.MISSION_BUILDER)
            {
                return;
            }

            // Don't show GUI unless we are in the appropriate scene
            if (!validScenes.Contains(HighLogic.LoadedScene))
            {
                return;
            }
        }

        #endregion

        #endregion
    }
}


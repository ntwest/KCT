using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using KSP.UI.Screens;

namespace KerbalConstructionTime
{
    public static partial class KCT_GUI
    {
        public static bool showMainGUI, showEditorGUI, showSOIAlert, showLaunchAlert, showTimeRemaining,
            showBuildList, showClearLaunch, showShipRoster, showCrewSelect, showSettings, showUpgradeWindow,
            showBLPlus, showRename, showFirstRun, showLaunchSiteSelector;

        public static bool clicked = false;

        public static GUIDataSaver guiDataSaver = new GUIDataSaver();

        private static bool unlockEditor;

        private static Vector2 scrollPos;

        private static Rect iconPosition = new Rect(Screen.width / 4, Screen.height - 30, 50, 30);//110
        private static Rect mainWindowPosition = new Rect(Screen.width / 3.5f, Screen.height / 3.5f, 350, 200);
        public static Rect editorWindowPosition = new Rect(Screen.width / 3.5f, Screen.height / 3.5f, 275, 135);
        private static Rect SOIAlertPosition = new Rect(Screen.width / 3, Screen.height / 3, 250, 100);

        public static Rect centralWindowPosition = new Rect((Screen.width - 150) / 2, (Screen.height - 50) / 2, 150, 50);


        //private static Rect launchAlertPosition = new Rect((Screen.width-75)/2, (Screen.height-100)/2, 150, 100);
        public static Rect timeRemainingPosition = new Rect((Screen.width - 90) / 4, Screen.height - 85, 90, 55);
        public static Rect buildListWindowPosition = new Rect(Screen.width - 400, 40, 400, 1);
        private static Rect crewListWindowPosition = new Rect((Screen.width - 360) / 2, (Screen.height / 4), 360, 1);
        private static Rect settingsPosition = new Rect((3 * Screen.width / 8), (Screen.height / 4), 300, 1);
        private static Rect upgradePosition = new Rect((Screen.width - 260) / 2, (Screen.height / 4), 260, 1);
        private static Rect bLPlusPosition = new Rect(Screen.width - 500, 40, 100, 1);

        public static GUISkin windowSkin;// = HighLogic.UISkin;// = new GUIStyle(HighLogic.Skin.window);
        //public static UISkinDef windowSkin;

        private static bool isKSCLocked = false, isEditorLocked = false;

        public delegate bool boolDelegatePCMString(ProtoCrewMember pcm, string partName);
        public static boolDelegatePCMString AvailabilityChecker;
        public static bool UseAvailabilityChecker = false;


        private static List<GameScenes> validScenes = new List<GameScenes> { GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.SPACECENTER, GameScenes.TRACKSTATION };

        private static GUIStyle redText;
        private static GUIStyle yellowText;
        private static GUIStyle greenText;
        private static GUIStyle normalButton;
        private static GUIStyle yellowButton;
        private static GUIStyle redButton;
        private static GUIStyle greenButton;
        
        public static void SetGUIPositions()
        {
            redText = new GUIStyle( GUI.skin.label );
            yellowText = new GUIStyle( GUI.skin.label );
            greenText = new GUIStyle( GUI.skin.label );
            normalButton = new GUIStyle( GUI.skin.button );
            yellowButton = new GUIStyle( GUI.skin.button );
            redButton = new GUIStyle( GUI.skin.button );
            greenButton = new GUIStyle( GUI.skin.button );

            redText.normal.textColor = Color.red;
            yellowText.normal.textColor = Color.yellow;
            greenText.normal.textColor = Color.green;
            yellowButton.normal.textColor = Color.yellow;
            yellowButton.hover.textColor = Color.yellow;
            yellowButton.active.textColor = Color.yellow;
            redButton.normal.textColor = Color.red;
            redButton.hover.textColor = Color.red;
            redButton.active.textColor = Color.red;
            greenButton.normal.textColor = Color.green;
            greenButton.hover.textColor = Color.green;
            greenButton.active.textColor = Color.green;

            GUISkin oldSkin = GUI.skin;
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && windowSkin == null)
                windowSkin = GUI.skin;
            GUI.skin = windowSkin;

            if (validScenes.Contains(HighLogic.LoadedScene)) //&& KCT_GameStates.settings.enabledForSave)//!(HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX && !KCT_GameStates.settings.SandboxEnabled))
            {
                /*if (!ToolbarManager.ToolbarAvailable && GUI.Button(iconPosition, "KCT", GUI.skin.button))
                {
                    onClick();
                }*/
                if (ToolbarManager.ToolbarAvailable && GameStates.kctToolbarButton != null)
                {
                    GameStates.kctToolbarButton.TexturePath = KCT_Utilities.GetButtonTexture(); //Set texture, allowing for flashing of icon.
                }


                if (showSettings)
                    //settingsPosition = GUILayout.Window(8955, settingsPosition, KCT_GUI.DrawSettings, "KCT Settings", HighLogic.Skin.window);
                    presetPosition = GUILayout.Window(8955, presetPosition, KCT_GUI.DrawPresetWindow, "KCT Settings", HighLogic.Skin.window);
                if (!KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled)
                    return;

                //if (showMainGUI)
                //    mainWindowPosition = GUILayout.Window(8950, mainWindowPosition, KCT_GUI.DrawMainGUI, "Kerbal Construction Time", HighLogic.Skin.window);
                if (showEditorGUI)
                    editorWindowPosition = GUILayout.Window(8953, editorWindowPosition, KCT_GUI.DrawEditorGUI, "Kerbal Construction Time", HighLogic.Skin.window);
                //if (showSOIAlert)
                //    SOIAlertPosition = GUILayout.Window(8951, SOIAlertPosition, KCT_GUI.DrawSOIAlertWindow, "SOI Change", HighLogic.Skin.window);
                //if (showLaunchAlert)
                //    centralWindowPosition = GUILayout.Window(8951, centralWindowPosition, KCT_GUI.DrawLaunchAlert, "KCT", HighLogic.Skin.window);
                if (showBuildList)
                    buildListWindowPosition = GUILayout.Window(8950, buildListWindowPosition, KCT_GUI.DrawBuildListWindow, "Build List", HighLogic.Skin.window);
                if (showClearLaunch)
                    centralWindowPosition = GUILayout.Window(8952, centralWindowPosition, KCT_GUI.DrawClearLaunch, "Launch site not clear!", HighLogic.Skin.window);
                if (showShipRoster)
                    crewListWindowPosition = GUILayout.Window(8955, crewListWindowPosition, KCT_GUI.DrawShipRoster, "Select Crew", HighLogic.Skin.window);
                if (showCrewSelect)
                    crewListWindowPosition = GUILayout.Window(8954, crewListWindowPosition, KCT_GUI.DrawCrewSelect, "Select Crew", HighLogic.Skin.window);
                if (showUpgradeWindow)
                    upgradePosition = GUILayout.Window(8952, upgradePosition, KCT_GUI.DrawUpgradeWindow, "Upgrades", HighLogic.Skin.window);
                if (showBLPlus)
                    bLPlusPosition = GUILayout.Window(8953, bLPlusPosition, KCT_GUI.DrawBLPlusWindow, "Options", HighLogic.Skin.window);
                if (showRename)
                    centralWindowPosition = GUILayout.Window(8954, centralWindowPosition, KCT_GUI.DrawRenameWindow, "Rename", HighLogic.Skin.window);
                if (showFirstRun)
                    centralWindowPosition = GUILayout.Window(8954, centralWindowPosition, KCT_GUI.DrawFirstRun, "Kerbal Construction Time", HighLogic.Skin.window);
                if (showPresetSaver)
                    presetNamingWindowPosition = GUILayout.Window(8952, presetNamingWindowPosition, KCT_GUI.DrawPresetSaveWindow, "Save as New Preset", HighLogic.Skin.window);
                if (showLaunchSiteSelector)
                    centralWindowPosition = GUILayout.Window(8952, centralWindowPosition, DrawLaunchSiteChooser, "Select Site", HighLogic.Skin.window);


                if (unlockEditor)
                {
                    EditorLogic.fetch.Unlock("KCTGUILock");
                    unlockEditor = false;
                }


                //Disable KSC things when certain windows are shown.
                if (showFirstRun || showRename || showUpgradeWindow || showSettings || showCrewSelect || showShipRoster || showClearLaunch)
                {
                    if (!isKSCLocked)
                    {
                        InputLockManager.SetControlLock(ControlTypes.KSC_FACILITIES, "KCTKSCLock");
                        isKSCLocked = true;
                    }
                }
                else //if (!showBuildList)
                {
                    if (isKSCLocked)
                    {
                        InputLockManager.RemoveControlLock("KCTKSCLock");
                        isKSCLocked = false;
                    }
                }
                GUI.skin = oldSkin;
            }
        }

        public static bool PrimarilyDisabled { get { return (KCT_PresetManager.PresetLoaded() && (!KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled || !KCT_PresetManager.Instance.ActivePreset.generalSettings.BuildTimes)); } }

        //private static void CheckKSCLock()
        //{
        //    //On mouseover code for build list inspired by Engineer's editor mousover code
        //    Vector2 mousePos = Input.mousePosition;
        //    mousePos.y = Screen.height - mousePos.y;
        //    if (HighLogic.LoadedScene == GameScenes.SPACECENTER && !isKSCLocked)
        //    {
        //        if ((showBuildList && buildListWindowPosition.Contains(mousePos)) || (showBLPlus && bLPlusPosition.Contains(mousePos)))
        //        {
        //            InputLockManager.SetControlLock(ControlTypes.KSC_FACILITIES, "KCTKSCLock");
        //            isKSCLocked = true;
        //        }
        //        //Log.Trace("KSC Locked");
        //    }
        //    else if (HighLogic.LoadedScene == GameScenes.SPACECENTER && isKSCLocked)
        //    {
        //        if (!(showBuildList && buildListWindowPosition.Contains(mousePos)) && !(showBLPlus && bLPlusPosition.Contains(mousePos)))
        //        {
        //            InputLockManager.RemoveControlLock("KCTKSCLock");
        //            isKSCLocked = false;
        //        }
        //        //Log.Trace("KSC UnLocked");
        //    }
        //}

        private static void CheckEditorLock()
        {
            //On mouseover code for editor inspired by Engineer's editor mousover code
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;
            if ((showEditorGUI && editorWindowPosition.Contains(mousePos)) && !isEditorLocked)
            {
                EditorLogic.fetch.Lock(true, false, true, "KCTEditorMouseLock");
                isEditorLocked = true;
                //Log.Trace("KSC Locked");
            }
            else if (!(showEditorGUI && editorWindowPosition.Contains(mousePos)) && isEditorLocked)
            {
                EditorLogic.fetch.Unlock("KCTEditorMouseLock");
                isEditorLocked = false;
                //Log.Trace("KSC UnLocked");
            }
        }

        public static void ClickOff()
        {
            Log.Trace("ClickOff");
            clicked = false;
            onClick();
        }

        public static void ClickOn()
        {
            Log.Trace("ClickOn");
            clicked = true;
            onClick();
        }

        public static void ClickToggle()
        {
            clicked = !clicked;
            onClick();
        }

        public static void onClick()
        {
            // clicked = !clicked;
            if (ToolbarManager.ToolbarAvailable && GameStates.kctToolbarButton != null)
                if (GameStates.kctToolbarButton.Important) GameStates.kctToolbarButton.Important = false;

            /*  if (!KCT_GameStates.settings.enabledForSave)
              {
                  ShowSettings();
                  return;
              }*/

            if (PrimarilyDisabled && (HighLogic.LoadedScene == GameScenes.SPACECENTER))
            {
                if (clicked)
                    ShowSettings();
                else
                    showSettings = false;
            }
            else if (HighLogic.LoadedScene == GameScenes.FLIGHT && !PrimarilyDisabled)
            {
                //showMainGUI = !showMainGUI;
                buildListWindowPosition.height = 1;
                showBuildList = clicked;
                showBLPlus = false;
                //listWindow = -1;
                ResetBLWindow();
            }
            else if ((HighLogic.LoadedScene == GameScenes.EDITOR) && !PrimarilyDisabled)
            {
                editorWindowPosition.height = 1;
                showEditorGUI = clicked;
                GameStates.showWindows[1] = showEditorGUI;
            }
            else if ((HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION) && !PrimarilyDisabled)
            {
                buildListWindowPosition.height = 1;
                showBuildList = clicked;
                showBLPlus = false;
                //listWindow = -1;
                ResetBLWindow();
                GameStates.showWindows[0] = showBuildList;
            }

            if (!GameStates.settings.PreferBlizzyToolbar)
            {
                if (KCTEvents.instance != null && KCTEvents.instance.KCTButtonStock != null)
                {
                    if (showBuildList || showSettings || showEditorGUI)
                    {
                        KCTEvents.instance.KCTButtonStock.SetTrue(false);
                    }
                    else
                    {
                        KCTEvents.instance.KCTButtonStock.SetFalse(false);
                    }
                }
            }
        }

        public static void onHoverOn()
        {
            Log.Trace("onHoverOn: Clicked = " + clicked);
            if (!PrimarilyDisabled)
            {
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight)
                {
                    if (!showBuildList)
                        ResetBLWindow();
                    showBuildList = true;
                }
            }
        }
        public static void onHoverOff()
        {
            Log.Trace("onHoverOff: Clicked = " + clicked);
            if (!PrimarilyDisabled && !clicked)
            {
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight)
                {
                    showBuildList = false;
                }
            }
        }


        public static void hideAll()
        {
            showEditorGUI = false;
            showLaunchAlert = false;
            showMainGUI = false;
            showSOIAlert = false;
            showTimeRemaining = false;
            showBuildList = false;
            showClearLaunch = false;
            showShipRoster = false;
            showCrewSelect = false;
            showSettings = false;
            showUpgradeWindow = false;
            showBLPlus = false;
            showRename = false;
            showFirstRun = false;
            showPresetSaver = false;
            showLaunchSiteSelector = false;

            //ClickOff();

            /*  if (!KCT_GameStates.settings.PreferBlizzyToolbar)
              {
                  if (KCT_Events.instance != null && KCT_Events.instance.KCTButtonStock != null)
                  {
                      KCT_Events.instance.KCTButtonStock.SetFalse(false);
                  }
              }*/
            clicked = false;

            //VABSelected = false;
            //SPHSelected = false;
            //TechSelected = false;
            //listWindow = -1;
            ResetBLWindow();
        }
    
        //private static string currentCategoryString = "NONE";
        //private static int currentCategoryInt = -1;
        public static string buildRateForDisplay;
        private static int rateIndexHolder = 0;
        public static Dictionary<string, int> PartsInUse = new Dictionary<string, int>();
        private static double finishedShipBP = -1;
        public static void DrawEditorGUI(int windowID)
        {
            if (EditorLogic.fetch == null)
            {
                return;
            }
            if (editorWindowPosition.width < 275) //the size keeps getting changed for some reason, so this will avoid that
            {
                editorWindowPosition.width = 275;
                editorWindowPosition.height = 1;
            }
            GUILayout.BeginVertical();
            //GUILayout.Label("Current KSC: " + KCT_GameStates.ActiveKSC.KSCName);
            if (!GameStates.EditorShipEditingMode) //Build mode
            {
                double buildTime = GameStates.EditorBuildTime;
                BuildListVessel.ListType type = EditorLogic.fetch.launchSiteName == "LaunchPad" ? BuildListVessel.ListType.VAB : BuildListVessel.ListType.SPH;
                //GUILayout.Label("Total Build Points (BP):", GUILayout.ExpandHeight(true));
                //GUILayout.Label(Math.Round(buildTime, 2).ToString(), GUILayout.ExpandHeight(true));
                GUILayout.BeginHorizontal();
                GUILayout.Label("Build Time at ");
                if (buildRateForDisplay == null) buildRateForDisplay = KCT_Utilities.GetBuildRate(0, type, null).ToString();
                buildRateForDisplay = GUILayout.TextField(buildRateForDisplay, GUILayout.Width(75));
                GUILayout.Label(" BP/s:");
                List<double> rates = new List<double>();
                if (type == BuildListVessel.ListType.VAB) rates = KCT_Utilities.BuildRatesVAB(null);
                else rates = KCT_Utilities.BuildRatesSPH(null);
                double bR;
                if (double.TryParse(buildRateForDisplay, out bR))
                {
                    if (GUILayout.Button("*", GUILayout.ExpandWidth(false)))
                    {
                        rateIndexHolder = (rateIndexHolder + 1) % rates.Count;
                        bR = rates[rateIndexHolder];
                        if (bR > 0)
                            buildRateForDisplay = bR.ToString();
                        else
                        {
                            rateIndexHolder = (rateIndexHolder + 1) % rates.Count;
                            bR = rates[rateIndexHolder];
                            buildRateForDisplay = bR.ToString();
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Label(MagiCore.Utilities.GetFormattedTime(buildTime / bR));
                }
                else
                {
                    GUILayout.EndHorizontal();
                    GUILayout.Label("Invalid Build Rate");
                }

                if (GameStates.EditorRolloutCosts > 0)
                    GUILayout.Label("Rollout Cost: " + Math.Round(GameStates.EditorRolloutCosts, 1));

                if (!GameStates.settings.OverrideLaunchButton)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Build"))
                    {
                        KCT_Utilities.AddVesselToBuildList();
                        //SwitchCurrentPartCategory();
                        KCT_Utilities.RecalculateEditorBuildTime(EditorLogic.fetch.ship);
                    }
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Show/Hide Build List"))
                {
                    showBuildList = !showBuildList;
                }

            }
            else //Edit mode
            {

                BuildListVessel ship = GameStates.editedVessel;
                if (finishedShipBP < 0 && ship.isFinished)
                    finishedShipBP = KCT_Utilities.GetBuildTime(ship.ExtractedPartNodes);
                double origBP = ship.isFinished ? finishedShipBP : ship.buildPoints; //If the ship is finished, recalculate times. Else, use predefined times.
                double buildTime = GameStates.EditorBuildTime;
                double difference = Math.Abs(buildTime - origBP);
                double progress;
                if (ship.isFinished) progress = origBP;
                else progress = ship.progress;
                double newProgress = Math.Max(0, progress - (1.1 * difference));
                //GUILayout.Label("Original: " + Math.Max(0, Math.Round(progress, 2)) + "/" + Math.Round(origBP, 2) + " BP (" + Math.Max(0, Math.Round(100 * (progress / origBP), 2)) + "%)");
                GUILayout.Label("Original: " + Math.Max(0, Math.Round(100 * (progress / origBP), 2)) + "%");
                //GUILayout.Label("Edited: " + Math.Round(newProgress, 2) + "/" + Math.Round(buildTime, 2) + " BP (" + Math.Round(100 * newProgress / buildTime, 2) + "%)");
                GUILayout.Label("Edited: " + Math.Round(100 * newProgress / buildTime, 2) + "%");

                BuildListVessel.ListType type = EditorLogic.fetch.launchSiteName == "LaunchPad" ? BuildListVessel.ListType.VAB : BuildListVessel.ListType.SPH;
                GUILayout.BeginHorizontal();
                GUILayout.Label("Build Time at ");
                if (buildRateForDisplay == null) buildRateForDisplay = KCT_Utilities.GetBuildRate(0, type, null).ToString();
                buildRateForDisplay = GUILayout.TextField(buildRateForDisplay, GUILayout.Width(75));
                GUILayout.Label(" BP/s:");
                List<double> rates = new List<double>();
                if (ship.type == BuildListVessel.ListType.VAB) rates = KCT_Utilities.BuildRatesVAB(null);
                else rates = KCT_Utilities.BuildRatesSPH(null);
                double bR;
                if (double.TryParse(buildRateForDisplay, out bR))
                {
                    if (GUILayout.Button("*", GUILayout.ExpandWidth(false)))
                    {
                        rateIndexHolder = (rateIndexHolder + 1) % rates.Count;
                        bR = rates[rateIndexHolder];
                        buildRateForDisplay = bR.ToString();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Label(MagiCore.Utilities.GetFormattedTime(Math.Abs(buildTime - newProgress) / bR));
                }
                else
                {
                    GUILayout.EndHorizontal();
                    GUILayout.Label("Invalid Build Rate");
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save Edits"))
                {

                    finishedShipBP = -1;
                    KCT_Utilities.AddFunds(ship.cost, TransactionReasons.VesselRollout);
                    BuildListVessel newShip = KCT_Utilities.AddVesselToBuildList();
                    if (newShip == null)
                    {
                        KCT_Utilities.SpendFunds(ship.cost, TransactionReasons.VesselRollout);
                        return;
                    }

                    ship.RemoveFromBuildList();
                    newShip.progress = newProgress;
                    Log.Trace("Finished? " + ship.isFinished);
                    if (ship.isFinished)
                        newShip.cannotEarnScience = true;

                    GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);

                    GameStates.EditorShipEditingMode = false;

                    InputLockManager.RemoveControlLock("KCTEditExit");
                    InputLockManager.RemoveControlLock("KCTEditLoad");
                    InputLockManager.RemoveControlLock("KCTEditNew");
                    InputLockManager.RemoveControlLock("KCTEditLaunch");
                    EditorLogic.fetch.Unlock("KCTEditorMouseLock");
                    Log.Trace("Edits saved.");

                    HighLogic.LoadScene(GameScenes.SPACECENTER);
                }
                if (GUILayout.Button("Cancel Edits"))
                {
                    Log.Trace("Edits cancelled.");
                    finishedShipBP = -1;
                    GameStates.EditorShipEditingMode = false;

                    InputLockManager.RemoveControlLock("KCTEditExit");
                    InputLockManager.RemoveControlLock("KCTEditLoad");
                    InputLockManager.RemoveControlLock("KCTEditNew");
                    InputLockManager.RemoveControlLock("KCTEditLaunch");
                    EditorLogic.fetch.Unlock("KCTEditorMouseLock");

                    ScrapYardWrapper.ProcessVessel(GameStates.editedVessel.ExtractedPartNodes);

                    HighLogic.LoadScene(GameScenes.SPACECENTER);
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Fill Tanks"))
                {
                    foreach (Part p in EditorLogic.fetch.ship.parts)
                    {
                        //fill as part prefab would be filled?
                        if (KCT_Utilities.PartIsProcedural(p))
                        {
                            foreach (PartResource rsc in p.Resources)
                            {
                                rsc.amount = rsc.maxAmount;
                            }
                        }
                        else
                        {
                            foreach (PartResource rsc in p.Resources)
                            {
                                PartResource templateRsc = p.partInfo.partPrefab.Resources.FirstOrDefault(r => r.resourceName == rsc.resourceName);
                                if (templateRsc != null)
                                    rsc.amount = templateRsc.amount;
                            }
                        }
                    }
                }
            }

            GUILayout.EndVertical();
            if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                GUI.DragWindow();

            CheckEditorLock();
            ClampWindow(ref editorWindowPosition, strict: false);
        }

        public static void DrawSOIAlertWindow(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("   Warp stopped due to SOI change.", GUILayout.ExpandHeight(true));
            GUILayout.Label("Vessel name: " + GameStates.lastSOIVessel, GUILayout.ExpandHeight(true));
            if (GUILayout.Button("Close"))
            {
                showSOIAlert = false;
            }
            GUILayout.EndVertical();
            if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                GUI.DragWindow();
        }

        public static void DrawLaunchAlert(int windowID)
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button("Build" + (GameStates.settings.WindowMode != 1 ? " Vessel" : "")))
            {
                KCT_Utilities.AddVesselToBuildList();
                //SwitchCurrentPartCategory();

                KCT_Utilities.RecalculateEditorBuildTime(EditorLogic.fetch.ship);
                showLaunchAlert = false;
                unlockEditor = true;
                KCT_GUI.centralWindowPosition.width = 150;
            }
            if (GUILayout.Button("Cancel"))
            {
                showLaunchAlert = false;
                centralWindowPosition.height = 1;
                unlockEditor = true;
                KCT_GUI.centralWindowPosition.width = 150;
            }
            GUILayout.EndVertical();
            if (GameStates.settings.WindowMode != 1)
                CenterWindow(ref centralWindowPosition);
        }

        public static void ResetBLWindow(bool deselectList = true)
        {
            buildListWindowPosition.height = 1;
            buildListWindowPosition.width = 500;
            if (deselectList)
                SelectList("None");

            //  listWindow = -1;
        }

        private static void ScrapVessel()
        {
            InputLockManager.RemoveControlLock("KCTPopupLock");
            //List<KCT_BuildListVessel> buildList = b.
            BuildListVessel b = KCT_Utilities.FindBLVesselByID(IDSelected);// = listWindow == 0 ? KCT_GameStates.VABList[IndexSelected] : KCT_GameStates.SPHList[IndexSelected];
            if (b == null)
            {
                Log.Trace("Tried to remove a vessel that doesn't exist!");
                return;
            }
            Log.Trace("Scrapping " + b.shipName);
            if (!b.isFinished)
            {
                List<ConfigNode> parts = b.ExtractedPartNodes;
                //double costCompleted = 0;
                //foreach (ConfigNode p in parts)
                //{
                //    costCompleted += KCT_Utilities.GetPartCostFromNode(p);
                //}
                //costCompleted = (costCompleted * b.ProgressPercent() / 100);
                b.RemoveFromBuildList();

                //only add parts that were already a part of the inventory
                if (ScrapYardWrapper.Available)
                {
                    List<ConfigNode> partsToReturn = new List<ConfigNode>();
                    foreach (ConfigNode partNode in parts)
                    {
                        if (ScrapYardWrapper.PartIsFromInventory(partNode))
                        {
                            partsToReturn.Add(partNode);
                        }
                    }
                    if (partsToReturn.Any())
                    {
                        ScrapYardWrapper.AddPartsToInventory(partsToReturn, false);
                    }
                }
            }
            else
            {
                b.RemoveFromBuildList();
                //add parts to inventory
                ScrapYardWrapper.AddPartsToInventory(b.ExtractedPartNodes, false); //don't count as a recovery
            }
            ScrapYardWrapper.SetProcessedStatus(ScrapYardWrapper.GetPartID(b.ExtractedPartNodes[0]), false);
            KCT_Utilities.AddFunds(b.cost, TransactionReasons.VesselRollout);
        }

        public static void DummyVoid() { InputLockManager.RemoveControlLock("KCTPopupLock"); }

        private static bool IsCrewable(List<Part> ship)
        {
            foreach (Part p in ship)
                if (p.CrewCapacity > 0) return true;
            return false;
        }

        private static int FirstCrewable(List<Part> ship)
        {
            for (int i = 0; i < ship.Count; i++)
            {
                Part p = ship[i];
                //Debug.Log(p.partInfo.name+":"+p.CrewCapacity);
                if (p.CrewCapacity > 0) return i;
            }
            return -1;
        }

        public static void DrawClearLaunch(int windowID)
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button("Recover Flight and Proceed"))
            {
                List<ProtoVessel> list = ShipConstruction.FindVesselsLandedAt(HighLogic.CurrentGame.flightState, GameStates.launchedVessel.launchSite);
                foreach (ProtoVessel pv in list)
                    ShipConstruction.RecoverVesselFromFlight(pv, HighLogic.CurrentGame.flightState);
                if (!IsCrewable(GameStates.launchedVessel.ExtractedParts))
                    GameStates.launchedVessel.Launch();
                else
                {
                    showClearLaunch = false;
                    centralWindowPosition.height = 1;
                    AssignInitialCrew();
                    showShipRoster = true;
                }
                centralWindowPosition.height = 1;
            }

            if (GUILayout.Button("Cancel"))
            {
                showClearLaunch = false;
                centralWindowPosition.height = 1;
            }
            GUILayout.EndVertical();
            CenterWindow(ref centralWindowPosition);
        }

        /// <summary>
        /// Assigns the initial crew to the roster, based on desired roster in the editor 
        /// </summary>
        public static void AssignInitialCrew()
        {
            GameStates.launchedCrew.Clear();
            pseudoParts = GameStates.launchedVessel.GetPseudoParts();
            parts = GameStates.launchedVessel.ExtractedParts;
            GameStates.launchedCrew = new List<CrewedPart>();
            foreach (PseudoPart pp in pseudoParts)
                GameStates.launchedCrew.Add(new CrewedPart(pp.uid, new List<ProtoCrewMember>()));
            //try to assign kerbals from the desired manifest
            if (!UseAvailabilityChecker && GameStates.launchedVessel.DesiredManifest?.Count > 0 && GameStates.launchedVessel.DesiredManifest.Exists(c => c != null))
            {
                Log.Trace("Assigning desired crew manifest.");
                List<ProtoCrewMember> available = GetAvailableCrew(string.Empty);
                Queue<ProtoCrewMember> finalCrew = new Queue<ProtoCrewMember>();
                //try to assign crew from the desired manifest
                foreach (string name in GameStates.launchedVessel.DesiredManifest)
                {
                    //assign the kerbal with that name to each seat, in order. Let's try that
                    ProtoCrewMember crew = null;
                    if (!string.IsNullOrEmpty(name))
                    {
                        crew = available.Find(c => c.name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                        if (crew != null && crew.rosterStatus != ProtoCrewMember.RosterStatus.Available) //only take those that are available
                        {
                            crew = null;
                        }
                    }

                    finalCrew.Enqueue(crew);
                }

                //check if any of these crew are even available, if not then go back to CrewFirstAvailable
                if (finalCrew.FirstOrDefault(c => c != null) == null)
                {
                    Log.Trace("Desired crew not available, falling back to default.");
                    CrewFirstAvailable();
                    return;
                }

                //Put the crew where they belong
                for (int i = 0; i < parts.Count; i++)
                {
                    Part part = parts[i];
                    for (int seat = 0; seat < part.CrewCapacity; seat++)
                    {
                        if (finalCrew.Count > 0)
                        {
                            ProtoCrewMember crewToInsert = finalCrew.Dequeue();
                            Log.Trace("Assigning " + (crewToInsert?.name ?? "null"));
                            GameStates.launchedCrew[i].crewList.Add(crewToInsert); //even add the nulls, then they should match 1 to 1
                        }
                    }
                }
            }
            else
            {
                CrewFirstAvailable();
            }
        }

        private static int partIndexToCrew;
        private static int indexToCrew;
        //private static List<String> partNames;
        private static List<PseudoPart> pseudoParts;
        private static List<Part> parts;
        public static bool randomCrew, autoHire;
        public static List<ProtoCrewMember> AvailableCrew;
        public static List<ProtoCrewMember> PossibleCrewForPart = new List<ProtoCrewMember>();
        public static void DrawShipRoster(int windowID)
        {
            System.Random rand = new System.Random();
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.MaxHeight(Screen.height / 2));
            GUILayout.BeginHorizontal();
            randomCrew = GUILayout.Toggle(randomCrew, " Randomize Filling");
            autoHire = GUILayout.Toggle(autoHire, " Auto-Hire Applicants");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (AvailableCrew == null)
            {
                AvailableCrew = GetAvailableCrew(string.Empty);
            }


            if (GUILayout.Button("Fill All"))
            {
                //foreach (AvailablePart p in KCT_GameStates.launchedVessel.GetPartNames())
                for (int j = 0; j < parts.Count; j++)
                {
                    Part p = parts[j];//KCT_Utilities.GetAvailablePartByName(KCT_GameStates.launchedVessel.GetPartNames()[j]).partPrefab;
                    if (p.CrewCapacity > 0)
                    {
                        if (UseAvailabilityChecker)
                        {
                            PossibleCrewForPart.Clear();
                            foreach (ProtoCrewMember pcm in AvailableCrew)
                                if (AvailabilityChecker(pcm, p.partInfo.name))
                                    PossibleCrewForPart.Add(pcm);
                        }
                        else
                            PossibleCrewForPart = AvailableCrew;

                        //if (!KCT_GameStates.launchedCrew.Keys.Contains(p.uid))
                        //KCT_GameStates.launchedCrew.Add(new List<ProtoCrewMember>());
                        for (int i = 0; i < p.CrewCapacity; i++)
                        {
                            if (GameStates.launchedCrew[j].crewList.Count <= i)
                            {
                                if (PossibleCrewForPart.Count > 0)
                                {
                                    int index = randomCrew ? new System.Random().Next(PossibleCrewForPart.Count) : 0;
                                    ProtoCrewMember crewMember = PossibleCrewForPart[index];
                                    if (crewMember != null)
                                    {
                                        GameStates.launchedCrew[j].crewList.Add(crewMember);
                                        PossibleCrewForPart.RemoveAt(index);
                                        if (PossibleCrewForPart != AvailableCrew)
                                            AvailableCrew.Remove(crewMember);
                                    }
                                }
                                else if (autoHire)
                                {
                                    if (HighLogic.CurrentGame.CrewRoster.Applicants.Count() == 0)
                                        HighLogic.CurrentGame.CrewRoster.GetNextApplicant();
                                    int index = randomCrew ? rand.Next(HighLogic.CurrentGame.CrewRoster.Applicants.Count() - 1) : 0;
                                    ProtoCrewMember hired = HighLogic.CurrentGame.CrewRoster.Applicants.ElementAt(index);
                                    HighLogic.CurrentGame.CrewRoster.HireApplicant(hired);
                                    List<ProtoCrewMember> activeCrew;
                                    activeCrew = GameStates.launchedCrew[j].crewList;
                                    if (activeCrew.Count > i)
                                    {
                                        activeCrew.Insert(i, hired);
                                        if (activeCrew[i + 1] == null)
                                            activeCrew.RemoveAt(i + 1);
                                    }
                                    else
                                    {
                                        for (int k = activeCrew.Count; k < i; k++)
                                        {
                                            activeCrew.Insert(k, null);
                                        }
                                        activeCrew.Insert(i, hired);
                                    }
                                    GameStates.launchedCrew[j].crewList = activeCrew;
                                }
                            }
                            else if (GameStates.launchedCrew[j].crewList[i] == null)
                            {
                                if (PossibleCrewForPart.Count > 0)
                                {
                                    int index = randomCrew ? new System.Random().Next(PossibleCrewForPart.Count) : 0;
                                    ProtoCrewMember crewMember = PossibleCrewForPart[index];
                                    if (crewMember != null)
                                    {
                                        GameStates.launchedCrew[j].crewList[i] = crewMember;
                                        PossibleCrewForPart.RemoveAt(index);
                                        if (PossibleCrewForPart != AvailableCrew)
                                            AvailableCrew.Remove(crewMember);
                                    }
                                }
                                else if (autoHire)
                                {
                                    if (HighLogic.CurrentGame.CrewRoster.Applicants.Count() == 0)
                                        HighLogic.CurrentGame.CrewRoster.GetNextApplicant();
                                    int index = randomCrew ? rand.Next(HighLogic.CurrentGame.CrewRoster.Applicants.Count() - 1) : 0;
                                    ProtoCrewMember hired = HighLogic.CurrentGame.CrewRoster.Applicants.ElementAt(index);
                                    HighLogic.CurrentGame.CrewRoster.HireApplicant(hired);
                                    List<ProtoCrewMember> activeCrew;
                                    activeCrew = GameStates.launchedCrew[j].crewList;
                                    if (activeCrew.Count > i)
                                    {
                                        activeCrew.Insert(i, hired);
                                        if (activeCrew[i + 1] == null)
                                            activeCrew.RemoveAt(i + 1);
                                    }
                                    else
                                    {
                                        for (int k = activeCrew.Count; k < i; k++)
                                        {
                                            activeCrew.Insert(k, null);
                                        }
                                        activeCrew.Insert(i, hired);
                                    }
                                    GameStates.launchedCrew[j].crewList = activeCrew;
                                }
                            }
                        }
                    }
                }
            }
            if (GUILayout.Button("Clear All"))
            {
                foreach (CrewedPart cP in GameStates.launchedCrew)
                {
                    cP.crewList.Clear();
                }
                PossibleCrewForPart.Clear();
                AvailableCrew = GetAvailableCrew(string.Empty);
            }
            GUILayout.EndHorizontal();
            int numberItems = 0;
            foreach (Part p in parts)
            {
                //Part p = KCT_Utilities.GetAvailablePartByName(s).partPrefab;
                if (p.CrewCapacity > 0)
                {
                    numberItems += 1 + p.CrewCapacity;
                }
            }
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(numberItems * 25 + 10), GUILayout.MaxHeight(Screen.height / 2));
            for (int j = 0; j < parts.Count; j++)
            {
                //Part p = KCT_Utilities.GetAvailablePartByName(KCT_GameStates.launchedVessel.GetPartNames()[j]).partPrefab;
                Part p = parts[j];
                if (p.CrewCapacity > 0)
                {
                    if (UseAvailabilityChecker)
                    {
                        PossibleCrewForPart.Clear();
                        foreach (ProtoCrewMember pcm in AvailableCrew)
                            if (AvailabilityChecker(pcm, p.partInfo.name))
                                PossibleCrewForPart.Add(pcm);
                    }
                    else
                        PossibleCrewForPart = AvailableCrew;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(p.partInfo.title.Length <= 25 ? p.partInfo.title : p.partInfo.title.Substring(0, 25));
                    if (GUILayout.Button("Fill", GUILayout.Width(75)))
                    {
                        if (GameStates.launchedCrew.Find(part => part.partID == p.craftID) == null)
                            GameStates.launchedCrew.Add(new CrewedPart(p.craftID, new List<ProtoCrewMember>()));
                        for (int i = 0; i < p.CrewCapacity; i++)
                        {
                            if (GameStates.launchedCrew[j].crewList.Count <= i)
                            {
                                if (PossibleCrewForPart.Count > 0)
                                {
                                    int index = randomCrew ? new System.Random().Next(PossibleCrewForPart.Count) : 0;
                                    ProtoCrewMember crewMember = PossibleCrewForPart[index];
                                    if (crewMember != null)
                                    {
                                        GameStates.launchedCrew[j].crewList.Add(crewMember);
                                        PossibleCrewForPart.RemoveAt(index);
                                        if (PossibleCrewForPart != AvailableCrew)
                                            AvailableCrew.Remove(crewMember);
                                    }
                                }
                                else if (autoHire)
                                {
                                    if (HighLogic.CurrentGame.CrewRoster.Applicants.Count() == 0)
                                        HighLogic.CurrentGame.CrewRoster.GetNextApplicant();
                                    int index = randomCrew ? rand.Next(HighLogic.CurrentGame.CrewRoster.Applicants.Count() - 1) : 0;
                                    ProtoCrewMember hired = HighLogic.CurrentGame.CrewRoster.Applicants.ElementAt(index);
                                    HighLogic.CurrentGame.CrewRoster.HireApplicant(hired);
                                    List<ProtoCrewMember> activeCrew;
                                    activeCrew = GameStates.launchedCrew[j].crewList;
                                    if (activeCrew.Count > i)
                                    {
                                        activeCrew.Insert(i, hired);
                                        if (activeCrew[i + 1] == null)
                                            activeCrew.RemoveAt(i + 1);
                                    }
                                    else
                                    {
                                        for (int k = activeCrew.Count; k < i; k++)
                                        {
                                            activeCrew.Insert(k, null);
                                        }
                                        activeCrew.Insert(i, hired);
                                    }
                                    GameStates.launchedCrew[j].crewList = activeCrew;
                                }
                            }
                            else if (GameStates.launchedCrew[j].crewList[i] == null)
                            {
                                if (PossibleCrewForPart.Count > 0)
                                {
                                    int index = randomCrew ? new System.Random().Next(PossibleCrewForPart.Count) : 0;
                                    GameStates.launchedCrew[j].crewList[i] = PossibleCrewForPart[index];
                                    if (PossibleCrewForPart != AvailableCrew)
                                        AvailableCrew.Remove(PossibleCrewForPart[index]);
                                    PossibleCrewForPart.RemoveAt(index);
                                }
                                else if (autoHire)
                                {
                                    if (HighLogic.CurrentGame.CrewRoster.Applicants.Count() == 0)
                                        HighLogic.CurrentGame.CrewRoster.GetNextApplicant();
                                    int index = randomCrew ? rand.Next(HighLogic.CurrentGame.CrewRoster.Applicants.Count() - 1) : 0;
                                    ProtoCrewMember hired = HighLogic.CurrentGame.CrewRoster.Applicants.ElementAt(index);
                                    HighLogic.CurrentGame.CrewRoster.HireApplicant(hired);
                                    List<ProtoCrewMember> activeCrew;
                                    activeCrew = GameStates.launchedCrew[j].crewList;
                                    if (activeCrew.Count > i)
                                    {
                                        activeCrew.Insert(i, hired);
                                        if (activeCrew[i + 1] == null)
                                            activeCrew.RemoveAt(i + 1);
                                    }
                                    else
                                    {
                                        for (int k = activeCrew.Count; k < i; k++)
                                        {
                                            activeCrew.Insert(k, null);
                                        }
                                        activeCrew.Insert(i, hired);
                                    }
                                    GameStates.launchedCrew[j].crewList = activeCrew;
                                }
                            }
                        }
                    }
                    if (GUILayout.Button("Clear", GUILayout.Width(75)))
                    {
                        GameStates.launchedCrew[j].crewList.Clear();
                        PossibleCrewForPart.Clear();
                        AvailableCrew = GetAvailableCrew(string.Empty);
                    }
                    GUILayout.EndHorizontal();
                    for (int i = 0; i < p.CrewCapacity; i++)
                    {
                        GUILayout.BeginHorizontal();
                        if (i < GameStates.launchedCrew[j].crewList.Count && GameStates.launchedCrew[j].crewList[i] != null)
                        {
                            ProtoCrewMember kerbal = GameStates.launchedCrew[j].crewList[i];
                            GUILayout.Label(kerbal.name + ", " + kerbal.experienceTrait.Title + " " + kerbal.experienceLevel); //Display the kerbal currently in the seat, followed by occupation and level
                            if (GUILayout.Button("Remove", GUILayout.Width(120)))
                            {
                                GameStates.launchedCrew[j].crewList[i].rosterStatus = ProtoCrewMember.RosterStatus.Available;
                                //KCT_GameStates.launchedCrew[j].RemoveAt(i);
                                GameStates.launchedCrew[j].crewList[i] = null;
                                AvailableCrew = GetAvailableCrew(string.Empty);
                            }
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Empty");
                            if (PossibleCrewForPart.Count > 0 && GUILayout.Button("Add", GUILayout.Width(120)))
                            {
                                showShipRoster = false;
                                showCrewSelect = true;
                                partIndexToCrew = j;
                                indexToCrew = i;
                                crewListWindowPosition.height = 1;
                            }
                            if (!UseAvailabilityChecker && AvailableCrew.Count == 0 && GUILayout.Button("Hire New", GUILayout.Width(120)))
                            {
                                int index = randomCrew ? rand.Next(HighLogic.CurrentGame.CrewRoster.Applicants.Count() - 1) : 0;
                                ProtoCrewMember hired = HighLogic.CurrentGame.CrewRoster.Applicants.ElementAt(index);
                                //hired.rosterStatus = ProtoCrewMember.RosterStatus.AVAILABLE;
                                //HighLogic.CurrentGame.CrewRoster.AddCrewMember(hired);
                                HighLogic.CurrentGame.CrewRoster.HireApplicant(hired);
                                List<ProtoCrewMember> activeCrew;
                                activeCrew = GameStates.launchedCrew[j].crewList;
                                if (activeCrew.Count > i)
                                {
                                    activeCrew.Insert(i, hired);
                                    if (activeCrew[i + 1] == null)
                                        activeCrew.RemoveAt(i + 1);
                                }
                                else
                                {
                                    for (int k = activeCrew.Count; k < i; k++)
                                    {
                                        activeCrew.Insert(k, null);
                                    }
                                    activeCrew.Insert(i, hired);
                                }
                                //availableCrew.Remove(crew);
                                GameStates.launchedCrew[j].crewList = activeCrew;
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Launch"))
            {
                GameStates.settings.RandomizeCrew = randomCrew;
                GameStates.settings.AutoHireCrew = autoHire;

                //if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                GameStates.launchedVessel.Launch();
                /* else
                 {
                     HighLogic.LoadScene(GameScenes.SPACECENTER);
                     KCT_GameStates.LaunchFromTS = true;
                     //KCT_GameStates.launchedVessel.Launch();
                 }*/
                showShipRoster = false;
                crewListWindowPosition.height = 1;

            }
            if (GUILayout.Button("Cancel"))
            {
                showShipRoster = false;
                GameStates.launchedCrew.Clear();
                crewListWindowPosition.height = 1;

                GameStates.settings.RandomizeCrew = randomCrew;
                GameStates.settings.AutoHireCrew = autoHire;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            CenterWindow(ref crewListWindowPosition);
        }

        public static void CrewFirstAvailable()
        {
            int partIndex = FirstCrewable(parts);
            if (partIndex > -1)
            {
                Part p = parts[partIndex];
                if (GameStates.launchedCrew.Find(part => part.partID == p.craftID) == null)
                    GameStates.launchedCrew.Add(new CrewedPart(p.craftID, new List<ProtoCrewMember>()));
                AvailableCrew = GetAvailableCrew(p.partInfo.name);
                for (int i = 0; i < p.CrewCapacity; i++)
                {
                    if (GameStates.launchedCrew[partIndex].crewList.Count <= i)
                    {
                        if (AvailableCrew.Count > 0)
                        {
                            int index = randomCrew ? new System.Random().Next(AvailableCrew.Count) : 0;
                            ProtoCrewMember crewMember = AvailableCrew[index];
                            if (crewMember != null)
                            {
                                GameStates.launchedCrew[partIndex].crewList.Add(crewMember);
                                AvailableCrew.RemoveAt(index);
                            }
                        }
                    }
                    else if (GameStates.launchedCrew[partIndex].crewList[i] == null)
                    {
                        if (AvailableCrew.Count > 0)
                        {
                            int index = randomCrew ? new System.Random().Next(AvailableCrew.Count) : 0;
                            GameStates.launchedCrew[partIndex].crewList[i] = AvailableCrew[index];
                            AvailableCrew.RemoveAt(index);
                        }
                    }
                }
            }
        }

        private static List<ProtoCrewMember> GetAvailableCrew(string partName)
        {
            List<ProtoCrewMember> availableCrew = new List<ProtoCrewMember>();
            List<ProtoCrewMember> roster;
            if (CrewRandR.API.Available)
                roster = CrewRandR.API.AvailableCrew.ToList();
            else
                roster = HighLogic.CurrentGame.CrewRoster.Crew.ToList();

            foreach (ProtoCrewMember crewMember in roster) //Initialize available crew list
            {
                bool available = true;
                if ((!UseAvailabilityChecker || string.IsNullOrEmpty(partName)) || AvailabilityChecker(crewMember, partName))
                {
                    if (crewMember.rosterStatus == ProtoCrewMember.RosterStatus.Available && !crewMember.inactive)
                    {
                        foreach (CrewedPart cP in GameStates.launchedCrew)
                        {
                            if (cP.crewList.Contains(crewMember))
                            {
                                available = false;
                                break;
                            }
                        }
                    }
                    else
                        available = false;
                    if (available)
                        availableCrew.Add(crewMember);
                }
            }


            foreach (ProtoCrewMember crewMember in HighLogic.CurrentGame.CrewRoster.Tourist) //Get tourists
            {
                bool available = true;
                if (crewMember.rosterStatus == ProtoCrewMember.RosterStatus.Available && !crewMember.inactive)
                {
                    foreach (CrewedPart cP in GameStates.launchedCrew)
                    {
                        if (cP.crewList.Contains(crewMember))
                        {
                            available = false;
                            break;
                        }
                    }
                }
                else
                    available = false;
                if (available)
                    availableCrew.Add(crewMember);
            }

            
            return availableCrew;
        }

        public static void DrawCrewSelect(int windowID)
        {
            //List<ProtoCrewMember> availableCrew = CrewAvailable();
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.MaxHeight(Screen.height / 2));
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(PossibleCrewForPart.Count * 28 * 2 + 35), GUILayout.MaxHeight(Screen.height / 2));

            float cWidth = 80;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:");
            GUILayout.Label("Courage:", GUILayout.Width(cWidth));
            GUILayout.Label("Stupidity:", GUILayout.Width(cWidth));
            //GUILayout.Space(cWidth/2);
            GUILayout.EndHorizontal();

            foreach (ProtoCrewMember crew in PossibleCrewForPart)
            {
                GUILayout.BeginHorizontal();
                //GUILayout.Label(crew.name);
                if (GUILayout.Button(crew.name+"\n"+crew.experienceTrait.Title+" "+crew.experienceLevel))
                {
                    List<ProtoCrewMember> activeCrew;
                    activeCrew = GameStates.launchedCrew[partIndexToCrew].crewList;
                    if (activeCrew.Count > indexToCrew)
                    {
                        activeCrew.Insert(indexToCrew, crew);
                        if (activeCrew[indexToCrew + 1] == null)
                            activeCrew.RemoveAt(indexToCrew + 1);
                    }
                    else
                    {
                        for (int i = activeCrew.Count; i < indexToCrew; i++)
                        {
                            activeCrew.Insert(i, null);
                        }
                        activeCrew.Insert(indexToCrew, crew);
                    }
                    PossibleCrewForPart.Remove(crew);
                    GameStates.launchedCrew[partIndexToCrew].crewList = activeCrew;
                    showCrewSelect = false;
                    showShipRoster = true;
                    crewListWindowPosition.height = 1;
                    break;
                }
                GUILayout.HorizontalSlider(crew.courage, 0, 1, HighLogic.Skin.horizontalSlider, HighLogic.Skin.horizontalSliderThumb, GUILayout.Width(cWidth));
                GUILayout.HorizontalSlider(crew.stupidity, 0, 1, HighLogic.Skin.horizontalSlider, HighLogic.Skin.horizontalSliderThumb, GUILayout.Width(cWidth));

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Cancel"))
            {
                showCrewSelect = false;
                showShipRoster = true;
                crewListWindowPosition.height = 1;
            }
            GUILayout.EndVertical();
            CenterWindow(ref crewListWindowPosition);
        }

       /* public static string newMultiplier, newBuildEffect, newInvEffect, newTimeWarp, newSandboxUpgrades, newUpgradeCount, newTimeLimit, newRecoveryModifier,
            newReconEffect, maxReconditioning, newNodeModifier;
        public static bool enabledForSave, enableAllBodies, forceStopWarp, instantTechUnlock, disableBuildTimes, checkForUpdates, versionSpecific, disableRecMsgs, disableAllMsgs,
            recon, debug, overrideLaunchBtn, autoAlarms, useBlizzyToolbar, allowParachuteRecovery, instantKSCUpgrades;
        */
        public static bool forceStopWarp, disableAllMsgs, debug, overrideLaunchBtn, autoAlarms, useBlizzyToolbar, debugUpdateChecking;
        public static int newTimewarp;

        public static double reconSplit;
        public static string newRecoveryModDefault;
        public static bool disableBuildTimesDefault, instantTechUnlockDefault, enableAllBodiesDefault, reconDefault, instantKSCUpgradeDefault;
        private static void ShowSettings()
        {
            newTimewarp = GameStates.settings.MaxTimeWarp;
            forceStopWarp = GameStates.settings.ForceStopWarp;
            disableAllMsgs = GameStates.settings.DisableAllMessages;
            debug = GameStates.settings.Debug;
            overrideLaunchBtn = GameStates.settings.OverrideLaunchButton;
            autoAlarms = GameStates.settings.AutoKACAlarms;
            useBlizzyToolbar = GameStates.settings.PreferBlizzyToolbar;
            debugUpdateChecking = GameStates.settings.CheckForDebugUpdates;

            showSettings = !showSettings;
        }

        public static void  CheckToolbar()
        {
            if (ToolbarManager.ToolbarAvailable && ToolbarManager.Instance != null && GameStates.settings.PreferBlizzyToolbar && GameStates.kctToolbarButton == null)
            {
                Log.Trace("Adding Toolbar Button");
                GameStates.kctToolbarButton = ToolbarManager.Instance.add("Kerbal_Construction_Time", "MainButton");
                if (GameStates.kctToolbarButton != null)
                {
                    if (!KCT_PresetManager.Instance.ActivePreset.generalSettings.Enabled) GameStates.kctToolbarButton.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
                    else GameStates.kctToolbarButton.Visibility = new GameScenesVisibility(new GameScenes[] { GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.EDITOR });
                    GameStates.kctToolbarButton.TexturePath = KCT_Utilities.GetButtonTexture();
                    GameStates.kctToolbarButton.ToolTip = "Kerbal Construction Time";
                    GameStates.kctToolbarButton.OnClick += ((e) =>
                    {
                        //KCT_GUI.clicked = !KCT_GUI.clicked;
                        KCT_GUI.ClickToggle();
                    });
                }
            }
            bool vis;
            if ( ApplicationLauncher.Ready && (!GameStates.settings.PreferBlizzyToolbar || !ToolbarManager.ToolbarAvailable) && (KCTEvents.instance.KCTButtonStock == null || !ApplicationLauncher.Instance.Contains(KCTEvents.instance.KCTButtonStock, out vis))) //Add Stock button
            {
                KCTEvents.instance.KCTButtonStock = ApplicationLauncher.Instance.AddModApplication(
                    KCT_GUI.ClickOn,
                    KCT_GUI.ClickOff,
                    KCT_GUI.onHoverOn,
                    KCT_GUI.onHoverOff,
                    KCTEvents.instance.DummyVoid, //TODO: List next ship here?
                    KCTEvents.instance.DummyVoid,
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.VAB,
                    GameDatabase.Instance.GetTexture(PluginAssemblyUtilities.iconKCTOn, false));

                ApplicationLauncher.Instance.EnableMutuallyExclusive(KCTEvents.instance.KCTButtonStock);
            }
        }

        private static int upgradeWindowHolder = 0;
        public static double sciCost = -13, fundsCost = -13;
        public static double nodeRate = -13, upNodeRate = -13;
        public static double researchRate = -13, upResearchRate = -13;
        private static void DrawUpgradeWindow(int windowID)
        {
            int spentPoints = KCT_Utilities.TotalSpentUpgrades(null);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            int upgrades = KCT_Utilities.TotalUpgradePoints();
            GUILayout.Label("Total Points: " + upgrades);
            GUILayout.Label("Available: " + (upgrades - spentPoints));
          //  if (KCT_Utilities.RSSActive)
           //     GUILayout.Label("Minimum Available: ");
            GUILayout.EndHorizontal();

            if (KCT_Utilities.CurrentGameHasScience())
            {
                //int cost = (int)Math.Min(Math.Pow(2, KCT_GameStates.PurchasedUpgrades[0]+2), 512);
                if (sciCost == -13)
                {
                    sciCost = KCT_MathParsing.GetStandardFormulaValue("UpgradeScience", new Dictionary<string, string>() { { "N", GameStates.PurchasedUpgrades[0].ToString() } });
                 //   double max = double.Parse(KCT_GameStates.formulaSettings.UpgradeScienceMax);
                 //   if (max > 0 && sciCost > max) sciCost = max;
                }
                double cost = sciCost;
                if (cost >= 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Buy Point: ");
                    if (GUILayout.Button(Math.Round(cost, 0) + " Sci", GUILayout.ExpandWidth(false)))
                    {
                        sciCost = KCT_MathParsing.GetStandardFormulaValue("UpgradeScience", new Dictionary<string, string>() { { "N", GameStates.PurchasedUpgrades[0].ToString() } });
                        //double max = double.Parse(KCT_GameStates.formulaSettings.UpgradeScienceMax);
                        //if (max > 0 && sciCost > max) sciCost = max;
                        cost = sciCost;

                        if (ResearchAndDevelopment.Instance.Science >= cost)
                        {
                            //ResearchAndDevelopment.Instance.Science -= cost;
                            ResearchAndDevelopment.Instance.AddScience(-(float)cost, TransactionReasons.None);
                            ++GameStates.PurchasedUpgrades[0];

                            sciCost = -13;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            if (KCT_Utilities.CurrentGameIsCareer())
            {
                //double cost = Math.Min(Math.Pow(2, KCT_GameStates.PurchasedUpgrades[1]+4), 1024) * 1000;
                if (fundsCost == -13)
                {
                    fundsCost = KCT_MathParsing.GetStandardFormulaValue("UpgradeFunds", new Dictionary<string, string>() { { "N", GameStates.PurchasedUpgrades[1].ToString() } });
                   // double max = double.Parse(KCT_GameStates.formulaSettings.UpgradeFundsMax);
                   // if (max > 0 && fundsCost > max) fundsCost = max;
                }
                double cost = fundsCost;
                if (cost >= 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Buy Point: ");
                    if (GUILayout.Button(Math.Round(cost, 0) + " Funds", GUILayout.ExpandWidth(false)))
                    {
                        fundsCost = KCT_MathParsing.GetStandardFormulaValue("UpgradeFunds", new Dictionary<string, string>() { { "N", GameStates.PurchasedUpgrades[1].ToString() } });
                     //   double max = int.Parse(KCT_GameStates.formulaSettings.UpgradeFundsMax);
                      //  if (max > 0 && fundsCost > max) fundsCost = max;
                        cost = fundsCost;

                        if (Funding.Instance.Funds >= cost)
                        {
                            KCT_Utilities.SpendFunds(cost, TransactionReasons.None);
                            ++GameStates.PurchasedUpgrades[1];


                            fundsCost = -13;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            //TODO: Calculate the cost of resetting
            int ResetCost = (int)KCT_MathParsing.GetStandardFormulaValue("UpgradeReset", new Dictionary<string, string> { { "N", GameStates.UpgradesResetCounter.ToString() } });
            if (ResetCost >= 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Reset Upgrades: ");
                if (GUILayout.Button(ResetCost+" Points", GUILayout.ExpandWidth(false)))
                {
                    if (spentPoints > 0 && (upgrades - spentPoints >= ResetCost)) //you have to spend some points before resetting does anything
                    {
                        GameStates.ActiveKSC.VABUpgrades = new List<int>() { 0 };
                        GameStates.ActiveKSC.SPHUpgrades = new List<int>() { 0 };
                        GameStates.ActiveKSC.RDUpgrades = new List<int>() { 0, 0 };
                        GameStates.TechUpgradesTotal = 0;
                        foreach (SpaceCenterConstruction ksc in GameStates.KSCs)
                        {
                            ksc.RDUpgrades[1] = 0;
                        }
                        nodeRate = -13;
                        upNodeRate = -13;
                        researchRate = -13;
                        upResearchRate = -13;

                        GameStates.ActiveKSC.RecalculateBuildRates();
                        GameStates.ActiveKSC.RecalculateUpgradedBuildRates();

                        foreach (KCT_TechItem tech in GameStates.TechList)
                            tech.UpdateBuildRate(GameStates.TechList.IndexOf(tech));

                        GameStates.UpgradesResetCounter++;
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("VAB")) { upgradeWindowHolder = 0; upgradePosition.height = 1; }
            if (GUILayout.Button("SPH")) { upgradeWindowHolder = 1; upgradePosition.height = 1; }
            if (KCT_Utilities.CurrentGameHasScience() && GUILayout.Button("R&D")) { upgradeWindowHolder = 2; upgradePosition.height = 1; }
            GUILayout.EndHorizontal();
            SpaceCenterConstruction KSC = GameStates.ActiveKSC;

            if (upgradeWindowHolder==0) //VAB
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("VAB Upgrades");
                GUILayout.EndHorizontal();
                scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height((KSC.VABUpgrades.Count + 1) * 26 + 5), GUILayout.MaxHeight(1 * Screen.height / 4));
                GUILayout.BeginVertical();
                for (int i = 0; i < KSC.VABRates.Count; i++)
                {
                    double rate = KCT_Utilities.GetBuildRate(i, BuildListVessel.ListType.VAB, KSC);
                    double upgraded = KCT_Utilities.GetBuildRate(i, BuildListVessel.ListType.VAB, KSC, true);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Rate "+(i+1));
                    GUILayout.Label(rate + " BP/s");
                    if (upgrades - spentPoints > 0 && (i == 0 || upgraded <= KCT_Utilities.GetBuildRate(i - 1, BuildListVessel.ListType.VAB, KSC)) && upgraded - rate > 0)
                    {
                        if (GUILayout.Button("+" + Math.Round(upgraded - rate,2), GUILayout.Width(45)))
                        {
                            if (i < KSC.VABUpgrades.Count)
                                ++KSC.VABUpgrades[i];
                            else
                                KSC.VABUpgrades.Add(1);
                            KSC.RecalculateBuildRates();
                            KSC.RecalculateUpgradedBuildRates();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
               /* GUILayout.BeginHorizontal();
                GUILayout.Label("Rate " + (KSC.VABUpgrades.Count + 1));
                GUILayout.Label("0 BP/s");
                if (KCT_GameStates.TotalUpgradePoints - spentPoints > 0 && ((KSC.VABUpgrades.Count + 1) * 0.05)
                    <= KCT_Utilities.GetBuildRate(KSC.VABUpgrades.Count - 1, KCT_BuildListVessel.ListType.VAB, KSC))
                {
                    if (GUILayout.Button("+" + ((KSC.VABUpgrades.Count + 1) * 0.05), GUILayout.Width(45)))
                    {
                        KSC.VABUpgrades.Add(1);
                        KSC.RecalculateBuildRates();
                        KSC.RecalculateUpgradedBuildRates();
                    }
                }
                GUILayout.EndHorizontal();*/
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }

            if (upgradeWindowHolder == 1) //SPH
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("SPH Upgrades");
                GUILayout.EndHorizontal();
                scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height((KSC.SPHUpgrades.Count + 1) * 26 + 5), GUILayout.MaxHeight(1 * Screen.height / 4));
                GUILayout.BeginVertical();
                for (int i = 0; i < KSC.SPHRates.Count; i++)
                {
                    double rate = KCT_Utilities.GetBuildRate(i, BuildListVessel.ListType.SPH, KSC);
                    double upgraded = KCT_Utilities.GetBuildRate(i, BuildListVessel.ListType.SPH, KSC, true);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Rate " + (i + 1));
                    GUILayout.Label(rate + " BP/s");
                    if (upgrades - spentPoints > 0 && (i == 0 || upgraded <= KCT_Utilities.GetBuildRate(i-1, BuildListVessel.ListType.SPH, KSC)) && upgraded-rate > 0)
                    {
                        if (GUILayout.Button("+" + Math.Round(upgraded - rate, 2), GUILayout.Width(45)))
                        {
                            if (i < KSC.SPHUpgrades.Count)
                                ++KSC.SPHUpgrades[i];
                            else
                                KSC.SPHUpgrades.Add(1);
                            KSC.RecalculateBuildRates();
                            KSC.RecalculateUpgradedBuildRates();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                /*GUILayout.BeginHorizontal();
                GUILayout.Label("Rate " + (KSC.SPHUpgrades.Count + 1));
                GUILayout.Label("0 BP/s");
                if (KCT_GameStates.TotalUpgradePoints - spentPoints > 0 && ((KSC.SPHUpgrades.Count + 1) * 0.05)
                    <= KCT_Utilities.GetBuildRate(KSC.SPHUpgrades.Count - 1, KCT_BuildListVessel.ListType.SPH, KSC))
                {
                    if (GUILayout.Button("+" + ((KSC.SPHUpgrades.Count + 1) * 0.05), GUILayout.Width(45)))
                    {
                        KSC.SPHUpgrades.Add(1);
                        KSC.RecalculateBuildRates();
                        KSC.RecalculateUpgradedBuildRates();
                    }
                }
                GUILayout.EndHorizontal();*/
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            if (upgradeWindowHolder == 2) //R&D
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("R&D Upgrades");
                GUILayout.EndHorizontal();

                if (researchRate == -13)
                {
                    Dictionary<string, string> normalVars = new Dictionary<string, string>() { { "N", KSC.RDUpgrades[0].ToString() }, {"R", KCT_Utilities.BuildingUpgradeLevel(SpaceCenterFacility.ResearchAndDevelopment).ToString() } };
                    KCT_MathParsing.AddCrewVariables(normalVars);
                    researchRate = KCT_MathParsing.GetStandardFormulaValue("Research", normalVars);

                    Dictionary<string, string> upVars = new Dictionary<string, string>() { { "N", (KSC.RDUpgrades[0]+1).ToString() }, { "R", KCT_Utilities.BuildingUpgradeLevel(SpaceCenterFacility.ResearchAndDevelopment).ToString() } };
                    KCT_MathParsing.AddCrewVariables(upVars);
                    upResearchRate = KCT_MathParsing.GetStandardFormulaValue("Research", upVars);
                }

                if (researchRate >= 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Research");
                    GUILayout.Label(Math.Round(researchRate * 86400, 2) + " sci/86400 BP");
                    if (upgrades - spentPoints > 0)
                    {
                        if (GUILayout.Button("+" + Math.Round((upResearchRate - researchRate) * 86400, 2), GUILayout.Width(45)))
                        {
                            ++KSC.RDUpgrades[0];
                            researchRate = -13;
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                double days = GameSettings.KERBIN_TIME ? 4 : 1;
                if (nodeRate == -13)
                {
                    nodeRate = KCT_MathParsing.ParseNodeRateFormula(0);
                        //KCT_MathParsing.GetStandardFormulaValue("Node", new Dictionary<string, string>() { { "N", KSC.RDUpgrades[1].ToString() }, { "R", KCT_Utilities.BuildingUpgradeLevel(SpaceCenterFacility.ResearchAndDevelopment).ToString() } });
                   // double max = double.Parse(KCT_GameStates.formulaSettings.NodeMax);
                  //  if (max > 0 && nodeRate > max) nodeRate = max;

                    upNodeRate = KCT_MathParsing.ParseNodeRateFormula(0, 0, true);
                    //KCT_MathParsing.GetStandardFormulaValue("Node", new Dictionary<string, string>() { { "N", (KSC.RDUpgrades[1] + 1).ToString() }, { "R", KCT_Utilities.BuildingUpgradeLevel(SpaceCenterFacility.ResearchAndDevelopment).ToString() } });
                  //  if (max > 0 && upNodeRate > max) upNodeRate = max;
                }
                double sci = 86400 * nodeRate;

                double sciPerDay = sci / days;
                //days *= KCT_GameStates.timeSettings.NodeModifier;
                GUILayout.BeginHorizontal();
                GUILayout.Label("Devel.");
                bool usingPerYear = false;
                if (sciPerDay > 0.1)
                {
                    GUILayout.Label(Math.Round(sciPerDay*1000)/1000 + " sci/day");
                }
                else
                {
                    //Well, looks like we need sci/year instead
                    int daysPerYear = KSPUtil.dateTimeFormatter.Year/KSPUtil.dateTimeFormatter.Day;
                    GUILayout.Label(Math.Round(sciPerDay * daysPerYear * 1000) / 1000 + " sci/yr");
                    usingPerYear = true;
                }
                if (upNodeRate != nodeRate && upgrades - spentPoints > 0)
                {
                    bool everyKSCCanUpgrade = true;
                    foreach (SpaceCenterConstruction ksc in GameStates.KSCs)
                    {
                        if (upgrades - KCT_Utilities.TotalSpentUpgrades(ksc) <= 0)
                        {
                            everyKSCCanUpgrade = false;
                            break;
                        }
                    }
                    if (everyKSCCanUpgrade)
                    {
                        double upSciPerDay = 86400 * upNodeRate / days;
                        string buttonText = Math.Round(1000 * upSciPerDay) / 1000 + " sci/day";
                        if (usingPerYear)
                        {
                            int daysPerYear = KSPUtil.dateTimeFormatter.Year / KSPUtil.dateTimeFormatter.Day;
                            buttonText = Math.Round(upSciPerDay * daysPerYear * 1000) / 1000 + " sci/yr";
                        }
                        if (GUILayout.Button(buttonText, GUILayout.ExpandWidth(false)))
                        {
                            ++GameStates.TechUpgradesTotal;
                            foreach (SpaceCenterConstruction ksc in GameStates.KSCs)
                                ksc.RDUpgrades[1] = GameStates.TechUpgradesTotal;

                            nodeRate = -13;
                            upNodeRate = -13;

                            foreach (KCT_TechItem tech in GameStates.TechList)
                            {
                                tech.UpdateBuildRate(GameStates.TechList.IndexOf(tech));
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();

            }
            if (GUILayout.Button("Close"))
            {
                showUpgradeWindow = false;
                if (!PrimarilyDisabled)
                {
                    //showBuildList = true;
                    if (KCTEvents.instance.KCTButtonStock != null)
                        KCTEvents.instance.KCTButtonStock.SetTrue();
                    else
                        showBuildList = true;
                }
            }
            GUILayout.EndVertical();
            if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                GUI.DragWindow();
        }

        public static void ResetFormulaRateHolders()
        {
            fundsCost = -13;
            sciCost = -13;
            nodeRate = -13;
            upNodeRate = -13;
            researchRate = -13;
            upResearchRate = -13;
            costOfNewLP = -13;
        }

        private static string newName = "";
        private static bool renamingLaunchPad = false;
        public static void DrawRenameWindow(int windowID)
        {
          /*  if (centralWindowPosition.y != (Screen.height - centralWindowPosition.height) / 2)
            {
                centralWindowPosition.y = (Screen.height - centralWindowPosition.height) / 2;
                centralWindowPosition.height = 1;
            }*/
            GUILayout.BeginVertical();
            GUILayout.Label("Name:");
            newName = GUILayout.TextField(newName);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                if (!renamingLaunchPad)
                {
                    BuildListVessel b = KCT_Utilities.FindBLVesselByID(IDSelected);
                    b.shipName = newName; //Change the name from our point of view
                    b.shipNode.SetValue("ship", newName);
                }
                else
                {
                    LaunchPad lp = GameStates.ActiveKSC.ActiveLPInstance;
                    lp.Rename(newName);
                }
                showRename = false;
                centralWindowPosition.width = 150;
                centralWindowPosition.x = (Screen.width - 150) / 2;
                showBuildList = true;
            }
            if (GUILayout.Button("Cancel"))
            {
                centralWindowPosition.width = 150;
                centralWindowPosition.x = (Screen.width - 150) / 2;
                showRename = false;
                showBuildList = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            CenterWindow(ref centralWindowPosition);
        }

        public static void DrawFirstRun(int windowID)
        {
            if (centralWindowPosition.width != 200)
            {
                centralWindowPosition.Set((Screen.width - 200) / 2, (Screen.height - 100) / 2, 200, 100);
            }
            GUILayout.BeginVertical();
            GUILayout.Label("Welcome to KCT! Follow the steps below to get set up.");
            //GUILayout.Label("Welcome to KCT! It is advised that you spend your " + (KCT_Utilities.TotalUpgradePoints()-KCT_Utilities.TotalSpentUpgrades(null)) + " upgrades to increase the build rate in the building you will primarily be using.");
            //GUILayout.Label("Please see the getting started guide included in the download or available from the forum for more information!");
           /* if (KCT_GameStates.settings.CheckForUpdates)
                GUILayout.Label("Due to your settings, automatic update checking is enabled. You can disable it in the Settings menu!");
            else
                GUILayout.Label("Due to your settings, automatic update checking is disabled. You can enable it in the Settings menu!");
            */
            //GUILayout.Label("\nNote: 0.24 introduced a bug that causes time to freeze while hovering over the Build List with the mouse cursor. Just move the cursor off of the window and time will resume.");
            if (GUILayout.Button("1 - Choose a Preset"))
            {
                //showFirstRun = false;
                centralWindowPosition.height = 1;
                centralWindowPosition.width = 150;
                ShowSettings();
                //showSettings = true;
            }
            if (!PrimarilyDisabled && KCT_Utilities.TotalUpgradePoints() > 0)
            {
                if (GUILayout.Button("2 - Spend Upgrades"))
                {
                    showFirstRun = false;
                    centralWindowPosition.height = 1;
                    centralWindowPosition.width = 150;
                    showUpgradeWindow = true;
                }
            }
            else
            {
                if (GUILayout.Button("2 - Close Window"))
                {
                    showFirstRun = false;
                    centralWindowPosition.height = 1;
                    centralWindowPosition.width = 150;
                }
            }

            /*if (GUILayout.Button("3 - Finished"))
            {
                showFirstRun = false;
                centralWindowPosition.height = 1;
                centralWindowPosition.width = 150;
                if (KCT_GameStates.settings.CheckForUpdates)
                    KCT_UpdateChecker.CheckForUpdate(true, KCT_GameStates.settings.VersionSpecific);

            }*/
            GUILayout.EndVertical();
            if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                GUI.DragWindow();
        }

        public static void CenterWindow(ref Rect window)
        {
            window.x = (float)((Screen.width - window.width) / 2.0);
            window.y = (float)((Screen.height - window.height) / 2.0);
        }

        /// <summary>
        /// Clamps a window to the screen
        /// </summary>
        /// <param name="window">The window Rect</param>
        /// <param name="strict">If true, none of the window can go past the edge.
        /// If false, half the window can. Defaults to false.</param>
        public static void ClampWindow(ref Rect window, bool strict = false)
        {
            if (strict)
            {
                if (window.x < 0)
                    window.x = 0;
                if (window.x + window.width > Screen.width)
                    window.x = Screen.width - window.width;

                if (window.y < 0)
                    window.y = 0;
                if (window.y + window.height > Screen.height)
                    window.y = Screen.height - window.height;
            }
            else
            {
                float halfW = window.width / 2;
                float halfH = window.height / 2;
                if (window.x + halfW < 0)
                    window.x = -halfW;
                if (window.x + halfW > Screen.width)
                    window.x = Screen.width - halfW;

                if (window.y + halfH < 0)
                    window.y = -halfH;
                if (window.y + halfH > Screen.height)
                    window.y = Screen.height - halfH;
            }
        }
    }

    public class GUIPosition
    {
        [Persistent] public string guiName;
        [Persistent] public float xPos, yPos;
        [Persistent] public bool visible;

        public GUIPosition() { }
        public GUIPosition(string name, float x, float y, bool vis)
        {
            guiName = name;
            xPos = x;
            yPos = y;
            visible = vis;
        }
    }

    public class GUIDataSaver
    {
        protected String filePath = PluginAssemblyUtilities.windowLocationsFilePath;
        [Persistent] GUIPosition editorPositionSaved, buildListPositionSaved;
        public void Save()
        {
            buildListPositionSaved = new GUIPosition("buildList", KCT_GUI.buildListWindowPosition.x, KCT_GUI.buildListWindowPosition.y, GameStates.showWindows[0]);
            editorPositionSaved = new GUIPosition("editor", KCT_GUI.editorWindowPosition.x, KCT_GUI.editorWindowPosition.y, GameStates.showWindows[1]);

            ConfigNode cnTemp = ConfigNode.CreateConfigFromObject(this, new ConfigNode());
            cnTemp.Save(filePath);
        }

        public void Load()
        {
            if (!System.IO.File.Exists(filePath))
                return;

            ConfigNode cnToLoad = ConfigNode.Load(filePath);
            ConfigNode.LoadObjectFromConfig(this, cnToLoad);

            KCT_GUI.buildListWindowPosition.x = buildListPositionSaved.xPos;
            KCT_GUI.buildListWindowPosition.y = buildListPositionSaved.yPos;
            GameStates.showWindows[0] = buildListPositionSaved.visible;

            KCT_GUI.editorWindowPosition.x = editorPositionSaved.xPos;
            KCT_GUI.editorWindowPosition.y = editorPositionSaved.yPos;
            GameStates.showWindows[1] = editorPositionSaved.visible;
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

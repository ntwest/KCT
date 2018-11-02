using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalConstructionTime
{
    public static class GameStates
    {
        public static double UT, lastUT=0.0;
        public static bool canWarp = false, warpInitiated = false;
        public static int lastWarpRate = 0;
        public static string lastSOIVessel = "";
        public static List<VesselType> VesselTypesForSOI = new List<VesselType>() { VesselType.Base, VesselType.Lander, VesselType.Probe, VesselType.Ship, VesselType.Station };
        public static List<Orbit.PatchTransitionType> SOITransitions = new List<Orbit.PatchTransitionType> { Orbit.PatchTransitionType.ENCOUNTER, Orbit.PatchTransitionType.ESCAPE };
        public static Settings settings = new Settings();

        public static SpaceCenterConstruction ActiveKSC = null;
        public static List<SpaceCenterConstruction> KSCs = new List<SpaceCenterConstruction>();
        public static string activeKSCName = "";
        public static bool UpdateLaunchpadDestructionState = false;
        public static int TechUpgradesTotal = 0;
        public static List<KCT_TechItem> TechList = new List<KCT_TechItem>();

        public static List<int> PurchasedUpgrades = new List<int>() { 0, 0 };
        public static int MiscellaneousTempUpgrades = 0, LastKnownTechCount = 0;
        public static float InventorySaleUpgrades = 0, InventorySalesFigures = 0;
        public static int UpgradesResetCounter = 0;
        public static BuildListVessel launchedVessel, editedVessel, recoveredVessel;
        public static List<CrewedPart> launchedCrew = new List<CrewedPart>();
        public static IButton kctToolbarButton;
        public static bool EditorShipEditingMode = false;
        public static bool firstStart = false;
        public static IKCTBuildItem targetedItem = null;
        public static double EditorBuildTime = 0, EditorRolloutCosts = 0;
        public static bool LaunchFromTS = false;
        public static List<AvailablePart> ExperimentalParts = new List<AvailablePart>();

        public static Dictionary<string, int> BuildingMaxLevelCache = new Dictionary<string, int>();

        public static List<bool> showWindows = new List<bool> { false, true }; //build list, editor
        public static string KACAlarmId = "";
        public static double KACAlarmUT = 0;

        public static KCT_OnLoadError erroredDuringOnLoad = new KCT_OnLoadError();


        public static int TemporaryModAddedUpgradesButReallyWaitForTheAPI = 0; //Reset when returned to the MainMenu
        public static int PermanentModAddedUpgradesButReallyWaitForTheAPI = 0; //Saved to the save file

        public static bool vesselErrorAlerted = false;

        public static bool PersistenceLoaded = false;
        public static void reset()
        {
            const string logBlockName = nameof( GameStates ) + "." + nameof( reset );
            using (EntryExitLogger.EntryExitLog( logBlockName, EntryExitLoggerOptions.All ))
            {
                firstStart = false;
                vesselErrorAlerted = false;

                PurchasedUpgrades = new List<int>() { 0, 0 };
                targetedItem = null;
                KCT_GUI.ResetFormulaRateHolders();

                InventorySaleUpgrades = 0;
                InventorySalesFigures = 0;

                ExperimentalParts.Clear();
                MiscellaneousTempUpgrades = 0;

                BuildingMaxLevelCache.Clear();

                lastUT = 0;
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

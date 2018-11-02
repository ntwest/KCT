﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KerbalConstructionTime
{
    public class Settings
    {
        [Persistent]
        public int MaxTimeWarp;
        [Persistent]
        public bool ForceStopWarp;
        [Persistent]
        public bool DisableAllMessages;
        [Persistent]
        public bool AutoKACAlarms;
        [Persistent]
        public bool Debug;
        [Persistent]
        public bool OverrideLaunchButton;
        [Persistent]
        public bool PreferBlizzyToolbar;
        [Persistent]
        public bool CheckForDebugUpdates;
        [Persistent]
        public bool RandomizeCrew;
        [Persistent]
        public bool AutoHireCrew;
        [Persistent]
        public int WindowMode = 1;

        public Settings()
        {
            MaxTimeWarp = TimeWarp.fetch.warpRates.Count() - 1;
            ForceStopWarp = false;
            DisableAllMessages = false;
            Debug = false;
            OverrideLaunchButton = true;
            AutoKACAlarms = true;
            PreferBlizzyToolbar = false;
        }

        public void Load()
        {
            if (File.Exists( PluginAssemblyUtilities.KCTConfigFilePath ))
            {
                ConfigNode cnToLoad = ConfigNode.Load( PluginAssemblyUtilities.KCTConfigFilePath );
                ConfigNode.LoadObjectFromConfig(this, cnToLoad);

                KCT_GUI.autoHire = AutoHireCrew;
                KCT_GUI.randomCrew = RandomizeCrew;
            }
        }

        public void Save()
        {
            if (!Directory.Exists(Path.GetDirectoryName(PluginAssemblyUtilities.KCTConfigFilePath)))
                Directory.CreateDirectory( Path.GetDirectoryName( PluginAssemblyUtilities.KCTConfigFilePath ) );

            ConfigNode cnTemp = ConfigNode.CreateConfigFromObject(this, new ConfigNode());
            cnTemp.Save(PluginAssemblyUtilities.KCTConfigFilePath);
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

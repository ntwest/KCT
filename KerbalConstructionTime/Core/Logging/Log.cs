﻿using System;
using System.Reflection;
using UnityEngine;
using System.Diagnostics;

namespace KerbalConstructionTime
{    
    static internal class Log
    {
        internal readonly static string VERSION = Assembly.GetAssembly( typeof( Log ) ).GetName().Version.Major + "." + Assembly.GetAssembly( typeof( Log ) ).GetName().Version.Minor + "." + Assembly.GetAssembly( typeof( Log ) ).GetName().Version.Build;
        internal readonly static string MOD = Assembly.GetAssembly( typeof( Log ) ).GetName().Name;
        public static readonly string logPrefix = MOD + "(" + VERSION + ")";
        internal readonly static string title = logPrefix + " - Serious Error";

        static internal LogSeverity loggingLevel;

        /// <summary>
        /// Show all messages of this level and below (e.g. Error only shows errors, while Info shows Errors, Warnings, and Info)
        /// </summary>
        static internal LogSeverity LoggingLevel
        {
            get => loggingLevel;
            set
            {
                loggingLevel = value;
                //System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                //Log.Trace( "Logging Level Set to " + value.ToString() + "\n" +  t.ToString(), "Log", true );
                Log.Trace( "Logging Level Set to " + value.ToString(), "Log", true );
            }
        }

        static Log()
        {
            //LoggingLevel = LogSeverity.Warning;
            LoggingLevel = LogSeverity.Trace;
        }

        static internal void Trace(string message, string caller = "", bool always = false)
        {
            Log.Write( message, caller, LogSeverity.Trace, always );
        }
        static internal void Info(string message, string caller = "", bool always = false)
        {
            Log.Write( message, caller, LogSeverity.Info, always );
        }
        static internal void Warning(string message, string caller = "", bool always = false)
        {
            Log.Write( message, caller, LogSeverity.Warning, always );
        }
        static internal void Error(string message, string caller = "", bool always = false)
        {
            Log.Write( message, caller, LogSeverity.Error, always );
        }

        static internal void PopupError(string message)
        {            
            PopupDialog.SpawnPopupDialog( new Vector2( 0.5f, 0.5f ), new Vector2( 0.5f, 0.5f ), Guid.NewGuid().ToString(), title, message, "OK", true, HighLogic.UISkin, false );
        }

        static internal void Write(string message, string caller = "", LogSeverity sev = LogSeverity.Warning, bool always = false)
        {
            // Return if we don't need to write messages for this severity
            if (!always && Log.LoggingLevel > sev)
            {
                return;
            }

            message = string.Format( "[{1}] <{2}>{3} {0} - ({4}) - {5}", DateTime.Now, logPrefix, sev, (always ? "-A" : ""), caller, message );
            switch (sev)
            {
                case LogSeverity.Error:
                    UnityEngine.Debug.LogError( message );
                    break;
                case LogSeverity.Warning:
                    UnityEngine.Debug.LogWarning( message );
                    break;
                default:
                    UnityEngine.Debug.Log( message );
                    break;
            }
        }
    }
}

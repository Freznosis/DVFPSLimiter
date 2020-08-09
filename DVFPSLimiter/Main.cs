using System;
using System.IO;
using System.Reflection;
using Harmony12;
using TinyJson;
using UnityEngine;
using UnityModManagerNet;
using Object = UnityEngine.Object;

namespace DVFPSLimiter
{
    public static class Main
    {

        /// <summary>
        /// File name for settings.
        /// </summary>
        private const string settingsJSON = "Settings.json";

        private class FPSLimiterSettings
        {
            public int fpsLimit = 60;
            public bool forceVsyncOff = false;
        }

        /// <summary>
        /// Main FPS Limiter settings.
        /// </summary>
        private static FPSLimiterSettings _fpsLimiterSettings;
        
        #region Cache

        private static string settingsFilePath;
        
        #endregion
        
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            Main.Enabled = modEntry.Enabled;
            Main.ModEntry = modEntry;

            #region Load Settings

            //Get settings file path
            settingsFilePath = Path.Combine(modEntry.Path, settingsJSON);

            //If path exist, try to load JSON file.
            if (File.Exists(settingsFilePath))
            {
                _fpsLimiterSettings = File.ReadAllText(settingsFilePath).FromJson<FPSLimiterSettings>();
            }
            else
            {
                /*
                //If there is no settings file, throw error.
                modEntry.Logger.Error($"Settings.json was not found in the directory {settingsFilePath}");
                
                return false;
                */
                
                //Create a new JSON file instead of throwing an error.
                _fpsLimiterSettings= new FPSLimiterSettings();
                File.WriteAllText(settingsFilePath, _fpsLimiterSettings.ToJson());
                
            }
            
            #endregion

            //Set FPS after world is loaded.
            WorldStreamingInit.LoadingFinished+= WorldStreamingInitOnLoadingFinished;
            
            return true;
        }

        private static void WorldStreamingInitOnLoadingFinished()
        {
            Application.targetFrameRate = _fpsLimiterSettings.fpsLimit;
            
            //Could probably do some magic here and change the vsync flag in save game options too.
            if (_fpsLimiterSettings.forceVsyncOff)
            {
                QualitySettings.vSyncCount = 0;
            }
        }

        public static UnityModManager.ModEntry ModEntry;
        public static bool Enabled;
    }
}
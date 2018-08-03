using System;
using IllusionPlugin;
using UnityEngine;
using System.Reflection;
using System.IO;
using UnofficialLeaderBoardPlugin;

    public class Plugin : IPlugin
    {
        public string Name
        {
            get { return "Unofficial Leaderboard by Umbranox"; }
        }

        public string Version
        {
            get { return "1.0.3"; }
        }

        public void OnApplicationStart()
        {

            PlayerPrefs.SetInt("lbPatched", 1);
        }

        public void OnApplicationQuit()
        {

        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public bool loaded = false;
        public void OnLevelWasInitialized(int level)
        {

            if (level != 1) return;
            if (!loaded)
            {
                Log("Plugin started");
                var leaderBoardsModel = PersistentSingleton<PlatformLeaderboardsModel>.instance;
                Log("Leaderboards model found");
                ReflectionUtil.SetPrivateField(leaderBoardsModel, "_platformLeaderboardsHandler", new CustomPlatformLeaderboardsHandler());
                Log("Set new leaderboardsHandler");
                loaded = true;
            }
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {

        }
        public string GetDirectoryPath(Assembly assembly)
        {
            string filePath = new Uri(assembly.CodeBase).LocalPath;
            return Path.GetDirectoryName(filePath);
        }
        private void Log(string data)
        {

            File.AppendAllText(@"LeaderBoardPluginLog.txt", data + Environment.NewLine);
        }
    }
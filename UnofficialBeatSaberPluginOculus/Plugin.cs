using System;
using IllusionPlugin;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;

namespace UnofficialLeaderBoardPlugin
{
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
            if (level == 1)
            {
                Initialize();
            }
        }

        public bool loaded = false;
        public void OnLevelWasInitialized(int level)
        {

            if (level != 1) return;
            if (!loaded)
            {
                var leaderBoardsModel = PersistentSingleton<PlatformLeaderboardsModel>.instance;
                ReflectionUtil.SetPrivateField(leaderBoardsModel, "_platformLeaderboardsHandler", new CustomPlatformLeaderboardsHandle());
                loaded = true;
            }

        }
        
        public void Initialize()
        {
            Request<User> oculusRequest = Users.GetLoggedInUser().OnComplete(delegate(Message<User> message)
            {
                Global.playerId = message.Data.ID;
                Global.playerName = message.Data.OculusID;
            });
        }
        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {

        }
    }
}
using System;
using System.IO;

namespace UnofficialLeaderBoardPlugin
{
    public static class Global
    {
        public static bool steam;
        public static ulong playerId;
        public static string playerName;
        public static void Log(string data)
        {
            File.AppendAllText(@"LeaderBoardPluginLog.txt", data + Environment.NewLine);
        }
    }
  
}

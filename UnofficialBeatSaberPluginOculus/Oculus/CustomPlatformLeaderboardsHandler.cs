using Oculus.Platform.Models;
using Oculus.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
namespace UnofficialLeaderBoardPlugin
{

    public class CustomPlatformLeaderboardsHandle :OculusPlatformLeaderboardsHandler
    {
        #region CustomScoreUI
       
        public override void GetScores(string leaderboadId, int count, int fromRank, PlatformLeaderboardsModel.ScoresScope scope, string referencePlayerId, HMAsyncRequest asyncRequest, PlatformLeaderboardsModel.GetScoresCompletionHandler completionHandler)
        {
            if (leaderboadId.Contains("∎"))
            {
                leaderboadId = FormatLeaderBoard(leaderboadId);

                switch (scope)
                {
                    case PlatformLeaderboardsModel.ScoresScope.AroundPlayer:
                        GetCustomScoreBehaviour.GetScore("http://scoresaber.com/a0461a2eac6bb4d1ba0b0e976e9740ac.php?id=" + leaderboadId + "&steamId=" + Global.playerId, completionHandler, leaderboadId, asyncRequest, OnGetScore);
                        break;
                    case PlatformLeaderboardsModel.ScoresScope.Global:
                        GetCustomScoreBehaviour.GetScore("http://scoresaber.com/a0461a2eac6bb4d1ba0b0e976e9740ac.php?id=" + leaderboadId, completionHandler, leaderboadId, asyncRequest, OnGetScore);
                        break;
                }
            }
            else
            {
                base.GetScores(leaderboadId, count, fromRank, scope, referencePlayerId, asyncRequest, completionHandler);
                //CallNonStaticFunctionDynamically("OculusPlatformLeaderboardsHandler", "Assembly-CSharp", "GetScores",
                //         new Type[] { typeof(string), typeof(int), typeof(int), typeof(PlatformLeaderboardsModel.ScoresScope),
                //                typeof(string), typeof(HMAsyncRequest), typeof(PlatformLeaderboardsModel.GetScoresCompletionHandler) },
                //               new object[] { leaderboadId, count, fromRank, scope, referencePlayerId, asyncRequest, completionHandler });
            }
        }
        private void OnGetScore(byte[] data, PlatformLeaderboardsModel.GetScoresCompletionHandler completionHandler, string leaderboadID, HMAsyncRequest asyncRequest)
        {
            string scoreData = System.Text.Encoding.UTF8.GetString(data);

            bool cancelRequest = false;
            if (asyncRequest != null)
            {
                asyncRequest.CancelHandler = delegate(HMAsyncRequest request)
                {
                    cancelRequest = true;
                };
            }
            int playerScoreIndex = -1;
            List<PlatformLeaderboardsModel.LeaderboardScore> leaders = PassLeaderBoardInfo(scoreData, ref playerScoreIndex);

            if (leaders.Count > 0)
            {
                if (cancelRequest == false)
                {
                    OkCompletionHandler(completionHandler, leaders.ToArray(), playerScoreIndex, asyncRequest);
                }
                else
                {
                    FailCompletionHandler(completionHandler, asyncRequest);
                }
            }
            else
            {
                FailCompletionHandler(completionHandler, asyncRequest);
            }
        }


        private void OkCompletionHandler(PlatformLeaderboardsModel.GetScoresCompletionHandler handler, PlatformLeaderboardsModel.LeaderboardScore[] leaders, int playerScoreIndex, HMAsyncRequest request)
        {
            if (request != null)
            {
                if (handler != null)
                {
                    handler(PlatformLeaderboardsModel.GetScoresResult.OK, leaders.ToArray(), playerScoreIndex);
                }
            }
        }
        private void FailCompletionHandler(PlatformLeaderboardsModel.GetScoresCompletionHandler handler, HMAsyncRequest request)
        {
            if (request != null)
            {
                if (handler != null)
                {
                    handler(PlatformLeaderboardsModel.GetScoresResult.Failed, new PlatformLeaderboardsModel.LeaderboardScore[0], 0);
                }
            }

        }
        #endregion

        #region Uploading
        public override void UploadScore(string leaderboadId, int score, HMAsyncRequest asyncRequest, PlatformLeaderboardsModel.UploadScoreCompletionHandler completionHandler)
        {
            try
            {
                if (leaderboadId.Contains("∎"))
                {
                    string text = "lb_" + leaderboadId;
                    this.PrepareCustomScore(text, score, completionHandler);
                    return;
                }
                else
                {
                    base.UploadScore(leaderboadId, score, asyncRequest, completionHandler);
                    //CallNonStaticFunctionDynamically("OculusPlatformLeaderboardsHandler", "Assembly-CSharp", "UploadScore",
                    //   new Type[] { typeof(string), typeof(int), typeof(HMAsyncRequest), typeof(PlatformLeaderboardsModel.UploadScoreCompletionHandler) },
                    //          new object[] { leaderboadId, score, asyncRequest, completionHandler });
                }
            }
            catch (Exception ex)
            {
                FailLog(ex);
                FailScoreCompletionHandler(completionHandler);
            }
        }
        
        private void PrepareCustomScore(string leaderBoard, int score, PlatformLeaderboardsModel.UploadScoreCompletionHandler completionHandler)
        {
            try
            {
                string[] array = leaderBoard.Split(new char[]
                    {
                        '∎'
                    });
                new Thread(() =>
                {
                    UploadCustomScore(array, score.ToString(), completionHandler);
                }).Start();
            }
            catch (Exception ex)
            {
                FailLog(ex);
                FailScoreCompletionHandler(completionHandler);
            }

        }

        
        private void UploadCustomScore(string[] score, string s, PlatformLeaderboardsModel.UploadScoreCompletionHandler completionHandler)
        {
            if (completionHandler != null)
            {
                completionHandler(PlatformLeaderboardsModel.UploadScoreResult.OK);
            }
            //Score uploader removed for security
        }
        #endregion

        #region Helpers

        private void CallNonStaticFunctionDynamically(string functionClass, string dependency, string function, Type[] methodSig, object[] parameters)
        {
            Type FunctionClass = Type.GetType(string.Format("{0},{1}", functionClass, dependency));
            if (FunctionClass != null)
            {
                object FunctionClassInstance = Activator.CreateInstance(FunctionClass);
                if (FunctionClassInstance != null)
                {
                    Type InstanceType = FunctionClassInstance.GetType();
                    MethodInfo Function = InstanceType.GetMethod(function, methodSig);
                    if (Function != null)
                    {
                        Function.Invoke(FunctionClassInstance, parameters);
                    }
                }
            }
        }
        private List<PlatformLeaderboardsModel.LeaderboardScore> PassLeaderBoardInfo(string scoreData, ref int playerScoreIndex)
        {
            List<PlatformLeaderboardsModel.LeaderboardScore> leaders = new List<PlatformLeaderboardsModel.LeaderboardScore>();
            try
            {
                if (scoreData != string.Empty)
                {
                    var decodedScoreData = SimpleJSON.JSON.Parse(scoreData);
                    for (int i = 0; i < decodedScoreData.Count; i += 3)
                    {
                        string rawId = decodedScoreData[i + 1];
                        string steamId = string.Empty;
                        string name = string.Empty;
                        if (rawId.Contains("|"))
                        {
                            steamId = rawId.Split('|')[0];
                            name = rawId.Split('|')[1];
                        }
                        else
                        {
                            steamId = rawId;
                        }

                        int rank = decodedScoreData[i];
                        int score = decodedScoreData[i + 2];
                        if (steamId == Global.playerId.ToString())
                        {
                            playerScoreIndex = i / 3;
                        }

                        leaders.Add(new PlatformLeaderboardsModel.LeaderboardScore(score, rank, name, steamId));
                    }
                    return leaders;
                }
                return leaders;
            }
            catch (Exception ex)
            {
                return leaders;
            }

        }
        private string FormatLeaderBoard(string leaderboard)
        {
            string difficulty = leaderboard.Split('∎')[5].Replace("Expert+", "ExpertPlus");
            leaderboard = leaderboard.Split('∎')[0];
            return "lb_" + leaderboard + difficulty;
        }
        private void Log(string data)
        {

            File.AppendAllText(@"LeaderBoardPluginLog.txt", data + Environment.NewLine);
        }
        private void FailScoreCompletionHandler(PlatformLeaderboardsModel.UploadScoreCompletionHandler completionHandler)
        {
            if (completionHandler != null)
            {
                completionHandler(PlatformLeaderboardsModel.UploadScoreResult.Falied);
            }
        }
        private void FailLog(Exception ex)
        {
           Log("Failed to upload score: " + ex.ToString());
        }
        #endregion
    }
}
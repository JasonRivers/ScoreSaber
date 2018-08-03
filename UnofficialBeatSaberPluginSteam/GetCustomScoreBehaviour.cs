using System;
using System.Collections;
using UnityEngine;
public class GetCustomScoreBehaviour : MonoBehaviour
{
    private static GetCustomScoreBehaviour _instance;

    public static void GetScore(string url, PlatformLeaderboardsModel.GetScoresCompletionHandler completionHandler, string leaderboadID, HMAsyncRequest asyncRequestd, Action<byte[], PlatformLeaderboardsModel.GetScoresCompletionHandler, string, HMAsyncRequest> callback)
    {
        if (_instance == null)
        {
            _instance = new GameObject("temp").AddComponent<GetCustomScoreBehaviour>();
        }

        _instance.StartCoroutine(_instance.GetScoreRoutine(url, completionHandler, leaderboadID, asyncRequestd, callback));
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator GetScoreRoutine(string url, PlatformLeaderboardsModel.GetScoresCompletionHandler completionHandler, string leaderboadID, HMAsyncRequest asyncRequestd, Action<byte[], PlatformLeaderboardsModel.GetScoresCompletionHandler, string, HMAsyncRequest> callback)
    {
        using (var www = new WWW(url))
        {

            yield return www;
            callback.Invoke(www.bytes, completionHandler, leaderboadID, asyncRequestd);
        }
    }
}
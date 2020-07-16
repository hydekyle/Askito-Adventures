using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public struct TweetData
{
    public int userID;
    public int tweetID;
    public int followers;
    public int friends;
    public int favourites;
    public int tweetsCount;
    public string accountName;
    public string pictureURL;
    public string nickName;
    public string createdAt;
}

public struct ServerResponse
{
    public int statusCode;
    public string dataJSON;
}

public class Db : MonoBehaviour
{
    string server_adress = "localhost:8080/hyde";

    private void Start()
    {
        StartCoroutine(GetTweetData(server_adress, tweetData =>
        {
            Debug.LogFormat("Lets gooo {0}", tweetData.accountName);
        }));
    }

    IEnumerator GetTweetData(string uri, Action<TweetData> tweetData)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            yield return request.SendWebRequest();
            if (!request.isNetworkError)
            {
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                if (response.statusCode == 1)
                {
                    tweetData(JsonUtility.FromJson<TweetData>(response.dataJSON));
                }
            }
            else
            {
                Debug.LogWarning("No me puedo conectar al servidor: " + request.error);
            }
        }
    }

}

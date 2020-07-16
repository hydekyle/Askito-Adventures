using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
    public string data;
}

public class Db : MonoBehaviour
{
    string server_adress = "localhost:8080";

    private void Start()
    {
        StartCoroutine(Conectar(server_adress));
    }

    IEnumerator Conectar(string uri)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            yield return request.SendWebRequest();
            if (!request.isNetworkError)
            {
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                if (response.statusCode == 0)
                {
                    Debug.Log(response.data);
                    TweetData myData = JsonUtility.FromJson<TweetData>(response.data);
                    Debug.Log(myData.favourites);
                }

            }
            else
            {
                Debug.LogWarning("No me puedo conectar al servidor: " + request.error);
            }
        }
    }

}

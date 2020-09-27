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

public class Db
{
    string server_adress = "localhost:8080/hyde";

    // public Db()
    // {
    //     GameManager.Instance.StartCoroutine(GetTweetData(server_adress, (tweetData, textureAvatar) =>
    //     {
    //         Debug.LogFormat("Incoming User {0}", tweetData.accountName);
    //         CanvasManager.Instance.avatarTweeter.sprite = Sprite.Create
    //         (
    //             textureAvatar,
    //              new Rect(0f, 0f, textureAvatar.width, textureAvatar.height),
    //              Vector2.zero
    //         );
    //     }));
    // }

    IEnumerator GetTweetData(string uri, Action<TweetData, Texture2D> tweetData)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            yield return request.SendWebRequest();
            if (!request.isNetworkError)
            {
                ServerResponse response;
                try
                {
                    response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                }
                catch
                {
                    Debug.LogWarning("JSON response very feo");
                    yield break;
                }
                if (response.statusCode == 1)
                {
                    TweetData data = JsonUtility.FromJson<TweetData>(response.dataJSON);

                    Texture2D texture;
                    using (UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(data.pictureURL))
                    {
                        yield return textureRequest.SendWebRequest();
                        texture = ((DownloadHandlerTexture)textureRequest.downloadHandler).texture;
                    }
                    tweetData(data, texture);
                }
            }
            else
            {
                Debug.LogWarning("No me puedo conectar al servidor: " + request.error);
            }
        }
    }

    public int GetSouls()
    {
        return PlayerPrefs.GetInt("Souls", 0);
    }

    private void SetSouls(int value)
    {
        PlayerPrefs.SetInt("Souls", value);
    }

    public void AddSouls(int value)
    {
        int total = PlayerPrefs.GetInt("Souls", 0) + value;
        SetSouls(total);
    }

}

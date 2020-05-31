using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DB : MonoBehaviour
{
    public static DB Instance;
    public List<string> tweets = new List<string>();

    private void Awake()
    {
        Instance = Instance ?? this;
    }

    public string GetRandomTweet()
    {
        return tweets[Random.Range(0, tweets.Count)];
    }
}

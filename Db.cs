using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
                Debug.Log(request.downloadHandler.text);
            }
            else
            {
                Debug.LogWarning("No me puedo conectar al servidor: " + request.error);
            }

        }
    }

}

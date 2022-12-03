using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Api.Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class Api : MonoBehaviour
{
    public string BaseUrl;

    private Uri BaseUri => new(BaseUrl);

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    UnityWebRequest ApiReq(string uri, string method, object body)
    {
        var req = new UnityWebRequest(new Uri(BaseUri, uri), method);
        if (body != null)
        {
            var json = JsonConvert.SerializeObject(body);
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));

        }
        req.downloadHandler = new DownloadHandlerBuffer();
        return req;
    }

    IEnumerator ApiCall<TRes>(string uri, string method, object body, Action<TRes?>? accept)
    {
        var req = ApiReq(uri, method, body);
        yield return req.SendWebRequest();
        if (accept != null && req.downloadHandler.data.Length != 0)
        {
            var res = JsonConvert.DeserializeObject<TRes>(req.downloadHandler.text);
            accept(res);
        }
    }

    IEnumerator GetRooms(Action<List<Room>> accept)
    {
        return ApiCall("rooms", "GET", null, accept);
    }

    IEnumerator GetRoomObjects(int id, Action<List<Obj>> accept)
    {
        return ApiCall($"rooms/{id}/objects", "GET", null, accept);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Assets.Scripts.Api.Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class Api : MonoBehaviour
{
    public string BaseUrl;

    public string AssetCachePath = "assets";

    public string ModelCachePath = "models";

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

    IEnumerator ApiCall<TRes>(string uri, string method, object body, Action<TRes?>? accept, Action<Exception> reject)
    {
        var req = ApiReq(uri, method, body);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            reject(new Exception(req.error));
            yield break;
        }

        if (req.responseCode is < 200 or >= 300)
        {
            reject(new Exception(req.downloadHandler.text));
            yield break;
        }

        if (accept == null || req.downloadHandler.data.Length == 0) yield break;
        try
        {
            var res = JsonConvert.DeserializeObject<TRes>(req.downloadHandler.text);
            accept(res);
        }
        catch (Exception e)
        {
            reject(e);
        }
    }

    IEnumerator GetRooms(Action<List<Room>> accept, Action<Exception> reject)
    {
        return ApiCall("rooms", "GET", null, accept, reject);
    }

    IEnumerator GetRoomObjects(int id, Action<List<Obj>> accept, Action<Exception> reject)
    {
        return ApiCall($"rooms/{id}/objects", "GET", null, accept, reject);
    }

    IEnumerator GetAsset(int id, Action<Asset> accept, Action<Exception> reject)
    {
        return ApiCall($"assets/{id}", "GET", null, accept, reject);
    }

    IEnumerator DownloadAsset(int id, Action<string> accept, Action<Exception> reject)
    {
        var path = Path.Combine(Application.persistentDataPath, AssetCachePath, id.ToString());
        if (File.Exists(path))
        {
            accept(path);
            yield break;
        }

        Asset asset = null;
        yield return StartCoroutine(GetAsset(id, a => asset = a, reject));
        if (asset == null) yield break;

        if (asset.Status != AssetStatus.Ready)
        {
            reject(new Exception("Asset not ready"));
            yield break;
        }

        var req = UnityWebRequest.Get(asset.Url);
        req.downloadHandler = new DownloadHandlerFile(path);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
        {
            reject(new Exception(req.error));
            yield break;
        }

        accept(path);
    }

    IEnumerator DownloadModel(Model model, Action<string> accept, Action<Exception> reject)
    {
        var folder = Path.Combine(Application.persistentDataPath, ModelCachePath, model.Id.ToString());
        var path = Path.Combine(folder, model.Path);
        if (File.Exists(path))
        {
            accept(path);
            yield break;
        }

        string assetPath = null;
        yield return StartCoroutine(DownloadAsset(model.AssetId, p => assetPath = p, reject));
        if (assetPath == null) yield break;

        try
        {
            ZipFile.ExtractToDirectory(assetPath, folder);
        }
        catch (Exception e)
        {
            reject(e);
            yield break;
        }

        accept(path);
    }
}
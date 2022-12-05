using Dummiesman;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Api.Models;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public ApiClient apiClient;
    public GameObject placeholderPrefab;
    private readonly OBJLoader _loader = new();

    void Start()
    {
        StartLoadingObjects();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public Coroutine StartLoadingObjects()
    {
        return StartCoroutine(LoadObjects());
    }

    IEnumerator LoadObjects()
    {
        List<Obj> objs = null;
        yield return StartCoroutine(apiClient.GetRoomObjects(RoomSelector.SelectedRoom.Id, res => objs = res, err => Debug.Log(err)));
        if (objs == null) yield break;
        List<Coroutine> coroutines = new List<Coroutine>();
        foreach (var obj in objs)
        {
            GameObject ph = null;
            if (obj.Model.Bounds != null)
            {
                ph = Instantiate(placeholderPrefab, obj.Translation, obj.Rotation);
                ph.name = $"{obj.Model.Name} Placeholder";
                ph.transform.localScale = obj.Scale;
            }

            var coroutine = StartCoroutine(LoadObject(obj, ph));
            coroutines.Add(coroutine);
        }

        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }
    }

    IEnumerator LoadObject(Obj obj, GameObject ph)
    {
        string modelPath = null;
        yield return StartCoroutine(apiClient.DownloadModel(obj.Model, res => modelPath = res, err => Debug.Log(err)));
        if (modelPath == null) yield break;

        var go = _loader.Load(modelPath);
        go.name = obj.Model.Name;
        var instance = Instantiate(go, obj.Translation, obj.Rotation);
        instance.transform.localScale = obj.Scale;
        if (ph != null)
        {
            Destroy(ph);
        }
    }
}
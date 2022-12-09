using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Api.Models;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public ApiClient apiClient;
    public GameObject placeholderPrefab;
    public List<ObjectController> objControllers = new();

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
        yield return StartCoroutine(apiClient.GetRoomObjects(RoomSelector.SelectedRoom.Id, res => objs = res,
            err => Debug.Log(err)));
        if (objs == null) yield break;
        List<Coroutine> coroutines = new List<Coroutine>();
        foreach (var obj in objs)
        {
            var instance = new GameObject(obj.Model.Name);
            var ctrl = instance.AddComponent<ObjectController>();
            ctrl.obj = obj;
            ctrl.apiClient = apiClient;
            ctrl.placeholderPrefab = placeholderPrefab;
            objControllers.Add(ctrl);
            coroutines.Add(StartCoroutine(ctrl.LoadObject()));
        }

        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }
    }
}
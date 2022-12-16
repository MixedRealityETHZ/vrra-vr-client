using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Api.Models;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public ApiClient apiClient;
    public GameObject placeholderPrefab;
    public Material placeholderMaterial;
    public List<ObjectController> objControllers = new();

    void Start()
    {
        StartLoadingObjects();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartLoadingObjects()
    {
        StartCoroutine(LoadObjects());
    }

    private void RemoveAllObjects()
    {
        foreach (var objController in objControllers)
        {
            Destroy(objController.gameObject);
        }

        objControllers.Clear();
    }

    private IEnumerator LoadObjects()
    {
        RemoveAllObjects();

        List<Obj> objs = null;
        yield return StartCoroutine(apiClient.GetRoomObjects(RoomSelector.SelectedRoom.Id, res => objs = res,
            err => Debug.Log(err)));
        if (objs == null) yield break;

        var coroutines = objs.Select(obj => StartCoroutine(AddObjectController(obj).LoadObject(obj))).ToList();

        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }
    }

    public IEnumerator AddObject(Model model, Transform trans)
    {
        var roomId = RoomSelector.SelectedRoom.Id;
        var obj = new Obj()
        {
            Id = 0,
            Model = model,
            Movable = true,
            RoomId = roomId,
            Rotation = trans.rotation,
            Scale = trans.localScale,
            Translation = trans.position,
        };
        var ctrl = AddObjectController(obj);
        var c1 = StartCoroutine(ctrl.LoadObject(obj));

        var body = new AddObjBody()
        {
            ModelId = model.Id,
            Movable = true,
            Rotation = trans.rotation,
            Scale = trans.localScale,
            Translation = trans.position,
        };
        var c2 = StartCoroutine(apiClient.AddObject(roomId, body, res => obj.Id = res.Id, err => Debug.Log(err)));
        yield return c1;
        ctrl.Freeze();
        yield return c2;
        ctrl.Unfreeze();
    }

    ObjectController AddObjectController(Obj obj)
    {
        var instance = new GameObject(obj.Model.Name);
        var ctrl = instance.AddComponent<ObjectController>();
        ctrl.apiClient = apiClient;
        ctrl.placeholderPrefab = placeholderPrefab;
        ctrl.placeholderMaterial = placeholderMaterial;
        objControllers.Add(ctrl);
        return ctrl;
    }

    public void EnableDeleteMode()
    {
        foreach (var objController in objControllers)
        {
            objController.EnableDeleteMode();
        }
    }

    public void CancelDelete()
    {
        foreach (var objController in objControllers)
        {
            objController.DisableDeleteMode();
        }
    }

    private void DeleteObject(ObjectController ctrl)
    {
        StartCoroutine(apiClient.DeleteObject(
            RoomSelector.SelectedRoom.Id,
            ctrl.obj.Id,
            () => { Destroy(ctrl.gameObject); },
            err => { Debug.LogError(err.Message, this); }
        ));
    }

    public void ConfirmDelete()
    {
        foreach (var objController in objControllers.Where(objController => objController.deleted))
        {
            DeleteObject(objController);
        }

        objControllers.RemoveAll(objController => objController.deleted);
    }
}
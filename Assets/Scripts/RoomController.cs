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
    public int pollInterval = 5;
    private Coroutine pollCoroutine = null;

    void Start()
    {
        pollCoroutine = StartCoroutine(PollObjectsUpdate());
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void RemoveAllObjects()
    {
        foreach (var objController in GetComponentsInChildren<ObjectController>())
        {
            Destroy(objController.gameObject);
        }
    }

    public void ReloadObjects()
    {
        if(pollCoroutine==null)
        {
            StopCoroutine(pollCoroutine);
            pollCoroutine = null;
        }
        RemoveAllObjects();
        pollCoroutine = StartCoroutine(PollObjectsUpdate());
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
        instance.transform.parent = transform;
        var ctrl = instance.AddComponent<ObjectController>();
        ctrl.apiClient = apiClient;
        ctrl.placeholderPrefab = placeholderPrefab;
        ctrl.placeholderMaterial = placeholderMaterial;
        return ctrl;
    }

    public void EnableDeleteMode()
    {
        foreach (var objController in GetComponentsInChildren<ObjectController>())
        {
            objController.EnableDeleteMode();
        }
    }

    public void CancelDelete()
    {
        foreach (var objController in  GetComponentsInChildren<ObjectController>())
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

        var objControllers = GetComponentsInChildren<ObjectController>();
        foreach (var objController in objControllers.Where(objController => objController.deleted))
        {
            DeleteObject(objController);
        }
    }
    
    private IEnumerator UpdateObjects()
    {
        List<Obj> objs = null;
        yield return StartCoroutine(apiClient.GetRoomObjects(RoomSelector.SelectedRoom.Id, res => objs = res,
            err => Debug.Log(err)));
        if (objs == null) yield break;

        var objControllers = GetComponentsInChildren<ObjectController>();
        var objMap = objControllers.ToDictionary(objController => objController.obj.Id, objController => objController);

        var coroutines = new List<Coroutine>();
        foreach (var obj in objs)
        {
            if (objMap.TryGetValue(obj.Id, out var objController))
            {
                objController.obj = obj;
                objController.UpdateInstance();
            }
            else
            {
                var coroutine = StartCoroutine(AddObjectController(obj).LoadObject(obj));
                coroutines.Add(coroutine);
            }
        }
        
        var toDelete = objMap.Keys.Except(objs.Select(obj => obj.Id)).ToList();
        foreach (var id in toDelete)
        {
            Destroy(objMap[id].gameObject);
        }
        

        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }
    }

    public IEnumerator PollObjectsUpdate()
    {
        while (true)
        {
            yield return StartCoroutine(UpdateObjects());
            yield return new WaitForSeconds(pollInterval);
        }
    }
}
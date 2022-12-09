using Dummiesman;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Api.Models;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
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
        yield return StartCoroutine(apiClient.GetRoomObjects(RoomSelector.SelectedRoom.Id, res => objs = res,
            err => Debug.Log(err)));
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

        var child = _loader.Load(modelPath);
        var parent = new GameObject(obj.Model.Name);
        parent.transform.position = obj.Translation;
        parent.transform.rotation = obj.Rotation;
        parent.transform.localScale = obj.Scale;
        child.transform.SetParent(parent.transform, false);

        ProcessObject(parent);
        if (ph != null)
        {
            Destroy(ph);
        }
    }

    void ProcessObject(GameObject obj)
    {
        var meshes = obj.GetComponentsInChildren<MeshFilter>();
        var combine = meshes.Select(mesh => new CombineInstance()
            { mesh = mesh.sharedMesh, transform = obj.transform.worldToLocalMatrix * mesh.transform.localToWorldMatrix }).ToArray();
        var mesh = new Mesh();
        mesh.CombineMeshes(combine);
        var collider = obj.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        collider.convex = true;
        var rigidbody = obj.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        var grabbable = obj.AddComponent<Grabbable>();
        var handGrabInteractable = obj.AddComponent<HandGrabInteractable>();
        handGrabInteractable.InjectOptionalPointableElement(grabbable);
        handGrabInteractable.InjectRigidbody(rigidbody);
        handGrabInteractable.HandAlignment = HandAlignType.None;
    }

    IEnumerator AddObject(Model model, Vector3 translation, Quaternion rotation, Vector3 Scale)
    {
        throw new NotImplementedException();
    }
}
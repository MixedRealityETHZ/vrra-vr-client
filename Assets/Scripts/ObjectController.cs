using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.Api.Models;
using Dummiesman;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public Obj obj;
    public ApiClient apiClient;
    public GameObject placeholderPrefab;

    private static readonly OBJLoader _loader = new();
    private GameObject _placeholder;
    private GameObject _instance;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public IEnumerator LoadObject()
    {
        var trans = transform;
        trans.position = obj.Translation;
        trans.rotation = obj.Rotation;
        trans.localScale = obj.Scale;

        if (obj.Model.Bounds != null)
        {
            _placeholder = Instantiate(placeholderPrefab, trans);
            _placeholder.name = "Placeholder";
        }

        string modelPath = null;
        yield return StartCoroutine(apiClient.DownloadModel(obj.Model, res => modelPath = res, err => Debug.Log(err)));
        if (modelPath == null) yield break;

        _instance = _loader.Load(modelPath);
        _instance.transform.SetParent(trans, false);

        if (_placeholder != null)
        {
            Destroy(_placeholder);
            _placeholder = null;
        }

        EnableInteraction();
    }

    public void EnableInteraction()
    {
        var meshes = _instance.GetComponentsInChildren<MeshFilter>();
        var combine = meshes.Select(mesh => new CombineInstance()
        {
            mesh = mesh.sharedMesh,
            transform = _instance.transform.worldToLocalMatrix * mesh.transform.localToWorldMatrix
        }).ToArray();
        var mesh = new Mesh();
        mesh.CombineMeshes(combine);
        var collider = _instance.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        collider.convex = true;
        var rigidbody = _instance.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        var grabbable = _instance.AddComponent<Grabbable>();
        var handGrabInteractable = _instance.AddComponent<HandGrabInteractable>();
        handGrabInteractable.InjectOptionalPointableElement(grabbable);
        handGrabInteractable.InjectRigidbody(rigidbody);
        handGrabInteractable.HandAlignment = HandAlignType.None;
        grabbable.WhenPointerEventRaised += e =>
        {
            switch (e.Type)
            {
                case PointerEventType.Hover:
                    break;
                case PointerEventType.Unhover:
                    break;
                case PointerEventType.Select:
                    Instantiate(_instance, transform);
                    break;
                case PointerEventType.Unselect:
                    break;
                case PointerEventType.Move:
                    break;
                case PointerEventType.Cancel:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };
    }
}
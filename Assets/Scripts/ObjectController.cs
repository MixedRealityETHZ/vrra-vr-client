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
    private Mesh _mesh;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public IEnumerator LoadObject(Obj o)
    {
        obj = o;
        _instance = new GameObject("Object")
        {
            transform =
            {
                position = obj.Translation,
                rotation = obj.Rotation,
                localScale = obj.Scale
            }
        };

        var trans = transform;
        _instance.transform.parent = trans;
        _placeholder = Instantiate(_instance, trans);
        _placeholder.name = "Placeholder";

        if (obj.Model.Bounds != null)
        {
            var ph = Instantiate(placeholderPrefab, _placeholder.transform);
            _placeholder.transform.localScale = obj.Model.Bounds.Value;
        }
        else
        {
            _placeholder.SetActive(false);
        }

        string modelPath = null;
        yield return StartCoroutine(apiClient.DownloadModel(obj.Model, res => modelPath = res, err => Debug.Log(err)));
        if (modelPath == null) yield break;

        var model = _loader.Load(modelPath);
        model.transform.SetParent(_instance.transform, false);

        if (obj.Movable) EnableInteraction();
        _placeholder.SetActive(false);
    }

    public void EnableInteraction()
    {
        var meshes = _instance.GetComponentsInChildren<MeshFilter>();
        var combine = meshes.Select(mesh => new CombineInstance()
        {
            mesh = mesh.sharedMesh,
            transform = _instance.transform.worldToLocalMatrix * mesh.transform.localToWorldMatrix
        }).ToArray();
        _mesh = new Mesh();
        _mesh.CombineMeshes(combine);
        _placeholder.GetComponentInChildren<MeshFilter>().mesh = _mesh;
        _placeholder.transform.localScale = Vector3.one;

        var collider = _instance.AddComponent<MeshCollider>();
        collider.sharedMesh = _mesh;
        collider.convex = true;
        var rigidbody = _instance.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        var grabbable = _instance.AddComponent<Grabbable>();
        var handGrabInteractable = _instance.AddComponent<HandGrabInteractable>();
        handGrabInteractable.InjectOptionalPointableElement(grabbable);
        handGrabInteractable.InjectRigidbody(rigidbody);
        handGrabInteractable.HandAlignment = HandAlignType.None;
        grabbable.WhenPointerEventRaised += HandlePointerEvent;
    }

    public void Freeze()
    {
        _placeholder.SetActive(true);
        _instance.SetActive(false);
    }

    public void Unfreeze()
    {
        _placeholder.SetActive(false);
        _instance.SetActive(true);
    }

    private void UpdateObj(Action accept)
    {
    }

    private void HandlePointerEvent(PointerEvent e)
    {
        switch (e.Type)
        {
            case PointerEventType.Hover:
                break;
            case PointerEventType.Unhover:
                break;
            case PointerEventType.Select:
                _placeholder.transform.position = _instance.transform.position;
                _placeholder.transform.rotation = _instance.transform.rotation;
                _placeholder.transform.localScale = _instance.transform.localScale;
                _placeholder.SetActive(true);
                break;
            case PointerEventType.Unselect:
                var body = new AddObjBody()
                {
                    ModelId = obj.Model.Id,
                    Movable = obj.Movable,
                    Rotation = _instance.transform.rotation,
                    Scale = _instance.transform.localScale,
                    Translation = _instance.transform.position
                };
                StartCoroutine(apiClient.UpdateObject(RoomSelector.SelectedRoom.Id, obj.Id, body,
                    () => { _placeholder.SetActive(false); }, err => Debug.Log(err)));
                break;
            case PointerEventType.Move:
                break;
            case PointerEventType.Cancel:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
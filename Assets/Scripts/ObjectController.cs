using System;
using System.Collections;
using System.Linq;
using AsImpL;
using Assets.Scripts.Api.Models;
using Oculus.Interaction;
using Oculus.Interaction.DistanceReticles;
using Oculus.Interaction.Grab;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public Obj obj;
    public ApiClient apiClient;
    public GameObject placeholderPrefab;
    public Material placeholderMaterial;

    private bool _deleteMode = false;
    public bool deleted = false;

    private GameObject _placeholder;
    private GameObject _prevState;
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

        _prevState = Instantiate(_instance, trans);
        _prevState.name = "Previous State";
        _prevState.SetActive(false);

        if (obj.Model.Bounds != null)
        {
            _placeholder = Instantiate(_instance, trans);
            _placeholder.name = "Placeholder";
            var ph = Instantiate(placeholderPrefab, _placeholder.transform);
            ph.transform.localPosition = obj.Model.Bounds.Center;
            ph.transform.localScale = obj.Model.Bounds.Size;
        }

        string modelPath = null;
        yield return StartCoroutine(apiClient.DownloadModel(obj.Model, res => modelPath = res, err => Debug.Log(err)));
        if (modelPath == null) yield break;

        var options = new ImportOptions()
        {
            zUp = false,
            convertToDoubleSided = true,
            reuseLoaded = true
        };
        if (obj.Movable)
        {
            options.buildColliders = true;
            options.colliderConvex = false;
            options.colliderTrigger = true;
        }

        var importer = gameObject.AddComponent<ObjectImporter>();
        yield return importer.ImportModelAsync(o.Model.Name, modelPath, _instance.transform, options);

        if (obj.Movable) EnableInteraction();
        if (_placeholder != null) Destroy(_placeholder);
    }

    public void EnableInteraction()
    {
        _mesh = _instance.GetComponentInChildren<MeshFilter>().mesh;
        var prevStateVis = new GameObject("Visual");
        prevStateVis.transform.SetParent(_prevState.transform, false);
        var meshFilter = prevStateVis.AddComponent<MeshFilter>();
        meshFilter.mesh = _mesh;
        var meshRenderer = prevStateVis.AddComponent<MeshRenderer>();
        meshRenderer.materials = Enumerable.Range(0, _mesh.subMeshCount).Select(i => placeholderMaterial).ToArray();

        var rigidbody = _instance.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        var oneGrabTrans = _instance.AddComponent<OneGrabFreeTransformer>();
        var twoGrabTrans = _instance.AddComponent<TwoGrabFreeTransformer>();
        twoGrabTrans.Constraints = new TwoGrabFreeTransformer.TwoGrabFreeConstraints()
        {
            MinScale = new FloatConstraint(),
            MaxScale = new FloatConstraint(),
        };

        var grabbable = _instance.AddComponent<Grabbable>();
        grabbable.InjectOptionalOneGrabTransformer(oneGrabTrans);
        grabbable.InjectOptionalTwoGrabTransformer(twoGrabTrans);

        var handGrabInteractable = _instance.AddComponent<HandGrabInteractable>();
        handGrabInteractable.InjectOptionalPointableElement(grabbable);
        handGrabInteractable.InjectRigidbody(rigidbody);
        handGrabInteractable.HandAlignment = HandAlignType.None;

        var disHandGrabInteractable = _instance.AddComponent<DistanceHandGrabInteractable>();
        disHandGrabInteractable.InjectOptionalPointableElement(grabbable);
        disHandGrabInteractable.InjectRigidbody(rigidbody);
        disHandGrabInteractable.HandAlignment = HandAlignType.None;
        disHandGrabInteractable.InjectSupportedGrabTypes(GrabTypeFlags.Pinch);
        disHandGrabInteractable.InjectPinchGrabRules(new GrabbingRule(
            HandFingerFlags.Thumb | HandFingerFlags.Index | HandFingerFlags.Middle, GrabbingRule.FullGrab));

        var moveProvider = _instance.AddComponent<MoveFromTargetProvider>();
        disHandGrabInteractable.InjectOptionalMovementProvider(moveProvider);

        var reticle = _instance.AddComponent<ReticleDataIcon>();
        reticle.InjectOptionalColliders(new Collider[] { GetComponent<Collider>() });
        grabbable.WhenPointerEventRaised += HandlePointerEvent;
    }

    public void Freeze()
    {
        _prevState.SetActive(true);
        _instance.SetActive(false);
    }

    public void Unfreeze()
    {
        _prevState.SetActive(false);
        _instance.SetActive(true);
    }

    public void EnableDeleteMode()
    {
        _deleteMode = true;
    }

    public void DisableDeleteMode()
    {
        _deleteMode = false;
        Unfreeze();
    }

    private void HandlePointerEvent(PointerEvent e)
    {
        switch (e.Type)
        {
            case PointerEventType.Hover:
                if (!_deleteMode) break;
                deleted = !deleted;
                if (deleted) Freeze();
                else Unfreeze();
                break;
            case PointerEventType.Unhover:
                break;
            case PointerEventType.Select:
                _prevState.transform.position = _instance.transform.position;
                _prevState.transform.rotation = _instance.transform.rotation;
                _prevState.transform.localScale = _instance.transform.localScale;
                _prevState.SetActive(true);
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
                    () =>
                    {
                        _prevState.transform.position = _instance.transform.position;
                        _prevState.transform.rotation = _instance.transform.rotation;
                        _prevState.transform.localScale = _instance.transform.localScale;
                        _prevState.SetActive(false);
                    }, err => Debug.Log(err)));
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
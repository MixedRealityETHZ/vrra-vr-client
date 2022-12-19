using Oculus.Interaction;
using Oculus.Interaction.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPose : MonoBehaviour
{
    [SerializeField] private ActiveStateSelector[] _poses;
    [SerializeField] private GameObject _menu;
    [SerializeField] private float _distance = .08f;
    [SerializeField] private float _offsetRight = .1f;
    [SerializeField] private float _offsetForward = -.12f;
    [SerializeField] private float _closeDelay = 1f;

    private HandRef _handAnchor = null;
    private Coroutine _coroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var pose in _poses)
        {
            pose.WhenSelected += () => Pose_WhenSelected(pose);
            pose.WhenUnselected += () => Pose_WhenUnselected();
        }
        _menu.SetActive(false);
    }

    private void Pose_WhenUnselected()
    {
        _coroutine = StartCoroutine(CloseMenu());
    }

    IEnumerator CloseMenu()
    {
        yield return new WaitForSeconds(_closeDelay);
        _menu.SetActive(false);
        _handAnchor = null;
        _coroutine = null;
    }

    private void Pose_WhenSelected(ActiveStateSelector pose)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        var hand = pose.GetComponent<HandRef>();
        _handAnchor = hand;
        _menu.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (_handAnchor == null) return;

        _handAnchor.GetRootPose(out Pose wristPose);
        var up = _handAnchor.Handedness == Handedness.Left ? wristPose.up : -wristPose.up;
        var right = _handAnchor.Handedness == Handedness.Left ? wristPose.right : -wristPose.right;
        var forward = _handAnchor.Handedness == Handedness.Left ? wristPose.forward : -wristPose.forward;
        
        _menu.transform.position = wristPose.position + _distance * up + _offsetForward * forward + _offsetRight * right;
        _menu.transform.rotation = Quaternion.LookRotation(-up);
    }
}

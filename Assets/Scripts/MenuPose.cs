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
    [SerializeField] private float _offset = .1f;

    private HandRef _handAnchor = null;

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
        _menu.SetActive(false);
        _handAnchor = null;
    }

    private void Pose_WhenSelected(ActiveStateSelector pose)
    {
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

        _menu.transform.position = wristPose.position + _distance * up + _offset * right;
        _menu.transform.rotation = Quaternion.LookRotation(-up);
    }
}

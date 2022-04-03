using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foot
{
    private Vector3 _Position = Vector3.zero;

    private bool _IsMoving = false;
    private float _MovementProgress = 0;
    private float _MovementSpeed = 0;
    private Vector3 _Target = Vector3.zero;
    private Vector3 _Origin = Vector3.zero;
    private AnimationCurve _Curve = null;
    private Transform _TargetTransform = null;

    public void SetPosition(Vector3 position) => _TargetTransform.position = position;
    public void SetTarget(Transform target)
    {
        _TargetTransform = target;
        _Position = target.position;
    }
    public void SetSpeed(float speed) => _MovementSpeed = speed;
    public Vector3 GetPosition() => _TargetTransform.position;
    public bool IsMoving() => _IsMoving;

    public void MoveTo(Vector3 destination, AnimationCurve curve, float speed)
    {
        _IsMoving = true;
        _MovementProgress = 0;
        _Target = destination;
        _Origin = _Position;
        _Curve = curve;
        _MovementSpeed = speed;
    }

    public void Update()
    {
        if (_IsMoving)
        {
            _MovementProgress = Mathf.Clamp01(_MovementProgress + _MovementSpeed * Time.deltaTime);
            if (1 - _MovementProgress < float.Epsilon) _IsMoving = false;

            _Position = Vector3.Lerp(_Origin, _Target, _MovementProgress) + Vector3.up * _Curve.Evaluate(_MovementProgress);
        }

        _TargetTransform.position = _Position;
    }

    public void DrawGizmo()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(_Position, .05f);

        if (_IsMoving)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_Target, .03f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_Origin, .03f);
        }
    }
}

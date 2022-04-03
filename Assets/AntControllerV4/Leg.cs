using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leg : MonoBehaviour
{
    [SerializeField] private Transform _Pivit = null;
    [SerializeField] private LayerMask _FloorMask = 0;
    [SerializeField] private float _RootAngle = 0;
    [SerializeField] private float _UpperAngle = 20;
    [SerializeField] private float _LowerAngle = 20;
    [SerializeField] private float _Radius = 1;
    [SerializeField, Min(2)] private int _RaySegments = 3;
    [SerializeField] private AnimationCurve _StepCurve = AnimationCurve.Constant(0, 1, 0);
    [SerializeField, Range(0f, 1f)] private float _AnticipateSpeed = .4f;
    [SerializeField, Range(0f, .2f)] private float _MaxIdealeDistance = .2f;
    [SerializeField, Range(0f, .2f)] private float _HeightAboveFloor = .2f;

    private Vector3[] _ArcPoints = new Vector3[0];

    private Vector3 _CurrentPosition = Vector3.zero;
    private Vector3 _TargetPosition = Vector3.zero;
    private Vector3 _IdealePosition = Vector3.zero;
    private Vector3 _IdealeNormal = Vector3.zero;

    private Vector3 _Origin = Vector3.zero;

    public Vector3 GetPosition() => _CurrentPosition;
    public Vector3 GetIdealePosition() => _IdealePosition;

    public void SetTarget(Vector3 target)
    {
        _Origin = _CurrentPosition;
        _TargetPosition = target;
    }

    public void Init()
    {
        _CurrentPosition = Quaternion.AngleAxis(_RootAngle, _Pivit.up) * _Pivit.forward * _Radius + _Pivit.position;
        _TargetPosition = _CurrentPosition;
        _Origin = _CurrentPosition;
    }

    public void UpdateIdeals()
    {
        UpdateArcPoints();
        UpdateIdealePositions();

        transform.position = _CurrentPosition;
    }

    public void UpdatePosition(float progress)
    {
        _CurrentPosition = Vector3.Lerp(_Origin, _TargetPosition, progress);
        transform.position = _CurrentPosition;
    }

    public void UpdateArcPoints()
    {
        if (_ArcPoints.Length != _RaySegments) _ArcPoints = new Vector3[_RaySegments];


        var delta = (float)(_UpperAngle + _LowerAngle) / (_RaySegments - 1);

        for (int i = 0; i < _RaySegments; i++)
        {
            _ArcPoints[i] = Quaternion.AngleAxis(_RootAngle, _Pivit.up) * Quaternion.AngleAxis(-_UpperAngle + delta * i, _Pivit.right) * _Pivit.forward * _Radius + _Pivit.position;
        }
    }

    public void UpdateIdealePositions()
    {
        for (int i = 1; i < _ArcPoints.Length; i++)
        {
            if (Physics.Linecast(_ArcPoints[i - 1], _ArcPoints[i], out var hit, _FloorMask))
            {
                _IdealePosition = hit.point + hit.normal * _HeightAboveFloor;
                _IdealeNormal = hit.normal;
                return;
            }
        }

        _IdealePosition = Quaternion.AngleAxis(_RootAngle, _Pivit.up) * _Pivit.forward * _Radius + _Pivit.position;
        _IdealeNormal = _Pivit.up;
    }

    private void OnDrawGizmosSelected()
    {
        if (_Pivit == null) return;

        UpdateArcPoints();
        UpdateIdealePositions();

        Gizmos.color = Color.white;
        //Gizmos.DrawLine(_Pivit.position, _ArcPoints.First());
        //Gizmos.DrawLine(_Pivit.position, _ArcPoints.Last());

        for (int i = 1; i < _ArcPoints.Length; i++)
        {
            Gizmos.DrawLine(_ArcPoints[i - 1], _ArcPoints[i]);
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_IdealePosition, .02f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_TargetPosition, .03f);
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(_CurrentPosition, .05f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leg : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Transform _Pivit = null;
    [SerializeField] private LayerMask _FloorMask = 0;

    [Header("Ideale Arc")]
    [SerializeField] private float _RootAngle = 0;
    [SerializeField] private float _UpperAngle = 20;
    [SerializeField] private float _LowerAngle = 20;
    [SerializeField] private float _Radius = 1;
    [SerializeField, Min(2)] private int _RaySegments = 3;

    [Header("Step")]
    [SerializeField, Range(0f, .2f)] private float _HeightAboveFloor = .2f;
    [SerializeField] private AnimationCurve _StepCurve = AnimationCurve.Constant(0, 1, 0);
    [SerializeField] private AnimationCurve _StepHeight = AnimationCurve.Constant(0, 1, 0);

    // Cached Parameters
    private Vector3 _Ideale = Vector3.zero;
    private Vector3 _Position = Vector3.zero;
    private Vector3 _Normal = Vector3.zero;
    private Vector3[] _ArcPoints = new Vector3[0];

    // Step Parametes
    private Vector3 _Target = Vector3.zero;
    private Vector3 _Tangent = Vector3.zero;
    private Vector3 _Origin = Vector3.zero;
    private float _StepDistance = 0;

    #region Public Getters

    public Vector3 Position => _Position;
    public Vector3 Ideale => _Ideale;
    public Vector3 Normal => _Normal;
    public Vector3[] ArcPoints => _ArcPoints;

    #endregion

    #region Update Cached

    private void UpdateArcPoints()
    {
        if (_ArcPoints.Length != _RaySegments) _ArcPoints = new Vector3[_RaySegments];


        var delta = (float)(_UpperAngle + _LowerAngle) / (_RaySegments - 1);

        for (int i = 0; i < _RaySegments; i++)
        {
            _ArcPoints[i] = Quaternion.AngleAxis(_RootAngle, _Pivit.up) * Quaternion.AngleAxis(-_UpperAngle + delta * i, _Pivit.right) * _Pivit.forward * _Radius + _Pivit.position;
        }
    }

    private void UpdateIdealePositions()
    {
        for (int i = 1; i < _ArcPoints.Length; i++)
        {
            if (Physics.Linecast(_ArcPoints[i - 1], _ArcPoints[i], out var hit, _FloorMask))
            {
                _Ideale = hit.point + hit.normal * _HeightAboveFloor;
                _Normal = hit.normal;
                return;
            }
        }

        _Ideale = Quaternion.AngleAxis(_RootAngle, _Pivit.up) * _Pivit.forward * _Radius + _Pivit.position;
        _Normal = _Pivit.up;
    }

    private void UpdatePosition(float progress)
    {
        if (progress <= 1)
        {
            _Position = Vector3.Lerp(_Origin, _Target, progress);
        }
    }

    #endregion

    #region Lifecycle Functions

    public void Init()
    {
        UpdateCache();

        UpdateLegPosition(0);

        _Position = _Ideale;
        _Target = _Position;
        _Origin = _Position;
    }

    public void UpdateCache()
    {
        UpdateArcPoints();
        UpdateIdealePositions();
    }

    public void UpdateLegPosition(float progress)
    {
        if (progress <= 1)
        {
            _Position = Vector3.Lerp(_Origin, _Target, progress);
            transform.position = _Position + _Tangent * _StepCurve.Evaluate(progress) * _StepDistance;
        } else
        {
            transform.position = _Position;
        }
    }

    public void SetTarget(Vector3 target, Vector3 tangent)
    {
        _Origin = _Position;
        _Target = target;
        _Tangent = tangent;
        _StepDistance = _StepHeight.Evaluate(Vector3.Distance(_Position, target));
    }

    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        if (_Pivit == null) return;

        UpdateArcPoints();
        UpdateIdealePositions();

        Gizmos.color = Color.white;

        for (int i = 1; i < _ArcPoints.Length; i++)
        {
            Gizmos.DrawLine(_ArcPoints[i - 1], _ArcPoints[i]);
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_Ideale, .02f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_Target, .03f);
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(_Position, .05f);
        }
    }

    #endregion
}

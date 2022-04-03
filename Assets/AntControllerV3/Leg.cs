using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField, Range(0f,1f)] private float _AnticipateSpeed = .4f;
    [SerializeField, Range(0f,.2f)] private float _MaxIdealeDistance = .2f;

    private Vector3 _Position = Vector3.zero;
    private Vector3 _StepPosition = Vector3.zero;


    private Vector3[] _ArcPoints = new Vector3[0];
    private Vector3 _TargetPosition = Vector3.zero;
    private Vector3 _StartPosition = Vector3.zero;
    private Vector3 _TargetPositionNormal = Vector3.zero;
    private Vector3 _IdealePosition = Vector3.zero;
    private Vector3 _IdealeNormal = Vector3.zero;
    private float _StepProgress = 1;
    private float _Speed = 4;
    private AntController.PositionAndRotation _SpeedPnR = new AntController.PositionAndRotation();

    public Vector3 GetPosition() => _Position;
    public Vector3 GetStepPosition() => _StepPosition;
    public Vector3 GetIdealePosition() => _IdealePosition;
    public Vector3 GetIdealeNormal() => _IdealeNormal;
    public bool IsMoving() => _StepProgress < 1;
    public bool IsOutsideSafeZone() => Vector3.Distance(_Position, _IdealePosition) > _MaxIdealeDistance;
    public bool IsAtIdeale() => _Position == _IdealePosition;
    public float GetDistanceFromIdeale() => Vector3.Distance(_Position, _IdealePosition);

    public void SetPosition(Vector3 position, Vector3 normal)
    {
        _StepProgress = 0;
        _StartPosition = transform.position;
        _TargetPosition = position;
        _TargetPositionNormal = normal;
    }
    public void SetSpeed(float speed) => _Speed = speed;
    public void SetPnR(AntController.PositionAndRotation pnr) => _SpeedPnR = pnr;

    public void Start()
    {
        UpdateLeg();

        _Position = _IdealePosition;
    }

    private void Update()
    {
        UpdateLeg();

        if (IsMoving())
        {
            _StepProgress = Mathf.Clamp01(_StepProgress + _Speed * Time.deltaTime);
            _Position = Vector3.Lerp(_StartPosition, _TargetPosition, _StepProgress);
            _StepPosition = _Position + _TargetPositionNormal * _StepCurve.Evaluate(_StepProgress);
            transform.position = _StepPosition;
        } else
        {
            transform.position = _Position;
        }
    }

    public void UpdateLeg()
    {
        UpdateArcPoints();
        UpdateIdealePositions();
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
            var start = MathUtils.PivotAround(_Pivit.position, _Pivit.up, _ArcPoints[i - 1], _SpeedPnR.Rotation.y * _AnticipateSpeed) + _Pivit.TransformDirection(_SpeedPnR.Position * _AnticipateSpeed);
            var end = MathUtils.PivotAround(_Pivit.position, _Pivit.up, _ArcPoints[i], _SpeedPnR.Rotation.y * _AnticipateSpeed) + _Pivit.TransformDirection(_SpeedPnR.Position * _AnticipateSpeed);

            if (Physics.Linecast(start, end, out var hit, _FloorMask))
            {
                _IdealePosition = hit.point;
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

        UpdateLeg();
       

        Gizmos.color = Color.white;
        //Gizmos.DrawLine(_Pivit.position, _ArcPoints.First());
        //Gizmos.DrawLine(_Pivit.position, _ArcPoints.Last());

        for (int i = 1; i < _ArcPoints.Length; i++)
        {
            Gizmos.DrawLine(_ArcPoints[i - 1], _ArcPoints[i]);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GetIdealePosition(), .02f);
    }
}

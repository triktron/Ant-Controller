using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : MonoBehaviour
{
    [Header("Legs")]
    [SerializeField] private Transform _LegsPivit = null;
    [SerializeField] private LegGroup _LegGroupA = new LegGroup();
    [SerializeField] private LegGroup _LegGroupB = new LegGroup();

    [Header("Body")]
    [SerializeField, Range(0.01f, 2f)] private float _BodyPositionSmoothTime = 1;
    [SerializeField, Range(0.01f, 2f)] private float _BodyRotationSmoothTime = 1;
    [SerializeField, Range(0f, 2f)] private float _BodyHeight = 1;
    [SerializeField, Min(2)] private int _BodyArcSegments = 5;
    [SerializeField, Range(0.01f, 180f)] private float _BodyArcUpperAngle = 1;
    [SerializeField, Range(0.01f, 180f)] private float _BodyArcLowerAngle = 1;
    [SerializeField] private LayerMask _FloorMask = 0;

    [Header("Movement")]
    [SerializeField, Range(0.01f, 2f)] private float _WalkSpeed = 1;
    [SerializeField, Range(0.01f, 20f)] private float _RunSpeed = 1;
    [SerializeField, Range(0.01f, 90f)] private float _RotationSpeed = 1;
    [SerializeField, Range(0.01f, 8f)] private float _CycleSpeed = 1;
    [SerializeField, Range(0f, 8f)] private float _CycleSpeedDistanceMultiplier = 1;

    private float _Cycle = 1;
    private bool _GroupATurn = true;
    private ControllerInputStruct _Input = ControllerInputStruct.Empty;

    private Vector3 _Center = Vector3.zero;
    private Vector3 _Forward = Vector3.forward;
    private Vector3 _Up = Vector3.up;
    private float _IdealeDistance = 0;

    private Vector3 _BodyPositionVelocity = Vector3.zero;
    private Vector3 _BodyRotationVelocity = Vector3.zero;

    private Vector3[] _ArcPoints = new Vector3[0];

    private Rigidbody _Rb;
    private Collider _Collider;

    #region Lifecycle Functions

    private void Start()
    {
        _LegGroupA.Init();
        _LegGroupB.Init();

        _Rb = GetComponent<Rigidbody>();
        _Collider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        UpdateLegPivit();

        _Cycle += Time.deltaTime * (_CycleSpeed + _CycleSpeedDistanceMultiplier * _IdealeDistance + _RunSpeed * _Input.Run);
        _LegGroupA.UpdateCache(_Cycle);
        _LegGroupB.UpdateCache(_Cycle);

        _Forward = ((_LegGroupA.Forward + _LegGroupB.Forward) * .5f).normalized;
        _Center = (_LegGroupA.Center + _LegGroupB.Center) * .5f;
        _IdealeDistance = (_LegGroupA.IdealeDistance + _LegGroupB.IdealeDistance) * .5f;
        _Up = Vector3.Cross(_Forward, _LegGroupB.Center - _LegGroupA.Center).normalized;
        if (Vector3.Angle(_Up, transform.up) > 90) _Up = -_Up;

        LegGroup currentTurn = _GroupATurn ? _LegGroupA : _LegGroupB;
        LegGroup nextTurn = _GroupATurn ? _LegGroupB : _LegGroupA;

        if (!Utils.IsInputStructZero(_Input) && !currentTurn.IsMoving() && !nextTurn.IsMoving() && currentTurn.WantsToMove())
        {
            currentTurn.StartMoving();
            _Cycle = 0;
            _GroupATurn = !_GroupATurn;
        }

        var bodyTarget = _Center;

        if (Physics.Raycast(transform.position + transform.up, -transform.up, out var hit, _BodyHeight*2+1, _FloorMask))
        {
            bodyTarget += -transform.up * (hit.distance - _BodyHeight - 1);
        }

        transform.position = Vector3.SmoothDamp(transform.position, bodyTarget, ref _BodyPositionVelocity, _BodyPositionSmoothTime);
        transform.rotation = Utils.SmoothDamp(transform.rotation, Quaternion.LookRotation(_Forward, _Up), ref _BodyRotationVelocity, _BodyRotationSmoothTime);
    }

    private void UpdateLegPivit()
    {
        UpdateArcPoints();

        for (int i = 1; i < _ArcPoints.Length; i++)
        {
            if (Physics.Linecast(_ArcPoints[i - 1], _ArcPoints[i], out var hit, _FloorMask))
            {
                var forward = Vector3.Cross(transform.right, hit.normal);

                _LegsPivit.position = hit.point;
                _LegsPivit.rotation = Quaternion.AngleAxis(_Input.Rotate * _RotationSpeed, hit.normal) * Quaternion.LookRotation(forward, hit.normal);
                
                return;
            }
        }

        _LegsPivit.position = transform.position + GetWorldInput();
        _LegsPivit.localRotation = Quaternion.Euler(0, _Input.Rotate * _RotationSpeed, 0);
    }

    public void SetInput(ControllerInputStruct input)
    {
        _Input = input;
    }

    #endregion

    #region Utils

    private Vector3 GetWorldInput() => transform.TransformDirection(new Vector3(_Input.Right, 0, _Input.Forward) * _WalkSpeed); // Mathf.Lerp(_WalkSpeed, _RunSpeed, _Input.Run)

    private void UpdateArcPoints()
    {
        if (_ArcPoints.Length != _BodyArcSegments) _ArcPoints = new Vector3[_BodyArcSegments];

        var radius = _WalkSpeed;// Mathf.Lerp(_WalkSpeed, _RunSpeed, _Input.Run)
        var delta = (float)(_BodyArcUpperAngle + _BodyArcLowerAngle) / (_BodyArcSegments - 1);
        var deltaHeight = 1f / (_BodyArcSegments - 1);
        var inputAngle = Mathf.Atan2(_Input.Right, _Input.Forward) * Mathf.Rad2Deg;
        var inputForce = new Vector2(_Input.Right, _Input.Forward).sqrMagnitude;

        for (int i = 0; i < _BodyArcSegments; i++)
        {
            var arcPoint = Quaternion.AngleAxis(inputAngle, transform.up) * Quaternion.AngleAxis(-_BodyArcUpperAngle + delta * i, transform.right) * transform.forward * radius + transform.position;
            var straigtPoint = -transform.up * i * deltaHeight * 2 + transform.up + transform.position;
            _ArcPoints[i] = Vector3.Lerp(straigtPoint, arcPoint, inputForce);
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            _LegGroupA.DrawGizmos();
            _LegGroupB.DrawGizmos();

            Utils.DrawArrow(_Center, _Forward, Color.red);
            Utils.DrawArrow(_Center, _Up, Color.blue);

            Gizmos.color = Color.magenta;
            for (int i = 1; i < _ArcPoints.Length; i++)
            {
                //Utils.DrawArrow(_ArcPoints[i - 1], _ArcPoints[i] - _ArcPoints[i - 1], Color.magenta, .15f);
            }
        }

        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, -transform.up * _BodyHeight);
    }

    #endregion
}

using MathUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : MonoBehaviour
{
    [SerializeField] private Transform _LegsPivit = null;
    [SerializeField] private LegGroup[] _LegGroups = new LegGroup[0];
    [SerializeField, Range(0.01f, 2f)] private float _BodySmoothTime = 1;
    [SerializeField, Range(0.01f, 2f)] private float _BodyRotationSmoothTime = 1;
    [SerializeField, Range(0f, 2f)] private float _BodyHeight = 1;
    [SerializeField, Range(0.01f, 2f)] private float _MoveDistance = 1;
    [SerializeField, Range(0.01f, 2f)] private float _RunSpeed = 1;
    [SerializeField, Range(0.01f, 90f)] private float _RotationDistance = 1;
    [SerializeField, Range(0.01f, 4f)] private float _CycleSpeed = 1;
    [SerializeField, Range(0f, 4f)] private float _CycleSpeedDistanceMultiplier = 1;

    private ControllerInputStruct _Input = ControllerInputStruct.Empty;

    private float _Cycle = 0;
    private Vector3 _BodyVelocity = Vector3.zero;
    private Quaternion _RotationVelocity = Quaternion.identity;


    private int _CurrentGroupIndex = 0;

    private int _NextGroupIndex => _CurrentGroupIndex == _LegGroups.Length - 1 ? 0 : _CurrentGroupIndex + 1;
    private int _PreviusGroupIndex => _CurrentGroupIndex == 0 ? _LegGroups.Length - 1 : _CurrentGroupIndex - 1;

    private void Start()
    {
        foreach (var group in _LegGroups)
        {
            group.Init();
        }
    }

    private void Update()
    {
        Vector3 targetPosition = GetGroupCenter();

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _BodyVelocity, _BodySmoothTime);


        Vector3 forward = GetAverageForward();
        Vector3 right = GetAverageRight();
        Vector3 up = Vector3.Cross(forward, right);

        transform.rotation = QuaternionUtil.SmoothDamp(transform.rotation, Quaternion.LookRotation(forward, up), ref _RotationVelocity, _BodyRotationSmoothTime);
        //transform.rotation = Quaternion.LookRotation(forward, up);

        _LegsPivit.position = targetPosition + GetWorldInput() + transform.up * _BodyHeight;
        _LegsPivit.localRotation = Quaternion.Euler(0, _Input.Rotate * _RotationDistance, 0);


        _Cycle += Time.deltaTime * (_CycleSpeed + _CycleSpeedDistanceMultiplier * AverageDistanceFromIdlea());

        foreach (var legGroup in _LegGroups)
        {
            legGroup.UpdateIdeals();
        }

        GetCurrentGroup().UpdatePosition(_Cycle);

        if (GetCurrentGroup().CanLeaveState() && GetNextGroup().CanEnterState())
        {
            GetCurrentGroup().LeaveState();
            GetNextGroup().EnterState();
            _CurrentGroupIndex = _NextGroupIndex;
            _Cycle = 0;
            GetCurrentGroup().UpdatePosition(_Cycle);
        }
    }

    private LegGroup GetCurrentGroup() => _LegGroups[_CurrentGroupIndex];
    private LegGroup GetNextGroup() => _LegGroups[_NextGroupIndex];

    public void SetInput(ControllerInputStruct input)
    {
        _Input = input;
    }

    private bool HasMovingLeg()
    {
        foreach (var legGroup in _LegGroups)
        {
            if (legGroup.IsMoving()) return true;
        }
        return false;
    }

    private Vector3 GetGroupCenter()
    {
        Vector3 sum = new Vector3();
        foreach (var legGroup in _LegGroups)
        {
            sum += legGroup.GetGroupCenter();
        }
        return sum.Devide(_LegGroups.Length);
    }

    public float AverageDistanceFromIdlea()
    {
        float sum = 0;
        foreach (var legGroup in _LegGroups)
        {
            sum += legGroup.AverageDistanceFromIdlea();
        }
        return sum / _LegGroups.Length;
    }

    public Vector3 GetAverageForward()
    {
        Vector3 sum = Vector3.zero;
        foreach (var legGroup in _LegGroups)
        {
            sum += legGroup.GetAverageForward();
        }
        return sum.Devide(_LegGroups.Length).normalized;
    }

    public Vector3 GetAverageRight()
    {
        Vector3 right = _LegGroups[0].GetAverageForward() - _LegGroups[1].GetAverageForward();

        return right;
    }

    private Vector3 GetWorldInput() => transform.TransformDirection(new Vector3(_Input.Right, 0, _Input.Forward) * Mathf.Lerp(_MoveDistance, _RunSpeed, _Input.Run));

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, -GetWorldInput());
    }
}

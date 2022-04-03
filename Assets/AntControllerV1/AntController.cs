using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : MonoBehaviour
{
    [SerializeField] private List<LegState> _LegStates = new List<LegState>();
    [SerializeField] private float _SpeedDampTime = .5f;
    [SerializeField] private float _LegSpeed = .5f;


    private Vector3 _Speed = Vector3.zero;
    private Vector3 _Velocety = Vector3.zero;
    private Vector3 _TargetSpeed = Vector3.zero;

    private Vector3 _RotationSpeed = Vector3.zero;
    private Vector3 _RotationVelocety = Vector3.zero;
    private Vector3 _TargetRotationSpeed = Vector3.zero;

    public void SetSpeed(Vector3 speed) => _TargetSpeed = speed;
    public void SetRotationSpeed(Vector3 speed) => _TargetRotationSpeed = speed;


    private void Start()
    {
        foreach (var state in _LegStates)
        {
            state.Init(transform);
        }
    }

    private void Update()
    {
        UpdateSpeed();
        UpdateLegs();
    }

    private void UpdateLegs()
    {
        float distanceFromIdeale = 0;
        foreach (var state in _LegStates)
        {
            distanceFromIdeale += state.GetDistanceSumFromIdeale();
        }

        foreach (var state in _LegStates)
        {
            state.Update(_TargetSpeed, distanceFromIdeale * _LegSpeed);
        }

        foreach (var state in _LegStates)
        {
            if (state.HasMovingLegs()) return;
        }


        LegState legOutsideSafeZone = null;
        int highestPriority = int.MinValue;
        foreach (var state in _LegStates)
        {
            if (state.HasLegOutsideSafeZone() && state.GetPriority() > highestPriority)
            {
                highestPriority = state.GetPriority();
                legOutsideSafeZone = state;
            }
        }

        if (legOutsideSafeZone != null)
        {
            legOutsideSafeZone.SetPriority(legOutsideSafeZone.GetPriority() - 1);
            legOutsideSafeZone.MoveLegsToIdealePosition();
        }
    }

    private void UpdateSpeed()
    {
        _Speed = Vector3.SmoothDamp(_Speed, _TargetSpeed, ref _Velocety, _SpeedDampTime);
        _RotationSpeed = Vector3.SmoothDamp(_RotationSpeed, _TargetRotationSpeed, ref _RotationVelocety, _SpeedDampTime);
        transform.Translate(_Speed * Time.deltaTime, Space.Self);
        transform.Rotate(_RotationSpeed * Time.deltaTime, Space.Self);
    }

    private void OnDrawGizmosSelected()
    {
        foreach(var state in _LegStates)
        {
            state.DrawGizmp(transform);
        }
    }
}

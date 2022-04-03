using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : MonoBehaviour
{
    public struct PositionAndRotation
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }


    [SerializeField] private LegGroup[] LegGroups = new LegGroup[0];
    [SerializeField] private float _SpeedDampTime = .5f;
    [SerializeField] private float _MinLegSpeed = .5f;
    [SerializeField] private float _LegSpeed = .5f;
    [SerializeField] private float _IdleTime = 2f;
    [SerializeField] private LayerMask _Floor = 0;
    [SerializeField] private float _FloorHeight = 0;


    [Header("Breathing")]
    [Range(0.01f, 20)]
    public float breathePeriod;
    [Range(0, 1)]
    public float breatheMagnitude;

    private PositionAndRotation _CurrentPnR = new PositionAndRotation();
    private PositionAndRotation _TargetPnR = new PositionAndRotation();
    private PositionAndRotation _VelocetyPnR = new PositionAndRotation();

    public void SetPNR(PositionAndRotation pnr) => _TargetPnR = pnr;

    private void Update()
    {
        UpdateSpeed();
        UpdateLegs();

        var normal = GetTargetNormal();

        //transform.rotation = Quaternion.LookRotation(transform.forward, normal);
        var slopeRotationMoving = Quaternion.FromToRotation(transform.up, normal);
        transform.rotation = Quaternion.Slerp(transform.rotation, slopeRotationMoving * transform.rotation, 10 * Time.deltaTime);

        Debug.DrawRay(transform.position + transform.up * .2f, -transform.up);
        if (Physics.Raycast(transform.position + transform.up*.2f, -transform.up, out var hit, 1, _Floor))
        {
            float t = (Time.time * 2 * Mathf.PI / breathePeriod) % (2 * Mathf.PI);


            float distError = hit.distance - _FloorHeight + breatheMagnitude * (Mathf.Sin(t) + 1f);
            transform.position -= transform.up * distError;
        }
    }

    private void UpdateSpeed()
    {
        _CurrentPnR.Position = Vector3.SmoothDamp(_CurrentPnR.Position, _TargetPnR.Position, ref _VelocetyPnR.Position, _SpeedDampTime);
        _CurrentPnR.Rotation = Vector3.SmoothDamp(_CurrentPnR.Rotation, _TargetPnR.Rotation, ref _VelocetyPnR.Rotation, _SpeedDampTime);
        transform.Translate(_CurrentPnR.Position * Time.deltaTime, Space.Self);
        transform.Rotate(_CurrentPnR.Rotation * Time.deltaTime, Space.Self);
    }

    private void UpdateLegs()
    {
        float idealeDistance = 0;
        foreach (var group in LegGroups)
        {
            group.SetPnR(_CurrentPnR);
            group.UpdateGroup();
            idealeDistance += group.GetDistanceFromIdeale();
        }

        foreach (var group in LegGroups)
        {
            group.SetSpeed(Mathf.Max(_MinLegSpeed, _LegSpeed * idealeDistance));
        }

        foreach (var group in LegGroups)
        {
            if (group.HasMovingLegs()) return;
        }


        LegGroup legOutsideSafeZone = null;
        uint highestPriority = uint.MaxValue;
        foreach (var state in LegGroups)
        {
            if (state.HasLegOutsideSafeZone() && state.GetPriority() < highestPriority)
            {
                highestPriority = state.GetPriority();
                legOutsideSafeZone = state;
            }
        }

        if (legOutsideSafeZone != null)
        {
            legOutsideSafeZone.SetPriority(legOutsideSafeZone.GetPriority() + 1);
            legOutsideSafeZone.MoveToIdeale();
        } else
        {
            foreach (var state in LegGroups)
            {
                if (state.GetIdleTime() > _IdleTime)
                {
                    state.MoveToIdeale();
                    break;
                }
            }
        }
    }

    private Vector3 GetTargetNormal()
    {
        Vector3 sum = new Vector3();
        foreach (var group in LegGroups)
        {
            sum += group.GetFloorNormal();
        }

        return new Vector3(sum.x / LegGroups.Length, sum.y / LegGroups.Length, sum.z / LegGroups.Length);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (var group in LegGroups)
        {
            var center = group.GetCenter();
            var normal = group.GetFloorNormal();

            Gizmos.DrawRay(center, normal * .4f);
        }

        Gizmos.color = Color.magenta;

        Gizmos.DrawRay(transform.position, GetTargetNormal() * .4f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LegGroup
{
    [SerializeField] private Leg[] _Legs = new Leg[0];

    private uint _Priority = 0;
    private float _Idle = 0;
    

    public uint GetPriority() => _Priority;
    public float GetIdleTime() => _Idle;
    public void SetPriority(uint priority) => _Priority = priority;
    public void SetPnR(AntController.PositionAndRotation pnr)
    {
        foreach (var leg in _Legs) leg.SetPnR(pnr);
    }

    public bool HasMovingLegs() => _Legs[0].IsMoving();

    public bool HasLegOutsideSafeZone() => _Legs[0].IsOutsideSafeZone();
    public bool IsAtIdeale() => _Legs[0].IsAtIdeale();
    public float GetDistanceFromIdeale() => _Legs[0].GetDistanceFromIdeale();

    public void SetSpeed(float speed)
    {
        foreach (var leg in _Legs)
        {
            leg.SetSpeed(speed);
        }
    }
    public Vector3 GetFloorNormal()
    {
        var center = _Legs[1].GetIdealePosition();
        var a = _Legs[0].GetIdealePosition();
        var b = _Legs[2].GetIdealePosition();

        return Vector3.Cross(a - center, b - center);
    }

    public Vector3 GetCenter()
    {
        Vector3 sum = new Vector3();
        foreach (var leg in _Legs)
        {
            sum += leg.GetPosition();
        }

        return new Vector3(sum.x / _Legs.Length, sum.y / _Legs.Length, sum.z / _Legs.Length);
    }

    public void UpdateGroup()
    {
        if (HasMovingLegs())
        {
            _Idle = 0;
        } else if (IsAtIdeale())
        {
            _Idle = -1;
        } else
        {
            _Idle += Time.deltaTime;
        }
    }

    public void MoveToIdeale()
    {
        foreach (var leg in _Legs)
        {
            leg.SetPosition(leg.GetIdealePosition(), leg.GetIdealeNormal());
        }
    }
}

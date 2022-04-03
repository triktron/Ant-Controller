using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LegState
{
    [SerializeField] private List<Leg> _Legs = new List<Leg>();

    private int _Priority = 0;

    public int GetPriority() => _Priority;
    public void SetPriority(int priority) => _Priority = priority;

    public void Init(Transform parrent)
    {
        foreach (var leg in _Legs)
        {
            leg.Init(parrent);
        }
    }

    public void Update(Vector3 parrentSpeed, float legSpeed)
    {
        foreach (var leg in _Legs)
        {
            leg.Update(parrentSpeed, legSpeed);
        }
    }

    public bool HasLegOutsideSafeZone()
    {
        foreach (var leg in _Legs)
        {
            if (!leg.IsInSafeZone()) return true;
        }
        return false;
    }

    public bool HasMovingLegs()
    {
        foreach (var leg in _Legs)
        {
            if (leg.IsMoving()) return true;
        }
        return false;
    }

    public void MoveLegsToIdealePosition()
    {
        foreach (var leg in _Legs)
        {
            leg.MoveToIdeale();
        }
    }

    public float GetDistanceSumFromIdeale()
    {
        float distance = 0;
        foreach (var leg in _Legs)
        {
            distance += leg.GetDistanceFromIdeale();
        }
        return distance;
    }

    public void DrawGizmp(Transform parrent)
    {
        foreach (var leg in _Legs)
        {
            leg.DrawGizmo(parrent);
        }
    }
}

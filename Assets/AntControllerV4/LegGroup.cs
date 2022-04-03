using MathUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LegGroup
{
    [SerializeField] private Leg[] _Legs = new Leg[0];
    [SerializeField, Range(0f, 1f)] private float _MinimumIdealeDistance = .2f;

    private bool _IsMoving = false;
    private float _Progress = 0;

    public bool IsMoving() => _IsMoving;

    public bool CanEnterState() => AverageDistanceFromIdlea() > _MinimumIdealeDistance;
    public bool CanLeaveState() => _Progress >= 1;

    public void EnterState()
    {
        _IsMoving = true;
        foreach (var leg in _Legs)
        {
            leg.SetTarget(leg.GetIdealePosition());
        }
    }
    public void LeaveState()
    {
        _IsMoving = false;
    }


    public void Init()
    {
        foreach (var leg in _Legs)
        {
            leg.Init();
        }
    }

    public void UpdateIdeals()
    {
        foreach (var leg in _Legs)
        {
            leg.UpdateIdeals();
        }
    }

    public void UpdatePosition(float progress)
    {
        _Progress = progress;

        foreach (var leg in _Legs)
        {
            leg.UpdatePosition(progress);
        }
    }

    public Vector3 GetGroupCenter()
    {
        Vector3 sum = new Vector3();
        foreach (var leg in _Legs)
        {
            sum += leg.GetPosition();
        }
        return sum.Devide(_Legs.Length);
    }

    public float AverageDistanceFromIdlea()
    {
        float sum = 0;
        foreach (var leg in _Legs)
        {
            sum += Vector3.Distance(leg.GetPosition(), leg.GetIdealePosition());
        }
        return sum / _Legs.Length;
    }

    public Vector3 GetAverageForward()
    {
        return (_Legs[1].GetPosition() - _Legs[0].GetPosition()).normalized;
    }
}

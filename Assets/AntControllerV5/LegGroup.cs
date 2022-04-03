using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LegGroup
{
    [SerializeField] private Leg[] _Legs = new Leg[0];
    [SerializeField, Range(0f, 1f)] private float _MinimumIdealeDistance = .2f;

    private bool _Moving = false;
    private float _Progress = 1;
    private Vector3 _Center = Vector3.zero;
    private Vector3 _IdealeCenter = Vector3.zero;
    private float _AverageIdealeDistance = 0;
    private Vector3 _Forward = Vector3.forward;

    #region Public Getters

    public float Progress => _Progress;
    public Vector3 Center => _Center;
    public Vector3 IdealeCenter => _IdealeCenter;
    public float IdealeDistance => _AverageIdealeDistance;
    public Vector3 Forward => _Forward;

    #endregion

    #region Lifecycle Functions

    public void Init()
    {
        foreach (var leg in _Legs)
        {
            leg.Init();
        }
    }

    public void UpdateCache(float progress)
    {
        if (_Moving)
        {
            _Progress = progress;
            if (progress >= 1) _Moving = false;
        }
        
        Vector3 positionSum = Vector3.zero;
        Vector3 idealePositionSum = Vector3.zero;
        float idealeDistanceSum = 0;

        foreach (var leg in _Legs)
        {
            leg.UpdateCache();
            leg.UpdateLegPosition(_Progress);

            positionSum += leg.Position;
            idealePositionSum += leg.Ideale;
            idealeDistanceSum += Vector3.Distance(leg.Position, leg.Ideale);
        }

        _Center = positionSum.Devide(_Legs.Length);
        _IdealeCenter = idealePositionSum.Devide(_Legs.Length);
        _AverageIdealeDistance = idealeDistanceSum / _Legs.Length;
        _Forward = (_Legs[1].Position - _Legs[0].Position).normalized;

        
    }

    #endregion

    #region State Handling

    public bool WantsToMove() => _AverageIdealeDistance > _MinimumIdealeDistance;
    public bool IsMoving() => _Moving;

    public void StartMoving()
    {
        _Moving = true;
        foreach (var leg in _Legs)
        {
            leg.SetTarget(leg.Ideale, leg.Normal);
        }
    }

    #endregion

    #region Debug

    public void DrawGizmos()
    {
        Utils.DrawArrow(_Center, _Forward * .4f, Color.red, .1f);
    }

    #endregion
}

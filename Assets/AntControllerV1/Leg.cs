using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Leg
{
    [SerializeField] private Transform _Target = null;
    [SerializeField] private LayerMask _FloorMask = 0;
    [SerializeField] private float _RootAngle = 0;
    [SerializeField] private float _UpperAngle = 20;
    [SerializeField] private float _LowerAngle = 20;
    [SerializeField] private float _Radius = 1;
    [SerializeField] private AnimationCurve _StepCurve = AnimationCurve.Constant(0,1,0);

    private Vector3 _ParrentSpeed = Vector3.zero;

    internal void Update(Vector3 parrentSpeed, float legSpeed)
    {
        _ParrentSpeed = parrentSpeed;
        _Foot.SetSpeed(legSpeed);
        _Foot.Update();
    }

    [SerializeField] private int _RaySegments = 3;

    private Transform _Parrent = null;
    private Foot _Foot = new Foot();

    public Transform GetTarget() => _Target;
    public bool IsMoving() => _Foot.IsMoving();

    public void Init(Transform parrent)
    {
        _Parrent = parrent;
        _Foot.SetTarget(_Target);
    }

    public Vector3 GetArcPoint(Transform parrent, int n)
    {
        float radius = Vector3.Distance(parrent.position, _Target.position);
        return parrent.position + Quaternion.Euler(Mathf.Lerp(-_UpperAngle, _LowerAngle, (float)n / _RaySegments), 0, 0) * parrent.forward * radius;
    }

    public IEnumerable<Vector3> GetAcrPointsLocal()
    {
        var segments = Mathf.Max(2, _RaySegments);

        var delta = (float)(_UpperAngle + _LowerAngle) / (segments-1);

        for (int i = 0; i < segments; i++)
        {
            yield return Quaternion.AngleAxis(_RootAngle, Vector3.up) * Quaternion.AngleAxis(-_UpperAngle + delta * i, Vector3.right) * Vector3.forward * _Radius;
        }
    }

    public IEnumerable<Vector3> GetAcrPoints()
    {
        return GetAcrPointsLocal().Select(p => _Parrent.TransformPoint(p));
    }

    public bool IsInSafeZone()
    {
        return Vector3.Distance(_Target.position, GetIdealePosition()) < .2f;
    }

    public float GetDistanceFromIdeale() => Vector3.Distance(GetIdealePosition(), _Foot.GetPosition());

    public Vector3 GetIdealePosition()
    {
        var arcPoints = GetAcrPoints().ToArray();

        for (int i = 1; i < arcPoints.Length; i++)
        {
            Vector3 origin = arcPoints[i - 1];
            Vector3 dir = arcPoints[i] - arcPoints[i - 1] + _Parrent.TransformDirection(_ParrentSpeed * .8f);
            float dist = Vector3.Magnitude(dir);

            if (Physics.Raycast(origin, dir, out var hit, dist, _FloorMask))
            {
                return hit.point;
            }
        }

        return _Parrent.TransformPoint(Quaternion.AngleAxis(_RootAngle, Vector3.up) * Vector3.forward * _Radius);
    }

    public void MoveToIdeale()
    {
        _Foot.MoveTo(GetIdealePosition(), _StepCurve, Mathf.Max(_ParrentSpeed.magnitude, 4));
    }

    public void DrawGizmo(Transform parrent)
    {
        if (_Target == null) return;

        _Parrent = parrent;

        var arcPoints = GetAcrPoints().ToArray();

        Gizmos.color = Color.white;
        Gizmos.DrawLine(parrent.position, arcPoints.First());
        Gizmos.DrawLine(parrent.position, arcPoints.Last());

        for (int i = 1; i < arcPoints.Length; i++)
        {
            Gizmos.DrawLine(arcPoints[i-1], arcPoints[i]);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GetIdealePosition(), .02f);

        _Foot.DrawGizmo();
    }
}

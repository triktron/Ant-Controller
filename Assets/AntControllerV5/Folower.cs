using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Folower : MonoBehaviour
{
    [SerializeField] private Transform _Target;
    [SerializeField] private AnimationCurve _PositionForce;
    [SerializeField] private AnimationCurve _RotationForce;

    private void Start()
    {
        transform.position = _Target.position;
        transform.rotation = _Target.rotation;
    }

    private void Update()
    {
        var distance = Vector3.Distance(transform.position, _Target.position);
        transform.position = Vector3.MoveTowards(transform.position, _Target.position, _PositionForce.Evaluate(distance) * Time.deltaTime);


        var angle = Quaternion.Angle(transform.rotation, _Target.rotation);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, _Target.rotation, _RotationForce.Evaluate(angle) * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, transform.forward);
        Gizmos.DrawSphere(transform.position, .3f);
    }
}

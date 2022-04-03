using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionGizmo : MonoBehaviour
{
    [SerializeField] private float _Size = 1;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * _Size);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * _Size);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * _Size);
    }
}

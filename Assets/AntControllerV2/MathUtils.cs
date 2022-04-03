using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtils : MonoBehaviour
{
    public static Vector3 PivotAround(Vector3 center, Vector3 axis, Vector3 point, float degrees) => Quaternion.AngleAxis(degrees, axis) * (point - center) + center;
}

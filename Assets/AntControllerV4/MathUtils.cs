using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MathUtils
{
    public static class Vector3Extension
    {
        public static Vector3 Devide(this Vector3 vector, float devider)
        {
            return new Vector3(vector.x / devider, vector.y / devider, vector.z / devider);
        }
    }
}
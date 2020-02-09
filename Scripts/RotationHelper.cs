using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RotationHelper
{
    public static float ClampToCircle(this float angle)
    {
        while (angle < 0)
        {
            angle += 360;
        }

        if (angle > 360)
        {
            angle = angle % 360;
        }

        return angle;
    }

    public static Vector3 ClampToCircle (this Vector3 euler)
    {
        euler.x = euler.x.ClampToCircle();
        euler.y = euler.y.ClampToCircle();
        euler.z = euler.z.ClampToCircle();

        return euler;
    }
}

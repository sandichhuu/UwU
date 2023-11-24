using System.Runtime.InteropServices;
using UnityEngine;

public class MathUtil
{
    // Evil floating point bit level hacking.
    [StructLayout(LayoutKind.Explicit)]
    private struct FloatIntUnion
    {
        [FieldOffset(0)]
        public float f;

        [FieldOffset(0)]
        public int tmp;
    }

    /// <summary>
    /// Rotates a point around a pivot
    /// </summary>
    /// <param name="point">Start position</param>
    /// <param name="pivot">Pivot position</param>
    /// <param name="rotation">Desired rotation around the pivot</param>
    /// <returns>The position after being rotated around the pivot</returns>
    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 dir = point - pivot;
        dir = rotation * dir;
        point = dir + pivot;

        return point;
    }

    /// <summary>
    /// Sample a parabola trajectory
    /// </summary>
    /// <param name="start">Start position</param>
    /// <param name="end">End position</param>
    /// <param name="height">Height of the parabola</param>
    /// <param name="t">Time parameter to sample</param>
    /// <returns>The position in the parabola at time t</returns>
    public static Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t)
    {
        if (Mathf.Abs(start.y - end.y) < 0.1f)
        {
            // Start and end are roughly level, pretend they are - simpler solution with less steps
            Vector3 travelDirection = end - start;
            Vector3 result = start + t * travelDirection;
            result.y += Mathf.Sin(t * Mathf.PI) * height;
            return result;
        }
        else
        {
            // Start and end are not level, gets more complicated
            Vector3 travelDirection = end - start;
            Vector3 levelDirecteion = end - new Vector3(start.x, end.y, start.z);
            Vector3 right = Vector3.Cross(travelDirection, levelDirecteion);
            Vector3 up = Vector3.Cross(right, travelDirection);
            if (end.y > start.y)
            {
                up = -up;
            }

            Vector3 result = start + t * travelDirection;
            result += (Mathf.Sin(t * Mathf.PI) * height) * up.normalized;
            return result;
        }
    }

    /// <summary>
    /// Closest point on line
    /// </summary>
    /// <param name="vA"></param>
    /// <param name="vB"></param>
    /// <param name="vPoint"></param>
    /// <returns></returns>
    public static Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
    {
        Vector3 vVector1 = vPoint - vA;
        Vector3 vVector2 = (vB - vA).normalized;

        float d = Vector3.Distance(vA, vB);
        float t = Vector3.Dot(vVector2, vVector1);

        if (t <= 0)
        {
            return vA;
        }

        if (t >= d)
        {
            return vB;
        }

        Vector3 vVector3 = vVector2 * t;
        Vector3 vClosestPoint = vA + vVector3;
        return vClosestPoint;
    }

    /// <summary>
    /// Determine the signed angle between two vectors, with normal 'n'
    /// as the rotation axis.
    /// </summary>
    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Low accuracy sqrt method for fast calculation.
    /// </summary>
    public static float FastSqrt(float z)
    {
        if (z == 0)
        {
            return 0;
        }

        FloatIntUnion u;
        u.tmp = 0;
        u.f = z;
        u.tmp -= 1 << 23; // Subtract 2^m.
        u.tmp >>= 1; // Divide by 2.
        u.tmp += 1 << 29; // Add ((b + 1) / 2) * 2^m.
        return u.f;
    }

    /// <summary>
    /// Returns the distance between a and b (approximately).
    /// </summary>
    public static float AproxDistance(Vector3 a, Vector3 b)
    {
        var vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        return InvSqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }

    public static float AproxDistance2D(Vector3 a, Vector3 b)
    {
        var vector = new Vector3(a.x - b.x, a.y - b.y, 0);
        return InvSqrt(vector.x * vector.x + vector.y * vector.y);
    }

    /// <summary>
    /// The Infamous Unusual Fast Inverse Square Root (TM).
    /// </summary>
    public static float InvSqrt(float z)
    {
        if (z == 0)
        {
            return 0;
        }

        FloatIntUnion u;
        u.tmp = 0;
        float xhalf = 0.5f * z;
        u.f = z;
        u.tmp = 0x5f375a86 - (u.tmp >> 1);
        u.f = u.f * (1.5f - xhalf * u.f * u.f);
        return u.f * z;
    }

    /// <summary>
    /// Returns the distance between a and b (fast but very low accuracy).
    /// </summary>
    public static float FastDistance(Vector3 a, Vector3 b)
    {
        var vector = GetVector(a, b);
        return FastSqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
    }

    private static Vector3 GetVector(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }
}
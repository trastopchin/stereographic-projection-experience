using System.Collections;
using UnityEngine;

/// <summary>
/// A class used as a library of helper mathematics functions.
/// </summary>
public static class Mathematics
{
    /// <summary>
    /// Clamps an integer value between min and max.
    /// </summary>
    /// <param name="value">The value we are clamping.</param>
    /// <param name="min">The minimum value we can return.</param>
    /// <param name="max">The maximum value we can return.</param>
    /// <returns>value clamped between min and max.</returns>
    public static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        else if (value > max) return max;
        else return value;
    }

    /// <summary>
    /// Computes 2 raised to the power power.
    /// </summary>
    /// <param name="power">The power 2 is raised to.</param>
    /// <returns>2 raised to the power power.</returns>
    public static int TwoTo(int power)
    {
        return 1 << power;
    }

    /// <summary>
    /// Returns whether or not a is approximately zero.
    /// </summary>
    /// <param name="a">The float we are comparing to 0.</param>
    /// <returns>Whether or not a is approximately 0.</returns>
    public static bool ApproximatelyZero(float a)
    {
        return Mathf.Approximately(a, 0);
    }

    /// <summary>
    /// Returns the smallest nonnegative between a and b.
    /// </summary>
    /// <param name="a">An input float.</param>
    /// <param name="b">An input float.</param>
    /// <returns>The smallest nonnegative between a and b. If both a and b are negative, returns -1.</returns>
    public static float SmallestNonNegative(float a, float b)
    {
        // If both are positive, return their minimum
        if (a >= 0 && b >= 0)
            return Mathf.Min(a, b);
        // If a is positive and b isn't, return a
        else if (a >= 0 && b < 0)
            return a;
        // If b is positive and a isn't, return b
        else if (a < 0 && b >= 0)
            return b;
        // Otherwise both are negative and we return -1
        else
            return -1;
    }

    /// <summary>
    /// Computes the smallest nonnegative root of the quadratic with coefficients a, b, and c.
    /// </summary>
    /// <param name="a">The x^2 coefficient of the quadratic.</param>
    /// <param name="b">The x^1 coefficient of the quadratic.</param>
    /// <param name="c">The x^0 coefficient of the quadratic.</param>
    /// <returns>The smallest nonnegative root of the quadratic ax^2 + bx + c. If no real positive root exists, returns -1.</returns>
    public static float QuadraticIntersect(float a, float b, float c)
    {
        // Compute discriminant
        float disc = b * b - 4 * a * c;
        // If discriminant is 0 we have 1 real root
        if (disc == 0)
        {
            float root = -b / (2 * a);
            if (root > 0)
                return root;
            else
                return -1;
        }
        // If discriminant is positive we have 2 real roots
        else if (disc > 0)
        {
            float sqrtDisc = Mathf.Pow(disc, .5f);
            float root1 = (-b + sqrtDisc) / (2 * a);
            float root2 = (-b - sqrtDisc) / (2 * a);
            return SmallestNonNegative(root1, root2);
        }
        // Otherwise we have two complex roots
        else
        {
            return -1;
        }
    }

    /// <summary>
    /// Computes the line parameter for the intersection of a line and a plane.
    /// </summary>
    /// <param name="e">A point on the line.</param>
    /// <param name="d">The direction vector of the line.</param>
    /// <param name="p">A point on the plane.</param>
    /// <param name="n">The normal of the plane.</param>
    /// <returns>Computes the line parameter for the intersection of the line parameterized by e+td and the plane containing the point p with normal n. If no such intersection exists, returns -1.</returns>
    public static float LinePlaneIntersection(Vector3 e, Vector3 d, Vector3 p, Vector3 n)
    {
        float normalDotDirection = Vector3.Dot(n, d);
        bool isParallel = ApproximatelyZero(normalDotDirection);

        if (isParallel)
        {
            return -1;
        }
        else
        {
            // We can derive this algebraically by substituting in x = e+td into the vector equation of the plane n*x = n*p (and using the billinearity of the dot product).
            float normalDotEyeToPlane = Vector3.Dot(n, e - p);
            return -normalDotEyeToPlane / normalDotDirection;
        }
    }

    /// <summary>
    /// Computes the ray parameter for the intersection of a ray and a plane.
    /// </summary>
    /// <param name="e">The eye of the ray.</param>
    /// <param name="d">The direction of the ray.</param>
    /// <param name="p">A point on the plane.</param>
    /// <param name="n">The normal of the plane.</param>
    /// <returns>Computes the ray parameter for the intersection of the ray parameterized by e+td and the plane containing the point p with normal n. If the ray parameter is negative or no such intersection exists, returns -1.</returns>
    public static float RayPlaneIntersection(Vector3 e, Vector3 d, Vector3 p, Vector3 n)
    {
        float normalDotEyeToPlane = Vector3.Dot(n, e - p);
        float normalDotDirection = Vector3.Dot(n, d);

        bool isContainedOrBehind = normalDotEyeToPlane < 0;
        bool isParallel = ApproximatelyZero(normalDotDirection);

        if (isContainedOrBehind)
        {
            return 0;
        }
        else if (isParallel)
        {
            return -1;
        }
        else
        {
            // We can derive this algebraically by substituting in x = e+td into the vector equation of the plane n*x = n*p (and using the billinearity of the dot product).
            return -normalDotEyeToPlane / normalDotDirection;
        }
    }

    /// <summary>
    /// Computes the ray parameter for the intersection of a ray and a sphere.
    /// </summary>
    /// <param name="e">The eye of the ray.</param>
    /// <param name="d">The direction of the ray.</param>
    /// <param name="c">The center of the sphere.</param>
    /// <param name="r">The radius of the sphere.</param>
    /// <returns>Computes the ray parameter for the intersection of the ray parameterized by e+td and the sphere with center c and radius r. If the ray parameter is negative or no such intersection exists, returns -1.</returns>
    public static float RaySphereIntersection(Vector3 e, Vector3 d, Vector3 c, float r)
    {
        // We can derive these coefficients algebraically by substituting in x = e+td into the vector equation of the sphere |x-c|^2 = r^2.
        float A = d.x * d.x + d.y * d.y + d.z * d.z;
        float B = 2 * (d.x * (e.x - c.x) + d.y * (e.y - c.y) + d.z * (e.z - c.z));
        float C = (e.x - c.x) * (e.x - c.x) + (e.y - c.y) * (e.y - c.y) + (e.z - c.z) * (e.z - c.z) - r * r;
        return QuadraticIntersect(A, B, C);
    }

    /// <summary>
    /// Returns whether or not a point is contained in a sphere.
    /// </summary>
    /// <param name="p">The point we are testing.</param>
    /// <param name="c">The center of the sphere.</param>
    /// <param name="r">The radius of the sphere.</param>
    /// <returns>Whether or not the point p is contained in the sphere with center c and radius r.</returns>
    public static bool InsideSphere(Vector3 p, Vector3 c, float r)
    {
        return (c - p).sqrMagnitude < r * r;
    }

    /// <summary>
    /// Returns whether or not a point lays on the surface of a sphere.
    /// </summary>
    /// <param name="p">The point we are testing.</param>
    /// <param name="c">The center of the sphere.</param>
    /// <param name="r">The radius of the sphere.</param>
    /// <returns>Whether or not the point p is contained on the surface of the sphere with center c and radius r.</returns>
    public static bool OnSphere(Vector3 p, Vector3 c, float r)
    {
        return Mathf.Approximately((c-p).sqrMagnitude, r*r);
    }

    /// <summary>
    /// Returns whether or not a point lays near the surface of a sphere.
    /// </summary>
    /// <param name="p">The point we are testing.</param>
    /// <param name="c">The center of the sphere.</param>
    /// <param name="r">The radius of the sphere.</param>
    /// <param name="epsilon">The epsilon threshold of nearness.</param>
    /// <returns>Whether or not the point p is near enough to the surface of the sphere with center c and radius r by threshold epsilon.</returns>
    public static bool NearSphere(Vector3 p, Vector3 c, float r, float epsilon)
    {
        return Mathf.Abs((c - p).sqrMagnitude - r * r) < epsilon;
    }

    /// <summary>
    /// Returns whether or not a point lays near the surface of a plane.
    /// </summary>
    /// <param name="p">The point we are testing.</param>
    /// <param name="pp">A point on the plane.</param>
    /// <param name="n">The normal of the plane.</param>
    /// <param name="epsilon">The epsilon threshold of nearness.</param>
    /// <returns>Whether or not the point p is near enough to the surface of the plane with point pp and normal n by threshold epsilon.</returns>
    public static bool NearPlane(Vector3 p, Vector3 pp, Vector3 n, float epsilon)
    {
        return Mathf.Abs(Vector3.Dot(n, p - pp)) < epsilon;
    }

    /// <summary>
    /// Computes the closest point on line 1 to line 2.
    /// </summary>
    /// <param name="pa">A point on line 1.</param>
    /// <param name="va">The direction vector of line 1.</param>
    /// <param name="pb">A point on line 2.</param>
    /// <param name="vb">The direction vector of line 2.</param>
    /// <returns></returns>
    public static Vector3 ClosestPointOnLine1ToLine2(Vector3 p1, Vector3 v1, Vector3 p2, Vector3 v2)
    {
        // Perpendicular to both line 1 and line 2
        Vector3 v1Xv2 = Vector3.Cross(v1, v2);

        // Defines a normal of a plane containing line 2 and v1Xv2
        Vector3 n = Vector3.Cross(v1Xv2, v2);

        // The intersection of that plane and line 1 is the closest point on line 1 to line 2
        float t = LinePlaneIntersection(p1, v1, p2, n);

        // Return corresponding point
        return p1 + t * v1;
    }
    /// <summary>
    /// https://www.geometrictools.com/Documentation/DistanceToCircle3.pdf
    /// </summary>
    /// <param name="p"></param>
    /// <param name="c"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static float PointToCircleDistance(Vector3 p, Vector3 c, Vector3 n, float r)
    {
        /*
        Vector3 delta = p - c;
        Vector3 q = Vector3.ProjectOnPlane()
        Vector3 ClosestPointOnCircle = 
        */
        return -1;
    }
}

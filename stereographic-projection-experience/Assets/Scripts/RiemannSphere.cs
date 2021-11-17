using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A MonoBehaviour that provides definitions of stereographic projection functions on a Riemann sphere.
/// </summary>
public class RiemannSphere : MonoBehaviour
{
    // The radius of the Riemann sphere
    public float radius = 2;

    // Start is called before the first frame update
    void Start()
    {
        // Make sure uniform radius
        this.transform.localScale = radius * 2 * Vector3.one;

    }

    /**
     * Returns the Riemann sphere's current north pole.
     */
    public Vector3 NorthPolPos
    {
        get
        {
            // The north pole always remains on top!
            return transform.position + radius * Vector3.up;
        }
    }

    /**
     * Stereographically projects a point on the Riemann sphere onto the plane.
     * 
     * \param p The point of the Riemann sphere we are projecting onto the plane.
     * \return The stereographic projection of p onto the plane.
     */
    public Vector3 StereoProj(Vector3 p)
    {
        // Setup stereographic projection
        Vector3 e = NorthPolPos;
        Vector3 d = p - e;

        // If e is behind the plane, reverse the ray direction
        float normalDotEyeToPlane = Vector3.Dot(Vector3.up, e - p);
        if (normalDotEyeToPlane < 0)
        {
            d = -d;
        }

        // RayPlaneIntersection appropriately returns t when e belongs to the plane
        float t = Mathematics.RayPlaneIntersection(e, d, Vector3.zero, Vector3.up);

        // If we have an intersection, return it
        if (t > 0)
            return e + t * d;
        // Otherwise return the zero vector
        else
            return Vector3.zero;
    }

    /**
     * Stereographically projects a point on the plane onto the Riemann sphere.
     * 
     * \param p The point of the plane we are projecting onto the Riemann sphere.
     * \return The inverse stereographic projection of p onto the plane.
     */
    public Vector3 StereoProjInv(Vector3 p)
    {
        // Setup stereographic projection
        Vector3 e = p;
        Vector3 d = NorthPolPos - p;

        // If our eye is close enough to the Riemann sphere, it is fixed
        if (Mathematics.OnSphere(p, transform.position, radius))
        {
            return e;
        }

        // If our eye is inside the Riemann sphere
        if (Mathematics.InsideSphere(e, transform.position, radius))
        {
            // Move it along the ray outside of the sphere
            e -= 2 * radius * d;
        }

        float t = Mathematics.RaySphereIntersection(e, d, transform.position, radius);

        // If we have an intersection, return it
        if (t > 0)
            return e + t * d;
        // Otherwise return the zero vector
        else
            return Vector3.zero;
    }
}

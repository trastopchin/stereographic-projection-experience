using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A circle gizmo that can be positioned, oriented, and raycasted.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class CircleGizmo : MonoBehaviour
{
    // Circle line renderer parameters
    public int detail = 10;
    private int numVertices;
    public LineRenderer lineRenderer;
    public float width = .02f;
    public Color color = Color.white;
    public float epsilon = .05f;

    // The radius of the circle
    public float Radius
    {
        get
        {
            // Because lossy scale is a scalar multiple of Vector3.one, we can just return the x compnent
            return transform.lossyScale.x;
        }

        set
        {
            transform.localScale = value * Vector3.one;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        numVertices = Mathematics.TwoTo(detail);
        InitializeLineRenderer();
    }

    /// <summary>
    /// Initializes the circle's line renderer
    /// </summary>
    private void InitializeLineRenderer()
    {
        // Set appropriate fields
        lineRenderer.positionCount = numVertices;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = width;
        SetColor(color);

        // Compute circle positions
        for (int i = 0; i < numVertices; i++)
        {
            // Compute circle coordinates
            float t = i / (float)(numVertices - 1);
            Vector3 p = Utils.UnitCircle(t);

            // Set line renderer position
            lineRenderer.SetPosition(i, p);
        }
    }

    /// <summary>
    /// Raycast the CircleGizmo's circular tube.
    /// </summary>
    /// <param name="ray">The ray that is being casted.</param>
    /// <param name="hitInfo">The raycast hit info that is being written out.</param>
    /// <param name="maxDistance">The maximum raycast distance.</param>
    /// <returns>Whether or not ray hit the circle. Does this by raycasting the plane and sphere containing the circle and determining if the resulting points are close enough to the plane and sphere respectively. Only sets the hitInfo point and normal fields.</returns>
    public bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
    {
        // Initialize hitInfo
        hitInfo = new RaycastHit();

        // Setup ray tracing parameters
        Vector3 e = ray.origin;
        Vector3 d = ray.direction;
        Vector3 p = transform.position;
        Vector3 n = transform.up;

        // First we try to raycast the plane containing the circle
        float t = Mathematics.RayPlaneIntersection(e, d, p, n);
        
        // If it's a viable approximation
        if (t > 0 && t < maxDistance)
        {
            // Compute intersection point on plane
            Vector3 planePoint = e + t * d;

            // Determine if the intersection point is close enough to the circle
            // Do this by testing if it's close enough to the sphere
            if (!Mathematics.NearSphere(planePoint, p, Radius, epsilon)) return false;

            // Otherwise the point is close enough and so we fill hitInfo and return true
            hitInfo.point = planePoint;
            hitInfo.normal = (planePoint - p).normalized;
            return true;
        }

        // We second try to raycast the sphere containing the circle
        // We do this twice in case the first raycast occludes the sphere
        for (int i = 0; i < 2; i++)
        {
            t = Mathematics.RaySphereIntersection(e, d, p, Radius);
            Vector3 spherePoint = e + t * d;

            // If it's a viable approximation
            if (t > 0 && t < maxDistance)
            {
                // Determine if the intersection point is close enough to the circle
                // Do this by testing if it's close enough to the plane
                if (Mathematics.NearPlane(spherePoint, p, n, epsilon))
                {
                    hitInfo.point = spherePoint;
                    hitInfo.normal = (spherePoint - p).normalized;
                    return true;
                }
                // If it's not a good approximation, perform a second raycast
                else
                {
                    e = spherePoint + .01f * d;
                }
            }
        }

        // If none of our raycasts work / return good enough approximations, return false
        return false;
    }

    /// <summary>
    /// Helper method returns whether or not the screen-point-to-ray intersects the CircleGizmo's circular tube.
    /// </summary>
    /// <param name="hitInfo">The resulting hitInfo.</param>
    /// <returns>Whether or not the screen-point-to-ray intersects the CircleGizmo's circlular tube.</returns>
    public bool IsBehindMouse(out RaycastHit hitInfo)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Raycast(ray, out hitInfo, 100.0f);
    }

    /// <summary>
    /// Helper method returns whether or not the screen-point-to-ray intersects the CircleGizmo's circular tube.
    /// </summary>
    /// <returns>Whether or not the screen-point-to-ray intersects the CircleGizmo's circlular tube.</returns>
    public bool IsBehindMouse()
    {
        RaycastHit hitInfo;
        return IsBehindMouse(out hitInfo);
    }

    /// <summary>
    /// Sets a CircleGizmo's color.
    /// </summary>
    /// <param name="color">The color we are setting.</param>
    public void SetColor(Color color)
    {
        Material material = lineRenderer.material;
        material.SetColor("_Emission", color);
    }
}

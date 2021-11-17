using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A task that requires the player to right-click to place points on the Riemann sphere / plane and left-click on the resulting stereographically projected point on the plane / Riemann sphere.
/// </summary>
public class StereographicProjectionTask : GameTask, IInputHandler
{
    // Interface with the input manager
    public InputManager inputManager;
    public int inputHandlerPriority = 0;

    // The next game task
    public GameTask nextGameTask;

    public GameObject riemannSphereCollection;
    public RiemannSphere riemannSphere;

    public GameObject pointPrefab;
    public GameObject linePrefab;
    private GameObject point;
    private GameObject line;

    public bool inverseProjection = true;
    public int numPointsToPlace = 3;
    private int numPointsPlaced = 0;
    public float transformationDuration = 2;

    // Parameters for adaptive point scaling
    public GameObject mainCamera;
    public float nearDist = 1;
    public float nearScale = 1;
    public float farDist = 100;
    public float farScale = 10;

    /// <summary>
    /// Instantiates and hides the point and line on awake.
    /// </summary>
    private void Awake()
    {
        point = GameObject.Instantiate(pointPrefab, transform);
        line = GameObject.Instantiate(linePrefab, transform);

        point.SetActive(false);
        line.SetActive(false);
    }

    /// <summary>
    /// Registers to the task manager and input manager. Sets the riemannSphereCollection to be active and starts the HighlightPointRoutine and PointPlacingRoutines.
    /// </summary>
    private void OnEnable()
    {
        // Register task to task manager and input handler
        AutoRegisterTaskToManager();
        inputManager.AddInputHandler(this, inputHandlerPriority);

        riemannSphereCollection.SetActive(true);

        StartCoroutine(Utils.AdaptiveScaleRoutine(point, mainCamera, nearDist, nearScale, farDist, farScale, null));
        StartCoroutine(HighlightPointRoutine());
        StartCoroutine(PointPlacingRoutine());
    }

    /// <summary>
    /// Removes the input handler and destroyes the point and line.
    /// </summary>
    private void OnDisable()
    {
        // Complete task and remove it from the input handler
        inputManager.RemoveInputHandler(this);

        GameObject.Destroy(point);
        GameObject.Destroy(line);
    }

    /// <summary>
    /// Returns whether or not the specified game object is behind the mouse.
    /// </summary>
    /// <param name="gameObject">The game object we are asking whether or not it is behind the mouse.</param>
    /// <returns>Whether or not the specified game object is behind the mouse.</returns>
    private bool isBehindMouse(GameObject gameObject)
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        return gameObject.GetComponent<Collider>().Raycast(ray, out hitInfo, 100.0f);
    }

    /// <summary>
    /// A routine that sets the point to a highlighted color when it is behind the mouse.
    /// </summary>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator HighlightPointRoutine()
    {
        point.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
        while (true)
        {
            Color highlightedColor = Color.white;
            if (isBehindMouse(point)) highlightedColor = Color.yellow;
            point.GetComponent<MeshRenderer>().material.color = highlightedColor;
            yield return null;
        }
    }

    // Delegate for choosing stereographic projection or its inverse
    private delegate Vector3 Mapping(Vector3 p);

    /// <summary>
    /// A routine that allows the player to right-click to place a point on the Riemann sphere or plane.
    /// </summary>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator PointPlacingRoutine()
    {
        // If we haven't place the goal number of points
        if (numPointsPlaced < numPointsToPlace)
        {
            // Start placing a point
            numPointsPlaced++;
            while (true)
            {
                // Hide the point
                point.SetActive(false);

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float t = -1;

                // If inverse projection we are placing the point on the plane
                if (inverseProjection)
                {
                    t = Mathematics.RayPlaneIntersection(ray.origin, ray.direction, Vector3.zero, Vector3.up);
                }
                // Otherwise we are placing the point on the sphere
                else
                {
                    t = Mathematics.RaySphereIntersection(ray.origin, ray.direction, riemannSphere.transform.position, riemannSphere.radius);
                }

                // If the ray intersected the Riemann sphere / plane
                if (t > 0)
                {
                    // Compute the inersectoin point
                    Vector3 intersectionPoint = ray.origin + t * ray.direction;

                    // Set the point to active and update the point's position
                    point.SetActive(true);
                    point.transform.position = intersectionPoint;

                    // If right mouse button is clicked
                    if (Input.GetMouseButtonDown(1))
                    {
                        // Skip a frame
                        yield return null;

                        // Determine the correct map and pass it to the next routine
                        Mapping map = inverseProjection ? (Mapping)riemannSphere.StereoProjInv : (Mapping)riemannSphere.StereoProj;

                        // Start the animation routne
                        StartCoroutine(StereographicProjectionAnimationRoutine(map));
                        yield break;
                    }
                }

                yield return null;
            }
        }
        // Otherwise transition to the next task
        else
        {
            TransitionToTask(nextGameTask);
            yield break;
        }
    }

    /// <summary>
    /// A routine that renders the animation of a point traveling along a ray during inverse / forward stereographic projection.
    /// </summary>
    /// <param name="map">The map for the particular inverse / forward stereographic projection we are using.</param>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator StereographicProjectionAnimationRoutine(Mapping map)
    {
        // Compute the point and line start and end
        Vector3 start = point.transform.position;
        Vector3 end = map(start);
        Vector3 dir = end - start;
        Vector3 lineStart = start - dir.normalized;
        Vector3 lineEnd = riemannSphere.NorthPolPos;

        // If it is not inverse stereographic projection adjust the line start and end
        if (!inverseProjection)
        {
            lineStart = riemannSphere.NorthPolPos ;
            lineEnd = end + dir.normalized;
        }

        // Show the line
        line.SetActive(true);

        // Orient the line
        Utils.OrientCylinderGlobal(line, lineStart, lineEnd, .025f);

        // Linearly interpolate the point's position
        float currentTime = 0;
        while (currentTime < transformationDuration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / transformationDuration;
            point.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        // Skip a frame
        yield return null;

        // Start the next routine
        StartCoroutine(CleanUpAndStartAgain());
        yield break;
    }

    /// <summary>
    /// This routine waits for the player to click the point resulting from inverse / forward stereographic projection before moving onto the next point.
    /// </summary>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator CleanUpAndStartAgain()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0) && isBehindMouse(point))
            {
                // Hide the point
                line.SetActive(false);

                // Skip a frame
                yield return null;

                // Move to the next routine
                StartCoroutine(PointPlacingRoutine());
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// We request to handle input if the right mouse button is clicked and the point is behind the mouse.
    /// </summary>
    /// <returns>Whether or not we are requesting to handle input.</returns>
    public bool RequestingToHandleInput()
    {
        return Input.GetMouseButtonDown(1) && isBehindMouse(point);
    }
}

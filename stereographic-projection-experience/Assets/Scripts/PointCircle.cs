using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

/// <summary>
/// A MonoBehaviour that renders a circle and points on that circle interacting with a Riemann sphere.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PointCircle : MonoBehaviour
{
    // The Riemann sphere the circle interacts with
    public RiemannSphere riemannSphere;

    // The Transformable component of that Riemann sphere
    public Transformable transformable;

    // The transform's last parent
    private Transform lastParent;

    // Circle fields
    public Vector3 center = Vector3.zero;
    public float radius = 1;

    // Circle line renderer fields
    public int detail = 10;
    private int numVertices;
    private Vector3[] currentVertices;
    private Vector3[] futureVertices;
    private LineRenderer lineRenderer;

    // Point fields
    // The parent transform used when instancing points
    public Transform instanceParent = null; 
    public int numPoints = 0;
    public float minPointSpacing = 0.1f;
    public GameObject pointPrefab;
    private GameObject[] points;
    private Vector3[] currentPointPositions;
    private Vector3[] futurePointPositions;
    public Color[] pointColors;
    // The maximum number of iterations used in the point placing algorithm
    public int maxPointPlacingIterations = 128;
    // An optional list of parameters used to manually override point placing
    public List<float> pointParametersOverride = null;

    // Behaviour parameters
    public float transformationDuration = 2f;
    public float errorDuration = 0.25f;
    public Color idleColor = Color.black;
    public Color transformingColor = Color.yellow;
    public Color errorColor = Color.red;

    // Unity events to call after projections take place
    public UnityEvent OnProjectedToSphere = new UnityEvent();
    public UnityEvent OnProjectedToPlane = new UnityEvent();

    // Circle states
    public enum State
    {
        Plane,
        Sphere
    }

    // Set initial state
    private State state = State.Plane;

    // Get state
    public State GetGeometricState() { return state; }

    // Delegate for transformations
    public delegate Vector3 Transformation(Vector3 p);

    // Bool to help facilitate initialization
    private bool initialized = false;

    // How far away the circle should be from the north pole
    public float northPoleEpsilon = 10 * Mathf.Epsilon;

    // Parameters for adaptive point scaling
    public GameObject mainCamera;
    public float nearDist = 1;
    public float nearScale = 1;
    public float farDist = 100;
    public float farScale = 10;

    // Parameters to pass a scale factor by reference to a coroutine
    private Vector3O scaleFactorObject = new Vector3O(Vector3.one);
    private Vector3 scaleFactor = Vector3.one;
    private Vector3 parentScale = Vector3.one;

    /// <summary>
    /// Calls Initialize().
    /// </summary>
    public void Awake()
    {
        if (!initialized) Initialize();
    }

    /// <summary>
    /// Creates the circle using the line renderer and instantiates the points.
    /// </summary>
    public void Initialize()
    {
        InitializeCircle();
        InitializePoints();
        initialized = true;
    }

    /// <summary>
    /// Destroys the instantiated points.
    /// </summary>
    private void OnDestroy()
    {
        foreach (GameObject point in points)
        {
            GameObject.Destroy(point);
        }
    }

    /// <summary>
    /// Starts the Idling routine on enable.
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine(IdlingRoutine());
    }

    /// <summary>
    /// The Idline routine processes user input and starts the ApplyLerpTransformation routine or the ErrorRoutine.
    /// </summary>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator IdlingRoutine()
    {
        while (true)
        {
            if (riemannSphere == null)
            {
                // Do nothing if no riemann sphere is set!
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                // If we are in the right geometric state and the projected circle isn't too close to the north pole
                if (state == State.Plane && !CircleIntersectNorthPole(true))
                {
                    StartCoroutine(ApplyTransformationRoutine(riemannSphere.StereoProjInv, State.Sphere));
                }
                // Otherwise start the error routine
                else
                {
                    StartCoroutine(ErrorRoutine());
                }
                yield break;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                // If we are in the right geometric state and the circle isn't too close to the north pole
                if (state == State.Sphere && !CircleIntersectNorthPole(false))
                {
                    StartCoroutine(ApplyTransformationRoutine(riemannSphere.StereoProj, State.Plane));
                }
                // Otherwise start the error routine
                else
                {
                    StartCoroutine(ErrorRoutine());
                }
                yield break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// The Error routine makes the player circle flash red.
    /// </summary>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator ErrorRoutine()
    {
        float currentTime = 0;
        while (currentTime < errorDuration)
        {
            // Compute current t \in [0, 1]
            float t = currentTime / errorDuration;

            // Update line renderer color
            Color currentColor = Color.Lerp(idleColor, errorColor, AnimationCurve(t));
            lineRenderer.material.SetColor("_Emission", currentColor);

            // Update current time
            currentTime += Time.deltaTime;

            // Return control at end of frame
            yield return null;
        }

        StartCoroutine(IdlingRoutine());
        yield break;
    }

    /// <summary>
    /// The ApplyTransformation routine renders an animation of the player circle transforming into the image of the player circle by the specified transformation.
    /// </summary>
    /// <param name="transformation">The transformation we are applying to the player circle.</param>
    /// <param name="targetState">The resulting state of the circle after the animation finishes.</param>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    IEnumerator ApplyTransformationRoutine(Transformation transformation, State targetState)
    {
        // Disable transformable
        transformable.ProcessingInput = false;

        // Compute future circle vertex positions
        for (int i = 0; i < numVertices; i++)
        {
            futureVertices[i] = transformation(currentVertices[i]);
        }

        // Compute future point positions
        for (int i = 0; i < numPoints; i++)
        {
            currentPointPositions[i] = points[i].transform.position;
            futurePointPositions[i] = transformation(currentPointPositions[i]);
        }

        // Update position loop
        float currentTime = 0;
        while (currentTime < transformationDuration)
        {
            // Compute current t \in [0, 1]
            float t = currentTime / transformationDuration;

            // Update line renderer vertices
            for (int i = 0; i < numVertices; i++)
            {
                Vector3 currentPosition = Vector3.Lerp(currentVertices[i], futureVertices[i], t);
                lineRenderer.SetPosition(i, currentPosition);
            }

            // Update line renderer color
            Color currentColor = Color.Lerp(idleColor, transformingColor, AnimationCurve(t));
            lineRenderer.material.SetColor("_Emission", currentColor);

            // Update point positions
            for (int i = 0; i < numPoints; i++)
            {
                points[i].transform.position = Vector3.Lerp(currentPointPositions[i], futurePointPositions[i], t);
            }

            // Update current time
            currentTime += Time.deltaTime;

            // Return control at end of frame
            yield return null;
        }

        // Update line renderer vertices after animation completes
        for (int i = 0; i < numVertices; i++)
        {
            lineRenderer.SetPosition(i, futureVertices[i]);
        }

        // Update point positions after animation completes
        for (int i = 0; i < numPoints; i++)
        {
            points[i].transform.position = futurePointPositions[i];
        }

        // Update current positions by swapping pointers
        Vector3[] prevCurrentVertices = currentVertices;
        currentVertices = futureVertices;
        futureVertices = prevCurrentVertices;

        // Enable transformable
        transformable.ProcessingInput = true;

        // Update current state
        state = targetState;

        // Start the Idling routine
        StartCoroutine(IdlingRoutine());

        // Invoke the corresponding OnProjected UnityAction
        if (transformation == riemannSphere.StereoProj) OnProjectedToPlane.Invoke();
        else OnProjectedToSphere.Invoke();

        // Transition back to the idle and target geometric states
        yield break;
    }

    /// <summary>
    /// Helper function for producing a smooth animation curve.
    /// </summary>
    /// <remarks>Graph this function on Desmos.</remarks>
    /// <param name="t">The time parameter to evaluate the curve in [0, 1].</param>
    /// <returns>The animation curve evaluated at t.</returns>
    private float AnimationCurve(float t)
    {
        return Mathf.Pow(1 - Mathf.Cos(t / Mathf.PI * 20), 2) / 2;
    }

    /// <summary>
    /// Sets the riemann sphere as the circle's and points' transform parent.
    /// </summary>
    public void SetRiemannSphereAsParent()
    {
        // Only do this if we are on the sphere
        if (state == State.Sphere)
        {
            lastParent = transform.parent;

            transform.SetParent(riemannSphere.transform);
            foreach (GameObject point in points)
            {
                point.transform.SetParent(riemannSphere.transform, true);
            }

            // Update the scaleFactorObject to undo that parent's scale
            scaleFactor = scaleFactorObject.data;
            parentScale = riemannSphere.transform.lossyScale;
            scaleFactorObject.data = Utils.divide(scaleFactor, parentScale);
        }
    }

    /// <summary>
    /// Unparents the circle and points and applies their previous transform.
    /// </summary>
    public void UnParentAndApplyTransform()
    {
        // Only do this if we are on the sphere
        if (state == State.Sphere)
        {
            // Detatch from parent and keep inherited transform
            transform.SetParent(lastParent, true);

            // Apply inherited transform to all vertices
            for (int i = 0; i < numVertices; i++)
            {
                Vector3 worldSpacePoint = transform.TransformPoint(currentVertices[i]);
                currentVertices[i] = worldSpacePoint;
                lineRenderer.SetPosition(i, worldSpacePoint);
            }

            // Reset transform
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.SetParent(lastParent);

            // Unparent the points
            foreach(GameObject point in points)
            {
                point.transform.SetParent(instanceParent, true);
            }

            // Update the scaleFactorObject to undo that parent's scale
            scaleFactorObject.data = scaleFactor;
        }
    }

    /// <summary>
    /// Creates the circle using the line renderer and initializes the vertex position arrays.
    /// </summary>
    private void InitializeCircle()
    {
        // Compute position count
        numVertices = Mathematics.TwoTo(detail);

        // Allocate current and future array vertices
        currentVertices = new Vector3[numVertices];
        futureVertices = new Vector3[numVertices];

        // Get line renderer
        lineRenderer = GetComponent<LineRenderer>();

        // Update line renderer position count
        lineRenderer.positionCount = numVertices;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = .03f;
        lineRenderer.useWorldSpace = false;

        // Compute intial positions
        for (int i = 0; i < numVertices; i++)
        {
            // Compute circle coordinates
            float t = i / (float)(numVertices - 1);
            Vector3 pos = center + radius * Utils.UnitCircle(t);

            // Set current and future vertices
            currentVertices[i] = pos;
            futureVertices[i] = pos;

            // Set line renderer position
            lineRenderer.SetPosition(i, pos);
        }
    }

    /// <summary>
    /// Randomly creates numPoints number of pointPrefabs that "stick" to the circle.
    /// </summary>
    private void InitializePoints()
    {
        points = new GameObject[numPoints];

        currentPointPositions = new Vector3[numPoints];
        futurePointPositions = new Vector3[numPoints];

        float[] pointParameters = new float[numPoints];
        float randomParameterSpaceOffset = Random.value;

        // Try to place each point minPointSpacing away from each other
        for (int i = 0; i < numPoints; i++)
        {
            // Instantiate point
            GameObject point = GameObject.Instantiate(pointPrefab, instanceParent);
            points[i] = point;
            Material material = point.GetComponent<MeshRenderer>().material; ;
            material.color = pointColors[i];

            // If we are not manually overriding the instantiated point parameters
            if (pointParametersOverride == null)
            {
                // Run our random-sampling based point-placing algorithm
                float currentParameter = 0;

                // Do nothing on the first iteration
                if (i == 0) { }
                // Place a point far enough away from the others on later iterations
                else
                {
                    // For the maximum number of point placing iterations
                    for (int j = 0; j < maxPointPlacingIterations; j++)
                    {
                        // Sample a random point
                        currentParameter = Random.value;
                        bool foundPoint = true;
                        // Iterate through the previously generated points
                        for (int k = 0; k < i; k++)
                        {
                            float distance = Mathf.Abs(pointParameters[k] - currentParameter);
                            // If it is too close to any of the previously generated points, discard it
                            if (distance < minPointSpacing || distance > 1 - minPointSpacing)
                            {
                                // We check the second conditional statement because 0 is equivalent to 1 on the unit circle
                                foundPoint = false;
                                break;
                            }
                        }
                        // If we found a point that is minPointSpacing asway
                        if (foundPoint) break;
                    }
                }
                // Record the generated point
                pointParameters[i] = currentParameter;

                // Place the point
                point.transform.position = center + radius * Utils.UnitCircle(currentParameter + randomParameterSpaceOffset);
            }

            // If we are manually overriding the instantiated point parameters
            else
            {
                point.transform.position = center + radius * Utils.UnitCircle(pointParametersOverride[i] + randomParameterSpaceOffset);
            }
        }

        // Adaptively scale the points
        foreach (GameObject point in points)
        {
            StartCoroutine(Utils.AdaptiveScaleRoutine(point, mainCamera, nearDist, nearScale, farDist, farScale, scaleFactorObject));
        }
    }

    /// <summary>
    /// Returns the number of vertices of the circle.
    /// </summary>
    /// <returns></returns>
    public int NumVertices()
    {
        return Mathematics.TwoTo(detail);
    }

    /// <summary>
    /// Returns the position of a point belonging to the circle tesselation used by the line renderer.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public Vector3 GetCirclePointPosition(int i)
    {
        return GetComponent<LineRenderer>().GetPosition(i);
    }

    /// <summary>
    /// Returns the position of on of the points instantiated to stick to the circle.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public Vector3 GetPointPosition(int i)
    {
        return points[i].transform.position;
    }

    /// <summary>
    /// Returns whether or not one of the vertices of the circle is too close to the north pole.
    /// </summary>
    /// <param name="projectToSphere">Whether or not to project the circle to the sphere before determining if the circle is too close to the north pole.</param>
    /// <returns>Whether or not one of the vertices of the circle is too close to the north pole.</returns>
    private bool CircleIntersectNorthPole(bool projectToSphere)
    {
        // Do nothing if not linked to the Riemann sphere
        if (riemannSphere == null) return false;

        // Iterate through each vertex of the circle
        for (int i = 0; i < numVertices; i++)
        {
            // Get the corresponding world space vertex
            Vector3 worldSpaceCirclePoint = transform.TransformPoint(currentVertices[i]);

            // Project it to the spehre if desired
            if (projectToSphere) worldSpaceCirclePoint = riemannSphere.StereoProjInv(worldSpaceCirclePoint);

            float distance = (riemannSphere.NorthPolPos - worldSpaceCirclePoint).magnitude;
            if (distance < northPoleEpsilon) {
                Debug.Log(distance);
                return true;
            }
            
        }
        return false;
    }
}

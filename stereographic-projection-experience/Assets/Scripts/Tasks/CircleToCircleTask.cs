using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

/// <summary>
/// A task that requires a player to construct a Möbius transformation that sends a black player circle + n colored points to a yellow target circle + n corresponding colored points.
/// </summary>
public class CircleToCircleTask : GameTask
{
    // The next game task
    public GameTask nextGameTask;

    public GameObject riemannSphereCollection;
    public RiemannSphere riemannSphere;
    public Transformable transformable;

    // Parameters for the player and target circle
    public PointCircle pointCirclePrefab;
    private PointCircle playerCircle;
    private PointCircle targetCircle;

    public GameObject pointPrefab;
    public Color[] pointColors = { Color.red, Color.green, Color.blue };

    public bool overridePointParameters = false;
    public List<float> playerPointParameters = new List<float>();
    public List<float> targetPointParameters = new List<float>();

    // Parameters for the specific circle-to-circle task.
    public int numCirclesToMatch = 3;
    public int numPoints = 0;
    private int numCirclesMatched = 0;
   
    // Parameters for comparing the player and target circle.
    public float epsilon = 0.01f;
    public int numSamples = 3;
    private int[] sampleIndices;

    // Win animation parameters
    public float winAnimationLength = 2f;
    public Color glowingColor;
    public float glowingSpeed;

    // Parameters for adaptive point scaling
    public GameObject mainCamera;
    public float nearDist = 1;
    public float nearScale = 1;
    public float farDist = 100;
    public float farScale = 10;

    /// <summary>
    /// Initializes the player circle and target circle.
    /// </summary>
    private void InitializeTaskObjects()
    {
        // Clamp the number of points to the range [0, 3]
        numPoints = Mathematics.Clamp(numPoints, 0, 3);

        playerCircle = InstantiatePointCircle(Utils.RandomInsideCircle(2), Random.Range(0.5f, 2), Color.black, true, numPoints, pointColors, playerPointParameters);
        targetCircle = InstantiatePointCircle(Utils.RandomInsideCircle(2), Random.Range(0.5f, 2), Color.yellow, false, numPoints, pointColors, targetPointParameters);

        // Initializes the indices used for sampling and comparing the circles.
        sampleIndices = new int[numSamples];
        System.Random random = new System.Random();
        for (int i = 0; i < numSamples; i++)
        {
            sampleIndices[i] = random.Next(0, playerCircle.NumVertices() - 1);
        }
    }

    /// <summary>
    /// Deletes the task bojects.
    /// </summary>
    private void DeleteTaskObjects()
    {
        GameObject.Destroy(playerCircle.gameObject);
        GameObject.Destroy(targetCircle.gameObject);
    }

    /// <summary>
    /// Initializes the task objects.
    /// </summary>
    private void Awake()
    {
        InitializeTaskObjects();
    }

    /// <summary>
    /// Initalizes the task.
    /// </summary>
    private void OnEnable()
    {
        // Register task to task manager and input handler
        AutoRegisterTaskToManager();
    }

    /// <summary>
    /// Stops and cleans up the task.
    /// </summary>
    private void OnDisable()
    {
        if (playerCircle != null) playerCircle.OnProjectedToPlane.RemoveAllListeners();

        transformable.OnTransformationStart.RemoveAllListeners();
        transformable.OnTransformationEnd.RemoveAllListeners();

        DeleteTaskObjects();
    }

    /// <summary>
    /// Instantiates a PointCircle on the plane.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="connectToRiemannSphere">Whether or not the circle is connected to the Riemann sphere via stereographic projection.</param>
    /// <param name="numPoints">The number of additional point goals on the circle.</param>
    /// <returns></returns>
    private PointCircle InstantiatePointCircle(Vector3 center, float radius, Color color, bool connectToRiemannSphere, int numPoints, Color[] pointColors, List<float> pointParametersOverride)
    {
        // Instantiate the circle.
        PointCircle pointCircle = (PointCircle)GameObject.Instantiate(pointCirclePrefab, transform);

        // Fill out its fields accordingly
        pointCircle.pointPrefab = pointPrefab;
        pointCircle.riemannSphere = connectToRiemannSphere ? riemannSphere : null;
        pointCircle.transformable = connectToRiemannSphere ? transformable : null;
        pointCircle.center = center;
        pointCircle.radius = radius;
        pointCircle.instanceParent = transform;
        pointCircle.detail = 10;
        pointCircle.numPoints = numPoints;
        pointCircle.minPointSpacing = 0.1f;
        pointCircle.pointColors = pointColors;
        pointCircle.maxPointPlacingIterations = 64;
        pointCircle.mainCamera = mainCamera;
        pointCircle.nearDist = nearDist;
        pointCircle.farDist = farDist;
        pointCircle.farScale = farScale;

        // Correctly pass the point parameters override
        if (overridePointParameters)
            pointCircle.pointParametersOverride = pointParametersOverride;
        else
            pointCircle.pointParametersOverride = null;

        // Initialize the point circle
        pointCircle.Initialize();

        // Setup the material
        Material material = pointCircle.GetComponent<LineRenderer>().material;
        material.color = color;
        material.SetColor("_Emission", color);

        // If we are connecting it to the Riemann sphere
        if (connectToRiemannSphere)
        {
            pointCircle.OnProjectedToPlane.AddListener(CompareCircles);

            Transformable transformable = riemannSphere.GetComponent<Transformable>();

            // Add the SetParent and UnParentAndApplyTransform actions
            transformable.OnTransformationStart.AddListener(pointCircle.SetRiemannSphereAsParent);
            transformable.OnTransformationEnd.AddListener(pointCircle.UnParentAndApplyTransform);
        }

        return pointCircle;
    }

    /// <summary>
    /// Compares the player circle to the target circle.
    /// </summary>
    /// <remarks>Requires that both the player and target circle belong to the real plane.</remarks>
    /// <returns></returns>
    private float ComparePlayerToTargetCircle()
    {
        // Compute circle distance based on number of samples
        float circleDistance = 0;
        for (int i = 0; i < numSamples; i++)
        {
            Vector3 playerPoint = playerCircle.GetCirclePointPosition(sampleIndices[i]);
            float localDistance = Mathf.Abs((playerPoint - targetCircle.center).magnitude - targetCircle.radius);
            circleDistance += localDistance;
        }
        circleDistance /= numSamples;

        // Compute point distance
        float pointDistance = 0;
        if (numPoints > 0)
        {
            for (int i = 0; i < numPoints; i++)
            {
                Vector3 playerPoint = playerCircle.GetPointPosition(i);
                Vector3 targetPoint = targetCircle.GetPointPosition(i);
                pointDistance += (playerPoint - targetPoint).magnitude;
            }
            pointDistance /= numPoints;
        }

        // Debug.Log("Circle Distance = " + circleDistance.ToString() + ", Point Distance = " + pointDistance.ToString() + ".");

        return circleDistance + pointDistance;
    }

    /// <summary>
    /// The function called when we want to compare circles. We only call this when a circle has been projected back to the plane.
    /// </summary>
    private void CompareCircles()
    {
        float distance = ComparePlayerToTargetCircle();
        if (distance < epsilon)
        {
            StartCoroutine(WinRoutine());
        }
    }

    /// <summary>
    /// A routine that displays the winning animation and transitions to the next circle-to-circle task. If all the specified circles were matched, the routine cleans up after this routine and transitions to the next one.
    /// </summary>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator WinRoutine()
    {
        // Display winning animation
        riemannSphere.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
        Vector3 lastPosition = riemannSphereCollection.transform.position;
        float time = 0;
        while (time < winAnimationLength)
        {
            time += Time.deltaTime;

            float sinwave = Mathf.Sin(glowingSpeed * time / winAnimationLength * 2 * Mathf.PI);
            riemannSphere.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.Lerp(Color.black, glowingColor, 0.5f * (1 + sinwave)));
            riemannSphereCollection.transform.position = lastPosition + 0.25f * sinwave * Vector3.up;
            yield return null;
        }
        riemannSphere.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
        riemannSphereCollection.transform.position = lastPosition;

        // Clean up after winning animation
        DeleteTaskObjects();

        // Make the player match more circles and points
        if (numCirclesMatched < numCirclesToMatch - 1)
        {
            numCirclesMatched++;
            InitializeTaskObjects();
        }
        // Otherwise transition to the next task
        else
        {
            TransitionToTask(nextGameTask);
            yield break;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

/// <summary>
/// A MonoBehaviour that uses Gizmos to allow a player to interactively translate and rotate a GameObject.
/// </summary>
public class Transformable : MonoBehaviour, IInputHandler
{
    // Interface with the input manager
    public InputManager inputManager;
    public int inputHandlerPriority = 0;

    // The transform we set as the parent of all instantiated GameObjects
    public Transform instanceParent = null;

    // States
    public enum State
    {
        Idling,
        Translating,
        Rotating
    }
    // We start out in the Idling state
    private State state = State.Idling;
    // When this is true it signals to start a new state routine.
    private bool stateChange = false;

    public State GetState() { return state; }

    // Parameters for all gizmos
    private GameObject gizmoCollection = null;
    public Color defaultGizmoColor = new Color( 1, 1, 1, 0.25f );
    public Color[] selectedGizmoColors = new Color[numArrowGizmos];

    // Parameters for the translation arrow gizmos
    public ArrowGizmo arrowGizmoPrefab;
    private const int numArrowGizmos = 3;
    private ArrowGizmo[] arrowGizmos = new ArrowGizmo[numArrowGizmos];
    private Vector3[] arrowGizmoNormals = new Vector3[numArrowGizmos];

    // Parameters for the translate operation
    private ArrowGizmo selectedArrowGizmo = null;
    private Vector3 lastPosition = Vector3.zero;

    // Parameters for the rotation circle gizmos
    public CircleGizmo circleGizmoPrefab;
    private const int numCircleGizmos = 3;
    private CircleGizmo[] circleGizmos = new CircleGizmo[numCircleGizmos];
    private Vector3[] circleGizmoNormals = new Vector3[numCircleGizmos];

    // Parameters for the rotate operation
    public float rotationSpeed = 1;
    private CircleGizmo selectedCircleGizmo = null;
    private Quaternion lastRotation = Quaternion.identity;

    // Parameters for all transformations
    private Vector3 lastRaycastedMousePosition = Vector3.zero;
    private Vector3 lastRaycastedMouseNormal = Vector3.zero;

    // Unity events for the transformation operations
    public UnityEvent OnTransformationStart = new UnityEvent();
    public UnityEvent OnTransformationUpdate = new UnityEvent();
    public UnityEvent OnTransformationEnd = new UnityEvent();

    // Public methods to start / stop processing input
    private bool processingInput = true;
    public bool ProcessingInput
    {
        get { return processingInput; }
        set { processingInput = value; }
    }

    /// <summary>
    /// Initializes transformation gizmos.
    /// </summary>
    void Awake()
    {
        inputManager.AddInputHandler(this, inputHandlerPriority);
        gizmoCollection = new GameObject("Gizmo Collection");
        gizmoCollection.transform.SetParent(instanceParent);

        // Initialize arrow directions and gizmo normals
        arrowGizmoNormals = new[] { Vector3.right, Vector3.up, Vector3.forward };
        circleGizmoNormals = new[] { Vector3.right, Vector3.up, Vector3.forward };

        // Instantiate and initialize arrow gizmos
        for (int i = 0; i < numArrowGizmos; i++) {
            ArrowGizmo arrowGizmo = GameObject.Instantiate(arrowGizmoPrefab, gizmoCollection.transform);
            arrowGizmos[i] = arrowGizmo;
            arrowGizmo.transform.position = transform.position;
            arrowGizmo.transform.up = arrowGizmoNormals[i];
            arrowGizmo.transform.localScale = 2 * Vector3.one;
            arrowGizmo.coneScale = 0.05f;
            arrowGizmo.cylinderLength = 0.75f;
            arrowGizmo.cylinderWidth = .02f;
            arrowGizmo.capsuleScale = 5;
            arrowGizmo.SetColor(defaultGizmoColor);
        }

        // Instantiate and initialize circle gizmos
        for (int i = 0; i < numCircleGizmos; i++)
        {
            CircleGizmo circleGizmo = GameObject.Instantiate(circleGizmoPrefab, gizmoCollection.transform);
            circleGizmos[i] = circleGizmo;
            circleGizmo.transform.position = transform.position;
            circleGizmo.transform.localScale = Vector3.one;
            circleGizmo.transform.up = circleGizmoNormals[i];
            circleGizmo.width = 0.05f;
            circleGizmo.epsilon = 0.2f;
            circleGizmo.SetColor(defaultGizmoColor);
        }
    }

    /// <summary>
    /// Activates the gizmos and starts the Idling routine.
    /// </summary>
    public void OnEnable()
    {
        state = State.Idling;
        stateChange = false;
        StartCoroutine(IdlingRoutine());
    }

    /// <summary>
    /// Updates transformation gizmos and handles state changes.
    /// </summary>
    void Update()
    {
        UpdateGizmoTransforms();
        HandleStateChange();
    }

    /// <summary>
    /// If a state change occurs, transition to the new state.
    /// </summary>
    private void HandleStateChange()
    {
        // If statechange
        if (stateChange)
        {
            // Set state change flag to false
            stateChange = false;

            // Appropriately transition to new state
            if (state == State.Idling)
            {
                StartCoroutine(IdlingRoutine());
            }
            else if (state == State.Translating)
            {
                StartCoroutine(TranslatingRoutine());
            }
            else if (state == State.Rotating)
            {
                StartCoroutine(RotatingRoutine());
            }
        }
    }

    /// <summary>
    /// The Idling routine highlights gizmos and handles transitions into different transformation states.
    /// </summary>
    /// <returns>Returns an IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator IdlingRoutine()
    {
        // Iterate through each gizmo
        while (true)
        {
            // If we are processing input
            if (processingInput)
            {
                // Only highlight one gizmo at a time
                bool highlightedGizmo = false;

                // Handle translation gizmos
                for (int i = 0; i < numArrowGizmos; i++)
                {
                    ArrowGizmo arrowGizmo = arrowGizmos[i];

                    // If we are hovering over a gizmo and can handle input
                    if (arrowGizmo.IsBehindMouse() && !highlightedGizmo && inputManager.RequestToHandleInput(this))
                    {
                        // Highlight selected gizmo
                        highlightedGizmo = true;
                        arrowGizmo.SetColor(selectedGizmoColors[i]);

                        // If the left mouse button is clicked, select this gizmo
                        if (Input.GetMouseButtonDown(0))
                        {
                            // Transition to Translating state
                            SelectTranslationGizmo(arrowGizmo);
                            yield break;
                        }
                    }
                    // Otherwise un-highlight the selected gizmo
                    else
                    {
                        arrowGizmo.SetColor(defaultGizmoColor);
                    }
                }

                // Handle rotation gizmos
                for (int i = 0; i < numCircleGizmos; i++)
                {
                    CircleGizmo circleGizmo = circleGizmos[i];

                    // If we are hovering over a gizmo
                    RaycastHit hitInfo;
                    if (circleGizmo.IsBehindMouse(out hitInfo) && !highlightedGizmo && inputManager.RequestToHandleInput(this))
                    {
                        // Highlight selected gizmo
                        highlightedGizmo = true;
                        circleGizmo.SetColor(selectedGizmoColors[i]);

                        // If the left mouse button is clicked, select this gizmo
                        if (Input.GetMouseButtonDown(0))
                        {
                            // Transition to Rotation state
                            SelectRotationGizmo(circleGizmo, hitInfo);
                            yield break;
                        }
                    }
                    // Otherwise un-highlight the selected gizmo
                    else
                    {
                        circleGizmo.SetColor(defaultGizmoColor);
                    }
                }               
            }
            // End frame update
            yield return null;
        }
    }

    /// <summary>
    /// Facilitates a transition to the Translating state.
    /// </summary>
    /// <param name="arrowGizmo">The selected ArrowGizmo we will use to translate.</param>
    private void SelectTranslationGizmo(ArrowGizmo arrowGizmo)
    {
        // Save selected arrow gizmo
        selectedArrowGizmo = arrowGizmo;

        // Record context for transforming routine
        lastPosition = transform.position;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        lastRaycastedMousePosition = Mathematics.ClosestPointOnLine1ToLine2(ray.origin, ray.direction, arrowGizmo.transform.position, arrowGizmo.transform.up);

        // Enter the translating state
        stateChange = true;
        state = State.Translating;
    }


    /// <summary>
    /// The Translating routine translates the Transformable along the selected ArrowGizmo's axis.
    /// </summary>
    /// <returns>Returns an IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator TranslatingRoutine()
    {
        // Call custom translating start event
        OnTransformationStart.Invoke();

        // While the translation is occuring
        while(!Input.GetMouseButtonUp(0))
        {
            // Find the closest point on the screen-to-point-line to the arrow-gizmo-translation-axis-line
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 raycastMousePosition = Mathematics.ClosestPointOnLine1ToLine2(ray.origin, ray.direction, selectedArrowGizmo.transform.position, selectedArrowGizmo.transform.up);

            // Compute the mouseDelta and project it onto the line defined by translation axis
            Vector3 mouseDelta = raycastMousePosition - lastRaycastedMousePosition;
            Vector3 translation = Vector3.Project(mouseDelta, selectedArrowGizmo.transform.up);

            // Update transformed position
            transform.position = lastPosition + translation;

            // Call custom translating update event
            OnTransformationUpdate.Invoke();

            // End frame update
            yield return null;
        }

        // Call custom translating end event
        OnTransformationEnd.Invoke();

        // Enter the idling state
        stateChange = true;
        state = State.Idling;
        yield break;
    }

    /// <summary>
    /// Facilitates a transition to the Rotating state.
    /// </summary>
    /// <param name="circleGizmo">The selected circle gizmo we will use to rotate.</param>
    private void SelectRotationGizmo(CircleGizmo circleGizmo, RaycastHit hitInfo)
    {
        // Save selected circle gizmo
        selectedCircleGizmo = circleGizmo;

        // Record context for transforming routine
        lastRotation = transform.rotation;
        lastRaycastedMousePosition = hitInfo.point;
        lastRaycastedMouseNormal = hitInfo.normal;

        // Enter the rotating state
        stateChange = true;
        state = State.Rotating;
    }

    /// <summary>
    /// The Rotating routine rotates the Transformable along the selected RotationGizmo's axis.
    /// </summary>
    /// <returns>Returns an IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator RotatingRoutine()
    {
        // Call custom translating start event
        OnTransformationStart.Invoke();

        // While the rotation is occuring
        while (!Input.GetMouseButtonUp(0))
        {
            Vector3 circleTangent = Vector3.Cross(lastRaycastedMouseNormal, selectedCircleGizmo.transform.up);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // Find the closest point on the screen-to-point-line to the circle-gizmo-rotation-tangent-line
            Vector3 raycastMousePosition = Mathematics.ClosestPointOnLine1ToLine2(ray.origin, ray.direction, lastRaycastedMousePosition, circleTangent);

            // Compute the mouseDelta and project it onto the line defined by translation axis
            Vector3 mouseDelta = raycastMousePosition - lastRaycastedMousePosition;
            Vector3 rotationVector = Vector3.Project(mouseDelta, circleTangent);

            float rotationSign = -Mathf.Sign(Vector3.Dot(rotationVector, circleTangent));
            float rotationMagnitude = rotationVector.magnitude;

            // Update transformed position

            Quaternion rotation = Quaternion.Euler(rotationSign * rotationSpeed * rotationMagnitude * selectedCircleGizmo.transform.up);
            transform.rotation = rotation * lastRotation; 

            // Call custom translating update event
            OnTransformationUpdate.Invoke();

            yield return null;
        }

        // Call custom translating end event
        OnTransformationEnd.Invoke();

        // Enter the idling state
        stateChange = true;
        state = State.Idling;
        yield break;
    }

    /// <summary>
    /// Updates each of the gizmo positions to the Transformable's position.
    /// </summary>
    private void UpdateGizmoTransforms()
    {
        // Update arrow gizmos
        foreach (ArrowGizmo arrowGizmo in arrowGizmos)
        {
            arrowGizmo.transform.position = transform.position;
        }

        // Update circle gizmos
        foreach (CircleGizmo circleGizmo in circleGizmos)
        {
            circleGizmo.transform.position = transform.position;
        }
    }

    /// <summary>
    /// Returns whether or not the transformable is requesting to handle input.
    /// </summary>
    /// <returns>Whether or not the camera controller is requesting to handle input.</returns>
    public bool RequestingToHandleInput()
    {
        // If any of the gizmos are behind the mouse, we are requesting to handle input
        foreach (ArrowGizmo arrowGizmo in arrowGizmos)
        {
            if (arrowGizmo.IsBehindMouse())
                return true;
        }
        foreach (CircleGizmo circleGizmo in circleGizmos)
        {
            if (circleGizmo.IsBehindMouse())
                return true;
        }

        return false;
    }
}

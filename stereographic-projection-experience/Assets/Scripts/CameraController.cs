using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A MonoBehaviour that allows the player to control the camera using the mouse.
/// </summary>
public class CameraController : MonoBehaviour, IInputHandler
{
    public InputManager inputManager;
    public int inputHandlerPriority = 0;

    // Camera orbit parameters
    public float orbitSpeed = 1;
    Vector3 lastMousePosition = Vector3.zero;
    Vector3 lastLocalEulerAngles = Vector3.zero;

    // Camera move parameters
    public float moveSpeed = 1;

    /// <summary>
    /// Initializes the camera controller and starts the Idling routine.
    /// </summary>
    void Start()
    {
        inputManager.AddInputHandler(this, inputHandlerPriority);

        // Start the idling state routine
        StartCoroutine(IdlingRoutine());

        // Start the always active moving routine
        StartCoroutine(MovingRoutine());
    }

    /// <summary>
    /// The Idling routine waits for mouse input.
    /// </summary>
    /// <returns>Returns an IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator IdlingRoutine()
    {
        while (true)
        {
            // If the left mouse button is clicked, begin orbit
            if (Input.GetMouseButtonDown(0) && inputManager.RequestToHandleInput(this))
            {
                // Record orbit context
                lastMousePosition = Input.mousePosition;
                lastLocalEulerAngles = transform.localEulerAngles;

                // Transition to orbiting state
                StartCoroutine(OrbitingRoutine());
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// The Rotating routine handles the camera orbit.
    /// </summary>
    /// <returns>Returns an IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator OrbitingRoutine()
    {
        // While the orbit is occuring
        while (!Input.GetMouseButtonUp(0))
        {
            // Use mouseDelta to rotate camera
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            Vector3 eulerAnglesOffset = new Vector3(mouseDelta.y, -mouseDelta.x, 0);
            transform.eulerAngles = lastLocalEulerAngles + orbitSpeed * eulerAnglesOffset;
            yield return null;
        }

        // Transition to idling state
        StartCoroutine(IdlingRoutine());

        yield break;
    }

    /// <summary>
    /// The moving routine allows the player to move the camera forward using the scroll wheel.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MovingRoutine()
    {
        while (true)
        {
            float mouseScrollDelta = Input.mouseScrollDelta.y;
            if (mouseScrollDelta != 0 && inputManager.RequestToHandleInput(this))
            {
                transform.position += mouseScrollDelta * moveSpeed * Time.deltaTime * transform.forward;
            }

            yield return null;
        }
        
    }

    /// <summary>
    /// Returns whether or not the camera controller is requesting to handle input.
    /// </summary>
    /// <returns>Whether or not the camera controller is requesting to handle input.</returns>
    public bool RequestingToHandleInput()
    {
        // We only request to handle input if the mouse button is clicked.
        return Input.GetMouseButton(0) || Input.mouseScrollDelta.y != 0;
    }
}
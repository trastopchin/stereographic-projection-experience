using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A task that requires the player to turn around to look at and click the glowing Riemann sphere.
/// </summary>
public class OrientationTask : GameTask, IInputHandler
{
    // Interface with the input manager
    public InputManager inputManager;
    public int inputHandlerPriority = 0;

    // The next game task
    public GameTask nextGameTask;

    // Script objects needed for the task
    public GameObject riemannSphereCollectionStatic;
    public RiemannSphere riemannSphere;

    // Parameters for the task
    public Color glowingColor;
    public float glowingSpeed;

    /// <summary>
    /// Registers to the task manager and input manager. Sets the riemannSphereCollectionStatic game object to active and starts the OrientationTaskRoutine and GlowingRoutines.
    /// </summary>
    private void OnEnable()
    {
        // Register task to task manager and input handler
        AutoRegisterTaskToManager();
        inputManager.AddInputHandler(this, inputHandlerPriority);

        riemannSphereCollectionStatic.SetActive(true);

        StartCoroutine(OrientationTaskRoutine());
        StartCoroutine(GlowingRoutine());
    }

    /// <summary>
    /// Removes the task form the input handler and stops the sphere glowing.
    /// </summary>
    private void OnDisable()
    {
        // Complete task and remove it from the input handler
        inputManager.RemoveInputHandler(this);

        // Reset glowing
        riemannSphere.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
    }

    /// <summary>
    /// This routine transitions to the next game task if the player left-clicks the Riemann sphere.
    /// </summary>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator OrientationTaskRoutine()
    {
        while (true)
        {
            // If the mouse button is down and the mouse is over the riemann sphere
            // A call to inputManager.RequestToHandleInput(this) implicitly calls this.RequestingToHangleInput() which is why we don't have to perform an additional riemann sphere raycast here. The raycast happens behind the scenes.
            if (Input.GetMouseButtonDown(0) && inputManager.RequestToHandleInput(this))
            {
                TransitionToTask(nextGameTask);
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Routine that makes the Riemann sphere glow.
    /// </summary>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator GlowingRoutine()
    {
        riemannSphere.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");

        while (true)
        {
            riemannSphere.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.Lerp(Color.black, glowingColor, (0.5f*(1+Mathf.Sin(glowingSpeed * Time.time)))));
            yield return null;
        }
    }

    /// <summary>
    /// Requests to handle input when the mouse is over the riemann sphere.
    /// </summary>
    /// <returns>Whether or not the mouse is over the riemann sphere.</returns>
    public bool RequestingToHandleInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        return riemannSphere.GetComponent<Collider>().Raycast(ray, out hitInfo, 100.0f);
    }
}

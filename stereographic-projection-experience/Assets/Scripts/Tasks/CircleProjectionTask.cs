using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Task that requires the player to stereographically project a PointCircle.
/// </summary>
public class CircleProjectionTask : GameTask
{
    // The next game task
    public GameTask nextGameTask;

    // Script objects relevant to the task
    public GameObject riemannSphereCollection;
    public RiemannSphere riemannSphere;
    public Transformable transformable;

    // Whether or not it's the inverse projection
    public bool inverse = true;

    // The point circle we are using
    public PointCircle playerCircle;

    /// <summary>
    /// Initializes the task objects.
    /// </summary>
    private void Awake()
    {
        // If it's the inverse projection
        if (inverse)
        {
            // Make the player circle active
            playerCircle.gameObject.SetActive(true);

            // Add the correct OnProjected UnityAction
            playerCircle.OnProjectedToSphere.AddListener(OnCircleProjection);

        }
        // Otherwise the player circle is already active
        else
        {
            // Add the correct OnProjected UnityAction
            playerCircle.OnProjectedToPlane.AddListener(OnCircleProjection);
        }

        // Add the SetParent and UnParentAndApplyTransform actions
        transformable.OnTransformationStart.AddListener(playerCircle.SetRiemannSphereAsParent);
        transformable.OnTransformationEnd.AddListener(playerCircle.UnParentAndApplyTransform);
    }

    /// <summary>
    /// Initalizes the task.
    /// </summary>
    private void OnEnable()
    {;
        AutoRegisterTaskToManager();
    }

    /// <summary>
    /// Stops and cleans up the task.
    /// </summary>
    private void OnDisable()
    {
        // Removes all the added player circle listeners
        playerCircle.OnProjectedToSphere.RemoveAllListeners();
        playerCircle.OnProjectedToSphere.RemoveAllListeners();
        

        // If not the inverse, then we have to clean up
        if (!inverse)
        {
            // Remove all added transformable listeners.
            transformable.OnTransformationStart.RemoveAllListeners();
            transformable.OnTransformationEnd.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Continuously compares the player circle to the target circle.
    /// </summary>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private void OnCircleProjection()
    {
        TransitionToTask(nextGameTask);
    }
}

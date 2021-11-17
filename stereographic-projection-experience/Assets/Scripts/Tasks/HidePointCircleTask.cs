using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A task that just hides the specified PointCircle.
/// </summary>
public class HidePointCircleTask : GameTask
{
    // The next game task
    public GameTask nextGameTask;
    public PointCircle pointCircle;

    private void OnEnable()
    {
        AutoRegisterTaskToManager();
    }

    public void Start()
    {
        // Hide the specified point circle on start
        pointCircle.gameObject.SetActive(false);

        // Transition to the next task
        TransitionToTask(nextGameTask);
    }
}

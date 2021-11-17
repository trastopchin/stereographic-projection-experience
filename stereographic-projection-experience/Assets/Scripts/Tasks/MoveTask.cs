using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A task that requires the player to use the mouse scroll wheel to move the camera.
/// </summary>
public class MoveTask : GameTask
{
    // The next game task
    public GameTask nextGameTask;

    // Parameters needed for the task
    public CameraController cameraController;
    public float moveDistance = 1;
    private Vector3 lastPosition = Vector3.zero;

    private void OnEnable()
    {
        lastPosition = cameraController.transform.position;

        AutoRegisterTaskToManager();
        StartCoroutine(MoveTaskRoutine());
    }

    /// <summary>
    /// A routine that transitions to the next task if the player moves the camera controller more than the specified distance.
    /// </summary>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator MoveTaskRoutine()
    {
        while(true)
        {
            if ((cameraController.transform.position - lastPosition).magnitude > moveDistance)
            {
                TransitionToTask(nextGameTask);
                yield break;
            }
            yield return null;
        }
    }
}

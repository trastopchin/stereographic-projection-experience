using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A task that requires a player to interact with a transformable to a specified extent.
/// </summary>
public class TransformableTask : GameTask
{
    // The next game task
    public GameTask nextGameTask;

    // The task parameters
    public Transformable transformable;
    public float translationDistanceGoal = 1;
    public float rotationDistanceGoal = 60;

    // The last position and euler angles
    private Vector3 startPosition = Vector3.zero;
    private Vector3 startEulerAngles = Vector3.zero;

    /// <summary>
    /// Registers the task to the task manger. Records the start position and euler angles.
    /// </summary>
    private void OnEnable()
    {
        AutoRegisterTaskToManager();

        // Record start position and euler angles
        startPosition = transformable.transform.position;
        startEulerAngles = transformable.transform.eulerAngles;

        StartCoroutine(TransformGoalRoutine());
    }

    /// <summary>
    /// A routine that continously checks if the transformation goal was met.
    /// </summary>
    /// <returns>An IEnumerator that can be invoked as a coroutine.</returns>
    private IEnumerator TransformGoalRoutine()
    {
        while (true)
        {
            // Determine if the goals were met
            bool metTranslationGoal = (transformable.transform.position - startPosition).magnitude >= translationDistanceGoal;
            Vector3 rotationOffset = (transformable.transform.eulerAngles - startEulerAngles);
            float rotationDelta = Mathf.Abs(rotationOffset.x) + Mathf.Abs(rotationOffset.y) + Mathf.Abs(rotationOffset.z);
            bool metRotationGoal = rotationDelta >= rotationDistanceGoal;
            
            // Check if the transformation goal was met
            if (metTranslationGoal && metRotationGoal)
            {
                TransitionToTask(nextGameTask);
                yield break;
            }
            yield return null;
        }
    }

}

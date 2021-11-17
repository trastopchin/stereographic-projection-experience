using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A task that sets the static riemann sphere collection inactive and activate sthe tranformable riemann sphere collection.
/// </summary>
public class StaticToTransformableTask : GameTask
{
    // The next game task
    public GameTask nextGameTask;

    public GameObject riemannSphereCollectionStatic;
    public GameObject riemannSphereCollectionTransformable;

    private void OnEnable()
    {
        AutoRegisterTaskToManager();

        riemannSphereCollectionStatic.SetActive(false);
        riemannSphereCollectionTransformable.SetActive(true);

        TransitionToTask(nextGameTask);
    }
}


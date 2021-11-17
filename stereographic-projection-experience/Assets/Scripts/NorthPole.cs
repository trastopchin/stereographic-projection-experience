using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A helper class to ensure the north pole game object stays at the Riemann sphere's north pole.
/// </summary>
public class NorthPole : MonoBehaviour
{
    public RiemannSphere riemannSphere;

    // Update is called once per frame
    void Update()
    {
        // Set the location to the Riemann sphere's north pole.
        transform.position = riemannSphere.NorthPolPos;
    }
}

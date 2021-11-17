using System.Collections;
using UnityEngine;

/// <summary>
/// A class used as a library of generic helper functions.
/// </summary>
public class Utils
{
    /// <summary>
    /// The AdaptiveScaleRoutine adaptively scales the target object with respect to its distance from the camera object.
    /// </summary>
    /// <param name="target">The target object that is being adaptively scaled.</param>
    /// <param name="camera">The camera object used as a reference for scaling.</param>
    /// <param name="nearDist">The distance at which the target object gets scaled by the nearScale.</param>
    /// <param name="nearScale">The scale multiplier applied to the target at the near distance.</param>
    /// <param name="farDist">The distance at which the target object gets scaled by the farScale.</param>
    /// <param name="farScale">The scale multiplier applied to the target at the far distance.</param>
    /// <param name="scaleFactorObject">An optional Float object that passes a scale factor by reference.</param>
    /// <returns>An IEnumerator that can be invoked as a coroutine</returns>
    public static IEnumerator AdaptiveScaleRoutine(GameObject target, GameObject camera, float nearDist, float nearScale, float farDist, float farScale, Vector3O scaleFactorObject)
    {
        // Record target scale
        Vector3 targetScale = target.transform.localScale;

        while (true)
        {
            // Compute distance parameter
            float dist = (target.transform.position - camera.transform.position).magnitude;

            // Linearly interpolate between the nearScale and farScale defined at the reference nearDist and farDist distances.
            float blendScale = Mathf.LerpUnclamped(nearScale, farScale, (dist - nearDist) / (farDist - nearDist));

            // Retrieve scale factor.
            Vector3 scaleFactor = Vector3.one;
            if (scaleFactorObject != null)
            {
                scaleFactor = scaleFactorObject.data;
            }

            // Update transform
            target.transform.localScale = Vector3.Scale(scaleFactor, blendScale * targetScale);
            yield return null;
        }
    }

    /// <summary>
    /// Set's a game object transform's active property recursively.
    /// </summary>
    /// <param name="gameObject">The game object we are setting active recursively.</param>
    /// <param name="active">Whether or not we are setting the game object(s) active.</param>
    public static void SetActiveRecursively(GameObject gameObject, bool active)
    {
        gameObject.SetActive(active);
        foreach (Transform child in gameObject.transform)
        {
            SetActiveRecursively(child.gameObject, active);
        }
    }

    /// <summary>
    /// Orients a cylinder in object space.
    /// </summary>
    /// <param name="cylinder">The cylindrical object to orient.</param>
    /// <param name="point1">The position the cylinder's body should start.</param>
    /// <param name="point2">The position the cylinder's body should end.</param>
    /// <param name="radius">The radius of the cylinder.</param>
    public static void OrientCylinderLocal(GameObject cylinder, Vector3 point1, Vector3 point2, float radius)
    {
        Vector3 midpoint = Vector3.Lerp(point1, point2, .5f);
        cylinder.transform.localPosition = midpoint;
        cylinder.transform.localScale = new Vector3(radius, (point2 - point1).magnitude / 2, radius);
    }

    /// <summary>
    /// Orients a cylinder in world space.
    /// </summary>
    /// <param name="cylinder">The cylindrical object to orient.</param>
    /// <param name="point1">The position the cylinder's body should start.</param>
    /// <param name="point2">The position the cylinder's body should end.</param>
    /// <param name="radius">The radius of the cylinder.</param>
    public static void OrientCylinderGlobal(GameObject cylinder, Vector3 point1, Vector3 point2, float radius)
    {
        Vector3 midpoint = Vector3.Lerp(point1, point2, .5f);
        cylinder.transform.position = midpoint;
        cylinder.transform.up = (point2 - point1).normalized;
        cylinder.transform.localScale = new Vector3(radius, (point2 - point1).magnitude / 2, radius);
    }

    /// <summary>
    /// Returns a random point inside a circle.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>A random point inside the circle with specified radius centered at the origin.</returns>
    public static Vector3 RandomInsideCircle(float radius)
    {
        return radius * 2 * new Vector3(Random.value - .5f, 0, Random.value - .5f);
    }

    /// <summary>
    /// Returns a point of the unit circle.
    /// </summary>
    /// <param name="t">The angle we evaluate the unit circle at. Scaled so that theta = t*2*pi.</param>
    /// <returns>A point of the unit circle corresponding to the angle theta.</returns>
    public static Vector3 UnitCircle(float t)
    {
        t *= Mathf.PI * 2;
        return new Vector3(Mathf.Cos(t), 0, Mathf.Sin(t));
    }

    public static Vector3 divide(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }
}

/// <summary>
/// Simple class that encapsulates a Vector3 struct in an object.
/// For the purposes of passing a Vector3 by reference into a coroutine.
/// </summary>
public class Vector3O
{
    public Vector3 data;

    public Vector3O(Vector3 data)
    {
        this.data = data;
    }
}

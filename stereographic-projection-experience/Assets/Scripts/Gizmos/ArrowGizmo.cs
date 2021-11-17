using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An arrow gizmo that can be positioned, oriented, and raycasted.
/// </summary>
public class ArrowGizmo : MonoBehaviour
{
    // The head of the arrow gizmo
    public GameObject cone;
    public float coneScale = 0.1f;

    // The body of the arrow gizmo
    public GameObject cylinder;
    public float cylinderLength = 0.1f;
    public float cylinderWidth = 0.1f;

    // A capsule used for raycasting the arrow gizmo
    public GameObject capsule;
    public float capsuleScale = 1f;

    /// <summary>
    /// Updates the arrow before any call Update.
    /// </summary>
    public void Start()
    {
        UpdateArrow();
    }

    /// <summary>
    /// Update's the head, body, and capsule of the arrow in object space.
    /// </summary>
    public void UpdateArrow()
    {
        // Update cone in object space
        cone.transform.localPosition = Vector3.zero + cylinderLength * Vector3.up;
        cone.transform.localScale = coneScale * Vector3.one;
        
        // Update cylinder in object space
        Utils.OrientCylinderLocal(cylinder, Vector3.zero, cylinderLength * Vector3.up, cylinderWidth);
        
        // Update capsule in object space
        capsule.transform.localScale = coneScale * coneScale * Vector3.one;
        Utils.OrientCylinderLocal(capsule, Vector3.zero, (cylinderLength + coneScale) * Vector3.up, cylinderWidth * capsuleScale);
     }

    /// <summary>
    /// Helper method exposes a raycast to the ArrowGizmo's capsule.
    /// </summary>
    /// <param name="ray">The ray that is being casted.</param>
    /// <param name="hitInfo">The raycast hit info that is being written out.</param>
    /// <param name="maxDistance">The maximum raycast distance.</param>
    /// <returns>Whether or not ray hit the capsule collider.</returns>
    public bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
    {
        return capsule.GetComponent<Collider>().Raycast(ray, out hitInfo, maxDistance);
    }

    /// <summary>
    /// Helper method returns whether or not the screen-point-to-ray intersects the ArrowGizmo's capsule.
    /// </summary>
    /// <returns>Whether or not the screen-point-to-ray intersects the ArrowGizmo's capsule.</returns>
    public bool IsBehindMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        return Raycast(ray, out hitInfo, 100.0f);
    }

    /// <summary>
    /// Sets an ArrowGizmo's colors.
    /// </summary>
    /// <param name="color">The color we are setting.</param>
    public void SetColor(Color color)
    {
        // Iterate through each of the GameObjects composing the ArrowGizmo
        foreach (Transform child in transform)
        {
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            // Only update the material color if we can
            if (meshRenderer != null)
            {
                Material material = meshRenderer.material;
                material.SetColor("_Emission", color);
            }
        }
    }
}

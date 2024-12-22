using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitcher : MonoBehaviour
{
    private Material[][] originalMaterials;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeMaterials(Material newMaterial)
    {
        // Find all renderers in the object and its children
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Store the original materials before changing
        originalMaterials = new Material[renderers.Length][];

        // Replace materials with the new one
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            // Store the original materials array
            originalMaterials[i] = renderer.materials;

            // Create an array with the same size but filled with the new material
            Material[] newMaterials = new Material[renderer.materials.Length];
            for (int j = 0; j < newMaterials.Length; j++)
            {
                newMaterials[j] = newMaterial;  // Apply new material to all slots
            }

            // Set the new materials array to the renderer
            renderer.materials = newMaterials;
        }
    }

    public void RestoreOriginalMaterials()
    {
        if (originalMaterials == null || originalMaterials.Length == 0)
        {
            Debug.LogWarning("Original materials have not been set.");
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            // Restore the original materials
            if (i < originalMaterials.Length)
            {
                renderer.materials = originalMaterials[i];
            }
        }
    }
}

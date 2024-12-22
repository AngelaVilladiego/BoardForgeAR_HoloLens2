using UnityEngine;
using System.Collections.Generic;


public class InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel; // Panel holding the inventory UI
    public GameObject inventoryItemPrefab; // The prefab for individual items
    public Transform gridContainer; // The GridLayoutGroup object
    public List<GameObject> gameObjects; // The list of GameObjects for inventory
    public Camera previewCamera; // Camera for rendering previews
    public RenderTexture previewRenderTexture; // Shared render texture


    private void Start()
    {
        PopulateInventory();
        inventoryPanel.SetActive(true); // Hide inventory at start
    }

    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    private void PopulateInventory()
    {
        foreach (var obj in gameObjects)
        {
            // Create an inventory item instance
            var item = Instantiate(inventoryItemPrefab, inventoryPanel.transform);
            var inventoryItem = item.GetComponent<InventoryItem>();

            // Initialize with name and prefab
            inventoryItem.Initialize(obj.name, obj);
        }
    }


    private RenderTexture GeneratePreview(GameObject asset)
    {
        // Temporary instance of the object for rendering
        GameObject temp = Instantiate(asset);
        temp.transform.position = previewCamera.transform.position + previewCamera.transform.forward * 2.0f;
        temp.transform.rotation = Quaternion.identity;

        // Render preview
        previewCamera.targetTexture = previewRenderTexture;
        previewCamera.Render();

        // Cleanup temporary object
        Destroy(temp);

        return previewRenderTexture;
    }
}

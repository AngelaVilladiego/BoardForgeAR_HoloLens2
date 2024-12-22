using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public GameObject modelContainer; // The parent for the 3D model
    public TextMeshProUGUI itemName; // Displays the name (optional)
    public GameObject itemPrefab; // The GameObject to spawn when selected
    private Button button;
    public GameObject spawnLocation;


    public void Initialize(string name, GameObject prefab)
    {
        itemName.text = name;
        itemPrefab = prefab;

        // Spawn the 3D preview model
        GameObject previewModel = Instantiate(prefab, modelContainer.transform);
        previewModel.transform.localPosition = Vector3.zero;
        previewModel.transform.localRotation = Quaternion.identity;
        previewModel.transform.localScale = Vector3.one * 0.1f; // Scale down for the preview
    }

    public void OnSelect()
    {
        // Logic when item is selected
        Debug.Log("Selected item: " + itemName.text);
        Instantiate(itemPrefab, Vector3.zero, Quaternion.identity); // Spawn in the world
    }

    void Start()
    {
        button = GetComponent<Button>(); // Get the Button component attached to this GameObject
        if (button != null)
        {
            button.onClick.AddListener(OnItemClicked); // Add listener for button click
        }
    }

    // Method to be called when the item is clicked
    private void OnItemClicked()
    {
        Debug.Log("Item selected: " + itemName.text); // You can replace this with more complex logic
        // Call the method to handle the selection (e.g., show details, equip item, etc.)
        SpawnItemInPlaySpace();
    }

    private void SpawnItemInPlaySpace()
    {
        if (spawnLocation != null && itemPrefab != null)
        {
            // Instantiate the item at the spawn location
            Instantiate(itemPrefab, spawnLocation.transform.position, Quaternion.identity);
        }
    }
    // This function can be expanded to do other actions when an item is selected
    private void HandleItemSelection()
    {
        // Example: Highlight the selected item or show detailed information
        // Example: Update UI to show item details or perform an action (like equipping or using the item)
        // (You can expand this part later)
    }
}

using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField]
    public GameObject menu;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private float offsetDistance = 0.01f;

    private Transform currentTarget;
    private BoundsControl currentBoundsControl;
    public bool ShouldUpdatePosition = false;

    private void Awake()
    {
        // Enforcing Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (currentTarget != null && currentBoundsControl != null && ShouldUpdatePosition)
        {
            Bounds bounds = currentBoundsControl.CurrentBounds;
            var vec = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);

            UpdateMenuPosition();
        }
    }

    /// <summary>
    /// Show the menu at the correct position for the clicked object.
    /// </summary>
    public void ShowMenu(GameObject clickedObject)
    {
        var boundsControl = clickedObject.GetComponent<BoundsControl>();
        // If the menu is already visible for the current target, do nothing
        if (boundsControl == currentBoundsControl) return;

        // else
        Debug.Log("New object clicked");

        // Update the new target
        currentBoundsControl = boundsControl;
        currentTarget = boundsControl.transform;

        // Show the menu
        menu.SetActive(true);

        UpdateMenuPosition();
    }

    /// <summary>
    /// Hide the menu.
    /// </summary>
    public void HideMenu()
    {
        if (menu != null)
        {
            menu.SetActive(false);
            currentTarget = null;
            currentBoundsControl = null;
        }
    }

    /// <summary>
    /// Update the menu's position to stay at the frontmost corner relative to the camera.
    /// </summary>
    private void UpdateMenuPosition()
    {
        // Calculate the bounds of the object
        Bounds bounds = currentBoundsControl.CurrentBounds;

        // Get the corners of the bounds in local space
        Vector3[] corners = new Vector3[]
        {
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), // Top-Left-Back
            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), // Top-Left-Front
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), // Top-Right-Back
            new Vector3(bounds.max.x, bounds.max.y, bounds.max.z)  // Top-Right-Front
        };

        // Transform the corners to world space
        Vector3 frontmostCorner = corners[0];
        float closestDistance = float.MaxValue;

        foreach (Vector3 corner in corners)
        {
            // Convert each corner to world space
            Vector3 worldCorner = currentTarget.TransformPoint(corner);

            // Check the distance to the camera (favor closer corners)
            Vector3 toCamera = (mainCamera.transform.position) - worldCorner;
            float distance = toCamera.magnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                frontmostCorner = worldCorner;
            }
        }

        // Offset the menu outward, ensuring it's always in front
        Vector3 cameraDirection = (mainCamera.transform.position - frontmostCorner).normalized;
        Vector3 menuPosition = frontmostCorner + cameraDirection * offsetDistance;

        // Update menu position and orientation
        menu.transform.position = menuPosition;
        menu.transform.LookAt(mainCamera.transform);
        menu.transform.rotation = Quaternion.LookRotation(menu.transform.position - mainCamera.transform.position);
    }
}

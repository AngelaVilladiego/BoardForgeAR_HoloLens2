using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

public class BoardMenuFollower : MonoBehaviour
{

    [SerializeField]
    private Transform boardTransform;

    [SerializeField]
    [Tooltip("Distance away from the board's edge along the z-axis")]
    private float zOffset = 0.01f;

    [SerializeField]
    [Tooltip("Distance above the boards origin along the y-axis")]
    private float yOffset = 0.05f;

    private Vector3 boardSize;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMenuPositionAndRotation();
    }

    private void UpdateMenuPositionAndRotation()
    {
        // Calculate the world-space forward direction of the board
        Vector3 worldForward = boardTransform.rotation * Vector3.forward;  // "In front" the board

        // Calculate the board's world-space size on the Z-axis
        float boardHalfZ = boardTransform.lossyScale.z / 2f;  // Use world scale to account for hierarchy

        // Calculate the position offset in world space
        Vector3 positionOffset = worldForward * (boardHalfZ + zOffset);  // Place the menu behind the board

        // Set the menu's position in world space
        Vector3 targetPosition = boardTransform.position + positionOffset;
        targetPosition.y += yOffset;  // Apply vertical offset
        transform.position = targetPosition;

        // Apply the board's rotation and flip the Y-axis while maintaining original X and Z rotations
        Quaternion currentRotation = transform.rotation;
        Quaternion flippedYRotation = Quaternion.Euler(0, boardTransform.eulerAngles.y + 180f, 0);
        transform.rotation = flippedYRotation * Quaternion.Euler(currentRotation.eulerAngles.x, 0, currentRotation.eulerAngles.z);
    }
}

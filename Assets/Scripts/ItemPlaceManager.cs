using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using MixedReality.Toolkit.SpatialManipulation;
using System.Linq;
using System;
using MixedReality.Toolkit;

public class ItemPlaceManager : MonoBehaviour
{
    public static ItemPlaceManager Instance;

    [SerializeField]
    private GameObject inventoryMenuObject;

    [SerializeField]
    private GameObject boardObject;

    [SerializeField]
    public float boardAndMenuXOffset = 0.1f;

    [SerializeField]
    public float boardAndMenuYOffset = 0.06f;

    [SerializeField]
    public float boardAndMenuZOffset = 0.06f;

    [SerializeField]
    private GameObject boardItemsParent;

    [SerializeField]
    private float rotationSlerpInterpolationSpeed = 2.5f;

    [SerializeField]
    private float scaleCorrectionLerpInterpolationSpeed = 4.2f;

    private float rotationLerpTimeCount = 0.0f;
    private float positionLerpTimeCount = 0.0f;
    private float scaleLerpTimeCount = 0.0f;

    private GameObject currentActiveObject;
    private Vector3 initialSpawnLocation;
    private List<GameObject> boardObjects = new List<GameObject>();
    private List<TransformConstraint> addedConstraints = new List<TransformConstraint>();

    private bool isLerpingRotation = false;
    private Quaternion? lerpFromRotation = null;
    private Quaternion? lerpToRotation = null;

    private bool isLerpingPosition = false;
    private Vector3? lerpFromPosition = null;
    private Vector3? lerpToPosition = null;

    private bool isLerpingScale = false;
    private Vector3? lerpFromScale = null;
    private Vector3? lerpToScale = null;

    private Dictionary<int, Transform> objectOriginalTransforms = new Dictionary<int, Transform>();

    private enum ManipulationState
    {
        None,
        Translation,
        Rotation,
        Scaling
    }

    private ManipulationState currentManipulationState = ManipulationState.None;

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
        //StartItemPlacePhase();
    }

    // Update is called once per frame
    void Update()
    {
        if ((isLerpingRotation) && (currentActiveObject == null || lerpFromRotation == null || lerpToRotation == null))
        {
            ResetRotationLerp();
        } else if (isLerpingRotation)
        {
            //Debug.Log("Slerping??");
            currentActiveObject.transform.rotation = Quaternion.Lerp((Quaternion)lerpFromRotation, (Quaternion)lerpToRotation, rotationLerpTimeCount * rotationSlerpInterpolationSpeed);
            rotationLerpTimeCount = rotationLerpTimeCount + Time.deltaTime;

            if (Math.Abs(currentActiveObject.transform.rotation.eulerAngles.y - ((Quaternion)lerpToRotation).eulerAngles.y) <= 1)
            {
                ResetRotationLerp();
            }
        }

        if ((isLerpingPosition) && (currentActiveObject == null || lerpFromPosition == null || lerpToPosition == null))
        {
            Debug.Log("resetting lerp?");
            ResetPositionLerp();
        }
        else if (isLerpingPosition)
        {
            Debug.Log("Lerping pls?");
            currentActiveObject.transform.position = Vector3.Lerp((Vector3)lerpFromPosition, (Vector3)lerpToPosition, positionLerpTimeCount * scaleCorrectionLerpInterpolationSpeed);
            positionLerpTimeCount = positionLerpTimeCount + Time.deltaTime;

            if (Vector3.Distance(currentActiveObject.transform.position , (Vector3)lerpToPosition) <= 0.0000001f)
            {
                Debug.Log(" Stopping, position is close enough");
                ResetPositionLerp();
            }
        }

        if ((isLerpingScale) && (currentActiveObject == null || lerpFromScale == null || lerpToScale == null))
        {
            Debug.Log("resetting scale lerp?");
            ResetScaleLerp();
        }
        else if (isLerpingScale)
        {
            Debug.Log("Lerping scale pls?");
            currentActiveObject.transform.localScale = Vector3.Lerp((Vector3)lerpFromScale, (Vector3)lerpToScale, scaleLerpTimeCount * scaleCorrectionLerpInterpolationSpeed);
            scaleLerpTimeCount = scaleLerpTimeCount + Time.deltaTime;

            if (Math.Abs((currentActiveObject.transform.lossyScale - ((Vector3)lerpToScale)).magnitude) <= 0.00001f)
            {
                ResetScaleLerp();
            }
        }

    }

    // this stuff is awful just make a darn class to handle this ugh
    // bad code *bonk*

    private void ResetPositionLerp()
    {
        Debug.Log("Resetting pos lerp");
        lerpFromPosition = null;
        lerpToPosition = null;
        isLerpingPosition = false;
        positionLerpTimeCount = 0;
    }

    private void ResetScaleLerp()
    {
        Debug.Log("Resetting scale lerp");
        lerpFromScale = null;
        lerpToScale = null;
        isLerpingScale = false;
        scaleLerpTimeCount = 0;
    }

    public void StartItemPlacePhase()
    {
        Debug.Log("Starting Item Place Phase...");
        // Get the board's top surface using its collider
        Collider boardCollider = boardObject.GetComponent<Collider>();
        if (boardCollider == null)
        {
            Debug.LogError("Board object must have a Collider component!");
            return;
        }
        float boardTopY = boardCollider.bounds.max.y; // Top surface Y position of the board

        initialSpawnLocation = new Vector3(boardObject.transform.position.x, boardTopY, boardObject.transform.position.z); // Top center of the board
        PositionMenuNextToBoard();
        inventoryMenuObject.SetActive(true);
    }

    public void SpawnPrefab(GameObject spawnPrefab)
    {
        ResetPositionLerp();
        ResetScaleLerp();
        ResetRotationLerp();

        if (currentActiveObject != null)
        {
            AcceptObjectChanges();
        }

        currentActiveObject = Instantiate(spawnPrefab, initialSpawnLocation, Quaternion.identity);

        //AlignItemBottomAndBoardTop(initialSpawnLocation.x, initialSpawnLocation.z);

        //Debug.Log("BEFORE LOCATION: " + currentActiveObject.transform.position);
        //// Get the object's half-height using its collider and calculate the offset of its transform origin/pivot to the collider's center
        //Collider objectCollider = currentActiveObject.GetComponent<Collider>();
        //if (objectCollider == null)
        //{
        //    Debug.LogError("object must have a Collider component!");
        //    return;
        //}

        //Vector3 colliderCenter = objectCollider.bounds.center;
        //Vector3 pivotToCenterOffset = colliderCenter - currentActiveObject.transform.position;

        //// Calculate the bottom of the object relative to its pivot
        //float objectHalfHeight = objectCollider.bounds.extents.y; // Half-height already
        //float objectBottomOffset = objectHalfHeight + pivotToCenterOffset.y;

        Vector3 newPosition = GetPositionVectorForBoardItemAlignment(initialSpawnLocation.x, initialSpawnLocation.z);

        // Adjust position so bottom aligns with board's top
        //Vector3 newPosition = initialSpawnLocation + new Vector3(0, objectBottomOffset, 0);
        //Debug.Log("AFTER LOCATION:  " + newPosition);
        currentActiveObject.transform.position = newPosition;

        currentActiveObject.transform.SetParent(boardItemsParent.transform, true);

        boardObjects.Add(currentActiveObject);
        objectOriginalTransforms.Add(currentActiveObject.GetInstanceID(), currentActiveObject.transform);

        EnableObjectInteraction(currentActiveObject);

        // Start with movement already selected and menu visible
        AllowMoveActiveObject();
        MenuManager.Instance.ShowMenu(currentActiveObject);
        MenuManager.Instance.ShouldUpdatePosition = true;
    }

    private void DisableObjectInteraction(GameObject obj)
    {
        ObjectManipulator objectManipulator = obj.GetComponent<ObjectManipulator>();

        objectManipulator.AllowedInteractionTypes = InteractionFlags.None;
        objectManipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.None;

        obj.GetComponent<Collider>().enabled = false;
    }

    private void EnableObjectInteraction(GameObject obj)
    {
        ObjectManipulator objectManipulator = obj.GetComponent<ObjectManipulator>();

        objectManipulator.AllowedInteractionTypes = InteractionFlags.Near | InteractionFlags.Ray;
        objectManipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.None;

        obj.GetComponent<Collider>().enabled = true;
    }

    private void DisableObjectManipulation(GameObject obj)
    {
        // remove added constraints
        foreach (TransformConstraint constraint in addedConstraints)
        {
            Destroy(constraint);
        }

        ObjectManipulator objectManipulator = obj.GetComponent<ObjectManipulator>();
        objectManipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.None;

        var boundsControl = obj.GetComponent<BoundsControl>();

        boundsControl.EnabledHandles = HandleType.None;
        boundsControl.HandlesActive = false;

        currentManipulationState = ManipulationState.None;
    }

    public void AllowMoveActiveObject()
    {
        DisableObjectManipulation(currentActiveObject);

        ObjectManipulator objectManipulator = currentActiveObject.GetComponent<ObjectManipulator>();
        
        objectManipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.Move;

        var moveAxisConstraintY = currentActiveObject.AddComponent<MoveAxisConstraint>();
        moveAxisConstraintY.ConstraintOnMovement = MixedReality.Toolkit.AxisFlags.YAxis;
        addedConstraints.Add(moveAxisConstraintY);

        var planeBoundsConstraint = currentActiveObject.AddComponent<PlaneBoundsConstraint>();
        planeBoundsConstraint.Board = boardObject;
        addedConstraints.Add(planeBoundsConstraint);

        var boundsControl = currentActiveObject.GetComponent<BoundsControl>();

        boundsControl.EnabledHandles = HandleType.Translation;
        boundsControl.HandlesActive = true;

        currentManipulationState = ManipulationState.Translation;
    }

    public void AllowRotateActiveObject()
    {
        DisableObjectManipulation(currentActiveObject);

        ObjectManipulator objectManipulator = currentActiveObject.GetComponent<ObjectManipulator>();

        objectManipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.Rotate;

        var rotationAxisConstraintXZ = currentActiveObject.AddComponent<RotationAxisConstraint>();
        rotationAxisConstraintXZ.ConstraintOnRotation = MixedReality.Toolkit.AxisFlags.XAxis | MixedReality.Toolkit.AxisFlags.ZAxis;
        addedConstraints.Add(rotationAxisConstraintXZ);

        var boundsControl = currentActiveObject.GetComponent<BoundsControl>();

        boundsControl.EnabledHandles = HandleType.Rotation;
        boundsControl.HandlesActive = true;

        currentManipulationState = ManipulationState.Rotation;
    }

    public void AllowScaleActiveObject()
    {
        DisableObjectManipulation(currentActiveObject);

        ObjectManipulator objectManipulator = currentActiveObject.GetComponent<ObjectManipulator>();

        objectManipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.Scale;

        var minMaxScaleConstraint = currentActiveObject.AddComponent<MinMaxScaleConstraint>();
        minMaxScaleConstraint.MinimumScale = objectOriginalTransforms[currentActiveObject.GetInstanceID()].localScale * 0.2f;
        minMaxScaleConstraint.MaximumScale = objectOriginalTransforms[currentActiveObject.GetInstanceID()].localScale * 2f;
        minMaxScaleConstraint.RelativeToInitialState = false;
        addedConstraints.Add(minMaxScaleConstraint);

        //var scaleFromBottomConstraint = currentActiveObject.AddComponent<ScaleFromBottomConstraint>();
        //scaleFromBottomConstraint.BoardTopY = initialSpawnLocation.y;
        //addedConstraints.Add(scaleFromBottomConstraint);

        //var stickToBottomConstraint = currentActiveObject.AddComponent<StickToBottomConstraint>();
        //stickToBottomConstraint.BoardTopY = initialSpawnLocation.y;
        //addedConstraints.Add(stickToBottomConstraint);

        var boundsControl = currentActiveObject.GetComponent<BoundsControl>();

        boundsControl.EnabledHandles = HandleType.Scale;
        boundsControl.HandlesActive = true;

        currentManipulationState = ManipulationState.Scaling;
    }

    public void AcceptObjectChanges()
    {
        ResetPositionLerp();
        ResetScaleLerp();
        ResetRotationLerp();

        DisableObjectManipulation(currentActiveObject);
        currentActiveObject = null;
        MenuManager.Instance.HideMenu();
        MenuManager.Instance.ShouldUpdatePosition = false;
    }

    public void OnActiveObjectManipulationStarted()
    {
        Debug.Log("resetting, new manip started");
        ResetRotationLerp();
        ResetPositionLerp();
        ResetScaleLerp();

        MenuManager.Instance.HideMenu();
        MenuManager.Instance.ShouldUpdatePosition = false;
    }

    public void OnActiveObjectManipulationEnded()
    {
        if (currentActiveObject != null)
        {
            if (currentManipulationState == ManipulationState.Rotation)
            {
                LerpRotationIfCollision();
            }

            if (currentManipulationState == ManipulationState.Scaling)
            {
                LerpScaleIfCollision();

                if (!isLerpingPosition && !isLerpingScale)
                {
                    LerpScalePositionAdjustment();
                }                
            }

            MenuManager.Instance.ShowMenu(currentActiveObject);
            MenuManager.Instance.ShouldUpdatePosition = true;
        }
    }

    public void DeleteActiveObject()
    {
        objectOriginalTransforms.Remove(currentActiveObject.GetInstanceID());
        boardObjects.Remove(currentActiveObject);
        Destroy(currentActiveObject);
        currentActiveObject = null;
        MenuManager.Instance.HideMenu();
        MenuManager.Instance.ShouldUpdatePosition = false;
    }

    public void OnObjectClicked(GameObject clickedObject)
    {
        if (clickedObject == currentActiveObject)
        {
            return;
        }

        if (currentActiveObject != null)
        {
            AcceptObjectChanges();
        }

        currentActiveObject = clickedObject;
        EnableObjectInteraction(currentActiveObject);
        AllowMoveActiveObject();
        MenuManager.Instance.ShowMenu(currentActiveObject);
        MenuManager.Instance.ShouldUpdatePosition = true;
    }

    public void PositionMenuNextToBoard()
    {

        if (boardObject == null || inventoryMenuObject == null)
        {
            Debug.LogError("Board or Menu object references are missing.");
            return;
        }

        // Get menu's world space canvas
        Canvas menuCanvas = inventoryMenuObject.GetComponent<Canvas>();
        if (menuCanvas == null || menuCanvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogError("The menu's canvas must be set to world space.");
            return;
        }

        // Get transforms
        Transform boardTransform = boardObject.transform;
        RectTransform menuRect = inventoryMenuObject.GetComponent<RectTransform>();
        Transform menuTransform = menuRect.transform;


        Vector2 menuWorldSize = GetWorldSpaceSize(menuRect);

        Vector3 worldRight = boardTransform.rotation * Vector3.left; // right of the board due to board's inverse rotatoin
        Vector3 worldForward = boardTransform.rotation * Vector3.forward;

        float boardHalfZ = boardTransform.lossyScale.z / 2f; // Half size on Z-Axis
        float boardHalfX = boardTransform.lossyScale.x / 2f; // Half size on the X-axis

        Vector3 positionOffset = worldForward * (boardHalfZ - boardAndMenuZOffset) + worldRight * (boardHalfX + boardAndMenuXOffset);

        // Get menu's bottom aligned with board
        Bounds boardBounds = boardObject.GetComponent<Renderer>().bounds;
        float boardBottomY = boardBounds.min.y;

        Vector3 targetPosition = boardTransform.position + positionOffset;
        targetPosition.y = boardBottomY + boardAndMenuYOffset;

        inventoryMenuObject.transform.position = targetPosition;

        // Calculate and apply the rotation: flip Y-axis and tilt on x-axis
        Quaternion flippedYRotation = Quaternion.Euler(0, boardTransform.eulerAngles.y + 180f, 0);
        inventoryMenuObject.transform.rotation = flippedYRotation * Quaternion.Euler(45f, 0, 0); // Keep tilt on X axis

        //Debug.Log($"Menu positioned at: {inventoryMenuObject.transform.position} with rotation: {inventoryMenuObject.transform.rotation.eulerAngles}");

        //Debug.DrawLine(boardTransform.position, targetPosition, Color.green, 5f);
    }

    Vector2 GetWorldSpaceSize(RectTransform rectTransform)
    {
        // Get the rect dimensions (local-space size)
        Vector2 localSize = rectTransform.rect.size;

        // Convert to world-space size by applying the lossy scale
        Vector2 worldSize = new Vector2(
            localSize.x * rectTransform.lossyScale.x,
            localSize.y * rectTransform.lossyScale.y
        );

        return worldSize;
    }

    //this stuff really outta be in its own class
    private bool BoundsRotatedWouldCollide(float angle)
    {
        Collider objectCollider = currentActiveObject.GetComponent<Collider>();
        Quaternion attemptedRotation = currentActiveObject.transform.rotation;
        Bounds boardBounds = boardObject.GetComponent<Collider>().bounds;
        // get a copy of the original collision bounds
        Bounds rotatedBoundsPos = objectCollider.bounds;

        if (angle != 0)
        {
            Vector3 objectPosition = currentActiveObject.transform.position;
            Vector3 colliderCenter = objectCollider.bounds.center;
            Quaternion rotationCheck = Quaternion.Euler(0f, attemptedRotation.eulerAngles.y + angle, 0f);

            Vector3 rotatedColliderCenterPos = rotationCheck * (colliderCenter - objectPosition) + objectPosition;

            //get new rotated bounds
            rotatedBoundsPos.center = rotatedColliderCenterPos;
        }

        // If rotated bounds are outside of or touching board bounds, bounds would collid
        return
            rotatedBoundsPos.min.x <= boardBounds.min.x ||
            rotatedBoundsPos.max.x >= boardBounds.max.x ||
            rotatedBoundsPos.min.z <= boardBounds.min.z ||
            rotatedBoundsPos.max.z >= boardBounds.max.z;
    }

    private bool BoundsResizedWouldCollide(Vector3 scale)
    {
        var worldScale = currentActiveObject.transform.TransformSize(scale);

        Collider objectCollider = currentActiveObject.GetComponent<Collider>();
        Bounds boardBounds = boardObject.GetComponent<Collider>().bounds;
        // get a copy of the original collision bounds
        Bounds rescaledBounds = new Bounds(objectCollider.bounds.center, worldScale);

        // If rescaled bounds are outside of or touching board bounds, bounds would collide
        return
            rescaledBounds.min.x <= boardBounds.min.x ||
            rescaledBounds.max.x >= boardBounds.max.x ||
            rescaledBounds.min.z <= boardBounds.min.z ||
            rescaledBounds.max.z >= boardBounds.max.z;
    }

    private bool CurrentBoundsCollide()
    {
        Bounds boardBounds = boardObject.GetComponent<Collider>().bounds;
        Bounds bounds = currentActiveObject.GetComponent<Collider>().bounds;

        return
           bounds.min.x <= boardBounds.min.x ||
           bounds.max.x >= boardBounds.max.x ||
           bounds.min.z <= boardBounds.min.z ||
           bounds.max.z >= boardBounds.max.z;
    }

    private void LerpRotationIfCollision()
    {
        if(!BoundsRotatedWouldCollide(0)) 
        { 
            return; 
        }

        float validAngle = 0; // return to 0 degree rotation if no valid angle found

        // find the nearest valid angle
        for (float angleDifference = 0; angleDifference <= 90; angleDifference += 5)
        {
           if (!BoundsRotatedWouldCollide(angleDifference))
           {
                validAngle = angleDifference;
                break;
           }

           if (!BoundsRotatedWouldCollide(angleDifference * -1))
            {
                validAngle = angleDifference * -1;
                break;
            }
        }
        //Debug.Log("VAlid angle: " + validAngle + " Attempted angle: " + currentActiveObject.transform.rotation.eulerAngles.y + " DIFFERENCE: " + (validAngle - currentActiveObject.transform.rotation.eulerAngles.y));

        lerpFromRotation = currentActiveObject.transform.rotation;
        lerpToRotation = Quaternion.Euler(0, validAngle, 0);
        isLerpingRotation = true;
    }

    private void LerpScaleIfCollision()
    {
        Debug.Log("LEARPING SCALE IF COLLISION...");
        var currScaleWorld = currentActiveObject.transform.lossyScale;
        if (!CurrentBoundsCollide())
        {
            Debug.Log("No collision detected. ??");
            return;
        }

        Vector3 smallestScaleWorld = objectOriginalTransforms[currentActiveObject.GetInstanceID()].lossyScale * 0.2f; // return to 1/5th original size (minimum size) if no other valid angle found
        Vector3 originalPosition = currentActiveObject.transform.position;

        Vector3 adjustedPosition = FindMinimumPositionToResolveCollision(currentActiveObject.transform.position, currScaleWorld);

        currentActiveObject.transform.position = adjustedPosition;

        if (!CurrentBoundsCollide())
        {
            Debug.Log("repositioned unscaled bounds do not collide");
            currentActiveObject.transform.position = originalPosition;

            //if smallest still collides, lerp to position and scale to reset point
            lerpToPosition = adjustedPosition;
            lerpFromPosition = currentActiveObject.transform.position;
            isLerpingPosition = true;

            Debug.Log("repositioning to fix scale bounds");
            Debug.Log("From: " + lerpFromPosition + " To: " + lerpToPosition);

            return;
        } 

        Vector3 validScaleWorld = FindLargestValidScale(smallestScaleWorld, currentActiveObject.transform.lossyScale);

        if (validScaleWorld == smallestScaleWorld && BoundsResizedWouldCollide(validScaleWorld))
        { 
            lerpToPosition = GetPositionVectorForBoardItemAlignment(initialSpawnLocation.x, initialSpawnLocation.z);
            lerpFromPosition = currentActiveObject.transform.position;
            isLerpingPosition = true;

            lerpToScale = objectOriginalTransforms[currentActiveObject.GetInstanceID()].localScale;
            lerpFromScale = currentActiveObject.transform.localScale;
            isLerpingScale = true;

            Debug.Log("full scale and postition reset required");
            Debug.Log("From pos: " + lerpFromPosition + " To pos: " + lerpToPosition);
            Debug.Log("From scale: " + lerpFromScale + " To scale: " + lerpToScale);
            return;
        }
                
        lerpFromScale = currentActiveObject.transform.localScale;
        var parentLossyScale = currentActiveObject.transform.parent.lossyScale;
        Vector3 validScaleLocal = new Vector3(
            validScaleWorld.x / parentLossyScale.x,
            validScaleWorld.y / parentLossyScale.y,
            validScaleWorld.z / parentLossyScale.z
        );
        lerpToScale = validScaleWorld;
        isLerpingScale = true;

        Debug.Log("Lerping scale started with lerpToScale = " + lerpToScale + " and lerp from scale =  " + lerpFromScale);
    }

    /// <summary>
    /// Finds the minimum position adjustment needed to resolve the collision.
    /// </summary>
    private Vector3 FindMinimumPositionToResolveCollision(Vector3 originalPosition, Vector3 scale)
    {
        Collider objectCollider = currentActiveObject.GetComponent<Collider>();
        Bounds boardBounds = boardObject.GetComponent<Collider>().bounds;

        Vector3 adjustedPosition = originalPosition;

        // Calculate the minimum adjustment in X and Z directions
        float minOffsetX = 0f;
        float minOffsetZ = 0f;

        if (objectCollider.bounds.min.x < boardBounds.min.x)
        {
            minOffsetX = boardBounds.min.x - objectCollider.bounds.min.x + 0.01f;
        }
        else if (objectCollider.bounds.max.x > boardBounds.max.x)
        {
            minOffsetX = boardBounds.max.x - objectCollider.bounds.max.x - 0.01f;
        }

        if (objectCollider.bounds.min.z < boardBounds.min.z)
        {
            minOffsetZ = boardBounds.min.z - objectCollider.bounds.min.z + 0.01f;
        }
        else if (objectCollider.bounds.max.z > boardBounds.max.z)
        {
            minOffsetZ = boardBounds.max.z - objectCollider.bounds.max.z - 0.01f;
        }

        // Apply the minimum offset to resolve the collision
        adjustedPosition.x += minOffsetX;
        adjustedPosition.z += minOffsetZ;

        return GetPositionVectorForBoardItemAlignment(adjustedPosition.x, adjustedPosition.z);
    }

    private Vector3 FindLargestValidScale(Vector3 defaultScale, Vector3 attemptedScale)
    {
        Vector3 currentScale = defaultScale; // smallest valid scale
        float minFactor = 1f;
        float maxFactor = Mathf.Min(attemptedScale.x / defaultScale.x, 
                                    attemptedScale.y / defaultScale.y, 
                                    attemptedScale.z / defaultScale.z);

        float largestValidFactor = minFactor;
        const float epsilon = 0.001f; // precision threshold for binary search

        while (maxFactor - minFactor > epsilon)
        {
            float midFactor = (minFactor + maxFactor) / 2;
            Vector3 testScale = defaultScale * midFactor;

            if (BoundsResizedWouldCollide(testScale))
            {
                // middle causes collision, search smaller range
                maxFactor = midFactor;
            }
            else
            {
                // mid scale valid, move upward
                largestValidFactor = midFactor;
                minFactor = midFactor;
            }
        }

        return defaultScale * largestValidFactor;
    }

    private void ResetRotationLerp()
    {
        isLerpingRotation = false;
        lerpToRotation = null;
        lerpFromRotation = null;
        rotationLerpTimeCount = 0;
    }

    private void LerpScalePositionAdjustment()
    {
        Debug.Log("Scaling from bottom adjustment called");
        lerpToPosition =  GetPositionVectorForBoardItemAlignment(currentActiveObject.transform.position.x, currentActiveObject.transform.position.z);
        lerpFromPosition = currentActiveObject.transform.position;

        if (Vector3.Distance((Vector3)lerpFromPosition, (Vector3)lerpToPosition) >= 0.00001f)
        {
            isLerpingPosition = true;
        }  else
        {
            ResetPositionLerp();
        }

    }

    private Vector3 GetPositionVectorForBoardItemAlignment(float desiredX, float desiredZ)
    {
        // Get the object's half-height using its collider and calculate the offset of its transform origin/pivot to the collider's center
        Collider objectCollider = currentActiveObject.GetComponent<Collider>();

        float colliderBottom = objectCollider.bounds.min.y;
        float pivotToBottomOffset = currentActiveObject.transform.position.y - colliderBottom;
        
        //Vector3 colliderCenter = objectCollider.bounds.center;
        //Vector3 pivotToCenterOffset = colliderCenter - currentActiveObject.transform.position;

        // Calculate the bottom of the object relative to its pivot
        //float objectHalfHeight = objectCollider.bounds.extents.y; // Half-height already
        //float objectBottomOffset = objectHalfHeight + pivotToCenterOffset.y;

        // Adjust position so bottom aligns with board's top
        Vector3 newPosition = new Vector3(desiredX, initialSpawnLocation.y + pivotToBottomOffset, desiredZ);
        //currentActiveObject.transform.position = newPosition;
        return newPosition;
    }

    public void Clear()
    {
        foreach (GameObject boardObject in boardObjects)
        {
            Destroy(boardObject);
            MenuManager.Instance.HideMenu();
            currentActiveObject = null;
            objectOriginalTransforms = new Dictionary<int, Transform>();
            ResetPositionLerp();
            ResetRotationLerp();
            ResetScaleLerp();
            addedConstraints = new List<TransformConstraint>();
        }
    }

    public void Reset()
    {
        Clear();
        inventoryMenuObject.SetActive(false);
    }
}

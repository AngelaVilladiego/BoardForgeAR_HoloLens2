using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlaneBoundsConstraint : TransformConstraint
{

    [SerializeField] private GameObject board;

    private Bounds boardBounds;

    public GameObject Board
    { 
        get { return board; } 
        set { 
            board = value;
            boardBounds = board.GetComponent<Collider>().bounds;
        } 
    }

    public override TransformFlags ConstraintType => TransformFlags.Move;

    public override void ApplyConstraint(ref MixedRealityTransform transform)
    {

        if (board == null)
        {
            throw new System.Exception("Board hasn't been provided!");
        }

        Vector3 position = transform.Position;
        // Get object's collider to account for its size
        Collider objectCollider = gameObject.GetComponent<Collider>();

        if (objectCollider == null)
        {
            throw new System.Exception("Object does not have a Collider component.");
        }

        Vector3 objectExtents = objectCollider.bounds.extents; // half size extents
        Vector3 colliderCenter = objectCollider.bounds.center;
        //Vector3 pivotToColliderOffset = colliderCenter - transform.Position;

        //// Clamp X and Z based on board's boundaries
        //position.x = Mathf.Clamp(position.x, boardBounds.min.x + objectExtents.x, boardBounds.max.x - objectExtents.x);
        //position.z = Mathf.Clamp(position.z, boardBounds.min.z + objectExtents.z, boardBounds.max.z - objectExtents.z);

        //// Update position but keep y the same
        //transform.Position = new Vector3(position.x, transform.Position.y, position.z);

        // Transform the object's world position to the board's local space
        Vector3 localPosition = board.transform.InverseTransformPoint(position);

        // Transform the collider's world space center to local space (relative to the board)
        Vector3 localColliderCenter = board.transform.InverseTransformPoint(colliderCenter);

        // Calculate the offset from the object's pivot to the collider's center in local space
        Vector3 pivotToColliderOffset = localColliderCenter - localPosition;

        Bounds localBoardBounds = new Bounds(board.transform.InverseTransformPoint(boardBounds.center),
                                              new Vector3(
                                                  boardBounds.size.x / board.transform.lossyScale.x,
                                                  boardBounds.size.y / board.transform.lossyScale.y,
                                                  boardBounds.size.z / board.transform.lossyScale.z
                                              ));

        Bounds localObjectBounds = new Bounds(board.transform.InverseTransformPoint(objectCollider.bounds.center),
                                              new Vector3(
                                                  objectCollider.bounds.size.x / board.transform.lossyScale.x,
                                                  objectCollider.bounds.size.y / board.transform.lossyScale.y,
                                                  objectCollider.bounds.size.z / board.transform.lossyScale.z
                                              ));

        // Clamp X and Z based on board's boundaries in local space
        localPosition.x = Mathf.Clamp(localPosition.x, localBoardBounds.min.x + localObjectBounds.extents.x, localBoardBounds.max.x - localObjectBounds.extents.x);
        localPosition.z = Mathf.Clamp(localPosition.z, localBoardBounds.min.z + localObjectBounds.extents.z, localBoardBounds.max.z - localObjectBounds.extents.z);

        // Transform the position back to world space
        transform.Position = board.transform.TransformPoint(localPosition);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToBottomConstraint : TransformConstraint
{
    private float boardTopY;

    public float BoardTopY
    {
        get { return boardTopY; }
        set { boardTopY = value; }
    }


    public override TransformFlags ConstraintType => TransformFlags.Move | TransformFlags.Scale;

    public override void ApplyConstraint(ref MixedRealityTransform transform)
    {
        Debug.Log("BEFORE LOCATION: " + gameObject.transform.position);

        Collider objectCollider = GetComponent<Collider>();
        if (objectCollider == null)
        {
            Debug.LogError("object must have a Collider component!");
            return;
        }
        float colliderBottom = objectCollider.bounds.min.y;
        float pivotToBottomOffset = gameObject.transform.position.y - colliderBottom;

        // Adjust position so bottom aligns with board's top
        Vector3 newPosition = new Vector3(gameObject.transform.position.x, boardTopY + pivotToBottomOffset, gameObject.transform.position.y);
        Debug.Log("AFTER LOCATION:  " + newPosition);
        transform.Position = newPosition;
    }

    // Start is called before the first frame update
    void Start()
    {
        ExecutionPriority = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

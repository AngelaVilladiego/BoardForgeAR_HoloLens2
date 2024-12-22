using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ScaleFromBottomConstraint : TransformConstraint
{
    Vector3 scalePivot;
    Vector3 previousScale;
    private float boardTopY;

    public float BoardTopY
    {
        get { return boardTopY; }
        set { boardTopY = value; }
    }

    public override TransformFlags ConstraintType => TransformFlags.Scale;

    public override void ApplyConstraint(ref MixedRealityTransform transform)
    {
        Debug.Log("BEFORE LOCATION: " + gameObject.transform.position);
        // Get the object's half-height using its collider and calculate the offset of its transform origin/pivot to the collider's center
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
        // Set pivot point for scale. This should never change since we're only ever scaling, not moving or rotating the object.
        Collider collider = GetComponent<Collider>();

        // bottom center, in world space of the collider's bounding
        var scalePivotWorld = new Vector3(collider.bounds.center.x, collider.bounds.min.y, collider.bounds.center.z);
        scalePivot = gameObject.transform.InverseTransformPoint(scalePivotWorld);
        previousScale = gameObject.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

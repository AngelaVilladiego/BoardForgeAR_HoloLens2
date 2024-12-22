using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationBoundsConstraint : TransformConstraint
{
    [SerializeField] private GameObject board;

    private Bounds boardBounds;
    private Quaternion prevRotation;
    private Vector3 originalPosition;

    public GameObject Board
    {
        get { return board; }
        set
        {
            board = value;
            boardBounds = board.GetComponent<Collider>().bounds;
        }
    }

    void Start()
    {
        prevRotation = transform.rotation;
        originalPosition = transform.position;
    }

    public override TransformFlags ConstraintType => TransformFlags.Rotate;

    public override void ApplyConstraint(ref MixedRealityTransform transform)
    {
        if (board == null)
        {
            throw new System.Exception("Board hasn't been provided!");
        }

        Collider objectCollider = this.GetComponent<Collider>();
        if (objectCollider == null)
        {
            Debug.LogError("Object does not have a Collider component.");
            return;
        }

        Quaternion currentRotation = transform.Rotation;

        // ENSURE NO MOVEMENT in case some real buggy stuff happens with attempting to force rotations on disallowed axes
        transform.Position = originalPosition;

        Vector3 objectPosition = transform.Position;
        Vector3 colliderCenter = objectCollider.bounds.center;

        float rotationDifference = currentRotation.eulerAngles.y - prevRotation.eulerAngles.y;
        float rotationDirection = 1f;

        if (rotationDifference < 0)
        {
            rotationDirection = -1;
        }

        float validAngle = currentRotation.eulerAngles.y + 50; //set this to the threshold of 5 degrees in the opposite direction past the last angle 

        // Iterate from the current angle to 5 degrees beyond the previous rotation in the opposite of the attempted rotation direction to avoid getting stuck
        for (float angleToCheck = currentRotation.eulerAngles.y; Math.Abs(angleToCheck - currentRotation.eulerAngles.y) <= 50 ; angleToCheck += 5)
        {
            Debug.Log("Looping with angle to check is " + angleToCheck);
            Quaternion rotationCheck = Quaternion.Euler(0f, angleToCheck, 0f);

            // take center of collider and perform a rotation on it at the rotation origin of the object, then move the center back to its original position
            Vector3 rotatedColliderCenter = rotationCheck * (colliderCenter - objectPosition) + objectPosition;

            // get a copy of the original collision bounds with the new rotation
            Bounds rotatedBounds = objectCollider.bounds;
            rotatedBounds.center = rotatedColliderCenter;

            // Check if rotated bounds meet or exceed the board's bounds 
            if (rotatedBounds.min.x < boardBounds.min.x ||
                rotatedBounds.max.x > boardBounds.max.x ||
                rotatedBounds.min.z < boardBounds.min.z ||
                rotatedBounds.max.z > boardBounds.max.z)
            {
                Debug.Log("Collision detected ?");
                // If they do, iterate angle backwards until the angle no longer causes contact
                continue;
            }

            // otherwise we can break the loop and set the new rotation
            validAngle = angleToCheck;
            break;
        }
        Debug.Log("VAlid angle: " + validAngle + " Attempted angle: " + currentRotation.eulerAngles.y + " DIFFERENCE: " + (validAngle - currentRotation.eulerAngles.y));

        transform.Rotation = Quaternion.Euler(0, validAngle, 0);
        prevRotation = transform.Rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

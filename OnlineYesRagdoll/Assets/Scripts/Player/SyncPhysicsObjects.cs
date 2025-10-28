using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncPhysicsObjects : MonoBehaviour
{
    Rigidbody rigidbody3D;
    ConfigurableJoint joint;

    //keep track for starting rotation
    Quaternion startLocalRotation;


    void Awake()
    {
        rigidbody3D = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        startLocalRotation = transform.localRotation;
    }

    public void UpdateJointFromAnimation()
    {
        //ConfigurableJointExtensions.SetTargetRotationLocal(joint,)
    }
}

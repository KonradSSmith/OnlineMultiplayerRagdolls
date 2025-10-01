using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3Compressed networkBasePosition;
    public Vector3Compressed networkCamPos;
    public QuaternionCompressed networkCamRot;
    //public NetworkBool isJumpPressed;
}
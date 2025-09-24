using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2Compressed movementInput;
    public Vector2Compressed cameraDir;
    public NetworkBool isJumpPressed;
}
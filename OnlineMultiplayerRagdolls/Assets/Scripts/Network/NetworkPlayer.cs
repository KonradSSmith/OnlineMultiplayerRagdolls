using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; set; }

    #region
    [Header("Dependencies")]
    [SerializeField] Rigidbody rb;
    [SerializeField] NetworkRigidbody3D rb3D;
    [SerializeField] ConfigurableJoint mainJoint;
    [Space]
    [Header("Movement")]
    [SerializeField] private float groundMoveSpeed;
    [SerializeField] private float airMoveSpeed;
    [SerializeField] private float JumpForce;
    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float extraGravity;
    [Space]
    [SerializeField] LayerMask groundLayer;
    [Space]
    [Header("Camera Dependencies")]
    [SerializeField] GameObject camPos;
    [SerializeField] Camera cam;
    [SerializeField] ConfigurableJoint headJoint;

    [Header("Camera Configurations")]
    [SerializeField] float sens;

    Vector2 lookVector = Vector2.zero;


    bool grounded = false;
    Vector2 moveInputVector = Vector2.zero;
    bool jumpButtonPressed = false;
    bool readyToJump = true;
    private NetworkInputData networkInputData;
    private Vector2 PlayerMovementInput;
    Vector3 flatVel = Vector3.zero;
    Vector3 limitedVel = Vector3 .zero;
    Vector3 moveDirection;
    #endregion

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            CheckAndRespawn();
            GroundCheck();
        }

        //only executed if we have input authority or we are the state authority
        if (GetInput(out networkInputData))
        {
            GetInput();
            MovePlayer();
            SpeedControl();
            RotateBodyAndCam();
        }

    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //movement data
        networkInputData.movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        //camera data
        networkInputData.cameraDir = new Vector2(Input.GetAxisRaw("Mouse Y") * sens, Input.GetAxisRaw("Mouse X") * sens);

        if (Input.GetKey(KeyCode.Space))
        {
            //jumpButtonPressed = true;
            networkInputData.isJumpPressed = true;
        }

        //reset jump button
        //jumpButtonPressed = false;

        return networkInputData;
    }
    private void RotateBodyAndCam()
    {
        lookVector -= networkInputData.cameraDir;
        lookVector.x = Mathf.Clamp(lookVector.x, -90, 90);

        Vector3 targetBodyRotation = new Vector3(0, lookVector.y, 0);
        Vector3 targetHeadRotation = new Vector3(headJoint.targetRotation.x - lookVector.x, 0, 0);

        mainJoint.targetRotation = Quaternion.Euler(targetBodyRotation);
        headJoint.targetRotation = Quaternion.Euler(targetHeadRotation);

        Camera.main.transform.position = camPos.transform.position;
        Camera.main.transform.rotation = camPos.transform.rotation;
    }

    private void GetInput()
    {
        Local.PlayerMovementInput = Local.networkInputData.movementInput;

        if (Local.networkInputData.isJumpPressed && Local.readyToJump && Local.grounded)
        {
            Local.readyToJump = false;

            Local.Jump();

            Local.StartCoroutine(ResetJump());
        }
      
    }

    private void GroundCheck()
    {
        Local.grounded = Physics.SphereCast(new Ray(Local.mainJoint.transform.position, Vector3.down), 0.8f, 0.4f, Local.groundLayer);

        if (Local.grounded)
        {
            Local.rb.linearDamping = Local.groundDrag;
        }
        else
        {
            Local.rb.linearDamping = Local.airDrag;
            Local.rb.AddForce(new Vector3(0, -Local.extraGravity, 0), ForceMode.Impulse);
        }
    }

    private void MovePlayer()
    {
        Local.moveDirection = new Vector3(transform.forward.x, 0, transform.forward.z) * PlayerMovementInput.y + new Vector3(transform.right.x, 0, transform.right.z) * PlayerMovementInput.x;

        if (Local.grounded)
        {
            Local.rb.AddForce(Local.moveDirection.normalized * Local.groundMoveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            Local.rb.AddForce(Local.moveDirection.normalized * Local.airMoveSpeed * 10f, ForceMode.Force);
        }

    }

    private void SpeedControl()
    {
        Local.flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (Local.grounded)
        {
            if (Local.flatVel.magnitude > Local.groundMoveSpeed)
            {
                Local.limitedVel = Local.flatVel.normalized * Local.groundMoveSpeed;
                Local.rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
        else
        {
            if (Local.flatVel.magnitude > Local.airMoveSpeed)
            {
                Local.limitedVel = Local.flatVel.normalized * Local.airMoveSpeed;
                Local.rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        //reset y velocity
        Local.rb.angularVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Local.rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);

    }

    IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(jumpCooldown);
        Local.readyToJump = true;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            cam.gameObject.SetActive(true);
            Utils.DebugLog("Spawned player with input authority");
        }
        else
        {
            Utils.DebugLog("Spawned player without input authority");
            cam.gameObject.SetActive(false);
        }

        transform.name = $"P_{Object.Id}";
    }

    private void CheckAndRespawn()
    {
        if (Local.rb3D.transform.position.y < -10)
        {
            Local.rb3D.Teleport(new Vector3(0, 3, 0), Quaternion.identity);
        }
    }

    public void PlayerLeft(PlayerRef player)
    {

    }
}

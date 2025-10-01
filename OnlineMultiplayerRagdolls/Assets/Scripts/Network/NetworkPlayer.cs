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

    }

    private void FixedUpdate()
    {
        if (Object.HasInputAuthority)
        {
            MovePlayer();
            GroundCheck();
        }
    }

    private void Update()
    {
        //only executed if we have input authority or we are the state authority
        if (Object.HasInputAuthority)
        {
            //CheckAndRespawn();
            GetInput();
            RotateBodyAndCam();
            SpeedControl();
        }
    }

    private void moveWithoutInputAuthority()
    {
        mainJoint.targetPosition = networkInputData.networkBasePosition;
    }

    public NetworkInputData SendNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //movement data
        //networkInputData.movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        networkInputData.networkBasePosition = transform.position;

        //camera data
        //networkInputData.cameraDir = new Vector2(Input.GetAxisRaw("Mouse Y") * sens, Input.GetAxisRaw("Mouse X") * sens);
        networkInputData.networkCamPos = camPos.transform.position;
        networkInputData.networkCamRot = camPos.transform.rotation;

        if (Input.GetKey(KeyCode.Space))
        {
            //jumpButtonPressed = true;
            //networkInputData.isJumpPressed = true;
        }

        //reset jump button
        //jumpButtonPressed = false;

        return networkInputData;
    }
    private void RotateBodyAndCam()
    {
        Vector2 mouseXY = new Vector2(Input.GetAxisRaw("Mouse Y") * sens, Input.GetAxisRaw("Mouse X") * sens);

        lookVector -= mouseXY;
        lookVector.x = Mathf.Clamp(lookVector.x, -90, 90);

        Vector3 targetBodyRotation = new Vector3(0, lookVector.y, 0);
        Vector3 targetHeadRotation = new Vector3(Local.headJoint.targetRotation.x - lookVector.x, 0, 0);

        Local.mainJoint.targetRotation = Quaternion.Euler(targetBodyRotation);
        Local.headJoint.targetRotation = Quaternion.Euler(targetHeadRotation);

        Camera.main.transform.position = Local.camPos.transform.position;
        Camera.main.transform.rotation = Local.camPos.transform.rotation;
    }

    private void GetInput()
    {
        Local.PlayerMovementInput = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && readyToJump && grounded)
        {
            Local.readyToJump = false;

            Jump();

            StartCoroutine(ResetJump());
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
        Local.moveDirection = new Vector3(transform.forward.x, 0, transform.forward.z) * Local.PlayerMovementInput.y + new Vector3(transform.right.x, 0, transform.right.z) * Local.PlayerMovementInput.x;

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
        Local.flatVel = new Vector3(Local.rb.linearVelocity.x, 0f, Local.rb.linearVelocity.z);

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

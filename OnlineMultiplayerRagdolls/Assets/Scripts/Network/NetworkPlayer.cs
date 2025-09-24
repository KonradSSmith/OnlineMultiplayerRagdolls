using Fusion;
using System.Collections;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; set; }

    #region
    [Header("Dependencies")]
    [SerializeField] Rigidbody rb;
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

    bool grounded = false;
    Vector2 moveInputVector = Vector2.zero;
    bool jumpButtonPressed = false;
    bool readyToJump = true;
    private NetworkInputData networkInputData;
    private Vector2 PlayerMovementInput;
    #endregion

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            GroundCheck();
        }

        //only executed if we have input authority or we are the state authority
        if (GetInput(out networkInputData))
        {
            GetInput();
            MovePlayer();
            SpeedControl();
        }
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //movement data
        networkInputData.movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKey(KeyCode.Space))
        {
            //jumpButtonPressed = true;
            networkInputData.isJumpPressed = true;
        }

        //reset jump button
        //jumpButtonPressed = false;

        return networkInputData;
    }

    private void GetInput()
    {
        PlayerMovementInput = networkInputData.movementInput;

        if (networkInputData.isJumpPressed && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            StartCoroutine(ResetJump());
        }
      
    }

    private void GroundCheck()
    {
        grounded = Physics.SphereCast(new Ray(mainJoint.transform.position, Vector3.down), 0.8f, 0.4f, groundLayer);

        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = airDrag;
            rb.AddForce(new Vector3(0, -extraGravity, 0), ForceMode.Impulse);
        }
    }

    private void MovePlayer()
    {
        Vector3 moveDirection = new Vector3(transform.forward.x, 0, transform.forward.z) * PlayerMovementInput.y + new Vector3(transform.right.x, 0, transform.right.z) * PlayerMovementInput.x;
    
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * groundMoveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * airMoveSpeed * 10f, ForceMode.Force);
        }

    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (grounded)
        {
            if (flatVel.magnitude > groundMoveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * groundMoveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
        else
        {
            if (flatVel.magnitude > airMoveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * airMoveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        //reset y velocity
        rb.angularVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);

    }

    IEnumerator ResetJump()
    {
        //35 minutes into tutorial
        yield return new WaitForSeconds(jumpCooldown);
        readyToJump = true;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;

            Utils.DebugLog("Spawned player with input authority");
        }
        else
        {
            Utils.DebugLog("Spawned player without input authority");
        }

        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {

    }
}

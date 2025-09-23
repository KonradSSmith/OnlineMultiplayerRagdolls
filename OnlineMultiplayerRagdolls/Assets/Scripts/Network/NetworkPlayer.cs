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
    bool readyToJump = true;
    private Vector3 PlayerMovementInput;
    #endregion

    private void Awake()
    {
        
    }

    void Update()
    {
        GetInput();
        
        GroundCheck();
        SpeedControl();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void GetInput()
    {
        PlayerMovementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if(Input.GetKeyDown(KeyCode.Space) && readyToJump && grounded)
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
        }
    }

    private void MovePlayer()
    {
        Vector3 moveDirection = new Vector3(transform.forward.x, 0, transform.forward.z) * PlayerMovementInput.z + new Vector3(transform.right.x, 0, transform.right.z) * PlayerMovementInput.x;
    
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * groundMoveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * airMoveSpeed * 10f, ForceMode.Force);
            rb.AddForce(new Vector3(0, -extraGravity, 0), ForceMode.Impulse);
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
        yield return new WaitForSeconds(jumpCooldown);
        readyToJump = true;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;

        }
    }

    public void PlayerLeft(PlayerRef player)
    {

    }
}

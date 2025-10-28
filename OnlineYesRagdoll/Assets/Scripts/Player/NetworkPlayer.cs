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
    [SerializeField] GameObject camTarget;
    [SerializeField] Camera cam;
    [SerializeField] ConfigurableJoint headJoint;

    [Header("Camera Configurations")]
    [SerializeField] float sens;

    Vector2 lookVector = Vector2.zero;
    Vector2 mouseXY = Vector2.zero;

    [Networked] private NetworkButtons PreviousButtons { get; set; }

    [Networked, Capacity(10)] public NetworkArray<Quaternion> networkPhysicsSyncedRotations { get; }

    bool grounded = false;

    bool readyToJump = true;
    Vector3 flatVel = Vector3.zero;
    Vector3 limitedVel = Vector3 .zero;
    Vector3 moveDirection;
    #endregion

    public override void FixedUpdateNetwork()
    {
        //runs on the local player
        if (GetInput(out NetInput input))
        {
            GetPlayerInput(input);
            RotateBodyAndCamTarget();
            GroundCheck();
            MovePlayer();
            SpeedControl();
            SyncPlayers();
        }
    }

    public void SyncPlayers()
    {
        //for (int i = 0; i < )
    }

    public override void Render()
    {
        RotateBodyAndCamTarget();
    }

    private void RotateBodyAndCamTarget()
    {
        lookVector -= mouseXY;
        lookVector.x = Mathf.Clamp(lookVector.x, -90, 90);

        Vector3 targetBodyRotation = new Vector3(0, lookVector.y, 0);
        Vector3 targetHeadRotation = new Vector3(headJoint.targetRotation.x - lookVector.x, 0, 0);

        mainJoint.targetRotation = Quaternion.Euler(targetBodyRotation);
        headJoint.targetRotation = Quaternion.Euler(targetHeadRotation);
    }

    private void GetPlayerInput(NetInput input)
    {
        moveDirection = (rb.transform.rotation * new Vector3(input.Direction.x, 0f, input.Direction.y)).normalized;
        mouseXY = new Vector2(-input.LookDelta.x * sens, input.LookDelta.y * sens);

        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Jump) && grounded)
        {
           readyToJump = false;

           Jump();

           StartCoroutine(ResetJump());
        }

        PreviousButtons = input.Buttons;
    }

    private void GroundCheck()
    {
        grounded = Physics.SphereCast(new Ray(mainJoint.transform.position, Vector3.down), 0.8f, 0.4f, groundLayer);

        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
            rb.AddForce(new Vector3(0, -extraGravity, 0), ForceMode.Impulse);
        }
    }

    private void MovePlayer()
    {
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
        flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (grounded)
        {
            if (flatVel.magnitude > groundMoveSpeed)
            {
                limitedVel = flatVel.normalized * groundMoveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
        else
        {
            if (flatVel.magnitude > airMoveSpeed)
            {
                limitedVel = flatVel.normalized * airMoveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        rb.angularVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);

    }

    IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(jumpCooldown);
        readyToJump = true;
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            cam.gameObject.SetActive(true);

            if (cam.gameObject != Camera.main.gameObject)
            {
                Camera.main.gameObject.SetActive(false);
            }

            CameraFollow.Singleton.SetTarget(camTarget.transform);
        }

        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {

    }
}

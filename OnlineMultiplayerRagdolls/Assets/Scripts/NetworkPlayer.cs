using NUnit.Framework.Interfaces;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    Vector2 moveInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;

    [SerializeField] float maxSpeed = 3;

    private Vector3 Velocity;
    private Vector3 PlayerMovementInput;
    private float xRotation;

    [Header("Components Needed")]
    [SerializeField] private CharacterController Controller;
    [SerializeField] private Transform Player;
    [SerializeField] ConfigurableJoint mainJoint;
    [Space]
    [Header("Movement")]
    [SerializeField] private float Speed;
    [SerializeField] private float JumpForce;
    [SerializeField] private float Sensitivity;
    [SerializeField] private float Gravity = 9.81f;

    [Tooltip("The current speed I should be moving at")]
    public float currentSpeed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        PlayerMovementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        

        //MovePlayer();

    }


    private void FixedUpdate()
    {
        //Vector3 forwardVel = transform.forward * currentSpeed * PlayerMovementInput.z;
        //Vector3 horizontalVel = transform.right * currentSpeed * PlayerMovementInput.x;
        Vector3 forwardVel = new Vector3(transform.forward.x, 0, transform.forward.z) * currentSpeed * PlayerMovementInput.z;
        Vector3 horizontalVel = new Vector3(transform.right.x, 0, transform.right.z) * currentSpeed * PlayerMovementInput.x;

        rb.linearVelocity = horizontalVel + forwardVel + new Vector3(0, rb.linearVelocity.y, 0);
    }

    private void MovePlayer()
    {
        Vector3 MoveVector = transform.TransformDirection(PlayerMovementInput);

        if (Controller.isGrounded)
        {
            Velocity.y = -1f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Velocity.y = JumpForce;
            }
        }

        Controller.Move(Velocity * Time.deltaTime);

    }


}

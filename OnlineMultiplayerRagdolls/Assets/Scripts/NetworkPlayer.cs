using NUnit.Framework.Interfaces;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NetworkPlayer : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    Vector2 moveInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;

    private Vector3 PlayerMovementInput;


    [Header("Components Needed")]
    [SerializeField] ConfigurableJoint mainJoint;
    [Space]
    [Header("Movement")]
    [SerializeField] private float Speed;
    [SerializeField] private float JumpForce;
    [SerializeField] private float groundDrag;

    bool grounded = false;
    public LayerMask groundLayer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        PlayerMovementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        grounded = Physics.Raycast(transform.position, Vector3.down, 1, groundLayer);

        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }


    private void FixedUpdate()
    {

        MovePlayer();

    }

    private void MovePlayer()
    {
        Vector3 moveDirection = new Vector3(transform.forward.x, 0, transform.forward.z) * PlayerMovementInput.z + new Vector3(transform.right.x, 0, transform.right.z) * PlayerMovementInput.x;
    
        rb.AddForce(moveDirection.normalized * Speed * 10f, ForceMode.Force);
    }

    //private float SpeedControl()
    //{
    //    Vector3 flatVel = new Vector3
    //}

}

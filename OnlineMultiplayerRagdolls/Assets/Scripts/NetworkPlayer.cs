using NUnit.Framework.Interfaces;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    [SerializeField] ConfigurableJoint mainJoint;

    Vector2 moveInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;

    [SerializeField] float maxSpeed = 3;

    [SerializeField] float sensX;
    [SerializeField] float sensY;
    float xRotation;
    float yRotation;


    Vector3 moveDirection = Vector3.zero;
    float moveSpeed = 10f;

    bool isGrounded = false;

    RaycastHit[] raycastHits = new RaycastHit[10];


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       

        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumpButtonPressed = true;
        }

    }


    private void FixedUpdate()
    {
        isGrounded = false;

        int numberOfHits = Physics.SphereCastNonAlloc(rb.position, 0.1f, transform.up * -1, raycastHits, 0.5f);

        for (int i = 0; i < numberOfHits; i++)
        {
            if (raycastHits[i].transform.root == transform)
            {
                continue;
            }

            isGrounded = true;
            break;
        }

        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * 10);
        }

        float inputMagnitude = moveInputVector.magnitude;

        if (inputMagnitude != 0)
        {
            //Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(moveInputVector.x, 0, moveInputVector.y), transform.up);

            //mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredDirection, Time.fixedDeltaTime * 300);
        }
    }


    

}

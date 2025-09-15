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


    bool isGrounded = false;

    RaycastHit[] raycastHits = new RaycastHit[10];


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumpButtonPressed = true;
        }

        Camera.main.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        mainJoint.transform.rotation = Quaternion.Euler(0, yRotation, 0);

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

        }
    }

}

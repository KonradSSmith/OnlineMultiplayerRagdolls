using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] GameObject camPos;
    [SerializeField] ConfigurableJoint headJoint;
    [SerializeField] GameObject body;
    [SerializeField] ConfigurableJoint mainJoint;

    [SerializeField] float sensX;
    [SerializeField] float sensY;
    float xRotation;
    float yRotation;
    Vector2 lookInputVector = Vector2.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;
        yRotation -= mouseX;
        xRotation -= mouseY;
        lookInputVector = new Vector2 (xRotation, yRotation);
        xRotation = Mathf.Clamp(xRotation, -90, 90);


        mainJoint.targetRotation = Quaternion.Euler(0, headJoint.targetRotation.y + yRotation, 0);
        headJoint.targetRotation = Quaternion.Euler(headJoint.targetRotation.x - xRotation, 0, 0);
        Camera.main.transform.position = camPos.transform.position;
        Camera.main.transform.rotation = camPos.transform.rotation;

        //if(lookInputVector.magnitude != 0)
        //{
            //Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(xRotation, 0, yRotation), transform.up);

            //mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredDirection, Time.fixedDeltaTime * 300);
        //}

        //mainJoint.transform.rotation = Quaternion.Euler(0, yRotation, 0);


    }
}

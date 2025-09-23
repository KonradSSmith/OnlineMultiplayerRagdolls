using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class PlayerCam : MonoBehaviour
{
    #region
    [Header("Dependencies")]
    [SerializeField] GameObject camPos;
    [SerializeField] ConfigurableJoint headJoint;
    [SerializeField] ConfigurableJoint mainJoint;

    [Header("Camera Configurations")]
    [SerializeField] float sens;

    Vector2 lookVector = Vector2.zero;
    #endregion

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        GetInput();
        RotateBodyAndCam();
    }

    private void RotateBodyAndCam()
    {
        Vector3 targetBodyRotation = new Vector3(0, lookVector.y, 0);
        Vector3 targetHeadRotation = new Vector3(headJoint.targetRotation.x - lookVector.x, 0, 0);

        mainJoint.targetRotation = Quaternion.Euler(targetBodyRotation);
        headJoint.targetRotation = Quaternion.Euler(targetHeadRotation);

        Camera.main.transform.position = camPos.transform.position;
        Camera.main.transform.rotation = camPos.transform.rotation;
    }

    private void GetInput()
    {
        Vector2 mouseXY = new Vector2(Input.GetAxisRaw("Mouse Y") * sens, Input.GetAxisRaw("Mouse X") * sens);

        lookVector -= mouseXY;
        lookVector.x = Mathf.Clamp(lookVector.x, -90, 90);
    }
}

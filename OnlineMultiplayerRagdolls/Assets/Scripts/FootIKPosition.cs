using UnityEngine;

public class FootIKPosition : MonoBehaviour
{

    [SerializeField] GameObject playerTorso;
    [SerializeField] Rigidbody playerBaseRigidbody;
    [SerializeField] FootIKPosition otherFoot;
    [SerializeField] float footToTorsoOffset;
    [SerializeField] float maxStableDistance;
    [SerializeField] float stepDistance;

    Vector3 globalSnapPosition;

    LayerMask layerMask;

    public bool stable = false;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("Ground");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RaycastHit hit;
        Physics.Raycast(new Vector3(playerTorso.transform.position.x + footToTorsoOffset, playerTorso.transform.position.y, playerTorso.transform.position.z), new Vector3(0, -1, 0), out hit, 2, layerMask);
        transform.position = hit.point;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, new Vector3(playerTorso.transform.position.x, transform.position.y, playerTorso.transform.position.z)) > maxStableDistance && otherFoot.stable)
        {
            stable = false;

            Vector3 newStepPosition = (playerTorso.transform.position + playerBaseRigidbody.linearVelocity.normalized);
            RaycastHit hit;
            Physics.Raycast(newStepPosition, new Vector3(0, -1, 0), out hit, 2, layerMask);
            transform.position = hit.point;

            stable = true;
        }
    }
}

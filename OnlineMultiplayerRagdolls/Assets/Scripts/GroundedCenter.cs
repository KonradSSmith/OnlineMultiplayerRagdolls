using UnityEngine;

public class GroundedCenter : MonoBehaviour
{
    [SerializeField] GameObject playerTorso;

    LayerMask layerMask;

    RaycastHit hit;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("Ground");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Physics.Raycast( playerTorso.transform.position, new Vector3(0, -1, 0), out hit, Mathf.Infinity, layerMask);
        transform.position = hit.point;
    }
}

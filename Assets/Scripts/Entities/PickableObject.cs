using UnityEngine;

public class PickableObject : MonoBehaviour, IPickable
{
    private FixedJoint joint;
    private Rigidbody rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();    
    }

    public void OnPicked(Transform attachPoint)
    {
        //Get Picked
        transform.position = attachPoint.position;
        transform.rotation = attachPoint.rotation;
        transform.SetParent(attachPoint);
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void OnDropped()
    {
        Destroy(joint);
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.SetParent(null);

    }
}

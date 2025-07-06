using UnityEngine;

public class Bullet : MonoBehaviour
{
    private PooledObject pooledObject;

    private void Start()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with " + collision.gameObject.name);

        IDestroyable destroyable = collision.gameObject.GetComponent<IDestroyable>();

        if (destroyable != null)
        {
            destroyable.OnCollided();
            
        }
        pooledObject.Destroy();


    }
}

using UnityEngine;

public class DestroyableObject : MonoBehaviour, IDestroyable
{
    public void OnCollided()
    {
        Destroy(gameObject);
    }
}

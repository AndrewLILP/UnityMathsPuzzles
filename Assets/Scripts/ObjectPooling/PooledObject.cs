using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class PooledObject : MonoBehaviour
{
    [SerializeField] private UnityEvent onReset; // Event to call when the object is reset

    ObjectPool associatedPool; // Reference to the pool this object belongs to

    private float timer;
    private bool setToDestroy = false;
    private float destroyTime = 0f; // Time after which the object will be destroyed if not returned to the pool

    public void SetObjectPool(ObjectPool pool)
    {
        associatedPool = pool;
        timer = 0;
        destroyTime = 0f;
        setToDestroy = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (setToDestroy)
        {
            timer += Time.deltaTime;
            if (timer >= destroyTime)
            {
                setToDestroy = false;
                timer = 0;
                setToDestroy = false;
                Destroy();
            }
        }
    }

    public void Destroy()
    {
        if (associatedPool != null)
        {
            associatedPool.RestoreObject(this);
        }
    }

    public void Destroy(float time)
    {
        setToDestroy = true;
        destroyTime = time;
    }

    public void ResetObject()
    {
        onReset?.Invoke();
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ObjectPool : MonoBehaviour
{
    public GameObject objectToPool; // The prefab to pool
    public int startsize;

    [SerializeField] private List<PooledObject> objectPool = new List<PooledObject>();
    [SerializeField] private List<PooledObject> usedPool = new List<PooledObject>();

    private PooledObject tempObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < startsize; i++)
        {
            AddNewObject();
            objectPool.Add(tempObject);
        }
    }

    void AddNewObject()
    {
        //2hrs repeat viewing
        tempObject = Instantiate(objectToPool, transform).GetComponent<PooledObject>();
        tempObject.gameObject.SetActive(false); //make object and turn off
        tempObject.SetObjectPool(this);
        objectPool.Add(tempObject);

    }

    public PooledObject GetPooledObject()
    {
        PooledObject tempObject;
        if (objectPool.Count > 0)
        {
            tempObject = objectPool[0];
            usedPool.Add(tempObject);
            objectPool.RemoveAt(0);
        }
        else
        {
            AddNewObject();
            tempObject = GetPooledObject();
        }

        tempObject.gameObject.SetActive(true);
        tempObject.ResetObject();
        return tempObject;
    }

    public void DestroyPooledObject(PooledObject obj, float time = 0)
    {
        if (time == 0)
        {
            obj.Destroy();
        }
        else
        {
            obj.Destroy(time);
        }
    }

    public void RestoreObject(PooledObject obj)
    {
        Debug.Log("Restore");
        obj.gameObject.SetActive(false);
        usedPool.Remove(obj);
        objectPool.Add(obj);
    }
}

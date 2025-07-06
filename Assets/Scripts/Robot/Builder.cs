using UnityEngine;

// created for Lesson 8

public class Builder : MonoBehaviour
{
    [SerializeField] private GameObject objectToBuild; // reference to the object to build
    [SerializeField] private Transform placementPoint; // reference to the placement point where the object will be built

    public void Build()
    {
        Instantiate(objectToBuild, placementPoint.position, placementPoint.rotation);

        // destroy the builder object after building
        Destroy(gameObject); // destroy the builder object after building

    }
}

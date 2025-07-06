using UnityEngine;

// created for lesson 9

public class LevelTrigger : MonoBehaviour
{
    [SerializeField] private LevelManager endingLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            endingLevel.EndLevel();
            Destroy(gameObject);
        }
    }
}

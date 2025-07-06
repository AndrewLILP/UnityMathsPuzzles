using UnityEngine;

public class SimpleKeycard : MonoBehaviour
{
    [SerializeField] private string keycardId = "bridge_access";

    private IKeycardService keycardService;
    private bool isCollected = false;

    private void Start()
    {
        keycardService = KeycardServiceManager.GetService();

        // Make it spin
        if (GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void Update()
    {
        if (!isCollected)
        {
            transform.Rotate(0, 90 * Time.deltaTime, 0); // Spin animation
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            keycardService.CollectKeycard(keycardId);

            // Hide keycard
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;

            // Destroy after 1 second
            Destroy(gameObject, 1f);
        }
    }
}
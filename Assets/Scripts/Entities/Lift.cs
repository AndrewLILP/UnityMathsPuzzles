using UnityEngine;

public class Lift : MonoBehaviour
{
    [SerializeField] private float moveDistance;
    [SerializeField] private bool isUp;
    [SerializeField] private float speed;

    bool isMoving;
    Vector3 targetPosition;

    public void ToggleLift()
    {
        if (isMoving) return; // Prevent multiple calls while moving
        
        if (isUp)
        {
            targetPosition = transform.localPosition - new Vector3(0, moveDistance, 0);// move back down
            isUp = false;
        }
        else
        {
            targetPosition = transform.localPosition + new Vector3(0, moveDistance, 0); // move up
            isUp = true;
        }
        isMoving = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            // Move towards the target position
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, speed * Time.deltaTime);
        }
        if (Vector3.Distance(transform.localPosition, targetPosition) < 0.05f)
        {
            // Stop moving when close enough to the target position
            isMoving = false;
            //transform.localPosition = targetPosition; // Ensure we snap to the exact position
        }
    }
}

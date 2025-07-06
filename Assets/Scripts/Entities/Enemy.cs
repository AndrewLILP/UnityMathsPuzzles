using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// patrol between 3 points using NavMeshAgent - or array of points
/// if player is seen, chase player - 
/// </summary>

public class Enemy : MonoBehaviour
{
    [SerializeField] private Transform[] targetPoints; // The points the enemy will move between
    // 1 point was, [SerializeField] private Transform targetPoint; - The point the enemy will move towards

    [SerializeField] private Transform enemyEye; // The point from which the enemy will "see" the player
    [SerializeField] private float playerCheckDistance; // The distance within which the enemy will check for the player
    [SerializeField] private float playerCheckRadius = 0.4f; // The radius within which the enemy will check for the player

    int currentTarget = 0; // The index of the current target point the enemy is moving towards


    private NavMeshAgent agent; // The NavMeshAgent component for pathfinding
                                // Start is called once before the first execution of Update after the MonoBehaviour is created

    public bool isIdle = true; // Flag to check if the enemy is idle
    public bool isPlayerFound;
    public bool isCloseToPlayer;

    public Transform player; // Reference to the player object
    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        // L7; agent.destination = targetPoint.position; // Set the destination to the target points
        agent.destination = targetPoints[currentTarget].position; // Set the destination to the first target point

    }

    // Update is called once per frame
    void Update()
    {
        if (isIdle)
        {
            Idle();
        }
        else if (isPlayerFound) 
        { 
            if(isPlayerFound) 
            { 
                FollowPlayer(); 
            }
        }


                


    }

    void Idle()
    {
        if (agent.remainingDistance < 0.2f)
        {
            currentTarget++;
            if (currentTarget >= targetPoints.Length)
                currentTarget = 0; // Reset to the first target point if all points have been visited
            agent.destination = targetPoints[currentTarget].position; // Set the new destination to the next target point ** line used twice = could refactor into a method
        }

        // Check if the player is within the enemy's sight using a sphere cast
        if (Physics.SphereCast(enemyEye.position, playerCheckRadius, transform.forward, out RaycastHit hit, playerCheckDistance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                Debug.Log("Player found!");
                isIdle = false; // Set the enemy to not idle
                isPlayerFound = true; // Set the enemy to have found the player
                player = hit.transform; // Set the player reference to the found player
                agent.destination = player.position; // Set the destination to the player's position
            }
        }


    }

    void FollowPlayer()
    {
        if (player != null)
        {
            if (Vector3.Distance(transform.position, player.position) > 10)
            {

                isPlayerFound = false; // Reset the player found flag if the player is too far away
                isIdle = true; // Set the enemy back to idle
            }

            // Attack
            if (Vector3.Distance(transform.position, player.position) < 2)
            {
                isCloseToPlayer = true; // Set the enemy to be close to the player
            }
            else
            {
                isCloseToPlayer = false; // Reset the close to player flag if the player is too far away
            }

            agent.destination = player.position; // Set the destination to the player's position

        }
        else
        {
            isPlayerFound = false; // Reset the player found flag if the player reference is null
            isIdle = true; // Set the enemy back to idle
            isCloseToPlayer = false; // Reset the close to player flag if the player reference is null
        }
    }

    void AttackPlayer()
    {
        // Implement attack logic here, e.g., reduce player health, play attack animation, etc.
        Debug.Log("Attacking player!");
        if (Vector3.Distance(transform.position, player.position) > 2)
        {
            isCloseToPlayer = false; // Reset the close to player flag if the player is too far away
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Set the color of the Gizmos to red
        Gizmos.DrawWireSphere(enemyEye.position, playerCheckRadius); // Draw a wire sphere at the enemy's eye position to visualize the player check radius
        Gizmos.DrawWireSphere(enemyEye.position + enemyEye.forward * playerCheckDistance, playerCheckRadius); // Draw a wire sphere at the enemy's eye position plus the forward direction multiplied by the player check distance to visualize the player check distance
        Gizmos.DrawLine(enemyEye.position, enemyEye.position + enemyEye.forward * playerCheckDistance); // Draw a line from the enemy's eye position in the forward direction to visualize the player check distance

    }
}

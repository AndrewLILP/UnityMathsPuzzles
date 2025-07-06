using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    private EnemyState currentState;

    public Transform[] targetPoints; // The points the enemy will move between
    public Transform enemyEye; // The point from which the enemy will "see" the player
    public float playerCheckDistance; // The distance within which the enemy will check for the player
    public float playerCheckRadius = 0.4f; // The radius within which the enemy will check for the player

    public NavMeshAgent agent;  // The NavMeshAgent component for pathfinding

    [HideInInspector] public Transform player; // Reference to the player object

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Initialize the enemy state to Idle
        currentState = new EnemyIdleState(this);
        currentState.OnStateEnter();
    }

    // Update is called once per frame
    void Update()
    {
        currentState.OnStateUpdate();

    }

    public void ChangeState(EnemyState newState)
    {
        currentState.OnStateExit();
        currentState = newState;
        currentState.OnStateEnter();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Set the color of the Gizmos to red
        Gizmos.DrawWireSphere(enemyEye.position, playerCheckRadius); // Draw a wire sphere at the enemy's eye position to visualize the player check radius
        Gizmos.DrawWireSphere(enemyEye.position + enemyEye.forward * playerCheckDistance, playerCheckRadius); // Draw a wire sphere at the enemy's eye position plus the forward direction multiplied by the player check distance to visualize the player check distance
        Gizmos.DrawLine(enemyEye.position, enemyEye.position + enemyEye.forward * playerCheckDistance); // Draw a line from the enemy's eye position in the forward direction to visualize the player check distance

    }
}

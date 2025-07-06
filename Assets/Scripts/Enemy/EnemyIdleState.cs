using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyIdleState : EnemyState
{
    int currentTarget = 0; // Index of the current target point the enemy is moving towards
    // constructor
    public EnemyIdleState(EnemyController enemy) : base(enemy) { }

    public override void OnStateEnter()
    {
        enemy.agent.destination = enemy.targetPoints[currentTarget].position; // Set the destination to the first target point
        Debug.Log("Enemy is Idle Enter");
    }

    public override void OnStateExit()
    {
        Debug.Log("Enemy Exisitng Idle");
    }

    public override void OnStateUpdate()
    {
        if (enemy.agent.remainingDistance < 0.2f)
        {
            currentTarget++;
            if (currentTarget >= enemy.targetPoints.Length)
                currentTarget = 0; // Reset to the first target point if all points have been visited
            enemy.agent.destination = enemy.targetPoints[currentTarget].position; // Set the new destination to the next target point ** line used twice = could refactor into a method
        }

        // Check if the player is within the enemy's sight using a sphere cast
        if (Physics.SphereCast(enemy.enemyEye.position, enemy.playerCheckRadius, enemy.transform.forward, out RaycastHit hit, enemy.playerCheckDistance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                Debug.Log("Player found!");
                enemy.player = hit.transform;
                enemy.agent.destination = enemy.player.position;

                //Move to the follow state
                enemy.ChangeState(new EnemyFollowState(enemy));
            }
        }
    }
}

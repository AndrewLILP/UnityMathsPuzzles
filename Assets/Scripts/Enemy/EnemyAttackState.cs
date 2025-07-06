using UnityEngine;

//updated in Lesson 7


public class EnemyAttackState : EnemyState
{
    float distanceToPlayer;
    Health playerHealth;
    float damageOverTime = 20f; // Example damage value, adjust as needed

    public EnemyAttackState(EnemyController enemy) : base(enemy)    
    {
        playerHealth = enemy.player.GetComponent<Health>(); // getting reference to player's health component from enemyController.cs
    }

    void Attack()
    {
        if (playerHealth != null)
        {
            playerHealth.DeductHealth(damageOverTime * Time.deltaTime); // Apply damage over time to the player
        }

    }

    public override void OnStateEnter()
    {
        Debug.Log("Enemy is attacking the player");
    }

    public override void OnStateExit()
    {
        Debug.Log("Enemy Exiting Attack State - NOT attacking player");
    }

    public override void OnStateUpdate()
    {
        if (enemy.player != null)
        {
            Attack(); // Call the attack method to apply damage to the player

            distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);

            if (distanceToPlayer > 2)
            {
                // If the player is too far away, switch back to follow state
                enemy.ChangeState(new EnemyFollowState(enemy));
            }
            
            else
            {
                enemy.agent.destination = enemy.player.position; // Keep moving towards the player;
                enemy.ChangeState(new EnemyIdleState(enemy)); // If the player is too far away, switch back to idle state
            }
        }

    }

}

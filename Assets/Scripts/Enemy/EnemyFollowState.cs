using UnityEngine;

public class EnemyFollowState : EnemyState
{
    float distanceToPlayer;

    //constructor

    public EnemyFollowState(EnemyController enemy) : base(enemy) { }
    
    public override void OnStateEnter()
    {
        Debug.Log("Enemy is following the player");

    }

    public override void OnStateExit()
    {
        Debug.Log("Enemy Exiting Follow State");

    }

    public override void OnStateUpdate()
    {
        if (enemy.player != null)
        {
            distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
            
            if (distanceToPlayer > 10)
            {
                enemy.ChangeState(new EnemyIdleState(enemy)); // Change to idle state if player is too far away
            }

            if (distanceToPlayer < 2)
            {
                enemy.ChangeState(new EnemyAttackState(enemy)); // Change to attack state if player is close enough
            }
            enemy.agent.destination = enemy.player.position; // Set the destination to the player's position
        }
        else
        {
            enemy.ChangeState(new EnemyIdleState(enemy)); // Change to idle state if player reference is null
        }

    }
}

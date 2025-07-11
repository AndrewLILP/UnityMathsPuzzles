using UnityEngine;

public class LessonNotes : MonoBehaviour
{
    /// <summary>
    /// Keycard ideas for Project (Midterm) - 
    /// shoot keycards as projectiles to open doors,
    /// use keycards to open doors as a collision trigger,
    /// 
    /// </summary>
    // Lesson 10 Notes:
    // modified files:
    // Trigger.cs - added UI


    // Lesson 9 Notes;
    // CutScene Tutorial
    // New files:
    // -- GameManager.cs 
    // -- LevelManager.cs
    // -- Keycard.cs
    // -- KeycardManager.cs
    // -- Trigger.cs
    // -- LevelTrigger.cs

    // modified files:



    // Lesson 8 Notes:
    // focus on Command Pattern
    // New files:
    // -- Command.cs
    // -- MoveCommand.cs
    // -- CommandInteractor.cs
    // -- BuildCommand.cs

    // modified files:
    // -- PlayerInput.cs - added new inputs for command pattern.
    // -- commandInteractor.cs - added new inputs for command pattern.


    // Lesson 7 (class7.mp4) Notes:
    // download asset files and go to 46mins
    // new files:
    // -- Health.cs
    // -- RocketShootStrategy.cs
    // -- BulletShootStrategy.cs
    // modified files:
    // -- EnemyAttackState.cs - 
    // -- EnemyFollowState.cs - added Health component to the enemy.
    // -- shootInteractor.cs - replaced with IShootStrategy interface.
    // -- PlayerInput.cs - added new inputs for shooting strategies.



    //---------------------
    // Lesson 6 Notes:
    // Changed files
    // -- bullet.cs - PooledObject component added, OnCollisionEnter method modified to use IDestroyable interface.
    // -- enemy.cs - L6 NavMeshAgent component added, Start method modified to set the destination to a target point.
    // --- using arrays - [SerializeField] private Transform[] targetPoints;
    // 
    // Refactor Enemy.cs using files: 
    // -- EnemyState.cs - Created to manage the enemy's state.
    // -- EnemyIdleState.cs - Created to handle the idle state of the enemy.
    // -- EnemyFollowState.cs - Created to handle the follow state of the enemy.
    // -- EnemyAttackState.cs - Created to handle the attack state of the enemy.
    // -- EnemyState.cs - Base class for enemy states, containing common functionality. Abstract class.
    // **** errors at 2:33 (Unity updates)

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

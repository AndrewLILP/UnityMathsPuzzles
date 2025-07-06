using UnityEngine;
using UnityEngine.AI; // for NavMeshAgent

// created for Lesson 8


public class MoveCommand : Command
{
    private NavMeshAgent agent;
    private Vector3 destination;

    public MoveCommand(NavMeshAgent agent, Vector3 destination)
    {
        this.agent = agent;
        this.destination = destination;
    }

    public override void Execute()
    {
        agent.SetDestination(destination);
    }


    /// <summary>
    /// This bool variable indicates whether the robot has reached its destination.
    /// </summary>
    /// <returns></returns>
    public override bool isComplete => ReachedDestination();


    /*public override bool isComplete - same thing as above, but with a property instead of a method
    {
        get
        {
            return ReachedDestination();
        }
    }
    */





    bool ReachedDestination()
    {
        if (agent.remainingDistance > 0.2f)
        {
            return false; // still moving
            // add something to remove waypoint pointer here if needed
        }
        return true; // reached destination
    }
}

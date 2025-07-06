using UnityEngine;
using UnityEngine.AI; // for NavMeshAgent

// created for Lesson 8

public class BuildCommand : Command
{
    private NavMeshAgent agent;
    private Builder builder;

    public BuildCommand(NavMeshAgent agent, Builder builder)
    {
        this.agent = agent;
        this.builder = builder;
    }

    public override void Execute()
    {
        // Set the agent's destination to the builder's position
        agent.SetDestination(builder.transform.position);
    }

    public override bool isComplete => BuildComplete();

    bool BuildComplete()
    {
        if (agent.remainingDistance > 0.1f)
            return false; // still moving

        if (builder != null)
            builder.Build(); // call the Build method on the builder
        return true; // building complete
    }

}

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI; // for NavMeshAgent

// created for Lesson 8

public class CommandInteractor : Interactor
{
    Queue<Command> commands = new Queue<Command>();

    [SerializeField] private NavMeshAgent agent; // reference to the NavMeshAgent component
    [SerializeField] private GameObject pointerPrefab; // reference to the pointer prefab
    [SerializeField] private Camera cam;

    private Command currentCommand; // the command currently being executed

    public override void Interact()
    {
        if (input.commandPressed)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(ray, out var hitInfo))
            {
                if (hitInfo.transform.CompareTag("Ground"))
                {
                    GameObject pointer = Instantiate(pointerPrefab);
                    pointer.transform.position = hitInfo.point;

                    commands.Enqueue(new MoveCommand(agent, hitInfo.point));

                }
                else if (hitInfo.transform.CompareTag("Builder"))
                {
                    commands.Enqueue(new BuildCommand(agent, hitInfo.transform.GetComponent<Builder>()));
                }
            }
        }
        ProcessCommands();

    }

    public void ProcessCommands()
    {
        if (currentCommand != null && !currentCommand.isComplete)
            return;
        if (commands.Count == 0)
            return;

        currentCommand = commands.Dequeue();
        currentCommand.Execute();
    }

}

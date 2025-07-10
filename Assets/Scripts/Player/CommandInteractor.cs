using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI; // for NavMeshAgent

// Updated CommandInteractor with wall tag protection

public class CommandInteractor : Interactor
{
    Queue<Command> commands = new Queue<Command>();

    [SerializeField] private NavMeshAgent agent; // reference to the NavMeshAgent component
    [SerializeField] private GameObject pointerPrefab; // reference to the pointer prefab
    [SerializeField] private Camera cam;

    [Header("Invalid Target Tags")]
    [SerializeField] private string[] invalidTags = { "Wall", "Enemy", "Obstacle" }; // Tags to ignore for movement commands

    private Command currentCommand; // the command currently being executed

    public override void Interact()
    {
        if (input.commandPressed)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(ray, out var hitInfo))
            {
                // Check if the target has an invalid tag
                if (IsInvalidTarget(hitInfo.transform))
                {
                    Debug.Log($"Cannot move to {hitInfo.transform.name} - Invalid target!");
                    // Optional: Play error sound or show visual feedback
                    return;
                }

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
                else
                {
                    // Handle other valid targets or provide feedback for unrecognized targets
                    Debug.Log($"Target {hitInfo.transform.name} is not a valid command target.");
                }
            }
        }
        ProcessCommands();
    }

    /// <summary>
    /// Check if the target has a tag that should prevent movement commands
    /// </summary>
    /// <param name="target">The transform to check</param>
    /// <returns>True if the target should be ignored</returns>
    private bool IsInvalidTarget(Transform target)
    {
        foreach (string invalidTag in invalidTags)
        {
            if (target.CompareTag(invalidTag))
            {
                return true;
            }
        }
        return false;
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

    /// <summary>
    /// Add a tag to the invalid targets list at runtime
    /// </summary>
    /// <param name="tag">Tag to add to invalid list</param>
    public void AddInvalidTag(string tag)
    {
        // Create new array with additional tag
        string[] newInvalidTags = new string[invalidTags.Length + 1];
        invalidTags.CopyTo(newInvalidTags, 0);
        newInvalidTags[invalidTags.Length] = tag;
        invalidTags = newInvalidTags;
    }

    /// <summary>
    /// Remove a tag from the invalid targets list
    /// </summary>
    /// <param name="tag">Tag to remove from invalid list</param>
    public void RemoveInvalidTag(string tag)
    {
        List<string> tagList = new List<string>(invalidTags);
        tagList.Remove(tag);
        invalidTags = tagList.ToArray();
    }
}
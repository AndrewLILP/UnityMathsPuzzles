using UnityEngine;
using UnityEngine.Events;


// created for Lesson 9

public class LevelManager : MonoBehaviour
{
    [SerializeField] private bool isFinalLevel;

    public UnityEvent onLevelStart;
    public UnityEvent onLevelEnd;

    public void StartLevel()
    {
        onLevelStart?.Invoke();
    }

    public void EndLevel()
    {
        onLevelEnd?.Invoke();

        if (isFinalLevel)
        {
            GameManager.GetInstance().ChangeState(GameManager.GameState.GameEnd, this);
        }
        else
        {
            GameManager.GetInstance().ChangeState(GameManager.GameState.LevelEnd, this);
        }
    }
}

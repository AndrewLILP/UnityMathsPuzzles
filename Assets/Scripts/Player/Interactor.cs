using UnityEngine;

public abstract class Interactor : MonoBehaviour
{
    protected PlayerInput input;

    private void Awake()
    {
        input = PlayerInput.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
        Interact();
    }

    public abstract void Interact();

}

// abtract classes often go in Awake()
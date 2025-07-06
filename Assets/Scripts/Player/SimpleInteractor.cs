using UnityEngine;

public class SimpleInteractor : Interactor
{
    [Header("Interact Settings")]
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float interactDistance;

    private RaycastHit raycastHit;
    private ISelectable selectable; // Interface for selectable objects

    public override void Interact()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out raycastHit, interactDistance, interactLayer, QueryTriggerInteraction.Ignore)) // trying QueryTriggerInteraction.Ignore
        {
            selectable = raycastHit.transform.GetComponent<ISelectable>(); // get the ISelectable component from the object hit by the raycast
            
            if (selectable != null)
            {
                selectable.OnHoverEnter(); // call the OnHoverEnter method of the ISelectable interface
                if (input.activatePressed) // check if the E key is pressed
                {
                    selectable.OnSelect(); // call the OnSelect method of the ISelectable interface
                }
            }
        }
        if (raycastHit.transform == null && selectable != null)
        {
            selectable.OnHoverExit(); // call the OnHoverExit method of the ISelectable interface
            selectable = null; // reset the selectable object
        }

    }
}

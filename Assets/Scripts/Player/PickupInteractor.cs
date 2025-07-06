using UnityEngine;

public class PickupInteractor : Interactor
{

    [Header("Picked & Dropped")]
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask pickupLayer; //1h52
    [SerializeField] private float pickupDistance;
    [SerializeField] private Transform attachTransform;

    private bool isPicked = false;
    private RaycastHit raycastHit;
    private IPickable pickable; // Interface for pickable objects

    public override void Interact()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out raycastHit, pickupDistance, pickupLayer, QueryTriggerInteraction.Ignore))
        {
            if (input.activatePressed && !isPicked)
            {
                pickable = raycastHit.transform.GetComponent<IPickable>();
                if (pickable == null) return;

                pickable.OnPicked(attachTransform); // call the OnPicked method of the IPickable interface
                isPicked = true; // Set the flag to indicate that an object is picked
                return;
            }
            // If the player is already holding something, drop it
            if (input.activatePressed && isPicked && pickable != null)
            {
                pickable.OnDropped(); // call the OnDropped method of the IPickable interface
                isPicked = false; // Reset the flag to indicate that no object is picked

            }
        }

    }

}

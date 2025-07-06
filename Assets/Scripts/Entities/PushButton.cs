using UnityEngine;
using UnityEngine.Events;

public class PushButton : MonoBehaviour, ISelectable
{
    [SerializeField] private Material defaultColour;
    [SerializeField] private Material hoverColour;
    [SerializeField] private MeshRenderer meshRenderer;

    public UnityEvent onPush; // fill in the inspector

    
    public void OnSelect()
    {
        onPush?.Invoke(); // this avoids crashes if the push button is not assigned in the inspector
    }
    public void OnHoverEnter()
    {
        meshRenderer.material = hoverColour;
    }
    public void OnHoverExit()
    {
        meshRenderer.material = defaultColour;
    }
}

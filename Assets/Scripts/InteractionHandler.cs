using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    private static InteractionHandler _instance;

    public static InteractionHandler Instance => _instance;

    private IInteractable _currentInteractableObject;


    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            _currentInteractableObject?.Interact();  
        }
    }

    public void SetInteractable(IInteractable interactable)
    {
        _currentInteractableObject = interactable;
    }

    public void RemoveInteractable(IInteractable interactable)
    {
        if (_currentInteractableObject == interactable)
            _currentInteractableObject = null;
    }
}

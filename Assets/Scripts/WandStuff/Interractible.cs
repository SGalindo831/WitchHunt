using UnityEngine;

public interface IInteractable
{
    void OnHoverEnter();
    void OnHoverExit();
    void OnInteract(Transform player);
    string GetInteractionPrompt();
    bool CanInteract();
}
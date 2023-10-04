using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractionTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private TutorialText _tutorialText;

    private Collider _collider;
    private Collider _otherCollider;

    protected TutorialText TutorialText => _tutorialText;

    protected Collider OtherCollider => _otherCollider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();

        Debug.Assert( _collider.isTrigger );
    }

    private void Start()
    {
        _tutorialText.enabled = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out PlayerV2 player))
        {
            EnterTriggerSpace(other);
        }
        else if (other.TryGetComponent(out EnemyAI enemy) && enemy.IsUnderControl)
        {
            EnterTriggerSpace(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerV2 player))
        {
            ExitTriggerSpace(other);
        }
        else if (other.TryGetComponent(out EnemyAI enemy) && enemy.IsUnderControl)
        {
            ExitTriggerSpace(other);
        }
    }

    protected virtual void EnterTriggerSpace(Collider other)
    {
        InteractionHandler.Instance.SetInteractable(this);
        _tutorialText.enabled = true;
        _otherCollider = other;
    }

    protected virtual void ExitTriggerSpace(Collider other)
    {
        InteractionHandler.Instance.RemoveInteractable(this);
        _tutorialText.enabled = false;
        _otherCollider = null;
    }

    public virtual void Interact()
    {
        Debug.Log("Interact");
    }
}

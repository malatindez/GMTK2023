using UnityEngine;

public class DoorTrigger : InteractionTrigger
{
    [SerializeField] private Door _door;
    [SerializeField] private int _doorAccess;

    [SerializeField] private string _defaultText = "Press 'E'";
    [SerializeField] private string _noAccessTextFormat = "access level {0} required";

    private int _currentAccessLevel;

    protected override void EnterTriggerSpace(Collider other)
    {
        _currentAccessLevel = other.GetComponent<AccessLevel>().accessLevel;

        if (_currentAccessLevel < _doorAccess)
        {
            TutorialText.Text.text = string.Format(_noAccessTextFormat, _doorAccess);
        }
        else
        {
            TutorialText.Text.text = _defaultText;
        }

        base.EnterTriggerSpace(other);
    }

    public override void Interact()
    {
        if (_currentAccessLevel >= _doorAccess)
        {
            if (!_door.IsOpen)
                _door.DoorOpen();
            else
                _door.DoorClose();
        }
    }
}

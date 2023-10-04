using UnityEngine;

public class DoorTrigger : InteractionTrigger
{
    [SerializeField] private Door _door;
    [SerializeField] private AccessLevel _minAccessLevel;
    [SerializeField] private AccessLevel _maxAccessLevel = AccessLevel.Violet;

    [SerializeField] private string _defaultText = "Press 'E'";
    [SerializeField] private string _noAccessTextFormat = "{0} access level required";

    private AccessLevel _currentAccessLevel;

    protected override void EnterTriggerSpace(Collider other)
    {
        _currentAccessLevel = other.GetComponent<AccessCard>()?.AccessLevel ?? AccessLevel.None;

        if (_currentAccessLevel < _minAccessLevel || _currentAccessLevel > _maxAccessLevel)
        {
            TutorialText.Text.text = string.Format(_noAccessTextFormat, _minAccessLevel);
        }
        else
        {
            TutorialText.Text.text = _defaultText;
        }

        base.EnterTriggerSpace(other);
    }

    public override void Interact()
    {
        if (_currentAccessLevel >= _minAccessLevel && _currentAccessLevel <= _maxAccessLevel)
        {
            if (!_door.IsOpen)
                _door.DoorOpen();
            else
                _door.DoorClose();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTerminalTrigger : InteractionTrigger
{
    [Space]
    [SerializeField] private string _title;
    [SerializeField] [TextArea(3, 10)] private string _text;
    [SerializeField] private Door[] _doors = new Door[0];

    [Space]
    [SerializeField] private AccessLevel _minAccessLevel;
    [SerializeField] private string _errorTitle;
    [SerializeField][TextArea(3, 10)] private string _errorText;

    private ITerminalCommand[] _commands;
    private ITerminalCommand _exitCommand;

    private void Start()
    {
        _commands = new ITerminalCommand[_doors.Length + 1];

        for (int i = 0; i < _doors.Length; i++)
        {
            _commands[i] = new DoorTerminalCommand(_doors[i]);
        }

        _exitCommand = new TerminalCommand(Terminal.Instance.HideTerminal) { Text = "Exit" };
        _commands[_doors.Length] = _exitCommand;
    }

    public override void Interact()
    {
        var currentAccessLevel = OtherCollider.GetComponent<AccessCard>()?.AccessLevel ?? AccessLevel.None;

        if (currentAccessLevel < _minAccessLevel)
            Terminal.Instance.ShowTerminal(_errorTitle, string.Format(_errorText, _minAccessLevel, currentAccessLevel), new ITerminalCommand[] { _exitCommand });
        else
            Terminal.Instance.ShowTerminal(_title, _text, _commands);
    }
}

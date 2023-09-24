using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaDoorTerminalTrigger : InteractionTrigger
{
    [SerializeField] private string _title;
    [SerializeField] [TextArea(3, 10)] private string _text;
    [SerializeField] private Door[] _doors = new Door[0];

    private ITerminalCommand[] _commands;

    private void Start()
    {
        _commands = new ITerminalCommand[_doors.Length + 1];

        for (int i = 0; i < _doors.Length; i++)
        {
            _commands[i] = new DoorTerminalCommand(_doors[i]);
        }

        _commands[_doors.Length] = new TerminalCommand(LaTerminal.Instance.HideTerminal) { Text = "Exit" };
    }

    public override void Interact()
    {
        LaTerminal.Instance.ShowTerminal(_title, _text, _commands);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DoorTerminalCommand : ITerminalCommand
{
    private Door _door;

    public string Text => GetText();

    public event EventHandler TextChanged;
    public event EventHandler CanExecuteChanged;

    public DoorTerminalCommand(Door door)
    {
        _door = door;
        _door.DoorStateChanged += OnDoorStateChanged;
    }

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        if (!_door.IsOpen)
            _door.DoorOpen();
        else
            _door.DoorClose();

        LaTerminal.Instance.HideTerminal();
    }

    protected virtual void OnTextChanged(EventArgs e)
    {
        TextChanged?.Invoke(this, e);
    }

    protected virtual void OnCanExecuteChanged(EventArgs e)
    {
        CanExecuteChanged?.Invoke(this, e);
    }

    protected virtual string GetText()
    {
        if (!_door.IsOpen)
            return $"Open {_door.DisplayName}";
        else
            return $"Close {_door.DisplayName}";
    }

    private void OnDoorStateChanged()
    {
        OnTextChanged(EventArgs.Empty);
    }
}

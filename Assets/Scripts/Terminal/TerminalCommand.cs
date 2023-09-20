using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using UnityEngine;

public class TerminalCommand : ITerminalCommand
{
    private Action _execute;
    private Func<bool> _canExecute;

    public event EventHandler CanExecuteChanged;
    public event EventHandler TextChanged;

    public string Text { get; set; }

    public TerminalCommand(Action execute) : this(execute, () => true)
    {
    }

    public TerminalCommand(Action execute, Func<bool> canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public void Execute() => _execute();

    public virtual bool CanExecute(object parameter) => _canExecute();

    public virtual void Execute(object parameter) => _execute();

    
}

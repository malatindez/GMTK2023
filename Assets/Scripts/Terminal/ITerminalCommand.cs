using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

public interface ITerminalCommand : ICommand
{
    event EventHandler TextChanged;

    string Text { get; set; }
}

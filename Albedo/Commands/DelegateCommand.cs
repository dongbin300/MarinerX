using System;
using System.Windows.Input;

namespace Albedo.Commands
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged = null;
        private readonly Action<object?> execute;

        public DelegateCommand(Action<object?> execute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            execute(parameter);
        }
    }
}

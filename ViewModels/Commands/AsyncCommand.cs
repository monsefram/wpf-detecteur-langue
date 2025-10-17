using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace tpfred2.ViewModels.Commands
{
    public class AsyncCommand : ICommand
    {
        private readonly Func<object?, Task> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public AsyncCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute)
        {
            _execute = execute ?? throw new NullReferenceException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public async void Execute(object? parameter)
        {
            await _execute(parameter);
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

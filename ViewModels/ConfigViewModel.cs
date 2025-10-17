using System.ComponentModel;
using System.Runtime.CompilerServices;
using tpfred2.Models;
using tpfred2.ViewModels.Commands;

namespace tpfred2.ViewModels
{
    public class ConfigViewModel : BaseViewModel
    {
        private readonly SettingsStore _store;

        public string Token
        {
            get => _store.Current.ApiToken;
            set { _store.Current.ApiToken = value; OnPropertyChanged(); }
        }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public event EventHandler<bool>? CloseRequested;

        public ConfigViewModel(SettingsStore store)
        {
            _store = store;
            SaveCommand = new RelayCommand(_ => { _store.Save(); CloseRequested?.Invoke(this, true); }, _ => true);
            CancelCommand = new RelayCommand(_ => { _store.Load(); CloseRequested?.Invoke(this, false); }, _ => true);

        }
    }
}

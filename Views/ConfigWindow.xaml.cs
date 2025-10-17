using System.Windows;
using tpfred2.ViewModels;

namespace tpfred2.Views
{
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (DataContext is ConfigViewModel vm)
                    vm.CloseRequested += (_, __) => Close();
            };
        }
    }
}

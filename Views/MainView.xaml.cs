using System.Windows;

namespace tpfred2.Views
{
    public partial class MainView : Window   // <- Window, pas UserControl ni autre
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainViewModel();

        }
    }
}

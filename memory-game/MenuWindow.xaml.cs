using System.Windows;

namespace memory_game
{
    /// <summary>
    /// Interaction logic for MenuWindow.xaml
    /// </summary>
    public partial class MenuWindow : Window
    {
        public MenuViewModel ViewModel { get; private set; }

        public MenuWindow()
        {
            InitializeComponent();
            ViewModel = new MenuViewModel();
            DataContext = ViewModel;
        }
    }
}
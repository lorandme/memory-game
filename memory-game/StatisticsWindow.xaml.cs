using System.Windows;

namespace memory_game
{
    /// <summary>
    /// Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        public StatisticsViewModel ViewModel { get; private set; }

        public StatisticsWindow()
        {
            InitializeComponent();
            ViewModel = new StatisticsViewModel(this);
            DataContext = ViewModel;
        }
    }
}
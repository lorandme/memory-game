using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace memory_game
{
    /// <summary>
    /// Interaction logic for AddUserWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {
        public AddUserViewModel ViewModel { get; }
        public AddUserWindow()
        {
            InitializeComponent();
            ViewModel = new AddUserViewModel();
            DataContext = ViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CreateUserCommand is RelayCommand createUserCommand)
                createUserCommand.Execute(this);

            if (ViewModel.CancelCommand is RelayCommand cancelCommand)
                cancelCommand.Execute(this);
        }

    }
}

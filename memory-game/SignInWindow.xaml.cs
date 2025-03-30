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
    /// Interaction logic for SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window
    {
        public SignInWindow()
        {
            InitializeComponent();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddUserWindow();
            addUserWindow.Owner = this;

            bool? result = addUserWindow.ShowDialog();

            if (result == true)
            {
                // Get the ViewModel
                var viewModel = DataContext as SignInViewModel;
                if (viewModel != null)
                {
                    // Get the created user from the AddUserWindow's ViewModel
                    var addUserViewModel = addUserWindow.DataContext as AddUserViewModel;
                    if (addUserViewModel != null && addUserViewModel.CreatedUser != null)
                    {
                        // Add the new user and save
                        viewModel.AddNewUser(addUserViewModel.CreatedUser);
                    }
                }
            }
        }
    }
}

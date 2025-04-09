using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace memory_game
{
    public class MenuViewModel : INotifyPropertyChanged
    {
        #region Properties

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand NewGameCommand { get; private set; }
        public ICommand OpenGameCommand { get; private set; }
        public ICommand ShowStatisticsCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand ShowAboutCommand { get; private set; }

        #endregion

        public MenuViewModel()
        {
            NewGameCommand = new RelayCommand(StartNewGame);
            OpenGameCommand = new RelayCommand(OpenGame);
            ShowStatisticsCommand = new RelayCommand(ShowStatistics);
            ExitCommand = new RelayCommand(Exit);
            ShowAboutCommand = new RelayCommand(ShowAbout);

            LoadCurrentUser();
        }

        #region Command Methods

        private void StartNewGame(object parameter)
        {
            try
            {
                var gameWindow = new GameWindow();
                gameWindow.Show();

                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting new game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenGame(object parameter)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON Files (*.json)|*.json",
                    Title = "Open Saved Game",
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
                };

                openFileDialog.Filter = $"{CurrentUser.Username} Saved Games (*.json)|{CurrentUser.Username}_*.json|All JSON Files (*.json)|*.json";

                if (openFileDialog.ShowDialog() == true)
                {
                    string savedGamePath = openFileDialog.FileName;
                    File.Copy(savedGamePath, "current_saved_game.json", true);

                    var gameWindow = new GameWindow();
                    gameWindow.Show();

                    CloseWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening saved game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowStatistics(object parameter)
        {
            MessageBox.Show("Statistics feature will be implemented soon!", "Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Exit(object parameter)
        {

            Window currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);

            var signInWindow = new SignInWindow();
            signInWindow.Show();

            currentWindow?.Close();
        }

        private void ShowAbout(object parameter)
        {
            MessageBox.Show(
                "Memory Game\n\n" +
                "Created by: Menyhart Lorand\n" +
                "Email: lorand.menyhart@student.unitbv.ro\n" +
                "Group: 10LF233\n" +
                "Specialization: Computer Science",
                "About Memory Game",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #endregion

        #region Helper Methods

        private void LoadCurrentUser()
        {
            try
            {
                if (File.Exists("active_user.json"))
                {
                    string json = File.ReadAllText("active_user.json");
                    CurrentUser = JsonSerializer.Deserialize<User>(json);
                }
                else
                {
                    CurrentUser = new User
                    {
                        Username = "DefaultUser",
                        ImagePath = "default.png"
                    };

                    MessageBox.Show("No active user found. Please go back to login screen.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                CurrentUser = new User { Username = "Error" };
            }
        }

        private void CloseWindow()
        {
            Window currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            currentWindow?.Close();
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
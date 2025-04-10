using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace memory_game
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private Window _window;
        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WinRate));
            }
        }

        public string WinRate
        {
            get
            {
                if (CurrentUser == null || CurrentUser.GamesPlayed == 0) return "0%";
                double rate = (double)CurrentUser.GamesWon / CurrentUser.GamesPlayed * 100;
                return $"{rate:F1}%";
            }
        }

        public ICommand CloseCommand { get; private set; }

        public StatisticsViewModel(Window window)
        {
            _window = window;
            CloseCommand = new RelayCommand(CloseWindow);
            LoadCurrentUserStatistics();
        }

        private void LoadCurrentUserStatistics()
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
                    CurrentUser = new User { Username = "Unknown" };
                    MessageBox.Show("Could not find active user information.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user statistics: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CurrentUser = new User { Username = "Error" };
            }
        }

        private void CloseWindow(object parameter)
        {
            _window?.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
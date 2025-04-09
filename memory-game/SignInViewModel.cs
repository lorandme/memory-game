using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Text.Json;

namespace memory_game
{
    public class SignInViewModel : INotifyPropertyChanged
    {
        private const string UsersFilePath = "users.json";

        #region Properties

        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUserSelected));
                OnPropertyChanged(nameof(SelectedUserImagePath));
            }
        }

        public bool IsUserSelected => SelectedUser != null;

        public string SelectedUserImagePath => SelectedUser?.ImagePath;

        #endregion

        #region Commands

        public ICommand DeleteUserCommand { get; private set; }
        public ICommand PlayCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        #endregion

        public SignInViewModel()
        {
            DeleteUserCommand = new RelayCommand(DeleteUser, param => IsUserSelected);
            PlayCommand = new RelayCommand(Play, param => IsUserSelected);
            ExitCommand = new RelayCommand(Exit);

            LoadUsers();
        }

        #region Command Methods

        private void DeleteUser(object parameter)
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete user '{SelectedUser.Username}'?\nThis will delete all saved games and statistics.",
                "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteUserSavedGames(SelectedUser.Username);

                Users.Remove(SelectedUser);
                SaveUsers();
                SelectedUser = null;

                MessageBox.Show("User deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Play(object parameter)
        {
            if (SelectedUser == null) return;

            SaveActiveUser(SelectedUser);

            Window currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);

            var menuWindow = new MenuWindow();
            menuWindow.Show();

            currentWindow?.Close();
        }



        private void Exit(object parameter)
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Public Methods

        public void AddNewUser(User newUser)
        {
            if (Users.Any(u => u.Username.Equals(newUser.Username, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"A user with the name '{newUser.Username}' already exists.",
                    "Duplicate User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Users.Add(newUser);

            SaveUsers();

            SelectedUser = newUser;

            MessageBox.Show($"User '{newUser.Username}' created successfully!",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region File Operations

        private void LoadUsers()
        {
            try
            {
                if (File.Exists(UsersFilePath))
                {
                    string json = File.ReadAllText(UsersFilePath);
                    var loadedUsers = JsonSerializer.Deserialize<ObservableCollection<User>>(json);
                    Users = loadedUsers ?? new ObservableCollection<User>();
                }
                else
                {
                    Users = new ObservableCollection<User>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Users = new ObservableCollection<User>();
            }
        }

        private void SaveUsers()
        {
            try
            {
                string json = JsonSerializer.Serialize(Users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(UsersFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveActiveUser(User user)
        {
            try
            {
                string json = JsonSerializer.Serialize(user);
                File.WriteAllText("active_user.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving active user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUserSavedGames(string username)
        {
            try
            {
                string savedGamePattern = $"{username}_*.json";
                string[] savedGames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, savedGamePattern);

                foreach (string gameFile in savedGames)
                {
                    File.Delete(gameFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting user saved games: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
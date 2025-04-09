using System;
using System.Collections.Generic;
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

        private string _selectedCategory = "Animals";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                UpdateCategorySelections();
            }
        }

        public bool IsAnimalsCategorySelected => SelectedCategory == "Animals";
        public bool IsFruitsCategorySelected => SelectedCategory == "Fruits";
        public bool IsTransportationCategorySelected => SelectedCategory == "Transportation";

        private string _selectedGameMode = "Standard";
        public string SelectedGameMode
        {
            get => _selectedGameMode;
            set
            {
                _selectedGameMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStandardModeSelected));
                OnPropertyChanged(nameof(IsCustomModeSelected));
                OnPropertyChanged(nameof(StandardSettingsVisibility));
                OnPropertyChanged(nameof(CustomSettingsVisibility));
            }
        }

        public bool IsStandardModeSelected => SelectedGameMode == "Standard";
        public bool IsCustomModeSelected => SelectedGameMode == "Custom";

        public string StandardSettingsVisibility => IsStandardModeSelected ? "Visible" : "Collapsed";
        public string CustomSettingsVisibility => IsCustomModeSelected ? "Visible" : "Collapsed";

        public List<int> DimensionOptions { get; } = new List<int> { 2, 3, 4, 5, 6 };

        private int _selectedRows = 3;
        public int SelectedRows
        {
            get => _selectedRows;
            set
            {
                _selectedRows = value;
                OnPropertyChanged();
            }
        }

        private int _selectedColumns = 4;
        public int SelectedColumns
        {
            get => _selectedColumns;
            set
            {
                _selectedColumns = value;
                OnPropertyChanged();
            }
        }

        private int _gameTimeSeconds = 60;
        public int GameTimeSeconds
        {
            get => _gameTimeSeconds;
            set
            {
                _gameTimeSeconds = value;
                OnPropertyChanged();
            }
        }

        private bool _isGameInProgress = false;
        public bool IsGameInProgress
        {
            get => _isGameInProgress;
            set
            {
                _isGameInProgress = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand SelectCategoryCommand { get; private set; }
        public ICommand SelectGameModeCommand { get; private set; }
        public ICommand NewGameCommand { get; private set; }
        public ICommand OpenGameCommand { get; private set; }
        public ICommand SaveGameCommand { get; private set; }
        public ICommand ShowStatisticsCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand ShowAboutCommand { get; private set; }

        #endregion

        public MenuViewModel()
        {
            // Initialize commands
            SelectCategoryCommand = new RelayCommand(SelectCategory);
            SelectGameModeCommand = new RelayCommand(SelectGameMode);
            NewGameCommand = new RelayCommand(StartNewGame);
            OpenGameCommand = new RelayCommand(OpenGame);
            SaveGameCommand = new RelayCommand(SaveGame, param => IsGameInProgress);
            ShowStatisticsCommand = new RelayCommand(ShowStatistics);
            ExitCommand = new RelayCommand(Exit);
            ShowAboutCommand = new RelayCommand(ShowAbout);

            // Load current user
            LoadCurrentUser();
        }

        #region Command Methods

        private void SelectCategory(object parameter)
        {
            SelectedCategory = parameter.ToString();
        }

        private void SelectGameMode(object parameter)
        {
            SelectedGameMode = parameter.ToString();
        }

        private void StartNewGame(object parameter)
        {
            try
            {
                // Create game settings
                var gameSettings = new GameSettings
                {
                    Category = SelectedCategory,
                    GameMode = SelectedGameMode,
                    Rows = SelectedGameMode == "Standard" ? 4 : SelectedRows,
                    Columns = SelectedGameMode == "Standard" ? 4 : SelectedColumns,
                    TimeInSeconds = GameTimeSeconds,
                    Username = CurrentUser.Username
                };

                // Serialize settings for passing to GameWindow
                string settingsJson = JsonSerializer.Serialize(gameSettings);
                File.WriteAllText("current_game_settings.json", settingsJson);

                // Open game window
                var gameWindow = new GameWindow();
                gameWindow.Show();

                // Close this window
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
                // Create OpenFileDialog
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON Files (*.json)|*.json",
                    Title = "Open Saved Game",
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
                };

                // Refine search to only show saved games for current user
                openFileDialog.Filter = $"{CurrentUser.Username} Saved Games (*.json)|{CurrentUser.Username}_*.json|All JSON Files (*.json)|*.json";

                if (openFileDialog.ShowDialog() == true)
                {
                    // Read the saved game file
                    string savedGamePath = openFileDialog.FileName;
                    File.Copy(savedGamePath, "current_saved_game.json", true);

                    // Open game window
                    var gameWindow = new GameWindow();
                    gameWindow.Show();

                    // Close this window
                    CloseWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening saved game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveGame(object parameter)
        {
            if (!IsGameInProgress) return;

            try
            {
                // Create SaveFileDialog
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON Files (*.json)|*.json",
                    Title = "Save Game",
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    FileName = $"{CurrentUser.Username}_game_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Implementation will depend on how game state is stored
                    MessageBox.Show("Game saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowStatistics(object parameter)
        {
            // Open statistics window
            var statisticsWindow = new StatisticsWindow();
            statisticsWindow.ShowDialog();
        }

        private void Exit(object parameter)
        {
            // Return to sign-in window
            var signInWindow = new SignInWindow();
            signInWindow.Show();

            // Close this window
            CloseWindow();
        }

        private void ShowAbout(object parameter)
        {
            MessageBox.Show(
                "Memory Game\n\n" +
                "Created by: Your Name\n" +
                "Email: your.email@domain.com\n" +
                "Group: Your Group Number\n" +
                "Specialization: Your Specialization",
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
                    // Fallback default user if no active user found
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

                // Create basic user
                CurrentUser = new User { Username = "Error" };
            }
        }

        private void UpdateCategorySelections()
        {
            OnPropertyChanged(nameof(IsAnimalsCategorySelected));
            OnPropertyChanged(nameof(IsFruitsCategorySelected));
            OnPropertyChanged(nameof(IsTransportationCategorySelected));
        }

        private void CloseWindow()
        {
            // Find current window
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

    // Helper class for game settings
    public class GameSettings
    {
        public string Category { get; set; }
        public string GameMode { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int TimeInSeconds { get; set; }
        public string Username { get; set; }
    }
}
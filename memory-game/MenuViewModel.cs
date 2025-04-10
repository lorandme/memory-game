using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace memory_game
{
    public class MenuViewModel : INotifyPropertyChanged
    {
        #region Properties

        private string _selectedCategory = "Nature";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        private int _gameTime = 60;
        public int GameTime
        {
            get => _gameTime;
            set
            {
                _gameTime = value;
                OnPropertyChanged();
            }
        }

        
        private string _selectedBoardSize;
        public string SelectedBoardSize
        {
            get => _selectedBoardSize;
            set
            {
                _selectedBoardSize = value;
                OnPropertyChanged();
            }
        }

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

        private bool _isStandardBoard;
        public bool IsStandardBoard
        {
            get => _isStandardBoard;
            set
            {
                _isStandardBoard = value;
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
        public ICommand SelectCategoryCommand { get; private set; }
        public ICommand SelectCustomBoardCommand { get; private set; }
        public ICommand SelectBoardSizeCommand { get; private set; }
        public ICommand SetGameTimeCommand { get; private set; }

        #endregion

        public MenuViewModel()
        {
            NewGameCommand = new RelayCommand(StartNewGame);
            OpenGameCommand = new RelayCommand(OpenGame);
            ShowStatisticsCommand = new RelayCommand(ShowStatistics);
            ExitCommand = new RelayCommand(Exit);
            ShowAboutCommand = new RelayCommand(ShowAbout);
            SelectCategoryCommand = new RelayCommand(SelectCategory);
            SelectCustomBoardCommand = new RelayCommand(SelectCustomBoard);
            SelectBoardSizeCommand = new RelayCommand(SelectBoardSize);
            SetGameTimeCommand = new RelayCommand(SetGameTime);

            LoadCurrentUser();
        }

        #region Command Methods

        private void StartNewGame(object parameter)
        {
            try
            {
                var gameSettings = new GameSettings
                {
                    Category = SelectedCategory,
                    IsStandardBoard = IsStandardBoard,
                    CustomBoardSize = SelectedBoardSize,
                    GameTime = GameTime
                };

                string json = JsonSerializer.Serialize(gameSettings);
                File.WriteAllText("game_settings.json", json);

                Window currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                var gameWindow = new GameWindow();
                gameWindow.Show();
                currentWindow?.Close();
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
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON Files (*.json)|*.json",
                    Title = "Open Saved Game",
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
                };

                openFileDialog.Filter = $"{CurrentUser.Username} Saved Games (*.json)|{CurrentUser.Username}_*.json";


                if (openFileDialog.ShowDialog() == true)
                {
                    string savedGamePath = openFileDialog.FileName;
                    File.Copy(savedGamePath, "current_saved_game.json", true);


                    Window currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                    var gameWindow = new GameWindow();
                    gameWindow.Show();
                    currentWindow?.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening saved game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowStatistics(object parameter)
        {
            try
            {
                var statisticsWindow = new StatisticsWindow();
                statisticsWindow.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                statisticsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing statistics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void SelectCategory(object parameter)
        {
            string category = (string)parameter;
            SelectedCategory = category;

            MessageBox.Show($"Category '{category}' selected.", "Category Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void SelectCustomBoard(object parameter)
        {
            string result = Microsoft.VisualBasic.Interaction.InputBox("Enter custom board size:", "Custom Board Size", "4");

            if (int.TryParse(result, out int boardSize))
            {
                SelectedBoardSize = result;
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectBoardSize(object parameter)
        {
            string boardSize = (string)parameter;
            if (boardSize == "Standard")
            {
                IsStandardBoard = true;
                SelectedBoardSize = "Standard";
            }
            else
            {
                IsStandardBoard = false;
                SelectedBoardSize = "Custom";
            }
        }

        private void SetGameTime(object parameter)
        {
            string result = Microsoft.VisualBasic.Interaction.InputBox("Enter game time (in seconds):", "Game Time", GameTime.ToString());

            if (int.TryParse(result, out int gameTime) && gameTime > 0)
            {
                GameTime = gameTime;
                MessageBox.Show($"Game time set to {gameTime} seconds.", "Game Time", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a valid positive number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Helper Methods

        private void LoadCurrentUser()
        {
            try
            {
                string json = File.ReadAllText("active_user.json");
                CurrentUser = JsonSerializer.Deserialize<User>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow()
        {
            Window currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            currentWindow?.Close();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class GameSettings
        {
            public string Category { get; set; }
            public bool IsStandardBoard { get; set; }
            public string CustomBoardSize { get; set; }
            public int GameTime { get; set; }
        }
    }
}

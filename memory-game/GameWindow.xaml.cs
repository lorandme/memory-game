using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace memory_game
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        public GameViewModel ViewModel { get; private set; }

        public GameWindow()
        {
            InitializeComponent();

            // Check if opening a saved game
            if (File.Exists("current_saved_game.json"))
            {
                try
                {
                    string json = File.ReadAllText("current_saved_game.json");
                    var gameState = JsonSerializer.Deserialize<GameState>(json);

                    // Initialize with saved game state
                    ViewModel = new GameViewModel(gameState);
                    DataContext = ViewModel;

                    // Delete the temporary file
                    File.Delete("current_saved_game.json");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading saved game: {ex.Message}. Starting new game instead.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    InitializeNewGame();
                }
            }
            else
            {
                InitializeNewGame();
            }
        }

        private void InitializeNewGame()
        {
            // Get parameters from active menu settings
            string category = "Nature"; // Default
            bool isStandardBoard = true;
            string customBoardSize = "4";
            int gameTime = 60; // Default: 60 seconds

            try
            {
                if (File.Exists("game_settings.json"))
                {
                    string json = File.ReadAllText("game_settings.json");
                    var settings = JsonSerializer.Deserialize<GameSettings>(json);

                    category = settings.Category;
                    isStandardBoard = settings.IsStandardBoard;
                    customBoardSize = settings.CustomBoardSize;
                    gameTime = settings.GameTime;
                }
                else
                {
                    // If no settings exist, create default settings
                    var settings = new GameSettings
                    {
                        Category = category,
                        IsStandardBoard = isStandardBoard,
                        CustomBoardSize = customBoardSize,
                        GameTime = gameTime
                    };

                    string json = JsonSerializer.Serialize(settings);
                    File.WriteAllText("game_settings.json", json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading game settings: {ex.Message}. Using defaults.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Initialize with new game
            ViewModel = new GameViewModel(category, isStandardBoard, customBoardSize, gameTime);
            DataContext = ViewModel;
        }
    }

    // Value converters for the UI
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (Visibility)value != Visibility.Visible;
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class GameSettings
    {
        public string Category { get; set; }
        public bool IsStandardBoard { get; set; }
        public string CustomBoardSize { get; set; }
        public int GameTime { get; set; }
    }
}
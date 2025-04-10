using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace memory_game
{


    public class GameViewModel : INotifyPropertyChanged
    {
        #region Fields
        private User _currentUser;
        private string _category;
        private int _rows;
        private int _columns;
        private DispatcherTimer _gameTimer;
        private int _timeRemaining;
        private int _initialTime;
        private ObservableCollection<Card> _cards;
        private Card _firstSelectedCard;
        private Card _secondSelectedCard;
        private int _matchesFound;
        private int _totalPairs;
        private bool _gameOver;
        #endregion

        public GameViewModel(GameState savedGame)
        {
            LoadCurrentUser();

            Category = savedGame.Category;
            Rows = savedGame.Rows;
            Columns = savedGame.Columns;
            _initialTime = savedGame.InitialTime;
            TimeRemaining = savedGame.TimeRemaining;

            CardClickCommand = new RelayCommand(FlipCard, CanFlipCard);
            SaveGameCommand = new RelayCommand(SaveGame);
            BackToMenuCommand = new RelayCommand(BackToMenu);

            Cards = new ObservableCollection<Card>(savedGame.Cards);
            _totalPairs = (Rows * Columns) / 2;
            _matchesFound = Cards.Count(c => c.IsMatched) / 2;

            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromSeconds(1);
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();
        }

        #region Properties
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        public int Rows
        {
            get => _rows;
            set
            {
                _rows = value;
                OnPropertyChanged();
            }
        }

        public int Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                OnPropertyChanged();
            }
        }

        public int TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedTimeRemaining));
            }
        }

        public string FormattedTimeRemaining
        {
            get
            {
                int minutes = TimeRemaining / 60;
                int seconds = TimeRemaining % 60;
                return $"{minutes:00}:{seconds:00}";
            }
        }

        public ObservableCollection<Card> Cards
        {
            get => _cards;
            set
            {
                _cards = value;
                OnPropertyChanged();
            }
        }

        public bool GameOver
        {
            get => _gameOver;
            set
            {
                _gameOver = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand CardClickCommand { get; private set; }
        public ICommand SaveGameCommand { get; private set; }
        public ICommand BackToMenuCommand { get; private set; }
        #endregion

        public GameViewModel(string category, bool isStandardBoard, string customBoardSize, int gameTime)
        {
            LoadCurrentUser();

            Category = category ?? "Nature"; // Default to Nature if not specified
            SetupBoard(isStandardBoard, customBoardSize);

            _initialTime = gameTime;
            TimeRemaining = gameTime;

            CardClickCommand = new RelayCommand(FlipCard, CanFlipCard);
            SaveGameCommand = new RelayCommand(SaveGame);
            BackToMenuCommand = new RelayCommand(BackToMenu);

            Cards = new ObservableCollection<Card>();
            LoadCardImages();
            ShuffleCards();

            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromSeconds(1);
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();
        }

        #region Game Logic Methods
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
                CurrentUser = new User { Username = "Guest" };
            }
        }

        private void SetupBoard(bool isStandardBoard, string customBoardSize)
        {
            if (isStandardBoard)
            {
                Rows = 4;
                Columns = 4;
            }
            else
            {
                if (int.TryParse(customBoardSize, out int size))
                {
                    size = Math.Max(2, Math.Min(6, size));
                    Rows = size;
                    Columns = size;
                }
                else
                {
                    Rows = 4;
                    Columns = 4;
                }
            }

            _totalPairs = (Rows * Columns) / 2;
        }

        private void LoadCardImages()
        {
            try
            {
                string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameImages", Category);

                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                    MessageBox.Show($"Images folder for category {Category} was created. Please add images to continue.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                var imageFiles = Directory.GetFiles(imagesFolder)
                    .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .ToList();

                if (imageFiles.Count < _totalPairs)
                {
                    MessageBox.Show($"Not enough images in {Category} category. Using placeholder images.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                    var placeholderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProfileImages", "default1.png");
                    imageFiles = Enumerable.Repeat(placeholderPath, _totalPairs).ToList();
                }

                var selectedImages = imageFiles.Take(_totalPairs).ToList();

                for (int i = 0; i < _totalPairs; i++)
                {
                    string imagePath = selectedImages[i];

                    var card1 = new Card
                    {
                        Id = i * 2,
                        ImagePath = imagePath,
                        IsFlipped = false,
                        IsMatched = false,
                        PairId = i
                    };

                    var card2 = new Card
                    {
                        Id = i * 2 + 1,
                        ImagePath = imagePath,
                        IsFlipped = false,
                        IsMatched = false,
                        PairId = i
                    };

                    Cards.Add(card1);
                    Cards.Add(card2);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading game images: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShuffleCards()
        {
            var random = new Random();
            var shuffledCards = Cards.OrderBy(c => random.Next()).ToList();
            Cards.Clear();

            foreach (var card in shuffledCards)
            {
                Cards.Add(card);
            }
        }

        private bool CanFlipCard(object parameter)
        {
            if (parameter is not Card card) return false;

            return !GameOver && !card.IsFlipped && !card.IsMatched && _secondSelectedCard == null;
        }

        private void FlipCard(object parameter)
        {
            if (parameter is not Card card) return;

            card.IsFlipped = true;

            if (_firstSelectedCard == null)
            {
                _firstSelectedCard = card;
            }
            else
            {
                _secondSelectedCard = card;

                var dispatcherTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(0.7)
                };

                dispatcherTimer.Tick += (s, e) =>
                {
                    CheckForMatch();
                    dispatcherTimer.Stop();
                };

                dispatcherTimer.Start();
            }
        }

        private void CheckForMatch()
        {
            if (_firstSelectedCard != null && _secondSelectedCard != null)
            {
                if (_firstSelectedCard.PairId == _secondSelectedCard.PairId)
                {
                    _firstSelectedCard.IsMatched = true;
                    _secondSelectedCard.IsMatched = true;
                    _matchesFound++;

                    if (_matchesFound == _totalPairs)
                    {
                        GameWon();
                    }
                }
                else
                {
                    _firstSelectedCard.IsFlipped = false;
                    _secondSelectedCard.IsFlipped = false;
                }

                _firstSelectedCard = null;
                _secondSelectedCard = null;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (TimeRemaining > 0)
            {
                TimeRemaining--;
            }
            else
            {
                GameLost();
            }
        }

        private void GameWon()
        {
            _gameTimer.Stop();
            GameOver = true;

            CurrentUser.GamesPlayed++;
            CurrentUser.GamesWon++;
            SaveUserStatistics();

            MessageBox.Show($"Congratulations {CurrentUser.Username}! You won the game!",
                "Game Won", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GameLost()
        {
            _gameTimer.Stop();
            GameOver = true;

            CurrentUser.GamesPlayed++;
            SaveUserStatistics();

            MessageBox.Show($"Sorry {CurrentUser.Username}, you ran out of time!",
                "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveUserStatistics()
        {
            try
            {
                string json = File.ReadAllText("users.json");
                var users = JsonSerializer.Deserialize<List<User>>(json);

                var user = users.FirstOrDefault(u => u.Username == CurrentUser.Username);
                if (user != null)
                {
                    user.GamesPlayed = CurrentUser.GamesPlayed;
                    user.GamesWon = CurrentUser.GamesWon;

                    json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText("users.json", json);

                    File.WriteAllText("active_user.json", JsonSerializer.Serialize(CurrentUser));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user statistics: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Command Methods
        private void SaveGame(object parameter)
        {
            try
            {
                var gameState = new GameState
                {
                    Category = Category,
                    Rows = Rows,
                    Columns = Columns,
                    TimeRemaining = TimeRemaining,
                    InitialTime = _initialTime,
                    Cards = Cards.ToList(),
                    UserId = CurrentUser.Username
                };

                string json = JsonSerializer.Serialize(gameState, new JsonSerializerOptions { WriteIndented = true });
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{CurrentUser.Username}_{timestamp}_saved_game.json";
                File.WriteAllText(fileName, json);

                MessageBox.Show($"Game saved successfully!", "Save Game", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToMenu(object parameter)
        {
            _gameTimer.Stop();

            var menuWindow = new MenuWindow();
            Window currentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
            menuWindow.Show();
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

    public class Card : INotifyPropertyChanged
    {
        private int _id;
        private string _imagePath;
        private bool _isFlipped;
        private bool _isMatched;
        private int _pairId;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                _isFlipped = value;
                OnPropertyChanged();
            }
        }

        public bool IsMatched
        {
            get => _isMatched;
            set
            {
                _isMatched = value;
                OnPropertyChanged();
            }
        }

        public int PairId
        {
            get => _pairId;
            set
            {
                _pairId = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GameState
    {
        public string Category { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int TimeRemaining { get; set; }
        public int InitialTime { get; set; }
        public List<Card> Cards { get; set; }
        public string UserId { get; set; }
    }
}
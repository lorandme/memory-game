using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace memory_game
{
    public class AddUserViewModel : INotifyPropertyChanged
    {
        private string _username;
        private int _currentImageIndex;
        private List<string> _availableImages;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCreateUser));
            }
        }

        public int CurrentImageIndex
        {
            get => _currentImageIndex;
            set
            {
                _currentImageIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentImagePath));
            }
        }

        public string CurrentImagePath => _availableImages != null && _availableImages.Count > 0 ?
                                         _availableImages[CurrentImageIndex] : null;

        public int TotalImages => _availableImages?.Count ?? 0;

        public bool CanCreateUser => !string.IsNullOrWhiteSpace(Username) && CurrentImagePath != null;

        public ICommand PreviousImageCommand { get; private set; }
        public ICommand NextImageCommand { get; private set; }
        public ICommand CreateUserCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public User CreatedUser { get; private set; }

        public AddUserViewModel()
        {
            PreviousImageCommand = new RelayCommand(PreviousImage);
            NextImageCommand = new RelayCommand(NextImage);
            CreateUserCommand = new RelayCommand(CreateUser, param => CanCreateUser);
            CancelCommand = new RelayCommand(Cancel);

            LoadAvailableImages();

            CurrentImageIndex = 0;
        }

        private void LoadAvailableImages()
        {
            try
            {
                string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProfileImages");

                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                _availableImages = Directory.GetFiles(imagesFolder)
                    .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .ToList();

                if (_availableImages.Count == 0)
                {
                    _availableImages = new List<string>
                    {
                        Path.Combine(imagesFolder, "default1.png"),
                        Path.Combine(imagesFolder, "default2.png"),
                        Path.Combine(imagesFolder, "default3.png")
                    };

                    MessageBox.Show("No profile images found. Please add image files to the ProfileImages folder.",
                        "Missing Images", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                OnPropertyChanged(nameof(TotalImages));
                OnPropertyChanged(nameof(CurrentImagePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profile images: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _availableImages = new List<string>();
            }
        }

        private void PreviousImage(object parameter)
        {
            if (_availableImages.Count == 0) return;

            CurrentImageIndex = (CurrentImageIndex - 1 + _availableImages.Count) % _availableImages.Count;
        }

        private void NextImage(object parameter)
        {
            if (_availableImages.Count == 0) return;

            CurrentImageIndex = (CurrentImageIndex + 1) % _availableImages.Count;
        }

        private void CreateUser(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || CurrentImagePath == null) return;

            CreatedUser = new User
            {
                Username = Username,
                ImagePath = CurrentImagePath,
                GamesPlayed = 0,
                GamesWon = 0
            };

            var window = parameter as Window;
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void Cancel(object parameter)
        {
            var window = parameter as Window;
            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NAudio.Wave;
using TagLib;

namespace MEAL_Music_Player
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> _songs = new ObservableCollection<string>();
        private WaveOutEvent? _waveOut;
        private AudioFileReader? _audioFile;
        private DispatcherTimer _timer;
        private int _currentSongIndex = -1;
        private double _songDuration;
        private bool LoopCurrentSong; // Add this variable

        public double CurrentPosition // Part of timebar functionality
        {
            get => _audioFile?.CurrentTime.TotalSeconds ?? 0;
            set
            {
                if (_audioFile != null)
                {
                    if (Math.Abs(_audioFile.CurrentTime.TotalSeconds - value) > 0.1)
                    {
                        _audioFile.CurrentTime = TimeSpan.FromSeconds(value);
                    }
                    PositionSlider.Value = value;
                    PositionTextBlock.Text = $"{_audioFile.CurrentTime:mm\\:ss}";
                }
            }
        }

        public MainWindow() // Initialize program
        {
            InitializeComponent();
            LoopCurrentSongButton.Checked += LoopCurrentSongButton_Checked;
            LoopCurrentSongButton.Unchecked += LoopCurrentSongButton_Unchecked;
            LoadSongs();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += OnTimedEvent;
            _timer.Start();
        }

        private void SongEnd(object sender, EventArgs e)
        {
            if (_audioFile != null && _audioFile.CurrentTime.TotalSeconds >= _audioFile.TotalTime.TotalSeconds)
            {
                if (LoopCurrentSongButton.IsChecked == true)
                {
                    // Loop the current song
                    NextButton_Click(this, new RoutedEventArgs());
                    Task.Delay(100).Wait();
                    PreviousButton_Click(this, new RoutedEventArgs());
                }
                else if (LoopCurrentSongButton.IsChecked == false)
                {
                    // Switch to the next song
                    NextButton_Click(this, new RoutedEventArgs());
                    Task.Delay(100).Wait();
                    PreviousButton_Click(this, new RoutedEventArgs());
                    Task.Delay(100).Wait();
                    NextButton_Click(this, new RoutedEventArgs());
                }
                else
                {
                    NextButton_Click(this, new RoutedEventArgs());
                    Task.Delay(100).Wait();
                    PreviousButton_Click(this, new RoutedEventArgs());
                    Task.Delay(100).Wait();
                    NextButton_Click(this, new RoutedEventArgs());
                }
            }
        }

        private void LoadSongs() // Load songs from Music in user folder
        {
            string musicFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Music");
            if (Directory.Exists(musicFolder))
            {
                var files = Directory.GetFiles(musicFolder, "*.mp3");
                foreach (var file in files)
                {
                    _songs.Add(file);
                }
            }
            else
            {
                MessageBox.Show("Music folder not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeAudio(string audioFilePath) // Audio initialization
        {
            _audioFile = new AudioFileReader(audioFilePath);
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_audioFile);
            _waveOut.Play();

            _songDuration = _audioFile.TotalTime.TotalSeconds;
            PositionSlider.Maximum = _songDuration;

            var tagFile = TagLib.File.Create(audioFilePath);
            SongTitleTextBlock.Text = tagFile.Tag.Title ?? Path.GetFileNameWithoutExtension(audioFilePath);
            ArtistTextBlock.Text = tagFile.Tag.FirstPerformer ?? "Unknown Artist";
            AlbumTextBlock.Text = tagFile.Tag.Album ?? "Unknown Album";

            TotalDurationTextBlock.Text = $"{_audioFile.TotalTime:mm\\:ss}";
        }

        private void OnTimedEvent(object? sender, EventArgs e) // Call SongEnd() every second
        {
            if (_audioFile != null && _waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
            {
                CurrentPosition = _audioFile.CurrentTime.TotalSeconds;
                SongEnd(this, EventArgs.Empty);
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e) // Play/Pause button functionality
        {
            if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
            {
                _waveOut.Pause();
                PlayPauseImage.Source = new BitmapImage(new Uri("/Assets/play.png", UriKind.Relative));
            }
            else if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Paused)
            {
                _waveOut.Play();
                PlayPauseImage.Source = new BitmapImage(new Uri("/Assets/pause.png", UriKind.Relative));
            }
            else if (_waveOut == null && _songs.Count > 0)
            {
                _currentSongIndex = 0;
                string firstSong = _songs[_currentSongIndex];
                if (firstSong != null)
                {
                    InitializeAudio(firstSong);
                    PlayPauseImage.Source = new BitmapImage(new Uri("/Assets/pause.png", UriKind.Relative));
                }
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e) // Previous button functionality
        {
            if (_currentSongIndex > 0)
            {
                _currentSongIndex--;
                _waveOut.Stop();
                string previousSong = _songs[_currentSongIndex];
                InitializeAudio(previousSong);
                PlayPauseImage.Source = new BitmapImage(new Uri("/Assets/pause.png", UriKind.Relative));
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e) // Next button functionality
        {
            if (_currentSongIndex < _songs.Count - 1)
            {
                _currentSongIndex++;
                _waveOut.Stop();
                string nextSong = _songs[_currentSongIndex];
                InitializeAudio(nextSong);
                PlayPauseImage.Source = new BitmapImage(new Uri("/Assets/pause.png", UriKind.Relative));
            }
        }

        private void LoopCurrentSongButton_Checked(object sender, RoutedEventArgs e)
        {
            LoopCurrentSong = true;
        }

        private void LoopCurrentSongButton_Unchecked(object sender, RoutedEventArgs e)
        {
            LoopCurrentSong = false;
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) // Part of timebar functionality
        {
            if (_audioFile != null && Math.Abs(PositionSlider.Value - CurrentPosition) > 0.1)
            {
                CurrentPosition = PositionSlider.Value;
            }
        }
    }

    public class WidthConverter : IValueConverter // Timebar width converter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return width * 0.7;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return width * 0.7;
            }
            return value;
        }
    }
}

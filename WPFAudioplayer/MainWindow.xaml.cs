using System.Windows;
using NAudio.Wave;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Forms;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using TagLib.Riff;
using File = System.IO.File;
using System.Threading;
using System.Drawing;

namespace WPFAudioplayer
{
    public partial class MainWindow : Window
    {

        public List<string> musicFiles; // list with pathes to audio files
        private BindingList<string> musicFilesBindingList; // list with names of audio files for listbox

        private int _defaultWidth = 680;

        private AudioFileReader audioFileReader; // audio reader (wav/mp3)
        private WaveOut waveOut; // audio output
        private OpenFileDialog ofd; // open files
        private OpenFileDialog ofdpl; // open playlist files

        bool loop, random;

        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            Width = _defaultWidth;
            ofd = new OpenFileDialog();
            ofdpl = new OpenFileDialog();
            waveOut = new WaveOut();
            musicFiles = new List<string>();
            musicFilesBindingList = new BindingList<string>();
            playListBox.ItemsSource = musicFilesBindingList;
            ofd.Multiselect = true;
            loop = random = false;
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
            ofd.Filter = "Audio Files (*.wav;*.mp3)|*.wav;*.mp3;";
            ofdpl.Filter = "Text Files (*.txt)|*.txt";
        }

        // Timer

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (waveOut.PlaybackState == PlaybackState.Playing)
            {
                string minutes = ((int)audioFileReader.TotalTime.TotalMinutes).ToString();
                if (int.Parse(minutes) < 10)
                {
                    minutes = $"0{minutes}";
                }
                string seconds = ((int)audioFileReader.TotalTime.TotalSeconds - (int.Parse(minutes) * 60)).ToString();
                if (int.Parse(seconds) < 10)
                {
                    seconds = $"0{seconds}";
                }
                trackBar.Maximum = (int)audioFileReader.TotalTime.TotalSeconds;
                trackBar.Value = (int)audioFileReader.CurrentTime.TotalSeconds;
                var currentSeconds = string.Format("{00:00}:{01:00}", audioFileReader.CurrentTime.Minutes, audioFileReader.CurrentTime.Seconds);
                trackstartLabel.Content = currentSeconds;
                trackendLabel.Content = $"{minutes}:{seconds}";
                if (trackBar.Value == trackBar.Maximum || (trackBar.Maximum - trackBar.Value) < 1.5)  
                {
                    if (loop == false)
                    {
                        if (random == false)
                        {
                            if (playListBox.Items.Count > 0)
                            {
                                if (playListBox.SelectedIndex == playListBox.Items.Count - 1)
                                {
                                    playListBox.SelectedIndex = 0;
                                }
                                else playListBox.SelectedIndex += 1;
                            }
                        }
                        else
                        {
                            Random random = new Random();
                            var rnd = random.Next(playListBox.Items.Count);
                            playListBox.SelectedIndex = rnd;
                        }
                    }
                    else
                    {
                        audioFileReader.Seek(0, SeekOrigin.Begin);
                    }
                }
            }
        }

        // Music Controls

        private void playButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (audioFileReader != null && waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Paused ||
                waveOut.PlaybackState == PlaybackState.Stopped) waveOut.Play();
            }
        } // Play Button
        private void playButton_MouseEnter(object sender, MouseEventArgs e)
        {
            playButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/playButton_hover.png")); 
        }
        private void playButton_MouseLeave(object sender, MouseEventArgs e)
        {
            playButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/playButton_idle.png"));
        }

        private void pauseButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (waveOut.PlaybackState == PlaybackState.Playing) waveOut.Pause();
        } // Pause Button
        private void pauseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            pauseButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/pauseButton_hover.png"));
        }
        private void pauseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            pauseButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/pauseButton_idle.png"));
        }

        private void stopButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (waveOut.PlaybackState == PlaybackState.Playing ||
                waveOut.PlaybackState == PlaybackState.Paused)
            {
                waveOut.Stop();
                audioFileReader.CurrentTime = TimeSpan.Zero;
                trackBar.Value = 0;
            }
        } // Stop Button
        private void stopButton_MouseEnter(object sender, MouseEventArgs e)
        {
            stopButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/stopButton_hover.png"));
        }
        private void stopButton_MouseLeave(object sender, MouseEventArgs e)
        {
            stopButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/stopButton_idle.png"));
        }

        private void loopButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var imageSource = new BitmapImage();
            if (loop == false)
            {
                imageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/loopButton_idle.png"));
                loopButton.Source = imageSource;
                loop = true;
            }
            else
            {
                imageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/loopoffButton_idle.png"));
                loopButton.Source = imageSource;
                loop = false;
            }
        } // Loop Button

        private void randomButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var imageSource = new BitmapImage();
            if (random == false)
            {
                imageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/randomonButton.png"));
                randomButton.Source = imageSource;
                random = true;
            }
            else
            {
                imageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/randomoffButton.png"));
                randomButton.Source = imageSource;
                random = false;
            }
        } // Random Button

        private void trackBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (audioFileReader != null && waveOut != null)
            {
                audioFileReader.Seek((long)(audioFileReader.WaveFormat.AverageBytesPerSecond * trackBar.Value), SeekOrigin.Begin);
            }
        } // Music TrackBar

        // Playlist

        private void playlistButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Width == _defaultWidth)
            {
                Canvas.SetLeft(closeButton, 970);
                Canvas.SetLeft(minButton, 945);
                Width = 1000;
            }
            else
            {
                Canvas.SetLeft(closeButton, 650);
                Canvas.SetLeft(minButton, 625);
                Width = _defaultWidth;
            }
        } // Playlist Show Button
        private void playlistButton_MouseEnter(object sender, MouseEventArgs e)
        {
            playlistButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/playlistButton_hover.png"));
        }
        private void playlistButton_MouseLeave(object sender, MouseEventArgs e)
        {
            playlistButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/playlistButton_idle.png"));
        }

        private void trackaddButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ofd.ShowDialog();
            for (int i = 0; i < ofd.FileNames.Length; i++)
            {
                musicFiles.Add(ofd.FileNames[i]);
                musicFilesBindingList.Add(Path.GetFileNameWithoutExtension(ofd.FileNames[i]));
            }
        } // Track Add Button
        private void trackaddButton_MouseEnter(object sender, MouseEventArgs e)
        {
            trackaddButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/trackaddButton_hover.png"));
        }
        private void trackaddButton_MouseLeave(object sender, MouseEventArgs e)
        {
            trackaddButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/trackaddButton_idle.png"));
        }

        private void trackdeleteButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (playListBox.Items.Count != 0)
            {
                musicFiles.RemoveAt(playListBox.SelectedIndex);
                musicFilesBindingList.RemoveAt(playListBox.SelectedIndex);
            }
        } // Track Delete Button
        private void trackdeleteButton_MouseEnter(object sender, MouseEventArgs e)
        {
            trackdeleteButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/trackdeleteButton_hover.png"));
        }
        private void trackdeleteButton_MouseLeave(object sender, MouseEventArgs e)
        {
            trackdeleteButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/trackdeleteButton_idle.png"));
        }

        private void playListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (audioFileReader != null)
            {
                audioFileReader.Dispose();
                waveOut.Dispose();
            }
            if (playListBox.Items.Count >= 1)
            {
                waveOut = new WaveOut();
                if (playListBox.SelectedIndex == -1) playListBox.SelectedIndex = 0;
                audioFileReader = new AudioFileReader(musicFiles[playListBox.SelectedIndex]);
                waveOut.Init(audioFileReader);
                waveOut.Play();
                LoadCoverImage();
                tracknameLabel.Content = playListBox.SelectedItem.ToString();
                TagLib.File file = TagLib.File.Create(musicFiles[playListBox.SelectedIndex]);
                var channels = file.Properties.AudioChannels;
                string format = Path.GetExtension(file.Name);
                string c = "";
                if (channels == 2) c = "Stereo";
                else c = "Mono";
                if (format == ".wav") format = "WAV";
                else if (format == ".mp3") format = "MP3";
                tracktagsLabel.Content = $"{format}, {waveOut.OutputWaveFormat.SampleRate + " kHz"}, {file.Properties.AudioBitrate + " kbps"}, {c}";
                timer.Start();
            }
            else
            {
                if (audioFileReader != null)
                {
                    audioFileReader.Dispose();
                    waveOut.Dispose();
                }
                musicFiles.Clear();
                musicFilesBindingList.Clear();
                tracknameLabel.Content = "";
                tracktagsLabel.Content = "";
                emptyCover.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/emptyCover.png"));
            }
        } // Change Track

        private void nextButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (playListBox.Items.Count > 0)
            {
                if (playListBox.SelectedIndex == playListBox.Items.Count - 1)
                {
                    playListBox.SelectedIndex = 0;
                }
                else playListBox.SelectedIndex += 1;
            }
        } // Next Track Button
        private void nextButton_MouseEnter(object sender, MouseEventArgs e)
        {
            nextButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/nextButton_hover.png"));
        }
        private void nextButton_MouseLeave(object sender, MouseEventArgs e)
        {
            nextButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/nextButton_idle.png"));
        }

        private void prevButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (playListBox.Items.Count > 0)
            {
                if (playListBox.SelectedIndex == 0)
                {
                    playListBox.SelectedIndex = playListBox.Items.Count - 1;
                }
                else playListBox.SelectedIndex -= 1;
            }
        } // Prev Track Button
        private void prevButton_MouseEnter(object sender, MouseEventArgs e)
        {
            prevButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/prevButton_hover.png"));
        }
        private void prevButton_MouseLeave(object sender, MouseEventArgs e)
        {
            prevButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/prevButton_idle.png"));
        }

        private void saveplaylistButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (playListBox.Items.Count != 0)
            {
                EnterName enterName = new EnterName(this);
                enterName.ShowDialog();
            }
        } // Save Playlist Button
        private void saveplaylistButton_MouseEnter(object sender, MouseEventArgs e)
        {
            saveplaylistButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/saveplaylistButton_hover.png"));
        }
        private void saveplaylistButton_MouseLeave(object sender, MouseEventArgs e)
        {
            saveplaylistButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/saveplaylistButton_idle.png"));
        }

        private void loadplaylistButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool? res = ofdpl.ShowDialog();
            if (res == true)
            {
                string[] strings = File.ReadAllLines(ofdpl.FileName);
                foreach (string s in strings)
                {
                    musicFiles.Add(s);
                    musicFilesBindingList.Add(Path.GetFileNameWithoutExtension(s));
                }
            }
        } // Load Playlist Button
        private void loadplaylistButton_MouseEnter(object sender, MouseEventArgs e)
        {
            loadplaylistButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/loadplaylistButton_hover.png"));
        }
        private void loadplaylistButton_MouseLeave(object sender, MouseEventArgs e)
        {
            loadplaylistButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/loadplaylistButton_idle.png"));
        }

        private void clearplaylistButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (playListBox.Items.Count != 0)
            {
                musicFiles.Clear();
                musicFilesBindingList.Clear();
            }
        } // Load Playlist Button
        private void clearplaylistButton_MouseEnter(object sender, MouseEventArgs e)
        {
            clearplaylistButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/clearplaylistButton_hover.png"));
        }
        private void clearplaylistButton_MouseLeave(object sender, MouseEventArgs e)
        {
            clearplaylistButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/clearplaylistButton_idle.png"));
        }

        // Load cover function

        private void LoadCoverImage()
        {
            var imageSource = new BitmapImage();
            TagLib.File file = TagLib.File.Create(musicFiles[playListBox.SelectedIndex]);
            if (file.Tag.Pictures.Length != 0)
            {
                TagLib.IPicture picture = file.Tag.Pictures[0];
                byte[] tmp = picture.Data.Data;
                MemoryStream memoryStream = new MemoryStream(tmp);
                memoryStream.Seek(0, SeekOrigin.Begin);
                imageSource.BeginInit();
                imageSource.StreamSource = memoryStream;
                imageSource.EndInit();
                emptyCover.Source = imageSource;
            }
            else
            {
                imageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/emptyCover.png"));
                emptyCover.Source = imageSource;
            }
        }

        // Form

        private void closeButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        } // Close Button
        private void closeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            closeButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/closeButton_hover.png"));
        }
        private void closeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            closeButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/closeButton_idle.png"));
        }

        private void minButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        } // Minimize Button
        private void minButton_MouseEnter(object sender, MouseEventArgs e)
        {
            minButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/minimizeButton_hover.png"));
        }
        private void minButton_MouseLeave(object sender, MouseEventArgs e)
        {
            minButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/minimizeButton_idle.png"));
        }

        private void Form_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        } // Form Drag Function

        // Volume controls

        private void volumeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (waveOut.Volume != 0)
            {
                volumeButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/volumeoffButton_idle.png"));
                waveOut.Volume = 0;
                volumeBar.Value = volumeBar.Minimum;
            }
            else
            {
                volumeButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/muteButton_idle.png"));
                waveOut.Volume = 1;
                volumeBar.Value = volumeBar.Maximum;
            }
        }

        private void convertButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (audioFileReader != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "WAV File (*.wav)|*.wav";
                saveFileDialog.FileName = playListBox.SelectedItem.ToString();
                saveFileDialog.ShowDialog();

                string format = Path.GetExtension(musicFiles[playListBox.SelectedIndex]);

                var reader = new Mp3FileReader(musicFiles[playListBox.SelectedIndex]);

                new Thread(() =>
                {
                    if (format == ".mp3")
                    {
                        WaveFileWriter.CreateWaveFile(saveFileDialog.FileName, reader);
                    }
                }).Start();
            }
        }

        private void volumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (volumeBar.Value == volumeBar.Minimum)
            {
                volumeButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/volumeoffButton_idle.png"));
            }
            else
            {
                volumeButton.Source = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}images/muteButton_idle.png"));
            }
            if (waveOut != null)
            {
                waveOut.Volume = (float)volumeBar.Value;
            }
        }
    }
}

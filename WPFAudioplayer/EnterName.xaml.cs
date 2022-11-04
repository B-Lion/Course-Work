using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace WPFAudioplayer
{
    public partial class EnterName : Window
    {
        private MainWindow mainWindow;
        private SaveFileDialog svd;

        public EnterName(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            svd = new SaveFileDialog();
            svd.Filter = "Text File (*.txt)|*.txt";
        }

        private void playlistnameokButton_Click(object sender, RoutedEventArgs e)
        {
            if (playlistnameTextBox.Text != string.Empty)
            {
                svd.FileName = playlistnameTextBox.Text;
                svd.ShowDialog();
                File.WriteAllLinesAsync($"{svd.FileName}", mainWindow.musicFiles);
                Close();
            }
            else
            {
                Close();
            }
        }
        private void EnterName_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}

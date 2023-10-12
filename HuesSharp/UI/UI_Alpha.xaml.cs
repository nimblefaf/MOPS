using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HuesSharp.UI
{
    /// <summary>
    /// Interaction logic for UI_Alpha.xaml
    /// </summary>
    public partial class UI_Alpha : UserControl
    {
        MainWindow main;
        PicPicker picPicker = new PicPicker();
        SongPicker songPicker = new SongPicker();
        public UI_Alpha(MainWindow MW)
        {
            InitializeComponent();
            main = MW;
            InitPickers();
        }

        private void InitPickers()
        {
            picPicker.HorizontalAlignment = HorizontalAlignment.Right;
            picPicker.VerticalAlignment = VerticalAlignment.Bottom;
            picPicker.MaxHeight = 200;
            picPicker.Width = 250;
            picPicker.Visibility = Visibility.Hidden;
            picPicker.Margin = new Thickness(0, 0, 172, 60);
            MainGrid.Children.Add(picPicker);

            songPicker.HorizontalAlignment = HorizontalAlignment.Right;
            songPicker.VerticalAlignment = VerticalAlignment.Bottom;
            songPicker.MaxHeight = 200;
            songPicker.Width = 400;
            songPicker.Visibility = Visibility.Hidden;
            songPicker.Margin = new Thickness(0, 0, 40, 60);
            MainGrid.Children.Add(songPicker);
        }

        private void nextSong_button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.next_song();
        }

        private void prevSong_button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.prev_song();
        }

        private void songs_button_Click(object sender, RoutedEventArgs e)
        {
            ToggleSongPickerVisivility();
        }

        private void nextImage_button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.next_image();
        }

        private void prevImage_button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.prev_image();
        }

        private void fullAuto_button_Click(object sender, RoutedEventArgs e)
        {
            main.full_auto_mode = true;
        }

        private void images_button_Click(object sender, RoutedEventArgs e)
        {
            TogglePicPickerVisivility();
        }
        public void ToggleSongPickerVisivility()
        {
            if (songPicker.Visibility == Visibility.Visible) songPicker.Visibility = Visibility.Hidden;
            else
            {
                if (picPicker.Visibility == Visibility.Visible) picPicker.Visibility = Visibility.Hidden;
                songPicker.Visibility = Visibility.Visible;
            }
        }
        public void TogglePicPickerVisivility()
        {
            if (picPicker.Visibility == Visibility.Visible) picPicker.Visibility = Visibility.Hidden;
            else
            {
                if (songPicker.Visibility == Visibility.Visible) songPicker.Visibility = Visibility.Hidden;
                picPicker.Visibility = Visibility.Visible;
            }
        }
    }
}

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

namespace MOPS.UI
{
    /// <summary>
    /// Interaction logic for UI_Alpha.xaml
    /// </summary>
    public partial class UI_Alpha : UserControl
    {
        MainWindow main;
        public UI_Alpha(MainWindow MW)
        {
            InitializeComponent();
            main = MW;
        }

        private void nextSong_button_Click(object sender, RoutedEventArgs e)
        {
            main.next_song();
        }

        private void prevSong_button_Click(object sender, RoutedEventArgs e)
        {
            main.prev_song();
        }

        private void songs_button_Click(object sender, RoutedEventArgs e)
        {
            if (main.songs_listbox.Visibility == Visibility.Hidden)
            {
                main.songs_listbox.Visibility = Visibility.Visible;
                main.images_listbox.Visibility = Visibility.Hidden;
            }
            else main.songs_listbox.Visibility = Visibility.Hidden;
        }

        private void nextImage_button_Click(object sender, RoutedEventArgs e)
        {
            main.next_image();
        }

        private void prevImage_button_Click(object sender, RoutedEventArgs e)
        {
            main.prev_image();
        }

        private void fullAuto_button_Click(object sender, RoutedEventArgs e)
        {
            main.full_auto_mode = true;
        }

        private void images_button_Click(object sender, RoutedEventArgs e)
        {
            if (main.images_listbox.Visibility == Visibility.Hidden)
            {
                main.images_listbox.Visibility = Visibility.Visible;
                main.songs_listbox.Visibility = Visibility.Hidden;
            }
            else main.images_listbox.Visibility = Visibility.Hidden;
        }
    }
}

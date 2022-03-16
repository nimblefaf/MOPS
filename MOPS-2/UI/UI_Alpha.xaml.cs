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

        public UI_Alpha()
        {
            InitializeComponent();
        }

        public void SetReference(MainWindow window)
        {
            main = window;
        }

        private void songs_TBB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (main.songs_listbox.Visibility == Visibility.Hidden)
            {
                main.songs_listbox.Visibility = Visibility.Visible;
                main.images_listbox.Visibility = Visibility.Hidden;
            }
            else main.songs_listbox.Visibility = Visibility.Hidden;
        }

        private void images_TBB_Copy_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (main.images_listbox.Visibility == Visibility.Hidden)
            {
                main.images_listbox.Visibility = Visibility.Visible;
                main.songs_listbox.Visibility = Visibility.Hidden;
            }
            else main.images_listbox.Visibility = Visibility.Hidden;
        }

        private void fullAuto_TBB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            main.full_auto_mode = true;
        }

        private void nextSong_TBB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            main.next_song();
        }

        private void prevSong_TBB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            main.prev_song();
        }

        private void nextImage_TBB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            main.next_image();
        }

        private void prevImage_TBB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            main.prev_image();
        }
    }
}

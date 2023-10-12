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
    /// Interaction logic for SongPicker.xaml
    /// </summary>
    public partial class SongPicker : UserControl
    {
        MainWindow main;
        public SongPicker()
        {
            InitializeComponent();
            main = (MainWindow)Application.Current.MainWindow;
            songs_listbox.ItemsSource = main.Core.enabled_songs;
        }

        private void songs_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (songs_listbox.SelectedIndex != -1 & songs_listbox.SelectedIndex != main.Core.current_song_ind)
            {
                main.Core.Change_Song(main.Core.enabled_songs[songs_listbox.SelectedIndex].Ind);
                main.Core.current_song_ind = songs_listbox.SelectedIndex;
            }
        }

        private void Songs_listbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}

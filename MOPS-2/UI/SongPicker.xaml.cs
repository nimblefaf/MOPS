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
    /// Interaction logic for SongPicker.xaml
    /// </summary>
    public partial class SongPicker : UserControl
    {
        MainWindow main;
        public SongPicker()
        {
            InitializeComponent();
            main = (MainWindow)Application.Current.MainWindow;
            songs_listbox.ItemsSource = main.enabled_songs;
        }

        private void songs_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (songs_listbox.SelectedIndex != -1)
            {
                main.Core.Change_Song(main.enabled_songs[songs_listbox.SelectedIndex].Ind);
            }
            else
            {
                
            }
        }

        private void Songs_listbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}

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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace MOPS_2
{
    public class rpsetnames
    {
        public string Name { get; set; }
    }
    
    
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public static List<rpsetnames> names = new List<rpsetnames>();

        public Settings()
        {
            InitializeComponent();
            respack_listbox.ItemsSource = names;
            stat_update();
        }

        private void SettingsWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right) Hide();
        }

        private void hide_button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void respack_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int ind = respack_listbox.SelectedIndex;
            rp_name_label.Content = ResPackManager.resPacks[ind].name;
            rp_author_label.Content = ResPackManager.resPacks[ind].author;
            rp_description_textbox.Text = ResPackManager.resPacks[ind].description;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void load_rp_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = ".zip";
            openFile.Filter = "Zip archive (.zip)|*.zip";
            if (openFile.ShowDialog() == true)
            {
                ResPackManager.SupremeReader(openFile.FileName);
                names.Add(new rpsetnames() { Name = ResPackManager.resPacks[ResPackManager.resPacks.Length - 1].name });
                respack_listbox.Items.Refresh();
                stat_update();
            }
        }

        private void stat_update()
        {
            ImagesNumber_label.Content = ResPackManager.allPics.Length;
            SongNumber_label.Content = ResPackManager.allSongs.Length;
        }
    }
}

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
    public class setdata
    {
        public string Name { get; set; }
        public bool State { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public static List<setdata> rp_names = new List<setdata>();
        public static List<setdata> song_names = new List<setdata>();
        public static List<setdata> images_names = new List<setdata>();

        public Settings()
        {
            InitializeComponent();
            respack_listbox.ItemsSource = rp_names;
            songs_listbox.ItemsSource = song_names;
            images_listbox.ItemsSource = images_names;
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

            song_names.Clear();

            int ceiling;
            if (ind == ResPackManager.resPacks.Length - 1) ceiling = ResPackManager.allSongs.Length;
            else ceiling = ResPackManager.resPacks[ind + 1].songs_start;
            for (int i = ResPackManager.resPacks[ind].songs_start; i < ceiling; i++)
            {
                song_names.Add(new setdata() { Name = ResPackManager.allSongs[i].title, State = ResPackManager.allSongs[i].enabled });
            }
            songs_listbox.Items.Refresh();

            images_names.Clear();
            if (ind == ResPackManager.resPacks.Length - 1) ceiling = ResPackManager.allPics.Length;
            else ceiling = ResPackManager.resPacks[ind + 1].pics_start;
            for (int i = ResPackManager.resPacks[ind].pics_start; i < ceiling; i++)
            {
                images_names.Add(new setdata() { Name = ResPackManager.allPics[i].name, State = ResPackManager.allPics[i].enabled });
            }
            images_listbox.Items.Refresh();
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
                if (ResPackManager.SupremeReader(openFile.FileName))
                {
                    rp_names.Add(new setdata() { Name = ResPackManager.resPacks[ResPackManager.resPacks.Length - 1].name, State = true });
                    respack_listbox.Items.Refresh();
                    stat_update();
                }
            }
        }

        private void stat_update()
        {
            ImagesNumber_label.Content = ResPackManager.allPics.Length;
            SongNumber_label.Content = ResPackManager.allSongs.Length;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void songs_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void images_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

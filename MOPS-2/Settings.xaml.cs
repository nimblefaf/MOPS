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
using System.Collections.ObjectModel;

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
        public static ObservableCollection<setdata> rp_names = new ObservableCollection<setdata>();
        public static ObservableCollection<setdata> song_names = new ObservableCollection<setdata>();
        public static ObservableCollection<setdata> images_names = new ObservableCollection<setdata>();

        public Settings()
        {
            InitializeComponent();
            respack_listbox.ItemsSource = rp_names;
            songs_listbox.ItemsSource = song_names;
            images_listbox.ItemsSource = images_names;
        }

        private void SettingsWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right) Hide();
        }

        private void hide_button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
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
            //Songs_tab.Header = "Songs: " + ResPackManager.resPacks[ind].songs_start;

            song_names.Clear();

            int ceiling;
            if (ind == ResPackManager.resPacks.Length - 1) ceiling = ResPackManager.allSongs.Length;
            else ceiling = ResPackManager.resPacks[ind + 1].songs_start;
            for (int i = ResPackManager.resPacks[ind].songs_start; i < ceiling; i++)
            {
                song_names.Add(new setdata() { Name = ResPackManager.allSongs[i].title, State = ResPackManager.allSongs[i].enabled });
            }

            images_names.Clear();
            if (ind == ResPackManager.resPacks.Length - 1) ceiling = ResPackManager.allPics.Length;
            else ceiling = ResPackManager.resPacks[ind + 1].pics_start;
            for (int i = ResPackManager.resPacks[ind].pics_start; i < ceiling; i++)
            {
                images_names.Add(new setdata() { Name = ResPackManager.allPics[i].name, State = ResPackManager.allPics[i].enabled });
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void load_rp_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = ".zip";
            openFile.Filter = "Zip Archive (.zip)|*.zip";
            if (openFile.ShowDialog() == true)
            {
                if (ResPackManager.SupremeReader(openFile.FileName))
                {
                    rp_names.Add(new setdata() { Name = ResPackManager.resPacks[ResPackManager.resPacks.Length - 1].name, State = true });

                    if (ResPackManager.resPacks[ResPackManager.resPacks.Length - 1].songs_count > 0)
                        for (int i = ResPackManager.resPacks[ResPackManager.resPacks.Length - 1].songs_start; i < ResPackManager.allSongs.Length; i++)
                            MainWindow.enabled_songs.Add(new rdata() { Name = ResPackManager.allSongs[i].title, Ind = i });
                    if (ResPackManager.resPacks[ResPackManager.resPacks.Length - 1].pics_count > 0)
                        for (int i = ResPackManager.resPacks[ResPackManager.resPacks.Length - 1].pics_start; i < ResPackManager.allPics.Length; i++)
                            MainWindow.enabled_images.Add(new rdata() { Name = ResPackManager.allPics[i].name, Ind = i });
                       
                    stat_update();
                }
            }
        }

        public void stat_update()
        {
            ImagesNumber_label.Content = MainWindow.enabled_images.Count() + "/" + ResPackManager.allPics.Length;
            SongNumber_label.Content = MainWindow.enabled_songs.Count().ToString() + "/" + ResPackManager.allSongs.Length.ToString();
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

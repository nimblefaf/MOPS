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

namespace MOPS
{
    public class setdata
    {
        public string Name { get; set; }
        public bool State { get; set; }
        public int Ind { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public static ObservableCollection<setdata> rp_names = new ObservableCollection<setdata>();
        public static ObservableCollection<setdata> song_names = new ObservableCollection<setdata>();
        public static ObservableCollection<setdata> images_names = new ObservableCollection<setdata>();
        MainWindow main;

        public Settings()
        {
            InitializeComponent();
            respack_listbox.ItemsSource = rp_names;
            songs_listbox.ItemsSource = song_names;
            images_listbox.ItemsSource = images_names;
        }

        public void SetReference(MainWindow window)
        {
            main = window;
        }

        private void SettingsWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right) Hide();
        }

        private void hide_button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            main.Focus();
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void respack_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int ind = respack_listbox.SelectedIndex;
            rp_name_label.Content = RPManager.ResPacks[ind].name;
            rp_author_label.Content = RPManager.ResPacks[ind].author;
            rp_description_textbox.Text = RPManager.ResPacks[ind].description;
            Songs_tab.Header = "Songs: " + RPManager.ResPacks[ind].songs_count;
            Images_tab.Header = "Images: " + RPManager.ResPacks[ind].pics_count;

            song_names.Clear();
            int ceiling = RPManager.ResPacks[ind].songs_start + RPManager.ResPacks[ind].songs_count;
            for (int i = RPManager.ResPacks[ind].songs_start; i < ceiling; i++)
            {
                song_names.Add(new setdata() { Name = RPManager.allSongs[i].title, State = RPManager.allSongs[i].enabled, Ind = i });
            }

            images_names.Clear();
            ceiling = RPManager.ResPacks[ind].pics_start + RPManager.ResPacks[ind].pics_count;
            for (int i = RPManager.ResPacks[ind].pics_start; i < ceiling; i++)
            {
                images_names.Add(new setdata() { Name = RPManager.allPics[i].name, State = RPManager.allPics[i].enabled, Ind = i });
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void load_rp_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog
            {
                DefaultExt = ".zip",
                Filter = "Zip Archive (.zip)|*.zip"
            };
            if (openFile.ShowDialog() == true)
            {
                if (RPManager.SupremeReader(openFile.FileName))
                {
                    rp_names.Add(new setdata() { Name = RPManager.ResPacks[RPManager.ResPacks.Length - 1].name, State = true });

                    if (RPManager.ResPacks[RPManager.ResPacks.Length - 1].songs_count > 0)
                        for (int i = RPManager.ResPacks[RPManager.ResPacks.Length - 1].songs_start; i < RPManager.allSongs.Length; i++)
                            MainWindow.enabled_songs.Add(new rdata() { Name = RPManager.allSongs[i].title, Ind = i });
                    if (RPManager.ResPacks[RPManager.ResPacks.Length - 1].pics_count > 0)
                        for (int i = RPManager.ResPacks[RPManager.ResPacks.Length - 1].pics_start; i < RPManager.allPics.Length; i++)
                            MainWindow.enabled_images.Add(new rdata() { Name = RPManager.allPics[i].name, Ind = i });
                       
                    stat_update();
                }
            }
        }

        public void stat_update()
        {
            ImagesNumber_label.Content = MainWindow.enabled_images.Count() + "/" + RPManager.allPics.Length;
            SongNumber_label.Content = MainWindow.enabled_songs.Count().ToString() + "/" + RPManager.allSongs.Length.ToString();
        }

        private void songs_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (songs_listbox.SelectedIndex != -1)
            {
                if (!song_names[songs_listbox.SelectedIndex].State)
                {
                    song_names[songs_listbox.SelectedIndex].State = true;
                    songs_listbox.Items.Refresh();
                    Add_Enabled_Song(song_names[songs_listbox.SelectedIndex]);
                    stat_update();
                }
                for (int i = 0; i < MainWindow.enabled_songs.Count; i++)
                    if (MainWindow.enabled_songs[i].Ind == song_names[songs_listbox.SelectedIndex].Ind)
                        main.songs_listbox.SelectedIndex = i;
            }
            
        }


        private void images_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(images_listbox.SelectedIndex != -1)
            {
                if (!images_names[images_listbox.SelectedIndex].State)
                {
                    images_names[images_listbox.SelectedIndex].State = true;
                    images_listbox.Items.Refresh();
                    Add_Enabled_Image(images_names[images_listbox.SelectedIndex]);
                    stat_update();
                }
                for (int i = 0; i < MainWindow.enabled_images.Count; i++)
                    if (MainWindow.enabled_images[i].Ind == images_names[images_listbox.SelectedIndex].Ind)
                    {
                        main.full_auto_mode = false;
                        main.images_listbox.SelectedIndex = i;
                    }
                        
            }
        }

        private void Song_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            foreach (setdata el in song_names)
            {
                if (el.State != RPManager.allSongs[el.Ind].enabled)
                {
                    if (!el.State) Remove_Disabled_Song(el);
                    else Add_Enabled_Song(el);
                    break;
                }
            }
            stat_update();
        }

        private void Remove_Disabled_Song(setdata elem)
        {
            RPManager.allSongs[elem.Ind].enabled = false;
            for (int i = 0; i < MainWindow.enabled_songs.Count; i++)
            {
                if (MainWindow.enabled_songs[i].Ind == elem.Ind)
                {
                    MainWindow.enabled_songs.RemoveAt(i);
                    break;
                }
            }
        }

        private void Add_Enabled_Song(setdata elem)
        {
            rdata copy = new rdata { Name = elem.Name, Ind = elem.Ind };
            if (MainWindow.enabled_songs.Count == 0) MainWindow.enabled_songs.Add(copy);
            else if (MainWindow.enabled_songs.Last().Ind < elem.Ind) MainWindow.enabled_songs.Add(copy);
            else for (int i = 0; i < MainWindow.enabled_songs.Count; i++)
                {
                    if (MainWindow.enabled_songs[i].Ind > elem.Ind)
                    {
                        MainWindow.enabled_songs.Insert(i, copy);
                        break;
                    }
                }
            RPManager.allSongs[elem.Ind].enabled = true;
        }

        private void Images_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            foreach (setdata el in images_names)
            {
                if (el.State != RPManager.allPics[el.Ind].enabled)
                {
                    if (!el.State) Remove_Disabled_Image(el);
                    else Add_Enabled_Image(el);
                    break;
                }
            }
            stat_update();
        }

        private void Remove_Disabled_Image(setdata elem)
        {
            RPManager.allPics[elem.Ind].enabled = false;
            for (int i = 0; i < MainWindow.enabled_images.Count; i++)
            {
                if (MainWindow.enabled_images[i].Ind == elem.Ind)
                {
                    MainWindow.enabled_images.RemoveAt(i);
                    if (MainWindow.enabled_images.Count == 0) main.ImageChange(-1);
                    else if (i == main.current_image_pos) main.ImageChange(i - 1);
                    else if (i < main.current_image_pos) main.current_image_pos--;
                    break;
                }
            }
        }

        private void Add_Enabled_Image(setdata elem)
        {
            rdata copy = new rdata { Name = elem.Name, Ind = elem.Ind };
            if (MainWindow.enabled_images.Count == 0)
            {
                MainWindow.enabled_images.Add(copy);
                main.ImageChange(0);
            }
            else if (MainWindow.enabled_images.Last().Ind < elem.Ind) MainWindow.enabled_images.Add(copy);
            else for (int i = 0; i < MainWindow.enabled_images.Count; i++)
                {
                    if (MainWindow.enabled_images[i].Ind > elem.Ind)
                    {
                        MainWindow.enabled_images.Insert(i, copy);
                        if (i <= main.current_image_pos) main.current_image_pos++;
                        break;
                    }
                }
            RPManager.allPics[elem.Ind].enabled = true;
        }

        private void RPList_CheckBox_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void enableAll_button_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                foreach (setdata el in song_names)
                {
                    if (!el.State)
                    {
                        el.State = true;
                        Add_Enabled_Song(el);
                    }
                }
                songs_listbox.Items.Refresh();
            }

            else
            {
                foreach (setdata el in images_names)
                {
                    if (!el.State)
                    {
                        el.State = true;
                        Add_Enabled_Image(el);
                    }
                }
                images_listbox.Items.Refresh();
            }
            if (main.current_image_pos == -1) main.ImageChange(0);
            stat_update();
        }

        private void disabelAll_button_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0) //FOR SONGS
            {
                foreach (setdata el in song_names)
                {
                    if (el.State)
                    {
                        el.State = false;
                        Remove_Disabled_Song(el);
                    }
                }
                songs_listbox.Items.Refresh();
            }

            else                               //FOR IMAGES
            {
                foreach (setdata el in images_names)
                {
                    if (el.State)
                    {
                        el.State = false;
                        Remove_Disabled_Image(el);
                    }
                }
                images_listbox.Items.Refresh();
                main.ImageChange(-1);
            }
            stat_update();
        }

        private void invert_button_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                foreach (setdata el in song_names)
                {
                    if (el.State)
                    {
                        el.State = false;
                        Remove_Disabled_Song(el);
                    }
                    else
                    {
                        el.State = true;
                        Add_Enabled_Song(el);
                    }
                }
                songs_listbox.Items.Refresh();
            }

            else
            {
                foreach (setdata el in images_names)
                {
                    if (el.State)
                    {
                        el.State = false;
                        Remove_Disabled_Image(el);
                    }
                    else
                    {
                        el.State = true;
                        Add_Enabled_Image(el);
                    }
                }
                images_listbox.Items.Refresh();
                if (MainWindow.enabled_images.Count == 0) main.ImageChange(-1);
                else if (main.current_image_pos == -1) main.ImageChange(0);
            }
            stat_update();
        }
    }
}

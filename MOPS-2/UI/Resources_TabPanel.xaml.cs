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
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Win32;
using System.Net;
using Newtonsoft.Json;
using System.Windows.Threading;
using System.Timers;

namespace MOPS.UI
{
    class RemoteRP_Info
    {
        public string author { get; set; }
        public string Name { get; set; }
        public string description { get; set; }
        public string[] images { get; set; }
        public string link { get; set; }
        public string[] songs { get; set; }
        public string url { get; set; }
        public bool loaded = false;
    }


    /// <summary>
    /// Interaction logic for Resources_TabPanel.xaml
    /// </summary>
    public partial class Resources_TabPanel : UserControl
    {
        public static ObservableCollection<setdata> rp_names = new ObservableCollection<setdata>();
        public static ObservableCollection<setdata> song_names = new ObservableCollection<setdata>();
        public static ObservableCollection<setdata> images_names = new ObservableCollection<setdata>();
        MainWindow main;
        RemoteRP_Info[] remoteRPs;

        private BackgroundWorker backgroundLoader;
        private BackgroundWorker remoteListLoader;
        private BackgroundWorker backgroundWebLoader;

        private DispatcherTimer ui_reset_timer;

        public Resources_TabPanel()
        {
            InitializeComponent();

            respack_listbox.ItemsSource = rp_names;
            songs_listbox.ItemsSource = song_names;
            images_listbox.ItemsSource = images_names;
            backgroundLoader = new BackgroundWorker();
            backgroundLoader.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(load_completed);
            backgroundLoader.DoWork +=
                new DoWorkEventHandler(load_dowork);
            backgroundLoader.ProgressChanged +=
                new ProgressChangedEventHandler(BGWorker_ProgressChanged);

            remoteListLoader = new BackgroundWorker();
            remoteListLoader.DoWork +=
                new DoWorkEventHandler(RemoteListLoad);
            remoteListLoader.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(RemoteListLoadCompleted);

            backgroundWebLoader = new BackgroundWorker();
            backgroundWebLoader.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(load_completed);
            backgroundWebLoader.DoWork +=
                new DoWorkEventHandler(webLoad_dowork);
            backgroundWebLoader.ProgressChanged +=
                new ProgressChangedEventHandler(BGWorker_ProgressChanged);

            ui_reset_timer = new DispatcherTimer();
            ui_reset_timer.Interval = TimeSpan.FromSeconds(3);
            ui_reset_timer.Tick += new EventHandler(ui_reset_timer_tick);

            Remote_listBox.Items.Add(new setdata() { Name = "Click to load the list" }) ;
        }

        public void SetReference(MainWindow window)
        {
            main = window;
        }

        private void respack_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (respack_listbox.SelectedIndex != -1)
            {
                if(Remote_listBox.IsEnabled) Remote_listBox.SelectedIndex = -1;
                load_remote_button.Visibility = Visibility.Hidden;
                enableAll_button.Visibility = Visibility.Visible;
                invert_button.Visibility = Visibility.Visible;
                disabelAll_button.Visibility = Visibility.Visible;

                int ind = respack_listbox.SelectedIndex;
                rp_name_label.Content = main.RPM.ResPacks[ind].name;
                rp_author_label.Content = main.RPM.ResPacks[ind].author;
                if (main.RPM.ResPacks[ind].description == "") rp_description_textbox.Text = "<no description>";
                else rp_description_textbox.Text = main.RPM.ResPacks[ind].description;
                Songs_tab.Header = "Songs: " + main.RPM.ResPacks[ind].songs_count;
                Images_tab.Header = "Images: " + main.RPM.ResPacks[ind].pics_count;

                songs_listbox.ItemTemplate = (DataTemplate)Resources["DataTemplateCheckBoxed"]; //showing checkboxes
                song_names.Clear();
                int ceiling = main.RPM.ResPacks[ind].songs_start + main.RPM.ResPacks[ind].songs_count;
                for (int i = main.RPM.ResPacks[ind].songs_start; i < ceiling; i++)
                {
                    song_names.Add(new setdata() { Name = main.RPM.allSongs[i].title, State = main.RPM.allSongs[i].enabled, Ind = i });
                }

                images_listbox.ItemTemplate = (DataTemplate)Resources["DataTemplateCheckBoxed"];
                images_names.Clear();
                ceiling = main.RPM.ResPacks[ind].pics_start + main.RPM.ResPacks[ind].pics_count;
                for (int i = main.RPM.ResPacks[ind].pics_start; i < ceiling; i++)
                {
                    images_names.Add(new setdata() { Name = main.RPM.allPics[i].name, State = main.RPM.allPics[i].enabled, Ind = i });
                }
            }
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
                load_rp_button.IsEnabled = false;
                Status_textBlock.Text = "Processing...";
                backgroundLoader.RunWorkerAsync(new string[] { openFile.FileName });
            }
        }

        public void load_dowork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.WorkerReportsProgress = true;
            e.Result = main.RPM.SupremeReader((string[])e.Argument, worker, e);
        }
        public void load_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            load_rp_button.IsEnabled = true;
            if (e.Result.ToString() != null)
            {
                foreach (Pics elem in (Pics[])e.Result)
                {
                    Array.Resize(ref main.RPM.allPics, main.RPM.allPics.Length + 1);
                    main.RPM.allPics[main.RPM.allPics.Length - 1] = elem;
                }

                bool preaload = false;
                if (rp_names.Count == 0) preaload = true;
                add_last_rps();
                if (preaload)
                {
                    if (main.RPM.ResPacks[0].name == "0x40 Hues v5.0 Defaults") main.ImageChange(55);
                    else if (main.RPM.allPics.Length > 0) main.ImageChange(0);
                    if (main.RPM.allSongs.Length > 0) main.songs_listbox.SelectedIndex = 0;
                }

            }
            ProgBar.Value = 0;
            Status_textBlock.Text = "Done";
            GC.Collect();
        }
        void BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgBar.Value = e.ProgressPercentage;
        }


        public void add_last_rps()
        {
            int num = main.RPM.ResPacks.Length - rp_names.Count;
            for (int j = num; j > 0; j--)
            {
                rp_names.Add(new setdata() { Name = main.RPM.ResPacks[main.RPM.ResPacks.Length - j].name, State = true });

                if (main.RPM.ResPacks[main.RPM.ResPacks.Length - j].songs_count > 0)
                    for (int i = main.RPM.ResPacks[main.RPM.ResPacks.Length - j].songs_start; i < main.RPM.ResPacks[main.RPM.ResPacks.Length - j].songs_start + main.RPM.ResPacks[main.RPM.ResPacks.Length - j].songs_count; i++)
                        MainWindow.enabled_songs.Add(new rdata() { Name = main.RPM.allSongs[i].title, Ind = i });
                if (main.RPM.ResPacks[main.RPM.ResPacks.Length - j].pics_count > 0)
                    for (int i = main.RPM.ResPacks[main.RPM.ResPacks.Length - j].pics_start; i < main.RPM.ResPacks[main.RPM.ResPacks.Length - j].pics_start + main.RPM.ResPacks[main.RPM.ResPacks.Length - j].pics_count; i++)
                        MainWindow.enabled_images.Add(new rdata() { Name = main.RPM.allPics[i].name, Ind = i });
            }
            stat_update();
        }
        //public void add_rp(int RP_id)
        //{
        //    rp_names.Add(new setdata() { Name = main.RPM.ResPacks[RP_id].name, State = true });

        //    if (main.RPM.ResPacks[RP_id].songs_count > 0)
        //        for (int i = main.RPM.ResPacks[RP_id].songs_start; i < main.RPM.allSongs.Length; i++)
        //            MainWindow.enabled_songs.Add(new rdata() { Name = main.RPM.allSongs[i].title, Ind = i });
        //    if (main.RPM.ResPacks[RP_id].pics_count > 0)
        //        for (int i = main.RPM.ResPacks[RP_id].pics_start; i < main.RPM.allPics.Length; i++)
        //            MainWindow.enabled_images.Add(new rdata() { Name = main.RPM.allPics[i].name, Ind = i });

        //    stat_update();
        //}

        /// <summary>
        /// Updates data in "total songs" and "total images" labels
        /// </summary>
        public void stat_update()
        {
            ImagesNumber_label.Content = MainWindow.enabled_images.Count() + "/" + main.RPM.allPics.Length;
            SongNumber_label.Content = MainWindow.enabled_songs.Count().ToString() + "/" + main.RPM.allSongs.Length.ToString();
        }

        private void songs_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (songs_listbox.SelectedIndex != -1 & load_remote_button.Visibility == Visibility.Hidden)
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
            if (images_listbox.SelectedIndex != -1 & load_remote_button.Visibility == Visibility.Hidden)
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
                if (el.State != main.RPM.allSongs[el.Ind].enabled)
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
            main.RPM.allSongs[elem.Ind].enabled = false;
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
            main.RPM.allSongs[elem.Ind].enabled = true;
        }

        private void Images_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            foreach (setdata el in images_names)
            {
                if (el.State != main.RPM.allPics[el.Ind].enabled)
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
            main.RPM.allPics[elem.Ind].enabled = false;
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
            main.RPM.allPics[elem.Ind].enabled = true;
        }

        private void RPList_CheckBox_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void enableAll_button_Click(object sender, RoutedEventArgs e)
        {
            if (ResTabControl.SelectedIndex == 0) //for songs
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

            else                                  //for images
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
            if (ResTabControl.SelectedIndex == 0) //FOR SONGS
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
            if (ResTabControl.SelectedIndex == 0) //for songs
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

            else                                // for images
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


        #region remote_rp_load

        private bool WebClient_IsActive = false;
        
        private void load_remote_button_Click(object sender, RoutedEventArgs e)
        {
            WC_UI_Reset();
            Remote_listBox.IsEnabled = false;
            load_remote_button.IsEnabled = false;
            Status_textBlock.Text = "Loading...";
            WebClient_IsActive = true;
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadDataCompleted += Wc_DownloadDataCompleted;
                wc.DownloadDataAsync(new Uri(remoteRPs[Remote_listBox.SelectedIndex].url));
            }
        }

        private void Wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            remoteRPs[Remote_listBox.SelectedIndex].loaded = true;
            Remote_listBox.IsEnabled = true;
            load_remote_button.IsEnabled = true;
            ProgBar.Value = 0;
            if (e.Error != null)
            {
                Status_textBlock.Text = "Error!";
                Status_textBlock.Foreground = Brushes.Red;
            }
            else if (Encoding.Default.GetString(e.Result.Take(6).ToArray()) == "<html>") //if the site returned 404 page
            {
                Status_textBlock.Text = "Error!";
                Status_textBlock.Foreground = Brushes.Red;
            }
            else
            {
                if (load_remote_button.Visibility == Visibility.Visible)
                {
                    load_remote_button.IsEnabled = false;
                    load_remote_button.Background = Brushes.GreenYellow;
                    load_remote_button.Content = "LOADED";
                }
                else Remote_listBox.SelectedIndex = -1;

                Status_textBlock.Text = "Processing...";
                backgroundWebLoader.RunWorkerAsync(e.Result);
                
            }
            WebClient_IsActive = false;
            ui_reset_timer.Start();
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgBar.Value = e.ProgressPercentage;
            bytesLoaded_textBlock.Text = e.BytesReceived.ToString() + "b";
            bytesToLoad_textBlock.Text = e.TotalBytesToReceive.ToString() + "b";
            percentLoaded_textBlock.Text = e.ProgressPercentage.ToString() + "%";
        }

        public void webLoad_dowork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.WorkerReportsProgress = true;
            e.Result = main.RPM.WebReader((byte[])e.Argument, worker, e);
        }

        private void ui_reset_timer_tick(object sender, EventArgs e)
        {
            WC_UI_Reset();
        }

        private void WC_UI_Reset()
        {
            ui_reset_timer.Stop();
            if (!WebClient_IsActive)
            {
                bytesLoaded_textBlock.Text = "0b";
                bytesToLoad_textBlock.Text = "0b";
                percentLoaded_textBlock.Text = "0%";
                Status_textBlock.Text = "Idle"; 
                Status_textBlock.Foreground = Brushes.Black;
            }
        }

        #endregion

        #region remote_list

        string url_new = "https://portal.0x40hu.es/resource_packs.json";
        //string url_classic = "https://cdn.0x40.ga/getRespacks.php";

        private string error_message;
        private string hues_json;
        private void RemoteListLoad(object sender, DoWorkEventArgs e)
        {
            hues_json = "";
            Exception exception;
            error_message = "";
            using (WebClient wc = new WebClient())
            {
                try
                {
                    //hues_json = wc.DownloadString(url);
                    hues_json = wc.DownloadString(url_new);
                }
                catch (WebException WE)
                {
                    exception = WE;
                    error_message = WE.Message;
                }
            }

        }
        private void RemoteListLoadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (error_message != "")
            {
                Remote_listBox.Items.Clear();
                Remote_listBox.Items.Add(new setdata() { Name = "Error! Click to try again" });
            }
            else if (hues_json.Substring(0, 6) == "<html>") //if loaded 404 page in json string
            {
                Remote_listBox.Items.Clear();
                Remote_listBox.Items.Add(new setdata() { Name = "Error! Click to try again" });
            }
            else
            {
                remoteRPs = JsonConvert.DeserializeObject<RemoteRP_Info[]>(hues_json);

                Remote_listBox.Items.Clear();
                Remote_listBox.ItemsSource = remoteRPs;
            }

            Remote_listBox.IsEnabled = true;
            Remote_listBox.Cursor = Cursors.Arrow;
        }

        private void Remote_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Remote_listBox.ItemsSource == null & Remote_listBox.IsEnabled == true & Remote_listBox.SelectedIndex == 0)
            {
                //((setdata)Remote_listBox.Items[0]).Name = "Loading...";
                Remote_listBox.Items.Clear();
                Remote_listBox.Items.Add(new setdata() { Name = "Loading..." });
                Remote_listBox.Cursor = Cursors.Wait;
                Remote_listBox.IsEnabled = false;
                remoteListLoader.RunWorkerAsync();
            }
            else if (Remote_listBox.SelectedIndex != -1)
            {
                respack_listbox.SelectedIndex = -1;
                load_remote_button.Visibility = Visibility.Visible;
                enableAll_button.Visibility = Visibility.Hidden;
                invert_button.Visibility = Visibility.Hidden;
                disabelAll_button.Visibility = Visibility.Hidden;

                if (remoteRPs[Remote_listBox.SelectedIndex].loaded)
                {
                    load_remote_button.Background = Brushes.GreenYellow;
                    load_remote_button.Content = "LOADED";
                    load_remote_button.IsEnabled = false;
                }
                else
                {
                    load_remote_button.Background = Brushes.LightGray;
                    load_remote_button.Content = "LOAD REMOTE";
                    load_remote_button.IsEnabled = true;
                }

                int ind = Remote_listBox.SelectedIndex;
                rp_name_label.Content = remoteRPs[ind].Name;
                rp_author_label.Content = remoteRPs[ind].author;
                rp_description_textbox.Text = remoteRPs[ind].description;
                Songs_tab.Header = "Songs: " + remoteRPs[ind].songs.Length;
                Images_tab.Header = "Images: " + remoteRPs[ind].images.Length;

                songs_listbox.ItemTemplate = (DataTemplate)Resources["DataTemplateStateless"]; //hiding checkboxes
                song_names.Clear();
                for (int i = 0; i < remoteRPs[ind].songs.Length; i++)
                {
                    song_names.Add(new setdata() { Name = remoteRPs[ind].songs[i], State = false, Ind = i });
                }

                images_listbox.ItemTemplate = (DataTemplate)Resources["DataTemplateStateless"];
                images_names.Clear();
                for (int i = 0; i < remoteRPs[ind].images.Length; i++)
                {
                    images_names.Add(new setdata() { Name = remoteRPs[ind].images[i], State = false, Ind = i });
                }
            }
        }
        #endregion
    }
}

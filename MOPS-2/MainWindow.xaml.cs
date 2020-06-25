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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;


namespace MOPS
{
    /// <summary>
    /// Template for the contents of images&songs listboxes
    /// </summary>
    public class rdata
    {
        public string Name { get; set; }
        public int Ind { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rnd = new Random();
        Audio Player = new Audio();
        
        public DispatcherTimer Timer = new DispatcherTimer(DispatcherPriority.Send);
        public DispatcherTimer AnimTimer = new DispatcherTimer(DispatcherPriority.Render);
        public DispatcherTimer ShortBlackoutTimer = new DispatcherTimer(DispatcherPriority.Render);
        double Correction;
        public double beat_length = 0;
        public double buildup_beat_len = 0;
        public static Hues.Palette[] hues = Hues.hues_normal;

        public static Settings set = new Settings();
        public RPManager RPM = new RPManager();

        public bool muted = false;
        public int muted_volume;

        public bool full_auto_mode = true;
        public bool buildup_enabled = true; // Need to redo it in a 3-mode trigger (to add "play once" option)
        /// <summary>
        /// Quality of blur, from 0 to 3. Zero for stretching a single image, 1-3 for moving copies of image to the center.
        /// </summary>
        public int blur_quality = -1;
        /// <summary>
        /// Duration of blur animation. From 0 to 3. [appr. from 1s to 0.3s]
        /// </summary>
        public double blur_decay = 1;
        /// <summary>
        /// How far away blur goes, from 0 to 3.
        /// </summary>
        public int blur_amount = 0;
        public bool blackouted = false;

        public int current_song = 0;
        public int current_image_pos = 55;
        private int anim_ind = 0;

        private string loop_rhythm;
        private string build_rhythm;
        public int rhythm_pos = 0;
        public int b_rhythm_pos = 0;
        /// <summary>
        /// List of enabled songs displayed in songs_listbox.
        /// </summary>
        public static ObservableCollection<rdata> enabled_songs = new ObservableCollection<rdata>();
        /// <summary>
        /// List of enabled images displayed in images_listbox.
        /// </summary>
        public static ObservableCollection<rdata> enabled_images = new ObservableCollection<rdata>();

        private BackgroundWorker backgroundWorker;

        public MainWindow()
        {
            InitializeComponent();
            //RPManager.SupremeReader("Packs/Defaults_v5.0.zip", backgroundWorker, new DoWorkEventArgs);
            

            Timer.Tick += new EventHandler(Timer_Tick);
            AnimTimer.Tick += new EventHandler(AnimTimer_Tick);
            ShortBlackoutTimer.Tick += new EventHandler(ShortBlackoutTimer_Tick);
            
            Player.SetReference(this);
            set.SetReference(this);
            

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(load_completed);
            backgroundWorker.DoWork +=
                new DoWorkEventHandler(load_dowork);
        }

        private void load_dowork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = RPM.SupremeReader((string)e.Argument, worker, e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            set.Owner = this;
            Init_Animations();
 
            timeline_color_change();

            
        }

        private void First_Load(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Wait;
            InfoBlock.Text = "Initializing...";
            InfoBlock.Cursor = Cursors.Wait;
            backgroundWorker.RunWorkerAsync("Packs/Defaults_v5.0.zip");
        }

        private void load_completed(object sender, RunWorkerCompletedEventArgs e)
        {
            foreach (Pics elem in (Pics[])e.Result)
            {
                Array.Resize(ref RPM.allPics, RPM.allPics.Length + 1);
                RPM.allPics[RPM.allPics.Length - 1] = elem;
            }

            //for (int i = 0; i < RPM.allSongs.Length; i++) enabled_songs.Add(new rdata() { Name = RPM.allSongs[i].title, Ind = i });
            //songs_listbox.ItemsSource = enabled_songs;

            //for (int i = 0; i < RPM.allPics.Length; i++) enabled_images.Add(new rdata() { Name = RPM.allPics[i].name, Ind = i });
            //images_listbox.ItemsSource = enabled_images;

            set.add_last_rp();
            songs_listbox.ItemsSource = enabled_songs;
            images_listbox.ItemsSource = enabled_images;

            //ImageChange(0);
            //songs_listbox.SelectedIndex = 0;

            full_auto_be.IsEnabled = true;
            images_be.IsEnabled = true;
            next_image_be.IsEnabled = true;
            prev_image_be.IsEnabled = true;

            next_song_be.IsEnabled = true;
            prev_song_be.IsEnabled = true;
            songs_be.IsEnabled = true;

            Cursor = Cursors.Arrow;
            //InfoBlock.Text = "Loaded";

            LightsWarning.Visibility = Visibility.Hidden;
            InfoBlock.Visibility = Visibility.Hidden;

            
        }

        private Storyboard SB_Blackout = new Storyboard();
        private DoubleAnimation Blackout = new DoubleAnimation();
        private Storyboard SB_Blackout_Short = new Storyboard();
        private DoubleAnimation Blackout_Short = new DoubleAnimation();
        private ThicknessAnimation Blackout_Blur = new ThicknessAnimation();

        private Storyboard SB = new Storyboard();
        private DoubleAnimation VerticalBlur_Simple = new DoubleAnimation();
        private ThicknessAnimation HorizontalBlur_Simple = new ThicknessAnimation();


        
        private void Init_Animations()
        {
            Blackout_Rectangle.Opacity = 0;

            Blackout_Short.BeginTime = new TimeSpan(0);
            Blackout_Short.From = 0;
            Blackout_Short.To = 1;
            Blackout_Short.Duration = TimeSpan.FromSeconds(0.2);
            Blackout_Short.FillBehavior = FillBehavior.Stop;
            SB_Blackout_Short.FillBehavior = FillBehavior.Stop;
            Storyboard.SetTargetProperty(Blackout_Short, new PropertyPath(OpacityProperty));
            SB_Blackout_Short.Children.Add(Blackout_Short);
            Storyboard.SetTarget(Blackout_Short, Blackout_Rectangle);

            
            Blackout.BeginTime = new TimeSpan(0);
            Blackout.FillBehavior = FillBehavior.Stop;
            Blackout.From = 0;
            Blackout.To = 1;
            Blackout.Duration = new Duration(TimeSpan.FromSeconds(0.15));
            Blackout.Completed += delegate (object sender, EventArgs e)
            {
                Blackout_Rectangle.Opacity = 1;
                blackouted = true;
            };
            Storyboard.SetTargetProperty(Blackout, new PropertyPath(OpacityProperty));
            SB_Blackout.Children.Add(Blackout);
            Storyboard.SetTarget(Blackout, Blackout_Rectangle);
            SB_Blackout.FillBehavior = FillBehavior.Stop;

            VerticalBlur_Simple.BeginTime = new TimeSpan(0);
            Storyboard.SetTargetProperty(VerticalBlur_Simple, new PropertyPath(HeightProperty));
            
            //CODE BELOW FOR SOME REASON STOPS THE THREAD WITHOUT THROWING AN EXCEPTION
            //VerticalBlur_Simple.From = image.Height + 20; 
            //VerticalBlur_Simple.To = image.Height;
            //VerticalBlur_Simple.To = image.Margin;
            //VerticalBlur_Simple.From = new Thickness(image.Margin.Left, image.Margin.Top + 25, image.Margin.Right, image.Margin.Bottom + 25);
            VerticalBlur_Simple.Duration = TimeSpan.FromSeconds(1);
            SB.Children.Add(VerticalBlur_Simple);
            Storyboard.SetTarget(VerticalBlur_Simple, image);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ChangedButton == MouseButton.Left) timeline_noblur();
            if (e.ChangedButton == MouseButton.Right)
            {
                if (set.IsVisible) set.Hide();
                else set.Show();
            }
        }

        //
        //Key Controls
        //

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (muted) toggle_mute();
            if (e.Delta < 0 & Player.Volume != 0)
            {
                Player.Volume -= 10;
                volume_label.Content = Player.Volume;
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
            }
            if (e.Delta > 0 & Player.Volume != 100)
            {
                if (muted) toggle_mute();
                Player.Volume += 10;
                volume_label.Content = Player.Volume;
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.M:
                    toggle_mute();
                    break;
                case Key.Up:
                    next_song();
                    break;
                case Key.Down:
                    prev_song();
                    break;
                case Key.Right:
                    next_image();
                    break;
                case Key.Left:
                    prev_image();
                    break;
                case Key.Space:
                    timeline_noblur();
                    break;
            }
        }

        private void next_song()
        {
            if (enabled_songs.Count > 1)
            {
                if (songs_listbox.SelectedIndex == enabled_songs.Count - 1) songs_listbox.SelectedIndex = 0;
                else songs_listbox.SelectedIndex += 1;
            }
        }
        private void prev_song()
        {
            if (enabled_songs.Count > 1)
            {
                if (songs_listbox.SelectedIndex <= 0) songs_listbox.SelectedIndex = songs_listbox.Items.Count - 1;
                else songs_listbox.SelectedIndex -= 1;
            }
        }

        private void toggle_mute()
        {
            if (!muted)
            {
                muted_volume = Player.Volume;
                volume_label.Content = 0;
                Player.Volume = 0;
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
                muted = true;
            }
            else
            {
                Player.Volume = muted_volume;
                volume_label.Content = Player.Volume;
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
                muted = false;
            }
        }

        private void next_image()
        {
            if (enabled_images.Count > 1)
            {
                if (current_image_pos == images_listbox.Items.Count - 1) images_listbox.SelectedIndex = 0;
                else images_listbox.SelectedIndex = current_image_pos + 1;
                full_auto_mode = false;
            }
        }
        private void prev_image()
        {
            if (enabled_images.Count > 1)
            {
                if (current_image_pos == 0) images_listbox.SelectedIndex = images_listbox.Items.Count - 1;
                else images_listbox.SelectedIndex = current_image_pos - 1;
                full_auto_mode = false;
            }
        }



        //
        // Timeline Effects Controls
        //

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeLine_Move();
            if (rhythm_pos >= 0)
            {
                Correction = beat_length * rhythm_pos - Player.GetPosOfStream(Player.Stream_L);
                if (Correction > 0 & Correction < beat_length * 1.5) Timer.Interval = TimeSpan.FromSeconds(Correction);
                else //Jumping forward to compensate lag
                {
                    if (rhythm_pos != 0 & Correction < 0) Timer.Interval = TimeSpan.FromMilliseconds(0.0001);
                }
            }
            else
            {
                b_rhythm_pos += 1;
                Correction = buildup_beat_len * (build_rhythm.Length + rhythm_pos) - Player.GetPosOfStream(Player.Stream_B);
                if (Correction > 0) Timer.Interval = TimeSpan.FromSeconds(Correction);
                else Timer.Interval = TimeSpan.FromMilliseconds(0.0001);
            }
        }

        private void TimelineLenghtFill()
        {
            if (timeline_label.Content.ToString().Length < 250)
                timeline_label.Content = timeline_label.Content.ToString() + loop_rhythm;
        }

        private void TimeLine_Move()
        {
            //Message_textBlock.Text = Width + "/" + Height;
            beat(timeline_label.Content.ToString()[2]);
            timeline_label.Content = timeline_label.Content.ToString().Remove(2, 1);
            TimelineLenghtFill();
            rhythm_pos += 1;
            if (rhythm_pos == loop_rhythm.Length) rhythm_pos = 0;
        }

        private void beat(char c)
        {
            if (c != '.') SB.Stop();
            if (Blackout_Rectangle.Opacity != 0 & c != '.')
            {
                SB_Blackout.Stop();
                blackouted = false;
                Blackout_Rectangle.Opacity = 0;
            }
            switch (c)
            {
                case 'o':
                    timeline_o();
                    break;
                case 'x':
                    timeline_x();
                    break;
                case '-':
                    timeline_noblur();
                    break;
                case '‑'://YES THATS A DIFFERENT ONE. Thanks to tylup RP.
                    timeline_noblur();
                    break;
                case ':':
                    timeline_color_change();
                    break;
                case '*':
                    timeline_image_change();
                    break;
                case '|':
                    timeline_blackout_short();
                    break;
                case '+':
                    timeline_blackout();
                    break;
            }
        }

        // Vertical blur (snare)
        private void timeline_x()
        {
            timeline_noblur();
            timeline_blur_vert();
        }
        // Horizontal blur (bass)
        private void timeline_o()
        {
            timeline_noblur();
            timeline_blur_hor();
        }

        // For '-' in the timeline
        private void timeline_noblur()
        {
            timeline_color_change();
            timeline_image_change();
        }
        // '+'
        private void timeline_blackout()
        {
            SB_Blackout.Begin();
        }
        // '¤'
        private void timeline_whiteout()
        {

        }
        // '|'
        private void timeline_blackout_short()
        {
            Blackout_Rectangle.Opacity = 1;
            timeline_noblur();
            ShortBlackoutTimer.Start();
        }
        private void ShortBlackoutTimer_Tick(object sender, EventArgs e)
        {
            Blackout_Rectangle.Opacity = 0;
            ShortBlackoutTimer.Stop();
        }
        // ':'
        public void timeline_color_change()
        {
            int index;
            while (true)
            {
                index = rnd.Next(0, hues.Length - 1);
                if (Background != hues[index].brush) break;
            }
            Background = hues[index].brush;
            color_label.Content = hues[index].name.ToUpper();
        }
        // '*'
        private void timeline_image_change()
        {
            if (full_auto_mode)
            {
                int i;
                if (enabled_images.Count != 0)
                {
                    while (true)
                    {
                        i = rnd.Next(0, enabled_images.Count - 1);
                        if (images_listbox.SelectedIndex != i) break;
                    }
                }
                else i = -1;
                ImageChange(i);
            }
        }


        
        

        // 'X' Vertical blur only
        private void timeline_blur_vert()
        {
            switch (blur_quality)
            {
                case 0:
                    SB.Begin();
                    break;
            }
        }

        // 'O' Vertical blur only
        private void timeline_blur_hor()
        {

        }

        // ')' Trippy cirle in
        private void timeline_circle_in()
        {

        }

        // '(' Trippy cirle out
        private void timeline_circle_out()
        {

        }

        // '~' Fade color
        private void timeline_fade()
        {

        }

        // '=' Fade and change image
        private void timeline_fade_image()
        {

        }

        // 'i' Invert all colours
        private void timeline_invert()
        {

        }

        // 'I' Invert & change image
        private void timeline_invert_w_image()
        {

        }

        // 's' Horizontal slice
        private void timeline_slice_hor()
        {

        }
        // 'S' Horizontal slice & change image
        private void timeline_slice_hor_w_im()
        {

        }
        // 'v' Vertical slice
        private void timeline_slice_ver()
        {

        }
        // 'V' Vertical slice & change image
        private void timeline_slice_ver_w_im()
        {

        }
        // '#' Double slice
        private void timeline_slice_double()
        {

        }
        // '@' Double slice and change image
        private void timeline_scice_double_w_im()
        {

        }


        //
        // Settings Window Position Controls
        //
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                set.Top = Top + (Height / 2) - (set.Height / 2);
                set.Left = Left + (Width / 2) - (set.Width / 2);
            }
            Smart_Stretch();
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            Smart_Stretch();
            if (WindowState == WindowState.Normal)
            {
                set.Top = Top + (Height / 2) - (set.Height / 2);
                set.Left = Left + (Width / 2) - (set.Width / 2);
            }
            if (WindowState == WindowState.Maximized)
            {
                set.Top = (SystemParameters.MaximizedPrimaryScreenHeight / 2) - (set.Height / 2);
                set.Left = (SystemParameters.MaximizedPrimaryScreenWidth / 2) - (set.Width / 2);
            }
        }
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            set.Top = Top + (Height / 2) - (set.Height / 2);
            set.Left = Left + (Width / 2) - (set.Width / 2);
        }
        private void Smart_Stretch()
        {
            if (WindowState == WindowState.Maximized)
            {
                if (SystemParameters.MaximizedPrimaryScreenWidth / SystemParameters.MaximizedPrimaryScreenHeight > image.ActualWidth / image.ActualHeight)
                    image.Stretch = Stretch.Uniform;
                else image.Stretch = Stretch.UniformToFill;
            }
            else
            {
                if (Width / Height > image.ActualWidth / image.ActualHeight) image.Stretch = Stretch.Uniform;
                else image.Stretch = Stretch.UniformToFill;
            }
                
        }



        //
        //
        //


        private void next_song_be_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            next_song();
        }
        private void songs_be_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (songs_listbox.Visibility == Visibility.Hidden)
            {
                songs_listbox.Visibility = Visibility.Visible;
                images_listbox.Visibility = Visibility.Hidden;
            }
            else songs_listbox.Visibility = Visibility.Hidden;
        }

        private void prev_song_be_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            prev_song();
        }

        private void songs_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Blackout_Rectangle.Opacity != 0)
            {
                SB_Blackout.Stop();
                Blackout_Rectangle.Opacity = 0;
            }
            if (songs_listbox.SelectedIndex != -1)
            {
                int i = enabled_songs[songs_listbox.SelectedIndex].Ind;
                Player.loop_mem = RPM.allSongs[i].buffer;
                loop_rhythm = RPM.allSongs[i].rhythm;
                song_label.Content = RPM.allSongs[i].title.ToUpper();
                timeline_label.Content = RPM.allSongs[i].rhythm;
                current_song = songs_listbox.SelectedIndex;

                rhythm_pos = 0;
                b_rhythm_pos = 0;
                if (RPM.allSongs[i].buildup_buffer != null & buildup_enabled)
                {
                    Player.build_mem = RPM.allSongs[i].buildup_buffer;
                    Player.Play_With_Buildup();
                    build_rhythm = RPM.allSongs[i].buildup_rhythm;
                    int expected_size = Convert.ToInt32(Math.Round(Audio.GetTimeOfStream(Player.Stream_B) / (Audio.GetTimeOfStream(Player.Stream_L) / loop_rhythm.Length)));
                    if (build_rhythm == null) //In case there is buildup music without beat string
                    {
                        build_rhythm = new string('.', expected_size);
                    }
                    else if (build_rhythm.Length < expected_size)
                    {
                        build_rhythm += new string('.', expected_size - build_rhythm.Length - 1);
                    }
                    if (timeline_label.Content.ToString().Length < 250) timeline_label.Content = build_rhythm + timeline_label.Content;
                    else timeline_label.Content = build_rhythm;
                    rhythm_pos -= expected_size;
                }
                else Player.Play_Without_Buildup();

                timeline_label.Content = ">>" + timeline_label.Content;

                beat_length = Audio.GetTimeOfStream(Player.Stream_L) / loop_rhythm.Length;
                
                TimelineLenghtFill();
                Timer.Interval = TimeSpan.FromTicks(Convert.ToInt64(beat_length * 1000 * 10000));
                if (RPM.allSongs[i].buildup_buffer != null & buildup_enabled)
                {
                    Timer.Interval = TimeSpan.FromSeconds(buildup_beat_len);
                    buildup_beat_len = Audio.GetTimeOfStream(Player.Stream_B) / build_rhythm.Length;
                }
                ShortBlackoutTimer.Interval = Timer.Interval;

                Player.Play();
                TimeLine_Move();
                Timer.Start();
            }
            else
            {
                if (enabled_songs.Count == 0)
                {
                    StopSong();
                }
            }
        }
        public void StopSong()
        {
            Timer.Stop();
            Player.Stop();
            song_label.Content = "NONE";
            beat_length = 0;
            timeline_label.Content = ">>.";
        }

        private void prev_image_be_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            prev_image();
        }

        private void next_image_be_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            next_image();
        }

        private void full_auto_be_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            full_auto_mode = true;
        }


        private void Images_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (images_listbox.SelectedIndex != -1)
            {
                ImageChange(images_listbox.SelectedIndex);
                full_auto_mode = false;
                images_listbox.SelectedIndex = -1;
            }
        }

        public void ImageChange(int p)
        {
            current_image_pos = p;
            if (p != -1)
            {
                int index = enabled_images[p].Ind;
                image.Source = RPM.allPics[index].pic;
                if (RPM.allPics[index].animation == null)
                {
                    AnimTimer.Stop();
                    character_label.Content = RPM.allPics[index].fullname.ToUpper();
                    switch (RPM.allPics[index].align)
                    {
                        case "left":
                            image.HorizontalAlignment = HorizontalAlignment.Left;
                            break;
                        case "center":
                            image.HorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        case "right":
                            image.HorizontalAlignment = HorizontalAlignment.Right;
                            break;
                    }
                    Smart_Stretch();
                }
                else
                {
                    anim_ind = 1;
                    AnimTimer.Interval = TimeSpan.FromMilliseconds(RPM.allPics[current_image_pos].frameDuration);
                    AnimTimer.Start();
                    switch (RPM.allPics[index].align)
                    {
                        case "left":
                            image.HorizontalAlignment = HorizontalAlignment.Left;
                            break;
                        case "center":
                            image.HorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        case "right":
                            image.HorizontalAlignment = HorizontalAlignment.Right;
                            break;
                    }
                    //Smart_Stretch();
                }
            }
            else
            {
                if (enabled_images.Count == 0)
                {
                    image.Source = null;
                    character_label.Content = "NONE";
                }
            }
        }

        private void image_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            Smart_Stretch();
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            image.Source = RPM.allPics[current_image_pos].animation[anim_ind];
            if (RPM.allPics[current_image_pos].animation.Length == anim_ind + 1) anim_ind = 0;
            else anim_ind++;
        }

        private void Images_be_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (images_listbox.Visibility == Visibility.Hidden)
            {
                images_listbox.Visibility = Visibility.Visible;
                songs_listbox.Visibility = Visibility.Hidden;
            }
            else images_listbox.Visibility = Visibility.Hidden;
        }

        private void Images_listbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void Songs_listbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        
    }
}

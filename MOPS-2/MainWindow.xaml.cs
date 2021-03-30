using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
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
using System.Globalization;

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
        

        //Stopwatch TMP_tester = new Stopwatch();
        
        //public Timer TTimer = new Timer(new TimerCallback);
        public DispatcherTimer Timer = new DispatcherTimer(DispatcherPriority.Send);
        DispatcherTimer AnimTimer = new DispatcherTimer(DispatcherPriority.Render);
        DispatcherTimer ShortBlackoutTimer = new DispatcherTimer(DispatcherPriority.Render);
        double Correction;
        public double beat_length = 0;

        public Label[] allLabels;
        public TextBlock[] allTextBlocks;

        Shaders.InvertColorEffect invertColorEffect = new Shaders.InvertColorEffect();
        public Shaders.ColorBlend_HardLightEffect HardLightEffect = new Shaders.ColorBlend_HardLightEffect();
        Shaders.HuesYBlur8Effect YBlur8 = new Shaders.HuesYBlur8Effect();
        Shaders.HuesXBlur8Effect XBlur8 = new Shaders.HuesXBlur8Effect();

        public Hues.Palette[] hues;

        public int CurrentColorInd = 0;
        public double MainImageOpacity;

        public static Settings set = new Settings();
        public RPManager RPM = new RPManager();

        public bool muted = false;
        public int muted_volume;
        public bool Colors_Inverted = false;

        public bool full_auto_mode = true;

        /// <summary>
        /// Quality of blur, from 0 to 3. Zero for stretching a single image, 1-3 for moving copies of image to the center.
        /// </summary>
        public int blur_quality = 1;
        /// <summary>
        /// Duration of blur animation. From 0 to 3. [appr. from 1s to 0.3s]
        /// </summary>
        public double blur_decay = 0.15;
        /// <summary>
        /// How far away blur goes in WPF dots.
        /// </summary>
        public int blur_Amount = 25;
        public bool blackouted = false;

        public int current_song = 0;
        public int current_image_pos = 55;
        private int anim_ind = 0;

        private string loop_rhythm;
        private string build_rhythm = "";
        public int rhythm_pos = 1;
        public int b_rhythm_pos = 1;

        /// <summary>
        /// List of enabled songs displayed in songs_listbox.
        /// </summary>
        public static ObservableCollection<rdata> enabled_songs = new ObservableCollection<rdata>();
        /// <summary>
        /// List of enabled images displayed in images_listbox.
        /// </summary>
        public static ObservableCollection<rdata> enabled_images = new ObservableCollection<rdata>();

        private BackgroundWorker backgroundLoader;
        public MainWindow()
        {
            InitializeComponent();

            Timer.Tick += new EventHandler(Timer_Tick);
            AnimTimer.Tick += new EventHandler(AnimTimer_Tick);
            ShortBlackoutTimer.Tick += new EventHandler(ShortBlackoutTimer_Tick);

            Player.SetReference(this);
            set.SetReference(this);

            switch ((ColorSet)Properties.Settings.Default.colorSet)
            {
                case ColorSet.Normal:
                    hues = Hues.hues_normal;
                    break;
                case ColorSet.Pastel:
                    hues = Hues.hues_pastel;
                    break;
                case ColorSet.Weed:
                    hues = Hues.hues_weed;
                    break;
            }
            if (Properties.Settings.Default.blendMode == 0) MainImageOpacity = 1;
            else MainImageOpacity = 0.7;

            backgroundLoader = new BackgroundWorker();
            backgroundLoader.DoWork +=
                new DoWorkEventHandler(load_dowork);
            backgroundLoader.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(load_completed);
            //For Debug
            //CornerBlock.Foreground = Brushes.Red;
            //timeline_label.Foreground = Brushes.Red;
        }

        private void load_dowork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            e.Result = RPM.SupremeReader((string)e.Argument, worker, e);
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
            InfoBlock.Cursor = Cursors.Arrow;
            InfoBlock.Text = "Loaded";

            CornerBlock.Visibility = Visibility.Hidden;
            InfoBlock.Visibility = Visibility.Hidden;

            GC.Collect();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            set.Owner = this;
            Init_Animations();      
            set.ColorBlend_UI_Update();

            timeline_color_change();
        }

        private void First_Load(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Wait;
            InfoBlock.Text = "Initializing...";
            InfoBlock.IsEnabled = false;
            InfoBlock.Cursor = Cursors.Wait;
            backgroundLoader.RunWorkerAsync("Packs/Defaults_v5.0.zip");
        }

        private Storyboard BlurAnimSB = new Storyboard();
        private DoubleAnimation BlurAnim = new DoubleAnimation();

        private Storyboard SB_Blackout = new Storyboard();
        private DoubleAnimation Blackout = new DoubleAnimation();
        private Storyboard SB_Blackout_Short = new Storyboard();
        private DoubleAnimation Blackout_Short = new DoubleAnimation();
        private ThicknessAnimation Blackout_Blur = new ThicknessAnimation();
        public ColorAnimation Fade = new ColorAnimation();

        private Storyboard SB_Fade = new Storyboard();

        private void Init_Animations()
        {
            BlurAnim.From = 0.02;
            BlurAnim.To = 0;
            BlurAnim.Duration = TimeSpan.FromSeconds(0.15);
            Storyboard.SetTargetProperty(BlurAnim, new PropertyPath("Effect.BlurAmount"));
            Storyboard.SetTarget(BlurAnim, image0);
            BlurAnimSB.Children.Add(BlurAnim);
            BlurAnimSB.FillBehavior = FillBehavior.Stop;

            Fade.FillBehavior = FillBehavior.HoldEnd;
            Fade.BeginTime = TimeSpan.FromSeconds(0);

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

            Storyboard.SetTargetProperty(Fade, new PropertyPath("Effect.Blend"));
            Storyboard.SetTarget(Fade, ImageGrid);
            SB_Fade.Children.Add(Fade);
            SB_Fade.FillBehavior = FillBehavior.Stop;
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
                    timeline_pic_and_color();
                    break;
                case Key.N:
                    timeline_invert();
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
            if (rhythm_pos >= 0)
            {
                if (rhythm_pos != 1) Correction = beat_length * (rhythm_pos - 1) - Player.GetPosOfStream(Player.Stream_L);
                if (Correction > 0) Timer.Interval = TimeSpan.FromSeconds(Correction);
                else if (rhythm_pos > 2) Timer.Interval = TimeSpan.FromTicks(10);
            }
            else
            {
                b_rhythm_pos += 1;
                Correction = beat_length * (build_rhythm.Length + rhythm_pos) - Player.GetPosOfStream(Player.Stream_B);
                if (Correction > 0) Timer.Interval = TimeSpan.FromSeconds(Correction);
                else Timer.Interval = TimeSpan.FromTicks(10);
            }
            TimeLine_Move(); //THIS MUST BE _AFTER_ THE TIMER.INTERVAL IS CORRECTED
            //CornerBlock.Text = rhythm_pos.ToString();
        }
        /// <summary> Check if displayed rhythm is too short and fills it if neccessary </summary>
        private void TimelineLenghtFill()
        {
            if (timeline_label.Content.ToString().Length < 250)
                timeline_label.Content = timeline_label.Content = string.Concat(timeline_label.Content.ToString(), loop_rhythm);
        }

        private void TimeLine_Move()
        {
            //CornerBlock.Text = rhythm_pos.ToString();
            beat(timeline_label.Content.ToString()[2]);
            timeline_label.Content = timeline_label.Content.ToString().Remove(2, 1);
            TimelineLenghtFill();
            rhythm_pos += 1;
            if (rhythm_pos == loop_rhythm.Length) rhythm_pos = 0;
        }
        /// <summary> Plays the event according to char </summary>
        private void beat(char c)
        {
            if (Blackout_Rectangle.Opacity != 0 & c != '.')
            {
                SB_Blackout.Stop();
                blackouted = false;
                Blackout_Rectangle.Opacity = 0;
            }
            if (c != '.') switch (c)
                {
                    case 'o':
                        timeline_o();
                        break;
                    case 'O':
                        timeline_blur_hor();
                        break;
                    case 'x':
                        timeline_x();
                        break;
                    case 'X':
                        timeline_blur_vert();
                        break;
                    case '-':
                        timeline_pic_and_color();
                        break;
                    case '‑'://YES THATS A DIFFERENT ONE. Thanks to tylup RP.
                        timeline_pic_and_color();
                        break;
                    case '~':
                        timeline_fade();
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
                    case 'i':
                        timeline_invert();
                        break;
                    case 'I':
                        timeline_invert_w_image();
                        break;
                    case '=':
                        timeline_fade_image();
                        break;
                }
        }

        #region Timeline events

        // Vertical blur (snare)
        private void timeline_x()
        {
            timeline_pic_and_color();
            timeline_blur_vert();
        }
        // Horizontal blur (bass)
        private void timeline_o()
        {
            timeline_pic_and_color();
            timeline_blur_hor();
        }

        // For '-' in the timeline
        private void timeline_pic_and_color()
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
            timeline_pic_and_color();
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
            GetRandomHue();
            
            ColorOverlap_Rectangle.Fill = hues[CurrentColorInd].brush;
            HardLightEffect.Blend = Color.FromArgb(179, hues[CurrentColorInd].brush.Color.R, hues[CurrentColorInd].brush.Color.G, hues[CurrentColorInd].brush.Color.B);
            color_label.Content = hues[CurrentColorInd].name.ToUpper(); 
        }
        // '*'
        private void timeline_image_change()
        {
            if (full_auto_mode & enabled_images.Count != 1)
            {
                if (enabled_images.Count != 0)
                {
                    ImageChange((current_image_pos + rnd.Next(1, enabled_images.Count - 1)) % enabled_images.Count);
                }
                else ImageChange(-1);
            }
        }


        
        

        // 'X' Vertical blur only
        private void timeline_blur_vert()
        {
            if (enabled_images.Count != 0) switch (blur_quality)
                {
                    case 1:
                        //foreach (Image img in blur_imgset_v8) img.Visibility = Visibility.Visible;
                        //image0.Opacity = MainImageOpacity / 2;
                        image0.Effect = YBlur8;
                        BlurAnimSB.Begin();
                        break;
                }
        }

        // 'O' Vertical blur only
        private void timeline_blur_hor()
        {
            if (enabled_images.Count != 0) switch (blur_quality)
                {
                    case 1:
                        //foreach (Image img in blur_imgset_v8) img.Visibility = Visibility.Visible;
                        //image0.Opacity = MainImageOpacity / 2;
                        image0.Effect = XBlur8;
                        BlurAnimSB.Begin();
                        break;
                }
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
            Fade.Duration = TimeSpan.FromSeconds((CountDots() + 1) * beat_length);
            if ((BlendMode)Properties.Settings.Default.blendMode == BlendMode.HardLight)
            {
                Fade.From = HardLightEffect.Blend;
                Fade.To = Color.FromArgb(179, ((SolidColorBrush)GetRandomHue().brush).Color.R, ((SolidColorBrush)GetRandomHue().brush).Color.G, ((SolidColorBrush)GetRandomHue().brush).Color.B);
                SB_Fade.Begin();
            }
            else
            {
                Fade.From = ((SolidColorBrush)ColorOverlap_Rectangle.Fill).Color;
                Fade.To = ((SolidColorBrush)GetRandomHue().brush).Color;
            }
            color_label.Content = hues[CurrentColorInd].name.ToUpper();
            ColorOverlap_Rectangle.Fill.BeginAnimation(SolidColorBrush.ColorProperty, Fade);
        }

        // '=' Fade and change image
        private void timeline_fade_image()
        {
            timeline_image_change();
            timeline_fade();
        }

        /// <summary> 'i' - inverts colors of the window </summary>
        private void timeline_invert()
        {
            if (Colors_Inverted)
            {
                Effect = null;

                Colors_Inverted = false;
            }
            else
            {
                Effect = invertColorEffect;

                Colors_Inverted = true;
            }
        }

        // 'I' Invert & change image
        private void timeline_invert_w_image()
        {
            timeline_pic_and_color();
            timeline_invert();
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

        #endregion

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
            if (RPM.allPics.Length != 0) Smart_Stretch();
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
                if (SystemParameters.MaximizedPrimaryScreenWidth / SystemParameters.MaximizedPrimaryScreenHeight > RPM.allPics[current_image_pos].pic.Width / RPM.allPics[current_image_pos].pic.Height)
                    image0.Stretch = Stretch.Uniform;
                else image0.Stretch = Stretch.UniformToFill;
            }
            else
            {
                if (Width / Height > RPM.allPics[current_image_pos].pic.Width / RPM.allPics[current_image_pos].pic.Height) image0.Stretch = Stretch.Uniform;
                else image0.Stretch = Stretch.UniformToFill;
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
            if (Colors_Inverted)
            {
                timeline_invert();
            }
            if (Blackout_Rectangle.Opacity != 0)
            {
                SB_Blackout.Stop();
                Blackout_Rectangle.Opacity = 0;
            }
            if (songs_listbox.SelectedIndex != -1)
            {
                int i = enabled_songs[songs_listbox.SelectedIndex].Ind;
                //Player.loop_mem = RPM.allSongs[i].buffer; //OOD
                Player.loop_mem = RPM.GetAudioFromZip(RPM.ResPacks[RPM.Get_rp_of_song(i)].path, RPM.allSongs[i].filename);
                if (Player.loop_mem.Length != 0)
                {
                    loop_rhythm = RPM.allSongs[i].rhythm;
                    song_label.Content = RPM.allSongs[i].title.ToUpper();
                    timeline_label.Content = RPM.allSongs[i].rhythm;
                    current_song = songs_listbox.SelectedIndex;

                    rhythm_pos = 1;
                    b_rhythm_pos = 1;
                    if (RPM.allSongs[i].buildup_filename != null & ( (BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.On | ( (BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.Once & !RPM.allSongs[i].buildup_played)))
                    {
                        if ((BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.Once) RPM.allSongs[i].buildup_played = true;
                        //Player.build_mem = RPM.allSongs[i].buildup_buffer; //OOD
                        Player.build_mem = RPM.GetAudioFromZip(RPM.ResPacks[RPM.Get_rp_of_song(i)].path, RPM.allSongs[i].buildup_filename);
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
                        if (timeline_label.Content.ToString().Length < 250) timeline_label.Content = string.Concat(build_rhythm, timeline_label.Content);
                        else timeline_label.Content = build_rhythm;
                        rhythm_pos = -expected_size;
                    }
                    else Player.Play_Without_Buildup();

                    timeline_label.Content = ">>" + timeline_label.Content;

                    beat_length = Audio.GetTimeOfStream(Player.Stream_L) / loop_rhythm.Length;

                    TimelineLenghtFill();
                    //Timer.Interval = TimeSpan.FromTicks(Convert.ToInt64(beat_length * 1000 * 10000));
                    Timer.Interval = TimeSpan.FromSeconds(beat_length);

                    ShortBlackoutTimer.Interval = Timer.Interval;

                    Player.Play();
                    TimeLine_Move();
                    Timer.Start();
                }
                else StopSong();
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
            BlurAnimSB.Stop();
            image0.Opacity = MainImageOpacity;
            current_image_pos = p;
            if (p != -1)
            {
                int index = enabled_images[p].Ind;
                image0.Source = RPM.allPics[index].pic;

                ////For debug:
                //CornerBlock.Text = index + ": " + RPM.allPics[index].pic.Format.ToString();

                if (RPM.allPics[index].animation == null)
                {
                    AnimTimer.Stop();
                    character_label.Content = RPM.allPics[index].fullname.ToUpper();
                    switch (RPM.allPics[index].align)
                    {
                        case "left":
                            image0.HorizontalAlignment = HorizontalAlignment.Left;
                            break;
                        case "center":
                            image0.HorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        case "right":
                            image0.HorizontalAlignment = HorizontalAlignment.Right;
                            break;
                    }
                    Smart_Stretch();
                }
                else
                {
                    anim_ind = 1;
                    AnimTimer.Interval = TimeSpan.FromMilliseconds(RPM.allPics[current_image_pos].frameDuration);
                    AnimTimer.Start();
                    ////For debug:
                    //CornerBlock.Text = index + ": " + RPM.allPics[index].animation[0].Format.ToString();

                    switch (RPM.allPics[index].align)
                    {
                        case "left":
                            image0.HorizontalAlignment = HorizontalAlignment.Left;
                            break;
                        case "center":
                            image0.HorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        case "right":
                            image0.HorizontalAlignment = HorizontalAlignment.Right;
                            break;
                    }
                    //Smart_Stretch();
                }
            }
            else
            {
                if (enabled_images.Count == 0)
                {
                    image0.Source = null;
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
            image0.Source = RPM.allPics[current_image_pos].animation[anim_ind];
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

        private Hues.Palette GetRandomHue()
        {
            CurrentColorInd = (CurrentColorInd + rnd.Next(1, hues.Length - 2)) % hues.Length;
            return hues[CurrentColorInd];
        }

        private int CountDots()
        {
            int Count = 0;
            int limit = build_rhythm.Length + (loop_rhythm.Length * 3);
            for (int i = 3; i < limit; i++)
            {
                if (timeline_label.Content.ToString()[i] != '.') break;
                else
                {
                    Count++;
                    if (timeline_label.Content.ToString().Length - 1 == i) timeline_label.Content = timeline_label.Content.ToString() + loop_rhythm;
                    if (i == limit - 1)
                    {
                        Count = 0;
                        break;
                    }
                }
            }
            return Count;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
        
    }
}
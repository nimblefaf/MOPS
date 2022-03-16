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
using DiscordRPC;

namespace MOPS
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rnd = new Random();

        internal Core Core = new Core();
        
        DispatcherTimer AnimTimer = new DispatcherTimer(DispatcherPriority.Render);
        public DispatcherTimer ShortBlackoutTimer = new DispatcherTimer(DispatcherPriority.Render);

        Shaders.InvertColorEffect invertColorEffect = new Shaders.InvertColorEffect();
        public Shaders.ColorBlend_HardLightEffect HardLightEffect = new Shaders.ColorBlend_HardLightEffect();
        Shaders.HuesYBlur8Effect YBlur8 = new Shaders.HuesYBlur8Effect();
        Shaders.HuesXBlur8Effect XBlur8 = new Shaders.HuesXBlur8Effect();
        Shaders.HuesYBlur14Effect YBlur14 = new Shaders.HuesYBlur14Effect();
        Shaders.HuesXBlur14Effect XBlur14 = new Shaders.HuesXBlur14Effect();
        Shaders.HuesYBlur26Effect YBlur26 = new Shaders.HuesYBlur26Effect();
        Shaders.HuesXBlur26Effect XBlur26 = new Shaders.HuesXBlur26Effect();

        public Hues.Palette[] hues;

        public int CurrentColorInd = 0;

        public RPManager RPM = new RPManager();


        public bool Colors_Inverted = false;

        public bool full_auto_mode = true;

        /// <summary>
        /// How far away blur goes in WPF dots.
        /// </summary>
        public double blur_amount = 0.02;
        public bool blackouted = false;

        public int current_image_pos = 0;
        private int anim_ind = 0;

        /// <summary>
        /// List of enabled songs displayed in songs_listbox.
        /// </summary>
        public static ObservableCollection<rdata> enabled_songs = new ObservableCollection<rdata>();
        /// <summary>
        /// List of enabled images displayed in images_listbox.
        /// </summary>
        public static ObservableCollection<rdata> enabled_images = new ObservableCollection<rdata>();

        public MainWindow()
        {
            InitializeComponent();

            AnimTimer.Tick += new EventHandler(AnimTimer_Tick);
            ShortBlackoutTimer.Tick += new EventHandler(ShortBlackoutTimer_Tick);

            Core.SetReference(this);
            PreloaderWin.SetReference(this);
            InnerWin.SetReference(this);
            Display_Alpha.SetReference(this);
            

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

            songs_listbox.ItemsSource = enabled_songs;
            images_listbox.ItemsSource = enabled_images;

            //For Debug
            //CornerBlock.Foreground = Brushes.Red;
            //timeline_label.Foreground = Brushes.Red;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //set.Owner = this; //that was for window
            Init_Animations();
            ColorBlend_Graphics_Update();
            BlurDecay_Upd();
            BlurAmount_Upd();

            timeline_color_change();
        }

        private Storyboard BlurAnimSB = new Storyboard();
        private DoubleAnimation BlurAnim = new DoubleAnimation();

        public Storyboard SB_Blackout = new Storyboard();
        private DoubleAnimation Blackout = new DoubleAnimation();
        private Storyboard SB_Blackout_Short = new Storyboard();
        private DoubleAnimation Blackout_Short = new DoubleAnimation();
        private DoubleAnimation Blackout_Blur = new DoubleAnimation();
        public ColorAnimation Fade = new ColorAnimation();

        private Storyboard SB_Fade = new Storyboard();

        private void Init_Animations()
        {
            BlurAnim.From = 0.02;
            BlurAnim.To = 0;
            BlurAnim.Duration = TimeSpan.FromSeconds(0.5);
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
            Blackout.Duration = TimeSpan.FromSeconds(0.15);
            Blackout.Completed += delegate (object sender, EventArgs e)
            {
                Blackout_Rectangle.Opacity = 1;
                blackouted = true;
            };
            Storyboard.SetTargetProperty(Blackout, new PropertyPath(OpacityProperty));

            Blackout_Blur.BeginTime = new TimeSpan(0);
            Blackout_Blur.FillBehavior = FillBehavior.Stop;
            Blackout_Blur.From = 0;
            Blackout_Blur.To = 0.02;
            Blackout_Blur.Duration = TimeSpan.FromSeconds(0.15);
            Storyboard.SetTargetProperty(Blackout_Blur, new PropertyPath("Effect.BlurAmount"));
            SB_Blackout.Children.Add(Blackout);
            SB_Blackout.Children.Add(Blackout_Blur);
            Storyboard.SetTarget(Blackout, Blackout_Rectangle);
            Storyboard.SetTarget(Blackout_Blur, image0);
            SB_Blackout.FillBehavior = FillBehavior.Stop;

            Storyboard.SetTargetProperty(Fade, new PropertyPath("Effect.Blend"));
            Storyboard.SetTarget(Fade, ImageGrid);
            SB_Fade.Children.Add(Fade);
            SB_Fade.FillBehavior = FillBehavior.Stop;
        }

        public void ColorBlend_Graphics_Update()
        {
            switch ((BlendMode)Properties.Settings.Default.blendMode)
            {
                case BlendMode.Plain:
                    image0.Opacity = 1;
                    ColorOverlap_Rectangle.Visibility = Visibility.Visible;
                    ImageGrid.Effect = null;
                    break;
                case BlendMode.Alpha:
                    image0.Opacity = 0.7;
                    ColorOverlap_Rectangle.Visibility = Visibility.Visible;
                    ImageGrid.Effect = null;
                    break;
                case BlendMode.HardLight:
                    image0.Opacity = 1;
                    ColorOverlap_Rectangle.Visibility = Visibility.Hidden;
                    ImageGrid.Effect = HardLightEffect;
                    Storyboard.SetTargetProperty(Fade, new PropertyPath("Effect.Blend"));
                    break;
            }
        }
        public void BlurDecay_Upd()
        {
            switch ((BlurDecay)Properties.Settings.Default.blurDecay)
            {
                case BlurDecay.Slow:
                    BlurAnim.Duration = TimeSpan.FromSeconds(0.35);
                    break;
                case BlurDecay.Medium:
                    BlurAnim.Duration = TimeSpan.FromSeconds(0.25);
                    break;
                case BlurDecay.Fast:
                    BlurAnim.Duration = TimeSpan.FromSeconds(0.15);
                    break;
                case BlurDecay.Fastest:
                    BlurAnim.Duration = TimeSpan.FromSeconds(0.10);
                    break;
            }

        }
        public void BlurAmount_Upd()
        {
            switch ((BlurAmount)Properties.Settings.Default.blurAmount)
            {
                case BlurAmount.Low:
                    BlurAnim.From = 0.005 / (Properties.Settings.Default.blurQuality+1);
                    break;
                case BlurAmount.Medium:
                    BlurAnim.From = 0.01 / (Properties.Settings.Default.blurQuality + 1);
                    break;
                case BlurAmount.High:
                    BlurAnim.From = 0.05 / (Properties.Settings.Default.blurQuality + 1);
                    break;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ChangedButton == MouseButton.Left) timeline_noblur();
            if (e.ChangedButton == MouseButton.Right)
            {
                if (InnerWin.IsVisible) InnerWin.Visibility = Visibility.Hidden;
                else InnerWin.Visibility = Visibility.Visible;
            }
        }

        #region KeyControls

        //
        //Key Controls
        //

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Core.ChangeVolume(e.Delta);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.M:
                    Core.toggle_mute();
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

        public void next_song()
        {
            if (enabled_songs.Count > 1)
            {
                if (songs_listbox.SelectedIndex == enabled_songs.Count - 1) songs_listbox.SelectedIndex = 0;
                else songs_listbox.SelectedIndex += 1;
            }
        }
        public void prev_song()
        {
            if (enabled_songs.Count > 1)
            {
                if (songs_listbox.SelectedIndex <= 0) songs_listbox.SelectedIndex = songs_listbox.Items.Count - 1;
                else songs_listbox.SelectedIndex -= 1;
            }
        }

        

        public void next_image()
        {
            if (enabled_images.Count > 1)
            {
                if (current_image_pos == images_listbox.Items.Count - 1) images_listbox.SelectedIndex = 0;
                else images_listbox.SelectedIndex = current_image_pos + 1;
                full_auto_mode = false;
            }
        }
        public void prev_image()
        {
            if (enabled_images.Count > 1)
            {
                if (current_image_pos == 0) images_listbox.SelectedIndex = images_listbox.Items.Count - 1;
                else images_listbox.SelectedIndex = current_image_pos - 1;
                full_auto_mode = false;
            }
        }

        #endregion

        

        #region Timeline events

        // Vertical blur (snare)
        public void timeline_x()
        {
            timeline_pic_and_color();
            timeline_blur_vert();
        }
        // Horizontal blur (bass)
        public void timeline_o()
        {
            timeline_pic_and_color();
            timeline_blur_hor();
        }

        // For '-' in the timeline
        public void timeline_pic_and_color()
        {
            timeline_color_change();
            timeline_image_change();
        }
        // '+'
        public void timeline_blackout()
        {
            Blackout_Rectangle.Fill = Brushes.Black;
            image0.Effect = XBlur8;
            SB_Blackout.Begin();
        }
        // '¤'
        public void timeline_whiteout()
        {
            Blackout_Rectangle.Fill = Brushes.White;
            image0.Effect = XBlur8;
            SB_Blackout.Begin();
        }
        // '|'
        public void timeline_blackout_short()
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
            Display_Alpha.color_textBlock.Text = hues[CurrentColorInd].name.ToUpper(); 
        }
        // '*'
        public void timeline_image_change()
        {
            if (full_auto_mode & enabled_images.Count != 1)
            {
                if (enabled_images.Count != 0)
                {
                    if (Properties.Settings.Default.shuffleImages) ImageChange((current_image_pos + rnd.Next(1, enabled_images.Count - 1)) % enabled_images.Count);
                    else ImageChange(++current_image_pos % enabled_images.Count);
                }
                else ImageChange(-1);
            }
        }





        // 'X' Vertical blur only
        public void timeline_blur_vert()
        {
            if (enabled_images.Count != 0) switch ((BlurQuality)Properties.Settings.Default.blurQuality)
                {
                    case BlurQuality.Low:
                        image0.Effect = YBlur8;
                        break;
                    case BlurQuality.Medium:
                        image0.Effect = YBlur14;
                        break;
                    case BlurQuality.High:
                        image0.Effect = YBlur26;
                        break;
                }
            BlurAnimSB.Begin();
        }

        // 'O' Vertical blur only
        public void timeline_blur_hor()
        {
            if (enabled_images.Count != 0) switch ((BlurQuality)Properties.Settings.Default.blurQuality)
                {
                    case BlurQuality.Low:
                        image0.Effect = XBlur8;
                        break;
                    case BlurQuality.Medium:
                        image0.Effect = XBlur14;
                        break;
                    case BlurQuality.High:
                        image0.Effect = XBlur26;
                        break;
                }
            BlurAnimSB.Begin();
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
        public void timeline_fade()
        {
            Fade.Duration = TimeSpan.FromSeconds((Core.CountDots() + 1) * Core.beat_length);
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
            Display_Alpha.color_textBlock.Text = hues[CurrentColorInd].name.ToUpper();
            ColorOverlap_Rectangle.Fill.BeginAnimation(SolidColorBrush.ColorProperty, Fade);
        }

        // '=' Fade and change image
        public void timeline_fade_image()
        {
            timeline_image_change();
            timeline_fade();
        }

        /// <summary> 'i' - inverts colors of the window </summary>
        public void timeline_invert()
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
        public void timeline_invert_w_image()
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


        //tied to the songs_listbox on SelectionChanged
        private void Change_Song(object sender, SelectionChangedEventArgs e)
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
                Core.Change_Song(i);
            }
            else
            {
                if (enabled_songs.Count == 0)
                {
                    Core.StopSong();
                }
            }
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
                    Display_Alpha.character_textBlock.Text = RPM.allPics[index].fullname.ToUpper();
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
                    AnimTimer.Interval = TimeSpan.FromMilliseconds(RPM.allPics[current_image_pos].frameDuration[0]);
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
                    Smart_Stretch();
                }
            }
            else
            {
                if (enabled_images.Count == 0)
                {
                    image0.Source = null;
                    Display_Alpha.character_textBlock.Text = "NONE";
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



        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            Core.discordRpcClient.Dispose();
        }
        
    }

    /// <summary>
    ///     The selected hue blend mode for drawing the image.
    /// </summary>
    public enum BlendMode
    {
        /// <summary>
        /// Image is alpha-blended over the hue.
        /// </summary>
        Plain = 0,
        /// <summary>
        /// Image is alpha-blended over the hue at 70% opacity.
        /// </summary>
        Alpha = 1,
        /// <summary>
        /// Image is alpha-blended over a white background.The hue is blended over the image with "hard light" mode at 70% opacity.
        /// </summary>
        HardLight = 2
    }
    public enum BuildUpMode
    {
        Off = 0,
        On = 1,
        Once = 2
    }
    public enum ColorSet
    {
        Normal = 0,
        Pastel = 1,
        Weed = 2
    }
    public enum BlurAmount
    {
        Low = 0,
        Medium = 1,
        High = 2
    }
    public enum BlurDecay
    {
        Slow = 0,
        Medium = 1,
        Fast = 2,
        Fastest = 3
    }
    public enum BlurQuality
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }

    /// <summary>
    /// Template for the contents of images&songs listboxes
    /// </summary>
    public class rdata
    {
        public string Name { get; set; }
        public int Ind { get; set; }
    }
}
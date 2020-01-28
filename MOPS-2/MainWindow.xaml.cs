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
using System.Threading;
using System.IO;
using Un4seen.Bass;
using System.Drawing;


namespace MOPS_2
{
    public class rdata
    {
        public string name { get; set; }
        public int ind { get; set; }
    }
    
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        Random rnd = new Random();

        public static Palette[] hues =
        {
            new Palette("Black", (SolidColorBrush)new BrushConverter().ConvertFromString("#000000")),
            new Palette("Brick", (SolidColorBrush)new BrushConverter().ConvertFromString("#550000")),
            new Palette("Crimson", (SolidColorBrush)new BrushConverter().ConvertFromString("#aa0000")),
            new Palette("Red", (SolidColorBrush)new BrushConverter().ConvertFromString("#ff0000")),
            new Palette("Turtle", (SolidColorBrush)new BrushConverter().ConvertFromString("#005500")),
            new Palette("Sludge", (SolidColorBrush)new BrushConverter().ConvertFromString("#555500")),
            new Palette("Brown", (SolidColorBrush)new BrushConverter().ConvertFromString("#aa5500")),
            new Palette("Orange", (SolidColorBrush)new BrushConverter().ConvertFromString("#ff5500")),
            new Palette("Green", (SolidColorBrush)new BrushConverter().ConvertFromString("#00aa00")),
            new Palette("Grass", (SolidColorBrush)new BrushConverter().ConvertFromString("#55aa00")),
            new Palette("Maize", (SolidColorBrush)new BrushConverter().ConvertFromString("#aaaa00")),
            new Palette("Citrus", (SolidColorBrush)new BrushConverter().ConvertFromString("#ffaa00")),
            new Palette("Lime", (SolidColorBrush)new BrushConverter().ConvertFromString("#00ff00")),
            new Palette("Leaf", (SolidColorBrush)new BrushConverter().ConvertFromString("#55ff00")),
            new Palette("Chartreuse", (SolidColorBrush)new BrushConverter().ConvertFromString("#aaff00")),
            new Palette("Yellow", (SolidColorBrush)new BrushConverter().ConvertFromString("#ffff00")),
            new Palette("Midnight", (SolidColorBrush)new BrushConverter().ConvertFromString("#000055")),
            new Palette("Plum", (SolidColorBrush)new BrushConverter().ConvertFromString("#550055")),
            new Palette("Pomegranate", (SolidColorBrush)new BrushConverter().ConvertFromString("#aa0055")),
            new Palette("Rose", (SolidColorBrush)new BrushConverter().ConvertFromString("#ff0055")),
            new Palette("Swamp", (SolidColorBrush)new BrushConverter().ConvertFromString("#005555")),
            new Palette("Dust", (SolidColorBrush)new BrushConverter().ConvertFromString("#555555")),
            new Palette("Dirt", (SolidColorBrush)new BrushConverter().ConvertFromString("#aa5555")),
            new Palette("Blossom", (SolidColorBrush)new BrushConverter().ConvertFromString("#ff5555")),
            new Palette("Sea", (SolidColorBrush)new BrushConverter().ConvertFromString("#00aa55")),
            new Palette("Ill", (SolidColorBrush)new BrushConverter().ConvertFromString("#55aa55")),
            new Palette("Haze", (SolidColorBrush)new BrushConverter().ConvertFromString("#aaaa55")),
            new Palette("Peach", (SolidColorBrush)new BrushConverter().ConvertFromString("#ffaa55")),
            new Palette("Spring", (SolidColorBrush)new BrushConverter().ConvertFromString("#00ff55")),
            new Palette("Mantis", (SolidColorBrush)new BrushConverter().ConvertFromString("#55ff55")),
            new Palette("Brilliant", (SolidColorBrush)new BrushConverter().ConvertFromString("#aaff55")),
            new Palette("Canary", (SolidColorBrush)new BrushConverter().ConvertFromString("#ffff55")),
            new Palette("Navy", (SolidColorBrush)new BrushConverter().ConvertFromString("#0000aa")),
            new Palette("Grape", (SolidColorBrush)new BrushConverter().ConvertFromString("#5500aa")),
            new Palette("Mauve", (SolidColorBrush)new BrushConverter().ConvertFromString("#aa00aa")),
            new Palette("Purple", (SolidColorBrush)new BrushConverter().ConvertFromString("#ff00aa")),
            new Palette("Cornflower", (SolidColorBrush)new BrushConverter().ConvertFromString("#0055aa")),
            new Palette("Deep", (SolidColorBrush)new BrushConverter().ConvertFromString("#5555aa")),
            new Palette("Lilac", (SolidColorBrush)new BrushConverter().ConvertFromString("#aa55aa")),
            new Palette("Lavender", (SolidColorBrush)new BrushConverter().ConvertFromString("#ff55aa")),
            new Palette("Aqua", (SolidColorBrush)new BrushConverter().ConvertFromString("#00aaaa")),
            new Palette("Steel", (SolidColorBrush)new BrushConverter().ConvertFromString("#55aaaa")),
            new Palette("Grey", (SolidColorBrush)new BrushConverter().ConvertFromString("#aaaaaa")),
            new Palette("Pink", (SolidColorBrush)new BrushConverter().ConvertFromString("#ffaaaa")),
            new Palette("Bay", (SolidColorBrush)new BrushConverter().ConvertFromString("#00ffaa")),
            new Palette("Marina", (SolidColorBrush)new BrushConverter().ConvertFromString("#55ffaa")),
            new Palette("Tornado", (SolidColorBrush)new BrushConverter().ConvertFromString("#aaffaa")),
            new Palette("Saltine", (SolidColorBrush)new BrushConverter().ConvertFromString("#ffffaa")),
            new Palette("Blue", (SolidColorBrush)new BrushConverter().ConvertFromString("#0000ff")),
            new Palette("Twilight", (SolidColorBrush)new BrushConverter().ConvertFromString("#5500ff")),
            new Palette("Orchid", (SolidColorBrush)new BrushConverter().ConvertFromString("#aa00ff")),
            new Palette("Magenta", (SolidColorBrush)new BrushConverter().ConvertFromString("#ff00ff")),
            new Palette("Azure", (SolidColorBrush)new BrushConverter().ConvertFromString("#0055ff")),
            new Palette("Liberty", (SolidColorBrush)new BrushConverter().ConvertFromString("#5555ff")),
            new Palette("Royalty", (SolidColorBrush)new BrushConverter().ConvertFromString("#aa55ff")),
            new Palette("Thistle", (SolidColorBrush)new BrushConverter().ConvertFromString("#ff55ff")),
            new Palette("Ocean", (SolidColorBrush)new BrushConverter().ConvertFromString("#00aaff")),
            new Palette("Sky", (SolidColorBrush)new BrushConverter().ConvertFromString("#55aaff")),
            new Palette("Periwinkle", (SolidColorBrush)new BrushConverter().ConvertFromString("#aaaaff")),
            new Palette("Carnation", (SolidColorBrush)new BrushConverter().ConvertFromString("#ffaaff")),
            new Palette("Cyan", (SolidColorBrush)new BrushConverter().ConvertFromString("#00ffff")),
            new Palette("Turquoise", (SolidColorBrush)new BrushConverter().ConvertFromString("#55ffff")),
            new Palette("Powder", (SolidColorBrush)new BrushConverter().ConvertFromString("#aaffff")),
            new Palette("White", (SolidColorBrush)new BrushConverter().ConvertFromString("#ffffff"))
        };

        public static Window set = new Settings();

        public bool muted = false;
        public int muted_volume;

        public int current_song = 0;
        public int current_image = 0;

        public double beat_length = 0;
        public double buildup_beat_len = 0;

        public static List<rdata> enabled_songs = new List<rdata>();
        public static List<rdata> enabled_images = new List<rdata>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) timeline_noblur();
            if (e.ChangedButton == MouseButton.Right)
            {
                
                if (!set.IsLoaded)
                {
                    set = new Settings();
                    set.Owner = this;
                }
                if (set.IsVisible) set.Hide();
                else set.Show();
            }
        }
        
        public void play_current_song()
        {
            FAFbass.PlayLoop(ResPackManager.allSongs[current_song].buffer, FAFbass.Volume);
            song_label.Content = ResPackManager.allSongs[current_song].title.ToUpper();
            beat_length = FAFbass.GetTimeOfStream(FAFbass.Stream) / ResPackManager.allSongs[current_song].rhythm.Length;
            timeline_label.Content = ResPackManager.allSongs[current_song].rhythm;
        }
        public void show_current_image()
        {
            image.Source = ResPackManager.allPics[current_image].png;
            character_label.Content = ResPackManager.allPics[current_image].fullname.ToUpper();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            set.Owner = this;
            
            ResPackManager.SupremeReader("Defaults_v5.0.zip");

            show_current_image();
            play_current_song();

            Settings.rp_names.Add(new setdata() { Name = ResPackManager.resPacks[0].name, State = true });
        }



        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (muted) toggle_mute();
            if(e.Delta < 0 & FAFbass.Volume != 0)
            {
                FAFbass.Volume -= 10;
                volume_label.Content = FAFbass.Volume;
                FAFbass.SetVolumeToStream(FAFbass.Stream, FAFbass.Volume);
            }
            if(e.Delta > 0 & FAFbass.Volume != 100)
            {
                if (muted) toggle_mute();
                FAFbass.Volume += 10;
                volume_label.Content = FAFbass.Volume;
                FAFbass.SetVolumeToStream(FAFbass.Stream, FAFbass.Volume);
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
            }
        }

        private void next_song()
        {
            for (int i = current_song + 1; true; i++)
            {
                if (ResPackManager.allSongs[i % (ResPackManager.allSongs.Length - 1)].enabled & ResPackManager.resPacks[ResPackManager.get_rp_of_song(i)].enabled)
                {
                    current_song = i % ResPackManager.allSongs.Length;
                    play_current_song();
                    break;
                }
                if (i == 100000) break; //Yeah, lame failsafe
            }


        }
        private void prev_song()
        {
            for (int i = current_song - 1; true; i--)
            {
                if (i == -1) i = ResPackManager.allSongs.Length - 1;
                if (ResPackManager.allSongs[i].enabled)
                {
                    current_song = i;
                    play_current_song();
                    break;
                }
                if (i == current_song) //looped a full circle with no songs enabled
                {

                    break;
                }
            }
        }

        private void toggle_mute()
        {
            if (!muted)
            {
                muted_volume = FAFbass.Volume;
                volume_label.Content = 0;
                FAFbass.Volume = 0;
                FAFbass.SetVolumeToStream(FAFbass.Stream, FAFbass.Volume);
                muted = true;
            }
            else
            {
                FAFbass.Volume = muted_volume;
                volume_label.Content = FAFbass.Volume;
                FAFbass.SetVolumeToStream(FAFbass.Stream, FAFbass.Volume);
                muted = false;
            }
        }

        private void next_image()
        {
            for (int i = current_image + 1; true; i++)
            {
                if (ResPackManager.allPics[i % (ResPackManager.allPics.Length - 1)].enabled)
                {
                    current_image = i % ResPackManager.allPics.Length;
                    show_current_image();
                    break;
                }
                if (i == 100000) break;
            }
        }
        private void prev_image()
        {
            for (int i = current_image - 1; true; i--)
            {
                if (i == -1) i = ResPackManager.allPics.Length - 1;
                if (ResPackManager.allPics[i].enabled)
                {
                    current_image = i;
                    show_current_image();
                    break;
                }
            }
        }



        //
        // Timeline Effects Controls
        //


        // Vertical blur (snare)
        private void timeline_x()
        {

        }
        // Horizontal blur (bass)
        private void timeline_o()
        {
            
        }

        /// <summary>
        /// For '-' in the timeline
        /// </summary>
        private void timeline_noblur()
        {
            timeline_color_change();
            timeline_image_change();
        }
        // '+'
        private void timeline_blackout()
        {

        }
        // '¤'
        private void timeline_whiteout()
        {

        }
        // '|'
        private void timeline_blackout_short()
        {

        }
        // ':'
        private void timeline_color_change()
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
            int index;
            while (true)
            {
                index = rnd.Next(0, ResPackManager.allPics.Length - 1);
                if (image.Source != ResPackManager.allPics[index].png) break;
            }
            image.Source = ResPackManager.allPics[index].png;
            switch (ResPackManager.allPics[index].align)
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
            character_label.Content = ResPackManager.allPics[index].fullname.ToUpper();
        }
        // 'X' Vertical blur only
        private void timeline_blur_vert()
        {

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
                set.Top = this.Top + (this.Height / 2) - (set.Height / 2);
                set.Left = this.Left + (this.Width / 2) - (set.Width / 2);
            }
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (set.IsVisible)
            {
                if (this.WindowState == WindowState.Normal)
                {
                    set.Top = this.Top + (this.Height / 2) - (set.Height / 2);
                    set.Left = this.Left + (this.Width / 2) - (set.Width / 2);
                }
                if (this.WindowState == WindowState.Maximized)
                {
                    set.Top = (SystemParameters.MaximizedPrimaryScreenHeight / 2) - (set.Height / 2);
                    set.Left = (SystemParameters.MaximizedPrimaryScreenWidth / 2) - (set.Width / 2);
                }
            }
        }
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            set.Top = this.Top + (this.Height / 2) - (set.Height / 2);
            set.Left = this.Left + (this.Width / 2) - (set.Width / 2);
        }

        
    }

    public struct Palette
    {
        public string name;
        public Brush brush;

        public Palette(string f1, Brush f2)
        {
            name = f1; brush = f2;
        }
    }

}

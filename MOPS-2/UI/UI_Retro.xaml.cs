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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MOPS.UI
{
    /// <summary>
    /// Interaction logic for UI_Retro.xaml
    /// </summary>
    public partial class UI_Retro : UserControl
    {
        MainWindow main;
        PicPicker picPicker = new PicPicker();
        SongPicker songPicker = new SongPicker();
        public UI_Retro(MainWindow MW)
        {
            InitializeComponent();
            main = MW;
            versionHex_textBlock.Text = "V=$" + HexifyVersion(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            updateImageModeText();
            InitHideAnimation();
            InitPickers();
        }
        private void InitPickers()
        {
            picPicker.HorizontalAlignment = HorizontalAlignment.Right;
            picPicker.VerticalAlignment = VerticalAlignment.Bottom;
            picPicker.MaxHeight = 200;
            picPicker.Width = 250;
            picPicker.Visibility = Visibility.Hidden;
            picPicker.Margin = new Thickness(0, 0, 40, 50);
            MainGrid.Children.Add(picPicker);

            songPicker.HorizontalAlignment = HorizontalAlignment.Right;
            songPicker.VerticalAlignment = VerticalAlignment.Bottom;
            songPicker.MaxHeight = 200;
            songPicker.Width = 400;
            songPicker.Visibility = Visibility.Hidden;
            songPicker.Margin = new Thickness(0, 0, 40, 50);
            MainGrid.Children.Add(songPicker);
        }

        private string HexifyVersion(string verstr)
        {
            int dot = verstr.IndexOf('.');
            int dot2 = verstr.IndexOf('.', dot + 1);
            string res = verstr.Substring(0, dot2);
            res = res.Remove(dot, 1);
            res = (Convert.ToInt32(res)).ToString("X");
            return res;
        }

        public void updateImageModeText()
        {
            if (main.full_auto_mode) mode_textBlock.Text = "M=FULL AUTO";
            else mode_textBlock.Text = "M=NORMAL";
        }




        #region HideUI
        private bool HideAnimationInProgress = false;
        public void ToggleHideUI()
        {
            if (!HideAnimationInProgress)
            {
                if (MainGrid.Opacity == 1)
                {
                    LilButtonGrid.Visibility = Visibility.Visible;
                    MainGrid.BeginAnimation(OpacityProperty, HideMainGridAnimation);
                    LilButtonGrid.BeginAnimation(OpacityProperty, ShowLilButtonAnimation);
                    MainGrid.Opacity = 0;
                    MainGrid.IsEnabled = false;
                    LilButtonGrid.Opacity = 1;
                    HideAnimationInProgress = true;
                }
                else
                {
                    MainGrid.Visibility = Visibility.Visible;
                    MainGrid.BeginAnimation(OpacityProperty, ShowMainGridAnimation);
                    LilButtonGrid.BeginAnimation(OpacityProperty, HideLilButtonAnimation);
                    MainGrid.Opacity = 1;
                    LilButtonGrid.IsEnabled = false;
                    LilButtonGrid.Opacity = 0;
                    HideAnimationInProgress = true;
                }
            }
        }

        private DoubleAnimation HideMainGridAnimation = new DoubleAnimation();
        private DoubleAnimation ShowMainGridAnimation = new DoubleAnimation();
        private DoubleAnimation HideLilButtonAnimation = new DoubleAnimation();
        private DoubleAnimation ShowLilButtonAnimation = new DoubleAnimation();

        private void InitHideAnimation()
        {
            HideMainGridAnimation.From = 1;
            HideMainGridAnimation.To = 0;
            HideMainGridAnimation.Duration = TimeSpan.FromSeconds(0.6);
            HideMainGridAnimation.Completed += delegate (object sender, EventArgs e) 
            { 
                MainGrid.Visibility = Visibility.Hidden;
            };
            
            ShowMainGridAnimation.From = 0;
            ShowMainGridAnimation.To = 1;
            ShowMainGridAnimation.Duration = TimeSpan.FromSeconds(0.6);
            ShowMainGridAnimation.Completed += delegate (object sender, EventArgs e) 
            { 
                MainGrid.IsEnabled = true;
                HideAnimationInProgress = false;
            };

            HideLilButtonAnimation.From = 1;
            HideLilButtonAnimation.To = 0;
            HideLilButtonAnimation.Duration = TimeSpan.FromSeconds(0.6);
            HideLilButtonAnimation.Completed += delegate (object sender, EventArgs e)
            {
                LilButtonGrid.Visibility = Visibility.Hidden;
            };

            ShowLilButtonAnimation.From = 0;
            ShowLilButtonAnimation.To = 1;
            ShowLilButtonAnimation.Duration = TimeSpan.FromSeconds(0.6);
            ShowLilButtonAnimation.Completed += delegate (object sender, EventArgs e)
            {
                LilButtonGrid.IsEnabled = true;
                HideAnimationInProgress = false;
            };
        }
        #endregion


        #region Button Clicks
        private void settings_button_Click(object sender, RoutedEventArgs e)
        {
            main.ToggleInnerWindow();
        }

        private void images_button_Click(object sender, RoutedEventArgs e)
        {
            TogglePicPickerVisivility();
        }
        public void TogglePicPickerVisivility()
        {
            if (picPicker.Visibility == Visibility.Visible) picPicker.Visibility = Visibility.Hidden;
            else
            {
                if (songPicker.Visibility == Visibility.Visible) songPicker.Visibility = Visibility.Hidden;
                picPicker.Visibility = Visibility.Visible;
            }
        }

        private void hideUI_button_Click(object sender, RoutedEventArgs e)
        {
            ToggleHideUI();
        }

        private void nextSong_button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.next_song();
        }

        private void prevSong_button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.prev_song();
        }

        private void songs_button_Click(object sender, RoutedEventArgs e)
        {
            ToggleSongPickerVisivility();
        }
        public void ToggleSongPickerVisivility()
        {
            if (songPicker.Visibility == Visibility.Visible) songPicker.Visibility = Visibility.Hidden;
            else
            {
                if (picPicker.Visibility == Visibility.Visible) picPicker.Visibility = Visibility.Hidden;
                songPicker.Visibility = Visibility.Visible;
            }
        }

        private void nextImage_button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.next_image();
        }

        private void prevImage_button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.prev_image();
        }

        private void fullAutoMode_button_Click(object sender, RoutedEventArgs e)
        {
            main.full_auto_mode = true; 
        }
        private void normalMode_Button_Click(object sender, RoutedEventArgs e)
        {
            main.full_auto_mode = false;
        }
        private void showUI_TBB_Click(object sender, RoutedEventArgs e)
        {
            ToggleHideUI();
        }
        #endregion

        #region hyperlinks

        private string songSource = "";
        public void songSourceUpdate(string source)
        {
            songSource = source;
            if (songSource == null) song_textBlock.Cursor = Cursors.Arrow;
            else song_textBlock.Cursor = Cursors.Hand;
        }
        private void song_textBlock_Click(object sender, RoutedEventArgs e)
        {
            if (songSource != null) System.Diagnostics.Process.Start(songSource);
        }
        string charSource = "";
        public void charSourceUpdate(string source)
        {
            charSource = source;
            if (charSource == "") character_textBlock.Cursor = Cursors.Arrow;
            else character_textBlock.Cursor = Cursors.Hand;
        }
        public void character_textBlock_Click(object sender, RoutedEventArgs e)
        {
            if (charSource != null) System.Diagnostics.Process.Start(charSource);
        }

        #endregion
    }
}

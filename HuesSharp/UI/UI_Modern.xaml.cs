using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HuesSharp.UI
{
    /// <summary>
    /// Interaction logic for UI_Modern.xaml
    /// </summary>
    public partial class UI_Modern : UserControl
    {
        MainWindow main;
        string nextChar = "";
        DispatcherTimer timer = new DispatcherTimer();
        PicPicker picPicker = new PicPicker();
        SongPicker songPicker = new SongPicker();

        public UI_Modern(MainWindow MW)
        {
            InitializeComponent();
            main = MW;
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += delegate { timer.Stop(); beatCenter.Visibility = Visibility.Hidden; };
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
            pickersGrid.Children.Add(picPicker);

            songPicker.HorizontalAlignment = HorizontalAlignment.Right;
            songPicker.VerticalAlignment = VerticalAlignment.Bottom;
            songPicker.MaxHeight = 200;
            songPicker.Width = 400;
            songPicker.Visibility = Visibility.Hidden;
            pickersGrid.Children.Add(songPicker);
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

        public void updateTimeline(string timeline)
        {
            timelineLeft_textBlock.Text = timeline;
            timelineRight_textBlock.Text = timeline;
            if (nextChar != ".")
            {
                AnimateChar(nextChar);
            }
            nextChar = timeline[0].ToString();
        }
        public void updateImageModeText()
        {
            if (main.full_auto_mode) fullautoToggle_Button.Content = "\uE902";
            else fullautoToggle_Button.Content = "\uE901";
        }
        public void updateVolumeText(int newVolume)
        {
            if (newVolume != volume_slider.Value) volume_slider.Value = newVolume;
            if (newVolume == 0)
            {
                mute_button.Content = "(VOL)";
                //mute_button.Margin = new Thickness(67, 0, 0, 37);
            }
            else
            {
                mute_button.Content = "VOL";
                //mute_button.Margin = new Thickness(79, 0, 0, 37);
            }
        }

        private void AnimateChar(string ch)
        {
            beatCenter.Text = ch;
            beatCenter.Visibility = Visibility.Visible;
            timer.Stop();
            timer.Start();
        }


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

        private void images_Button_Click(object sender, RoutedEventArgs e)
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

        private void songs_Button_Click(object sender, RoutedEventArgs e)
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

        private void randomSong_Button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.random_song();
        }

        private void fullautoToggle_Button_Click(object sender, RoutedEventArgs e)
        {
            if (main.full_auto_mode) main.full_auto_mode = false;
            else main.full_auto_mode = true;
        }

        private void prevSong_Button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.prev_song();
        }

        private void nextSong_Button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.next_song();
        }

        private void prevImage_Button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.prev_image();
        }

        private void nextImage_Button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.next_image();
        }

        private void info_button_Click(object sender, RoutedEventArgs e)
        {
            main.show_info_page();
        }

        private void settings_button_Click(object sender, RoutedEventArgs e)
        {
            main.ToggleInnerWindow();
        }

        private void volume_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (main != null) main.Core.SetVolume((int)e.NewValue);
        }

        private void mute_button_Click(object sender, RoutedEventArgs e)
        {
            main.Core.toggle_mute();
        }

        #region HideUI
        private void hideUI_button_Click(object sender, RoutedEventArgs e)
        {
            ToggleHideUI();
        }

        private void ToggleUIBottomButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleHideUI();
        }

        private bool HideAnimationInProgress = false;
        public void ToggleHideUI()
        {
            if (!HideAnimationInProgress)
            {
                if (ui_State == UI_State.Shown)
                {
                    ToggleUIBottomButton.Visibility = Visibility.Visible;
                    BottomGrid.BeginAnimation(OpacityProperty, HideBottomGridAnimation);
                    LilBottomGrid.BeginAnimation(OpacityProperty, ShowLilButtonAnimation);
                    BottomGrid.Opacity = 0;
                    BottomGrid.IsEnabled = false;
                    ToggleUIBottomButton.Opacity = 1;
                    HideAnimationInProgress = true;

                    ui_State = UI_State.BottomHidden;
                }
                else if (ui_State == UI_State.BottomHidden)
                {
                    TopGrid.BeginAnimation(OpacityProperty, HideTopGridAnimation);
                    TopGrid.Opacity = 0;
                    TopGrid.IsEnabled = false;
                    HideAnimationInProgress = true;

                    ui_State = UI_State.AllHidden;
                }
                else
                {
                    BottomGrid.Visibility = Visibility.Visible;
                    TopGrid.Visibility = Visibility.Visible;
                    BottomGrid.BeginAnimation(OpacityProperty, ShowBottomGridAnimation);
                    TopGrid.BeginAnimation(OpacityProperty, ShowTopGridAnimation);
                    LilBottomGrid.BeginAnimation(OpacityProperty, HideLilButtonAnimation);
                    BottomGrid.Opacity = 1;
                    ToggleUIBottomButton.IsEnabled = false;
                    ToggleUIBottomButton.Opacity = 0;
                    HideAnimationInProgress = true;

                    ui_State = UI_State.Shown;
                }
            }
        }

        private DoubleAnimation HideBottomGridAnimation = new DoubleAnimation();
        private DoubleAnimation ShowBottomGridAnimation = new DoubleAnimation();
        private DoubleAnimation HideTopGridAnimation = new DoubleAnimation();
        private DoubleAnimation ShowTopGridAnimation = new DoubleAnimation();
        private DoubleAnimation HideLilButtonAnimation = new DoubleAnimation();
        private DoubleAnimation ShowLilButtonAnimation = new DoubleAnimation();

        private void InitHideAnimation()
        {
            HideBottomGridAnimation.From = 1;
            HideBottomGridAnimation.To = 0;
            HideBottomGridAnimation.Duration = TimeSpan.FromSeconds(0.6);
            HideBottomGridAnimation.Completed += delegate (object sender, EventArgs e)
            {
                BottomGrid.Visibility = Visibility.Hidden;
            };

            ShowBottomGridAnimation.From = 0;
            ShowBottomGridAnimation.To = 1;
            ShowBottomGridAnimation.Duration = TimeSpan.FromSeconds(0.6);
            ShowBottomGridAnimation.Completed += delegate (object sender, EventArgs e)
            {
                BottomGrid.IsEnabled = true;
                HideAnimationInProgress = false;
            };

            HideTopGridAnimation.From = 1;
            HideTopGridAnimation.To = 0;
            HideTopGridAnimation.Duration = TimeSpan.FromSeconds(0.6);
            HideTopGridAnimation.Completed += delegate (object sender, EventArgs e)
            {
                TopGrid.Visibility = Visibility.Hidden;
                HideAnimationInProgress = false;
            };

            ShowTopGridAnimation.From = 0;
            ShowTopGridAnimation.To = 1;
            ShowTopGridAnimation.Duration = TimeSpan.FromSeconds(0.6);
            ShowTopGridAnimation.Completed += delegate (object sender, EventArgs e)
            {
                TopGrid.IsEnabled = true;
                HideAnimationInProgress = false;
            };

            HideLilButtonAnimation.From = 1;
            HideLilButtonAnimation.To = 0;
            HideLilButtonAnimation.Duration = TimeSpan.FromSeconds(0.6);
            HideLilButtonAnimation.Completed += delegate (object sender, EventArgs e)
            {
                ToggleUIBottomButton.Visibility = Visibility.Hidden;
            };

            ShowLilButtonAnimation.From = 0;
            ShowLilButtonAnimation.To = 1;
            ShowLilButtonAnimation.Duration = TimeSpan.FromSeconds(0.6);
            ShowLilButtonAnimation.Completed += delegate (object sender, EventArgs e)
            {
                ToggleUIBottomButton.IsEnabled = true;
                HideAnimationInProgress = false;
            };
        }

        private enum UI_State
        {
            Shown = 0,
            BottomHidden = 1,
            AllHidden = 2
        }
        private UI_State ui_State = 0;
        #endregion


    }
}

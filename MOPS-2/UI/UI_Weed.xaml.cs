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

namespace MOPS.UI
{
    /// <summary>
    /// Interaction logic for UI_Weed.xaml
    /// </summary>
    public partial class UI_Weed : UserControl
    {
        MainWindow main;
        string nextChar = "";
        Random rnd = new Random();
        public UI_Weed(MainWindow MW)
        {
            InitializeComponent();
            main = MW;
            versionHex_textBlock.Text = "V=$" + HexifyVersion(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            updateImageModeText();
            InitHideAnimation();
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

        public void updateTimeline(string timeline)
        {
            timelineLeft_textBlock.Text = timeline;
            timelineRight_textBlock.Text = timeline;
            if (nextChar == "X" | nextChar == "O")
            {
                AnimateChar(nextChar);
            }
            nextChar = char.ToUpper(timeline[0]).ToString();
        }

        #region FloatingCharAnim

        private void AnimateChar(string ch)
        {
            double duration = 0.5;

            TextBlock tb = new TextBlock();
            tb.Text = ch;
            tb.FontSize = 30;
            tb.Margin = new Thickness(-20, 13, 0, 0);
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            tb.VerticalAlignment = VerticalAlignment.Top;
            RotateTransform rt = new RotateTransform(0);
            tb.RenderTransform = rt;
            TimelineGrid.Children.Add(tb);
            Grid.SetColumn(tb, 2);

            ThicknessAnimation thicknessAnim = new ThicknessAnimation();
            DoubleAnimation opacityAnim = new DoubleAnimation();
            DoubleAnimation rotateAnim = new DoubleAnimation();
            Storyboard FloatingCharSB = new Storyboard();
            FloatingCharSB.Children.Add(opacityAnim);
            FloatingCharSB.Children.Add(thicknessAnim);
            FloatingCharSB.Children.Add(rotateAnim);

            opacityAnim.From = 1;
            opacityAnim.To = 0;
            opacityAnim.Duration = TimeSpan.FromSeconds(duration);
            thicknessAnim.From = new Thickness(-20, 13, 0, 0);
            thicknessAnim.To = new Thickness(-20, 50, 0, 0);
            thicknessAnim.Duration = TimeSpan.FromSeconds(duration);
            rotateAnim.From = 0;
            rotateAnim.To = new int[]{ -20, 0, 20}[rnd.Next(0, 3)];
            rotateAnim.Duration = TimeSpan.FromSeconds(duration);
            Storyboard.SetTarget(rotateAnim, tb);
            Storyboard.SetTargetProperty(rotateAnim, new PropertyPath("(TextBlock.RenderTransform).(RotateTransform.Angle)"));
            Storyboard.SetTarget(opacityAnim, tb);
            Storyboard.SetTargetProperty(opacityAnim, new PropertyPath("Opacity"));
            Storyboard.SetTarget(thicknessAnim, tb);
            Storyboard.SetTargetProperty(thicknessAnim, new PropertyPath("Margin"));

            FloatingCharSB.Completed += delegate (object sender, EventArgs e)
            {
                TimelineGrid.Children.Remove(tb);
            };

            FloatingCharSB.Begin();
        }

        #endregion FloatingCharAnim

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

        private void chars_button_Click(object sender, RoutedEventArgs e)
        {
            main.ToggleCharList();
        }

        private void hideUI_button_Click(object sender, RoutedEventArgs e)
        {
            ToggleHideUI();
        }

        private void nextSong_button_Click(object sender, RoutedEventArgs e)
        {
            main.next_song();
        }

        private void prevSong_button_Click(object sender, RoutedEventArgs e)
        {
            main.prev_song();
        }

        private void songs_button_Click(object sender, RoutedEventArgs e)
        {
            main.ToggleSongList();
        }

        private void nextImage_button_Click(object sender, RoutedEventArgs e)
        {
            main.next_image();
            updateImageModeText();
        }

        private void prevImage_button_Click(object sender, RoutedEventArgs e)
        {
            main.prev_image();
            updateImageModeText();
        }

        private void fullAutoMode_button_Click(object sender, RoutedEventArgs e)
        {
            main.full_auto_mode = true;
            updateImageModeText();
        }
        private void normalMode_Button_Click(object sender, RoutedEventArgs e)
        {
            main.full_auto_mode = false;
            updateImageModeText();
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

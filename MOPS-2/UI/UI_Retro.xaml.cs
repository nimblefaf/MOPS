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
        public UI_Retro(MainWindow MW)
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

        public void ToggleHideUI()
        {
            if (MainGrid.Opacity == 1)
            {
                LilButtonGrid.Visibility = Visibility.Visible;
                MainGrid.BeginAnimation(OpacityProperty, HideMainGridAnimation);
                LilButtonGrid.BeginAnimation(OpacityProperty, ShowLilButtonAnimation);
                MainGrid.Opacity = 0;
                MainGrid.IsEnabled = false;
                LilButtonGrid.Opacity = 1;
            }
            else
            {
                MainGrid.Visibility = Visibility.Visible;
                MainGrid.BeginAnimation(OpacityProperty, ShowMainGridAnimation);
                LilButtonGrid.BeginAnimation(OpacityProperty, HideLilButtonAnimation);
                MainGrid.Opacity = 1;
                LilButtonGrid.IsEnabled = false;
                LilButtonGrid.Opacity = 0;
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
            };
        }

#region Button Clicks
        private void settings_button_Click(object sender, RoutedEventArgs e)
        {
            main.ToggleInnerWindow();
        }

        private void chars_button_Click(object sender, RoutedEventArgs e)
        {
            if (main.images_listbox.Visibility == Visibility.Hidden)
            {
                main.images_listbox.Visibility = Visibility.Visible;
                main.songs_listbox.Visibility = Visibility.Hidden;
            }
            else main.images_listbox.Visibility = Visibility.Hidden;
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
            if (main.songs_listbox.Visibility == Visibility.Hidden)
            {
                main.songs_listbox.Visibility = Visibility.Visible;
                main.images_listbox.Visibility = Visibility.Hidden;
            }
            else main.songs_listbox.Visibility = Visibility.Hidden;
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
    }
}

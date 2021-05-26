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
using System.Windows.Media.Animation;

namespace MOPS.UI
{
    public class setdata
    {
        public string Name { get; set; }
        public bool State { get; set; }
        public int Ind { get; set; }
    }

    /// <summary>
    /// Interaction logic for Options_TabPanel.xaml
    /// </summary>
    public partial class Options_TabPanel : UserControl
    {
        public Options_TabPanel()
        {
            InitializeComponent();
            Options_UI_Update();
        }
        MainWindow main;
        public void SetReference(MainWindow window)
        {
            main = window;
        }


        private void Options_UI_Update() //to update UI after loading settings
        {
            switch ((BuildUpMode)Properties.Settings.Default.buildUpMode)
            {
                case BuildUpMode.On:
                    OB_buildOn.Background = Brushes.White;
                    break;
                case BuildUpMode.Off:
                    OB_buildOff.Background = Brushes.White;
                    break;
                case BuildUpMode.Once:
                    OB_buildOnce.Background = Brushes.White;
                    break;
            }
            switch ((ColorSet)Properties.Settings.Default.colorSet)
            {
                case ColorSet.Normal:
                    OB_colorNormal.Background = Brushes.White;
                    break;
                case ColorSet.Pastel:
                    OB_colorPastel.Background = Brushes.White;
                    break;
                case ColorSet.Weed:
                    OB_colorWeed.Background = Brushes.White;
                    break;
            }
            switch ((BlendMode)Properties.Settings.Default.blendMode)
            {
                case BlendMode.Plain:
                    OB_colorBlend_Plain.Background = Brushes.White;
                    break;
                case BlendMode.Alpha:
                    OB_colorBlend_Alpha.Background = Brushes.White;
                    break;
                case BlendMode.HardLight:
                    OB_colorBlend_HardLight.Background = Brushes.White;
                    break;
            }
            if (Properties.Settings.Default.discordMode) OB_DiscordOn.Background = Brushes.White;
            else OB_DiscordOff.Background = Brushes.White;
        }


        private void OB_DiscordOffButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.discordMode = false;
            OB_DiscordOn.Background = Brushes.LightGray;
            OB_DiscordOff.Background = Brushes.White;
            main.discordRpcClient.Dispose();
        }
        private void OB_DiscordOnButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.discordMode = true;
            OB_DiscordOn.Background = Brushes.White;
            OB_DiscordOff.Background = Brushes.LightGray;
            main.discord_rpc_init();
        }


        private void OB_colorBlend_Plain_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blendMode = (int)BlendMode.Plain;
            main.ColorBlend_Graphics_Update();
            OB_colorBlend_Plain.Background = Brushes.White;
            OB_colorBlend_Alpha.Background = Brushes.LightGray;
            OB_colorBlend_HardLight.Background = Brushes.LightGray;
        }
        private void OB_colorBlend_Alpha_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blendMode = (int)BlendMode.Alpha;
            main.ColorBlend_Graphics_Update();
            OB_colorBlend_Plain.Background = Brushes.LightGray;
            OB_colorBlend_Alpha.Background = Brushes.White;
            OB_colorBlend_HardLight.Background = Brushes.LightGray;
        }
        private void OB_colorBlend_HardLight_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blendMode = (int)BlendMode.HardLight;
            main.ColorBlend_Graphics_Update();
            OB_colorBlend_Plain.Background = Brushes.LightGray;
            OB_colorBlend_Alpha.Background = Brushes.LightGray;
            OB_colorBlend_HardLight.Background = Brushes.White;
        }


        private void OB_colorNormal_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.colorSet = (int)ColorSet.Normal;
            main.hues = Hues.hues_normal;
            OB_colorNormal.Background = Brushes.White;
            OB_colorPastel.Background = Brushes.LightGray;
            OB_colorWeed.Background = Brushes.LightGray;
        }
        private void OB_colorPastel_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.colorSet = (int)ColorSet.Pastel;
            main.hues = Hues.hues_pastel;
            OB_colorNormal.Background = Brushes.LightGray;
            OB_colorPastel.Background = Brushes.White;
            OB_colorWeed.Background = Brushes.LightGray;
        }
        private void OB_colorWeed_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.colorSet = (int)ColorSet.Weed;
            main.hues = Hues.hues_weed;
            OB_colorNormal.Background = Brushes.LightGray;
            OB_colorPastel.Background = Brushes.LightGray;
            OB_colorWeed.Background = Brushes.White;
        }


        private void OB_buildOnButton_Click(object sender, RoutedEventArgs e)
        {
            if ((BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.Once) for (int i = 0; i < main.RPM.allSongs.Length; i++) main.RPM.allSongs[i].buildup_played = false;
            Properties.Settings.Default.buildUpMode = (int)BuildUpMode.On;
            OB_buildOn.Background = Brushes.White;
            OB_buildOff.Background = Brushes.LightGray;
            OB_buildOnce.Background = Brushes.LightGray;
        }
        private void OB_buildOffButton_Click(object sender, RoutedEventArgs e)
        {
            if ((BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.Once) for (int i = 0; i < main.RPM.allSongs.Length; i++) main.RPM.allSongs[i].buildup_played = false;
            Properties.Settings.Default.buildUpMode = (int)BuildUpMode.Off;
            OB_buildOff.Background = Brushes.White;
            OB_buildOn.Background = Brushes.LightGray;
            OB_buildOnce.Background = Brushes.LightGray;
        }
        private void OB_buildOnceButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.buildUpMode = (int)BuildUpMode.Once;
            OB_buildOnce.Background = Brushes.White;
            OB_buildOff.Background = Brushes.LightGray;
            OB_buildOn.Background = Brushes.LightGray;
        }



    }
}

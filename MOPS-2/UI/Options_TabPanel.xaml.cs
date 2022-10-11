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
        }
        MainWindow main;
        public void SetReference(MainWindow window)
        {
            main = window;
            Options_UI_Update();
        }


        public void Options_UI_Update() //to update UI after loading settings
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
            switch ((BlurAmount)Properties.Settings.Default.blurAmount)
            {
                case BlurAmount.Low:
                    OB_BlurAmountLOW.Background = Brushes.White;
                    break;
                case BlurAmount.Medium:
                    OB_BlurAmountMED.Background = Brushes.White;
                    break;
                case BlurAmount.High:
                    OB_BlurAmountHIGH.Background = Brushes.White;
                    break;
            }
            switch ((BlurDecay)Properties.Settings.Default.blurDecay)
            {
                case BlurDecay.Slow:
                    OB_BlurDecaySLOW.Background = Brushes.White;
                    break;
                case BlurDecay.Medium:
                    OB_BlurDecayMED.Background = Brushes.White;
                    break;
                case BlurDecay.Fast:
                    OB_BlurDecayFAST.Background = Brushes.White;
                    break;
                case BlurDecay.Fastest:
                    OB_BlurDecayEXFAST.Background = Brushes.White;
                    break;
            }
            switch ((BlurQuality)Properties.Settings.Default.blurQuality)
            {
                case BlurQuality.Low:
                    OB_BlurQualLOW.Background = Brushes.White;
                    break;
                case BlurQuality.Medium:
                    OB_BlurQualMED.Background = Brushes.White;
                    break;
                case BlurQuality.High:
                    OB_BlurQualHIGH.Background = Brushes.White;
                    break;
            }
            switch ((UIStyle)Properties.Settings.Default.uiStyle)
            {
                case UIStyle.Alpha:
                    OB_UI_alpha.Background = Brushes.White;
                    OB_UI_mini.Background = Brushes.LightGray;
                    OB_UI_retro.Background = Brushes.LightGray;
                    OB_UI_weed.Background = Brushes.LightGray;
                    OB_UI_modern.Background = Brushes.LightGray;
                    break;
                case UIStyle.Mini:
                    OB_UI_alpha.Background = Brushes.LightGray;
                    OB_UI_mini.Background = Brushes.White;
                    OB_UI_retro.Background = Brushes.LightGray;
                    OB_UI_weed.Background = Brushes.LightGray;
                    OB_UI_modern.Background = Brushes.LightGray;
                    break;
                case UIStyle.Retro:
                    OB_UI_alpha.Background = Brushes.LightGray;
                    OB_UI_mini.Background = Brushes.LightGray;
                    OB_UI_retro.Background = Brushes.White;
                    OB_UI_weed.Background = Brushes.LightGray;
                    OB_UI_modern.Background = Brushes.LightGray;
                    break;
                case UIStyle.Weed:
                    OB_UI_alpha.Background = Brushes.LightGray;
                    OB_UI_mini.Background = Brushes.LightGray;
                    OB_UI_retro.Background = Brushes.LightGray;
                    OB_UI_weed.Background = Brushes.White;
                    OB_UI_modern.Background = Brushes.LightGray;
                    break;
                case UIStyle.Modern:
                    OB_UI_alpha.Background = Brushes.LightGray;
                    OB_UI_mini.Background = Brushes.LightGray;
                    OB_UI_retro.Background = Brushes.LightGray;
                    OB_UI_weed.Background = Brushes.LightGray;
                    OB_UI_modern.Background = Brushes.White;
                    break;
            }


            if (Properties.Settings.Default.discordMode) OB_DiscordOn.Background = Brushes.White;
            else OB_DiscordOff.Background = Brushes.White;

            if (Properties.Settings.Default.skipPreloadWarn) OB_SkipPreloadOn.Background = Brushes.White;
            else OB_SkipPreloadOff.Background = Brushes.White;

            if (Properties.Settings.Default.shuffleImages) OB_ShuffleImagesOn.Background = Brushes.White;
            else OB_ShuffleImagesOff.Background = Brushes.White;
        }


        private void OB_DiscordOffButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.discordMode = false;
            OB_DiscordOn.Background = Brushes.LightGray;
            OB_DiscordOff.Background = Brushes.White;
            main.Core.discordRpcClient.Dispose();
        }
        private void OB_DiscordOnButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.discordMode = true;
            OB_DiscordOn.Background = Brushes.White;
            OB_DiscordOff.Background = Brushes.LightGray;
            main.Core.discord_rpc_init();
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
            if ((BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.Once) for (int i = 0; i < main.Core.RPM.allSongs.Length; i++) main.Core.RPM.allSongs[i].buildup_played = false;
            Properties.Settings.Default.buildUpMode = (int)BuildUpMode.On;
            OB_buildOn.Background = Brushes.White;
            OB_buildOff.Background = Brushes.LightGray;
            OB_buildOnce.Background = Brushes.LightGray;
        }
        private void OB_buildOffButton_Click(object sender, RoutedEventArgs e)
        {
            if ((BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.Once) for (int i = 0; i < main.Core.RPM.allSongs.Length; i++) main.Core.RPM.allSongs[i].buildup_played = false;
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

        private void OB_BlurAmountLOW_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blurAmount = (int)BlurAmount.Low;
            main.BlurAmount_Upd();
            OB_BlurAmountLOW.Background = Brushes.White;
            OB_BlurAmountMED.Background = Brushes.LightGray;
            OB_BlurAmountHIGH.Background = Brushes.LightGray;
        }

        private void OB_BlurAmountMED_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blurAmount = (int)BlurAmount.Medium;
            main.BlurAmount_Upd();
            OB_BlurAmountLOW.Background = Brushes.LightGray;
            OB_BlurAmountMED.Background = Brushes.White;
            OB_BlurAmountHIGH.Background = Brushes.LightGray;
        }

        private void OB_BlurAmountHIGH_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blurAmount = (int)BlurAmount.High;
            main.BlurAmount_Upd();
            OB_BlurAmountLOW.Background = Brushes.LightGray;
            OB_BlurAmountMED.Background = Brushes.LightGray;
            OB_BlurAmountHIGH.Background = Brushes.White;
        }

        private void OB_BlurDecaySLOW_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blurDecay = (int)BlurDecay.Slow;
            main.BlurDecay_Upd();
            OB_BlurDecaySLOW.Background = Brushes.White;
            OB_BlurDecayMED.Background = Brushes.LightGray;
            OB_BlurDecayFAST.Background = Brushes.LightGray;
            OB_BlurDecayEXFAST.Background = Brushes.LightGray;
        }

        private void OB_BlurDecayMED_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blurDecay = (int)BlurDecay.Medium;
            main.BlurDecay_Upd();
            OB_BlurDecaySLOW.Background = Brushes.LightGray;
            OB_BlurDecayMED.Background = Brushes.White;
            OB_BlurDecayFAST.Background = Brushes.LightGray;
            OB_BlurDecayEXFAST.Background = Brushes.LightGray;
        }

        private void OB_BlurDecayFAST_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blurDecay = (int)BlurDecay.Fast;
            main.BlurDecay_Upd();
            OB_BlurDecaySLOW.Background = Brushes.LightGray;
            OB_BlurDecayMED.Background = Brushes.LightGray;
            OB_BlurDecayFAST.Background = Brushes.White;
            OB_BlurDecayEXFAST.Background = Brushes.LightGray;
        }

        private void OB_BlurDecayEXFAST_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blurDecay = (int)BlurDecay.Fastest;
            main.BlurDecay_Upd();
            OB_BlurDecaySLOW.Background = Brushes.LightGray;
            OB_BlurDecayMED.Background = Brushes.LightGray;
            OB_BlurDecayFAST.Background = Brushes.LightGray;
            OB_BlurDecayEXFAST.Background = Brushes.White;
        }


        private void OB_BlurQualLOW_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blurQuality = (int)BlurQuality.Low;
            OB_BlurQualLOW.Background = Brushes.White;
            OB_BlurQualMED.Background = Brushes.LightGray;
            OB_BlurQualHIGH.Background = Brushes.LightGray;
            main.BlurAmount_Upd();
        }

        private void OB_BlurQualMED_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blurQuality = (int)BlurQuality.Medium;
            OB_BlurQualLOW.Background = Brushes.LightGray;
            OB_BlurQualMED.Background = Brushes.White;
            OB_BlurQualHIGH.Background = Brushes.LightGray;
            main.BlurAmount_Upd();
        }

        private void OB_BlurQualHIGH_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.blurQuality = (int)BlurQuality.High;
            OB_BlurQualLOW.Background = Brushes.LightGray;
            OB_BlurQualMED.Background = Brushes.LightGray;
            OB_BlurQualHIGH.Background = Brushes.White;
            main.BlurAmount_Upd();
        }

        private void OB_SkipPreloadOff_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.skipPreloadWarn = false;
            OB_SkipPreloadOn.Background = Brushes.LightGray;
            OB_SkipPreloadOff.Background = Brushes.White;
        }

        private void OB_SkipPreloadOn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.skipPreloadWarn = true;
            OB_SkipPreloadOn.Background = Brushes.White;
            OB_SkipPreloadOff.Background = Brushes.LightGray;
        }

        private void OB_ShuffleImagesOff_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.shuffleImages = false;
            OB_ShuffleImagesOn.Background = Brushes.LightGray;
            OB_ShuffleImagesOff.Background = Brushes.White;
        }

        private void OB_ShuffleImagesOn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.shuffleImages = true;
            OB_ShuffleImagesOn.Background = Brushes.White;
            OB_ShuffleImagesOff.Background = Brushes.LightGray;
        }

        private void OB_UI_alpha_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.uiStyle != (int)UIStyle.Alpha)
            {
                Properties.Settings.Default.uiStyle = (int)UIStyle.Alpha;
                OB_UI_alpha.Background = Brushes.White;
                OB_UI_mini.Background = Brushes.LightGray;
                OB_UI_retro.Background = Brushes.LightGray;
                OB_UI_weed.Background = Brushes.LightGray;
                OB_UI_modern.Background = Brushes.LightGray;
                main.UIStyle_Graphics_Update();
            }
        }

        private void OB_UI_mini_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.uiStyle != (int)UIStyle.Mini)
            {
                Properties.Settings.Default.uiStyle = (int)UIStyle.Mini;
                OB_UI_alpha.Background = Brushes.LightGray;
                OB_UI_mini.Background = Brushes.White;
                OB_UI_retro.Background = Brushes.LightGray;
                OB_UI_weed.Background = Brushes.LightGray;
                OB_UI_modern.Background = Brushes.LightGray;
                main.UIStyle_Graphics_Update();
            }
        }

        private void OB_UI_retro_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.uiStyle != (int)UIStyle.Retro)
            {
                Properties.Settings.Default.uiStyle = (int)UIStyle.Retro;
                OB_UI_alpha.Background = Brushes.LightGray;
                OB_UI_mini.Background = Brushes.LightGray;
                OB_UI_retro.Background = Brushes.White;
                OB_UI_weed.Background = Brushes.LightGray;
                OB_UI_modern.Background = Brushes.LightGray;
                main.UIStyle_Graphics_Update();
            }
        }

        private void OB_UI_weed_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.uiStyle != (int)UIStyle.Weed)
            {
                Properties.Settings.Default.uiStyle = (int)UIStyle.Weed;
                OB_UI_alpha.Background = Brushes.LightGray;
                OB_UI_mini.Background = Brushes.LightGray;
                OB_UI_retro.Background = Brushes.LightGray;
                OB_UI_weed.Background = Brushes.White;
                OB_UI_modern.Background = Brushes.LightGray;
                main.UIStyle_Graphics_Update();
            }
        }

        private void OB_UI_modern_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.uiStyle != (int)UIStyle.Modern)
            {
                Properties.Settings.Default.uiStyle = (int)UIStyle.Modern;
                OB_UI_alpha.Background = Brushes.LightGray;
                OB_UI_mini.Background = Brushes.LightGray;
                OB_UI_retro.Background = Brushes.LightGray;
                OB_UI_weed.Background = Brushes.LightGray;
                OB_UI_modern.Background = Brushes.White;
                main.UIStyle_Graphics_Update();
            }
        }
    }
}

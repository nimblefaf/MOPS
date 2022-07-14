using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MOPS
{
    internal class UIHandler
    {
        internal UIHandler()
        {
            MainWin = (MainWindow)Application.Current.MainWindow;
            Display_Alpha = new UI.UI_Alpha(MainWin);
            Display_Retro = new UI.UI_Retro(MainWin);
            Display_Weed = new UI.UI_Weed(MainWin);

            AudioTimer = new DispatcherTimer();
            AudioTimer.Interval = TimeSpan.FromMilliseconds(25);
            AudioTimer.Tick += delegate (object sender, EventArgs e)
            {
                switch ((UIStyle)Properties.Settings.Default.uiStyle)
                {
                    case UIStyle.Retro:
                        Display_Retro.TimeHex_textBlock.Text = "T=" + HexifyTime(GetPosOfSong(), 5);
                        break;
                    case UIStyle.Weed:
                        Display_Weed.TimeHex_textBlock.Text = "T=" + HexifyTime(GetPosOfSong(), 5);
                        break;
                }
            };
            InitTBTimer();
        }
        private double GetPosOfSong()
        {
            if (MainWin.Core.rhythm_pos >= 0) return MainWin.Core.Player.GetPosOfStream(MainWin.Core.Player.Stream_L);
            else return MainWin.Core.Player.GetPosOfStream(MainWin.Core.Player.Stream_B);
        }

        public UI.UI_Alpha Display_Alpha;
        public UI.UI_Mini Display_Mini = new UI.UI_Mini();
        public UI.UI_Retro Display_Retro;
        public UI.UI_Weed Display_Weed;

        public DispatcherTimer AudioTimer;

        public MainWindow MainWin;
        public void UpdateEverything()
        {
            if (MainWin != null)
            {
                UpdateTimeline(MainWin.Core.current_timeline);
                UpdateSongInfo(MainWin.Core.RPM.allSongs[MainWin.Core.current_song_ind]);
                UpdateVolumeDisplayed(MainWin.Core.current_volume);
                UpdateColorName(MainWin.hues[MainWin.CurrentColorInd].name);
                UpdatePicName(MainWin.Core.RPM.allPics[MainWin.current_image_pos]);
                UpdateMiscInfo();
            }
        }

        public void UpdateMiscInfo()
        {
            switch ((UIStyle)Properties.Settings.Default.uiStyle)
            {
                case UIStyle.Retro:
                    Display_Retro.updateImageModeText();
                    break;
                case UIStyle.Weed:
                    Display_Weed.updateImageModeText();
                    break;
            }
        }

        public void UpdateSongInfo(Songs song, bool noSong = false)
        {
            if (noSong) switch ((UIStyle)Properties.Settings.Default.uiStyle)
                {
                    case UIStyle.Alpha:
                        Display_Alpha.timeline_textBlock.Text = ">>.";
                        Display_Alpha.song_textBlock.Text = "NONE";
                        break;
                    case UIStyle.Mini:
                        Display_Mini.timeline_textBlock.Text = ">>.";
                        break;
                    case UIStyle.Retro:
                        Display_Retro.timeline_textBlock.Text = ">>.";
                        Display_Retro.song_textBlock.Content = "NONE";
                        break;
                    case UIStyle.Weed:
                        Display_Weed.timelineLeft_textBlock.Text = "";
                        Display_Weed.timelineRight_textBlock.Text = "";
                        Display_Weed.song_textBlock.Content = "NONE";
                        break;

                }
            else switch ((UIStyle)Properties.Settings.Default.uiStyle)
                {
                    case UIStyle.Alpha:
                        Display_Alpha.song_textBlock.Text = song.title.ToUpper();
                        break;
                    case UIStyle.Mini:
                        break;
                    case UIStyle.Retro:
                        Display_Retro.song_textBlock.Content = song.title.ToUpper();
                        Display_Retro.songSourceUpdate(song.source);
                        break;
                    case UIStyle.Weed:
                        Display_Weed.song_textBlock.Content = song.title.ToUpper();
                        Display_Weed.songSourceUpdate(song.source);
                        break;
                }
        }

        public void UpdateVolumeDisplayed(int Volume)
        {
            switch ((UIStyle)Properties.Settings.Default.uiStyle)
            {
                case UIStyle.Alpha:
                    Display_Alpha.volume_textBlock.Text = Volume.ToString();
                    break;
            }
        }
        public void UpdateColorName(string ColorName)
        {
            switch ((UIStyle)Properties.Settings.Default.uiStyle)
            {
                case UIStyle.Alpha:
                    Display_Alpha.color_textBlock.Text = ColorName.ToUpper();
                    break;
                case UIStyle.Retro:
                    Display_Retro.color_textBlock.Text = ColorName.ToUpper();
                    Display_Retro.colorHex_textBlock.Text = "C=" + Hexify(MainWin.CurrentColorInd, 2);
                    break;
                case UIStyle.Weed:
                    Display_Weed.color_textBlock.Text = ColorName.ToUpper();
                    Display_Weed.colorHex_textBlock.Text = "C=" + Hexify(MainWin.CurrentColorInd, 2);
                    break;
            }
        }
        public void UpdatePicName(Pics pic)
        {
            switch ((UIStyle)Properties.Settings.Default.uiStyle)
            {
                case UIStyle.Alpha:
                    Display_Alpha.character_textBlock.Text = pic.fullname.ToUpper();
                    break;
                case UIStyle.Retro:
                    if (pic.fullname == null) Display_Retro.character_textBlock.Content = "I=" + "NONE";
                    else Display_Retro.character_textBlock.Content = "I=" + pic.fullname.ToUpper();
                    Display_Retro.charSourceUpdate(pic.source);
                    break;
                case UIStyle.Weed:
                    if (pic.fullname == null) Display_Weed.character_textBlock.Content = "I=" + "NONE";
                    else Display_Weed.character_textBlock.Content = "I=" + pic.fullname.ToUpper();
                    Display_Weed.charSourceUpdate(pic.source);
                    break;
            }
        }
        public void UpdateTimeline(string Timeline)
        {
            switch ((UIStyle)Properties.Settings.Default.uiStyle)
            {
                case UIStyle.Alpha:
                    Display_Alpha.timeline_textBlock.Text = ">>" + Timeline;
                    break;
                case UIStyle.Mini:
                    Display_Mini.timeline_textBlock.Text = ">>" + Timeline;
                    break;
                case UIStyle.Retro:
                    Display_Retro.timeline_textBlock.Text = ">>" + Timeline;
                    Display_Retro.BHex_textBlock.Text = "B=" + Hexify(MainWin.Core.rhythm_pos, 4);
                    break;
                case UIStyle.Weed:
                    Display_Weed.updateTimeline(Timeline);
                    Display_Weed.BHex_textBlock.Text = "B=" + Hexify(MainWin.Core.rhythm_pos, 4);
                    break;
            }
        }

        public void ToggleHideUI()
        {
            switch ((UIStyle)Properties.Settings.Default.uiStyle)
            {
                case UIStyle.Alpha:

                    break;
                case UIStyle.Mini:
                    Display_Mini.ToggleHideUI();
                    break;
                case UIStyle.Retro:
                    Display_Retro.ToggleHideUI();
                    break;
                case UIStyle.Weed:
                    Display_Weed.ToggleHideUI();
                    break;
            }
        }

        private string Hexify(int num, int len)
        {
            string res;
            if (num > -1) res = "$0x";
            else res = "-0x";
            string hex = Math.Abs(num).ToString("X");
            if (hex.Length < len) hex = new string('0', len - hex.Length) + hex;
            return res + hex;
        }
        private string HexifyTime(double num, int len)
        {
            string res;
            if (MainWin.Core.rhythm_pos > -1) res = "$0x";
            else
            {
                res = "-0x";
                num = MainWin.Core.Player.build_len_seconds - num;
            }


            string hex = num.ToString();
            int sepPos = hex.IndexOf(',');
            if (sepPos != -1)
            {
                hex.Remove(sepPos, 1);
                string left = hex.Substring(0, sepPos);
                string right = hex.Substring(sepPos + 1);
                if (right.Length > 3) right = right.Substring(0, 3);
                else if (right.Length < 3) right += new string('0', 3 - right.Length);
                hex = left + right;
            }
            else
            {
                hex += new string('0', 3);
            }
            hex = Convert.ToInt32(hex).ToString("X");
            if (hex.Length < len) hex = new string('0', len - hex.Length) + hex;
            return res + hex;
        }

        DispatcherTimer TextBlockTimer = new DispatcherTimer();
        private void InitTBTimer()
        {
            //from 153 to 0
            TextBlockTimer.Interval = TimeSpan.FromSeconds(0.01);
            TextBlockTimer.Tick += TextBlockTimer_Tick;
        }
        private bool BlurIsVertical;
        public void TBAnimStart(bool IsVertical)
        {
            TextBlockTimer.Stop();
            BlurIsVertical = IsVertical;
            switch ((UIStyle)Properties.Settings.Default.uiStyle)
            {
                case UIStyle.Retro:
                    Display_Retro.XHex_textBlock.Text = "X=$0x00";
                    Display_Retro.YHex_textBlock.Text = "Y=$0x00";
                    break;
                case UIStyle.Weed:
                    Display_Weed.XHex_textBlock.Text = "X=$0x00";
                    Display_Weed.YHex_textBlock.Text = "Y=$0x00";
                    break;
            }
            TextBlockTimer.Start();
        }
        private double BlurState = 0;
        private int GetPercented()
        {
            double.TryParse(MainWin.DirtyHack_textBlock.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out BlurState);
            double perc_BlurState = BlurState / (double)MainWin.BlurAnim.From;
            return Convert.ToInt32(153 * perc_BlurState);
        }
        private void TextBlockTimer_Tick(object sender, EventArgs e)
        {
            string hex = GetPercented().ToString("X");
            if (hex.Length < 2) hex = "0" + hex;
            if (BlurIsVertical)
                switch ((UIStyle)Properties.Settings.Default.uiStyle)
                {
                    case UIStyle.Retro:
                        Display_Retro.YHex_textBlock.Text = "Y=$0x" + hex;
                    break;
                    case UIStyle.Weed:
                        Display_Weed.YHex_textBlock.Text = "Y=$0x" + hex;
                        break;
                }
            else
                switch ((UIStyle)Properties.Settings.Default.uiStyle)
                {
                    case UIStyle.Retro:
                        Display_Retro.XHex_textBlock.Text = "X=$0x" + hex;
                        break;
                    case UIStyle.Weed:
                        Display_Weed.XHex_textBlock.Text = "X=$0x" + hex;
                        break;
                }
        }
    }
}

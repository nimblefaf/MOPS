using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MOPS
{
    internal class UIHandler
    {
        internal UIHandler()
        {
            MainWin = (MainWindow)Application.Current.MainWindow;
            Display_Alpha = new UI.UI_Alpha(MainWin);
        }
        public UI.UI_Alpha Display_Alpha;
        public UI.UI_Mini Display_Mini = new UI.UI_Mini();

        public MainWindow MainWin;
        public void UpdateEverything()
        {
            if (MainWin != null)
            {
                UpdateTimeline(MainWin.Core.current_timeline);
                UpdateSongInfo(MainWin.Core.RPM.allSongs[MainWin.Core.current_song_ind]);
                UpdateVolumeDisplayed(MainWin.Core.current_volume);
                UpdateColorName(MainWin.hues[MainWin.CurrentColorInd].name);
                UpdatePicName(MainWin.Core.RPM.allPics[MainWin.Core.current_image_pos].fullname);
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
                }
            else switch ((UIStyle)Properties.Settings.Default.uiStyle)
                {
                    case UIStyle.Alpha:
                        Display_Alpha.song_textBlock.Text = song.title.ToUpper();
                        break;
                    case UIStyle.Mini:

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
                case UIStyle.Mini:
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
                case UIStyle.Mini:
                    break;
            }
        }
        public void UpdatePicName(string PicName)
        {
            switch ((UIStyle)Properties.Settings.Default.uiStyle)
            {
                case UIStyle.Alpha:
                    Display_Alpha.character_textBlock.Text = PicName.ToUpper();
                    break;
                case UIStyle.Mini:
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
            }
        }

    }
}

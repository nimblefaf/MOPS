using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MOPS
{
    internal class Core
    {
        public Core()
        {
            
        }
        MainWindow main;
        public void SetReference(MainWindow window)
        {
            main = window;
        }

        Audio Player = new Audio();

        private string loop_rhythm;
        private string build_rhythm = "";
        public int rhythm_pos = 1;
        public int b_rhythm_pos = 1;
        double correction = 0;
        double beat_length = 0;
        public DispatcherTimer MainTimer = new DispatcherTimer(DispatcherPriority.Send);


        //
        // Timeline Effects Controls
        //

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (rhythm_pos >= 0)
            {
                if (rhythm_pos != 1) correction = beat_length * (rhythm_pos - 1) - Player.GetPosOfStream(Player.Stream_L);
                if (correction > 0) MainTimer.Interval = TimeSpan.FromSeconds(correction);
                else if (rhythm_pos > 2) MainTimer.Interval = TimeSpan.FromTicks(10);
            }
            else
            {
                b_rhythm_pos += 1;
                correction = beat_length * (build_rhythm.Length + rhythm_pos) - Player.GetPosOfStream(Player.Stream_B);
                if (correction > 0) MainTimer.Interval = TimeSpan.FromSeconds(correction);
                else MainTimer.Interval = TimeSpan.FromTicks(10);
            }
            TimeLine_Move(); //THIS MUST BE _AFTER_ THE TIMER.INTERVAL IS CORRECTED
            //CornerBlock.Text = rhythm_pos.ToString();
        }
        /// <summary> Check if displayed rhythm is too short and fills it if neccessary </summary>
        private void TimelineLenghtFill()
        {
            if (main.Display_Alpha.timeline_textBlock.Text.Length < 250)
                main.Display_Alpha.timeline_textBlock.Text = main.Display_Alpha.timeline_textBlock.Text = string.Concat(main.Display_Alpha.timeline_textBlock.Text, loop_rhythm);
        }

        private void TimeLine_Move()
        {
            //CornerBlock.Text = rhythm_pos.ToString();
            beat(main.Display_Alpha.timeline_textBlock.Text[2]);
            main.Display_Alpha.timeline_textBlock.Text = main.Display_Alpha.timeline_textBlock.Text.Remove(2, 1);
            TimelineLenghtFill();
            rhythm_pos += 1;
            if (rhythm_pos == loop_rhythm.Length) rhythm_pos = 0;
        }
        /// <summary> Plays the event according to char </summary>
        private void beat(char c)
        {
            if (main.Blackout_Rectangle.Opacity != 0 & c != '.')
            {
                main.SB_Blackout.Stop();
                main.blackouted = false;
                main.Blackout_Rectangle.Opacity = 0;
            }
            if (c != '.') switch (c)
                {
                    case 'o':
                        main.timeline_o();
                        break;
                    case 'O':
                        main.timeline_blur_hor();
                        break;
                    case 'x':
                        main.timeline_x();
                        break;
                    case 'X':
                        main.timeline_blur_vert();
                        break;
                    case '-':
                        main.timeline_pic_and_color();
                        break;
                    case '‑'://YES THATS A DIFFERENT ONE. Thanks to tylup RP.
                        main.timeline_pic_and_color();
                        break;
                    case '~':
                        main.timeline_fade();
                        break;
                    case ':':
                        main.timeline_color_change();
                        break;
                    case '*':
                        main.timeline_image_change();
                        break;
                    case '|':
                        main.timeline_blackout_short();
                        break;
                    case '+':
                        main.timeline_blackout();
                        break;
                    case 'i':
                        main.timeline_invert();
                        break;
                    case 'I':
                        main.timeline_invert_w_image();
                        break;
                    case '=':
                        main.timeline_fade_image();
                        break;
                    case '¤':
                        main.timeline_whiteout();
                        break;
                }
        }
    }
}

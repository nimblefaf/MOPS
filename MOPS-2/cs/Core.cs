using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using DiscordRPC;

namespace MOPS
{
    internal class Core
    {
        public Core()
        {
            if (Properties.Settings.Default.discordMode) discord_rpc_init();
            MainTimer.Tick += MainTimer_Tick;     
        }

        MainWindow main;
        public void SetReference(MainWindow window)
        {
            main = window;
        }

        Audio Player = new Audio();

        public bool muted = false;
        public int muted_volume;
        public int current_song_ind = 0;
        public int current_image_pos = 0;
        public string loop_rhythm;
        public string build_rhythm = "";
        public int rhythm_pos = 1;
        public int b_rhythm_pos = 1;
        double correction = 0;
        public double beat_length = 0;
        public DispatcherTimer MainTimer = new DispatcherTimer(DispatcherPriority.Send);


        public DiscordRpcClient discordRpcClient = new DiscordRpcClient("842763717179342858");
        public void discord_rpc_init()
        {
            if (!discordRpcClient.IsInitialized)
            {
                discordRpcClient = new DiscordRpcClient("842763717179342858");
                discordRpcClient.Initialize();
                if (beat_length == 0)
                    discordRpcClient.SetPresence(new RichPresence()
                    {
                        Details = "No song selected",
                        State = "¯\\_(ツ)_/¯",
                        Assets = new Assets()
                        {
                            LargeImageKey = "hues_csharp_main3",
                            LargeImageText = "That's Kyubey, The Cutest Waifu",

                        }
                    });
                else discordRpcClient.SetPresence(new RichPresence()
                {
                    Details = "Playing song",
                    State = main.RPM.allSongs[current_song_ind].title,
                    Assets = new Assets()
                    {
                        LargeImageKey = "hues_csharp_main3",
                        LargeImageText = "That's Kyubey, The Cutest Waifu",

                    }
                });
            }
        }


        private void MainTimer_Tick(object sender, EventArgs e)
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

        public void Change_Song(int i)
        {
            if (main.RPM.allSongs[i].buffer != null) Player.loop_mem = main.RPM.allSongs[i].buffer;
            else Player.loop_mem = main.RPM.GetAudioFromZip(main.RPM.ResPacks[main.RPM.Get_rp_of_song(i)].path, main.RPM.allSongs[i].filename);
            if (Player.loop_mem.Length != 0)
            {
                loop_rhythm = main.RPM.allSongs[i].rhythm;
                main.Display_Alpha.song_textBlock.Text = main.RPM.allSongs[i].title.ToUpper();
                main.Display_Alpha.timeline_textBlock.Text = main.RPM.allSongs[i].rhythm;
                current_song_ind = main.songs_listbox.SelectedIndex;

                rhythm_pos = 1;
                b_rhythm_pos = 1;
                if (main.RPM.allSongs[i].buildup_filename != null & ((BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.On | ((BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.Once & !main.RPM.allSongs[i].buildup_played)))
                {
                    if ((BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.Once) main.RPM.allSongs[i].buildup_played = true;
                    if (main.RPM.allSongs[i].buffer != null) Player.build_mem = main.RPM.allSongs[i].buildup_buffer;
                    else Player.build_mem = main.RPM.GetAudioFromZip(main.RPM.ResPacks[main.RPM.Get_rp_of_song(i)].path, main.RPM.allSongs[i].buildup_filename);
                    Player.Play_With_Buildup();
                    build_rhythm = main.RPM.allSongs[i].buildupRhythm;
                    int expected_size = Convert.ToInt32(Math.Round(Audio.GetTimeOfStream(Player.Stream_B) / (Audio.GetTimeOfStream(Player.Stream_L) / loop_rhythm.Length)));
                    if (build_rhythm == null) //In case there is buildup music without beat string
                    {
                        build_rhythm = new string('.', expected_size);
                    }
                    else if (build_rhythm.Length < expected_size)
                    {
                        build_rhythm += new string('.', expected_size - build_rhythm.Length - 1);
                    }
                    if (main.Display_Alpha.timeline_textBlock.Text.Length < 250) main.Display_Alpha.timeline_textBlock.Text = string.Concat(build_rhythm, main.Display_Alpha.timeline_textBlock.Text);
                    else main.Display_Alpha.timeline_textBlock.Text = build_rhythm;
                    rhythm_pos = -expected_size;
                }
                else Player.Play_Without_Buildup();

                main.Display_Alpha.timeline_textBlock.Text = ">>" + main.Display_Alpha.timeline_textBlock.Text;

                beat_length = Audio.GetTimeOfStream(Player.Stream_L) / loop_rhythm.Length;

                TimelineLenghtFill();
                //Timer.Interval = TimeSpan.FromTicks(Convert.ToInt64(beat_length * 1000 * 10000));
                MainTimer.Interval = TimeSpan.FromSeconds(beat_length);
                main.ShortBlackoutTimer.Interval = MainTimer.Interval;

                if (Properties.Settings.Default.discordMode)
                {
                    discordRpcClient.SetPresence(new RichPresence()
                    {
                        Details = "Playing song",
                        State = main.RPM.allSongs[i].title,
                        Assets = new Assets()
                        {
                            LargeImageKey = "hues_csharp_main3",
                            LargeImageText = "That's Kyubey, The Cutest Waifu",
                        }
                    }
                    );
                }

                Player.Play();
                TimeLine_Move();
                MainTimer.Start();
            }
            else StopSong();

        }
        public void StopSong()
        {
            MainTimer.Stop();
            Player.Stop();
            main.Display_Alpha.song_textBlock.Text = "NONE";
            beat_length = 0;
            main.Display_Alpha.timeline_textBlock.Text = ">>.";
            if (Properties.Settings.Default.discordMode)
            {
                discordRpcClient.SetPresence(new RichPresence()
                {
                    Details = "No song selected",
                    State = "¯\\_(ツ)_/¯",
                    Assets = new Assets()
                    {
                        LargeImageKey = "hues_csharp_main3",
                        LargeImageText = "That's Kyubey, The Cutest Waifu",

                    }
                }
                );
            }
        }

        public void ChangeVolume(int Delta)
        {
            if (muted) toggle_mute();
            if (Delta < 0 & Player.Volume != 0)
            {
                Player.Volume -= 10;
                main.Display_Alpha.volume_textBlock.Text = Player.Volume.ToString();
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
            }
            if (Delta > 0 & Player.Volume != 100)
            {
                if (muted) toggle_mute();
                Player.Volume += 10;
                main.Display_Alpha.volume_textBlock.Text = Player.Volume.ToString();
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
            }
        }
        public void toggle_mute()
        {
            if (!muted)
            {
                muted_volume = Player.Volume;
                main.Display_Alpha.volume_textBlock.Text = Player.Volume.ToString();
                Player.Volume = 0;
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
                muted = true;
            }
            else
            {
                Player.Volume = muted_volume;
                main.Display_Alpha.volume_textBlock.Text = Player.Volume.ToString();
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
                muted = false;
            }
        }

        public int CountDots()
        {
            int Count = 0;
            int limit = build_rhythm.Length + (loop_rhythm.Length * 3);
            for (int i = 3; i < limit; i++)
            {
                if (main.Display_Alpha.timeline_textBlock.Text[i] != '.') break;
                else
                {
                    Count++;
                    if (main.Display_Alpha.timeline_textBlock.Text.Length - 1 == i) main.Display_Alpha.timeline_textBlock.Text = main.Display_Alpha.timeline_textBlock.Text + loop_rhythm;
                    if (i == limit - 1)
                    {
                        Count = 0;
                        break;
                    }
                }
            }
            return Count;
        }
    }
}

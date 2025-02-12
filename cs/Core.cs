﻿using DiscordRPC;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace HuesSharp
{
    internal class Core
    {
        public MainWindow MainWin;
        public Core()
        {
            if (Properties.Settings.Default.discordMode) discord_rpc_init();
            MainTimer.Tick += MainTimer_Tick;
            MainTimer.Interval = TimeSpan.FromTicks(10);
        }
        public void SetReferences()
        {
            MainWin = (MainWindow)Application.Current.MainWindow;
            UIHandler = new UIHandler();
        }

        Random rnd = new Random();

        public RPManager RPM = new RPManager();
        public Audio Player = new Audio();
        public UIHandler UIHandler;

        public bool muted = false;
        private bool build_is_playing = false;
        public int current_volume = 50;
        public int muted_volume;
        public int current_song_ind = 0;
        public int current_image_pos = 0;
        public string loop_rhythm;
        public string build_rhythm = "";
        public string current_timeline = "";
        public int rhythm_pos = 1;
        public int b_rhythm_pos = 1;
        public double beat_length = 0;
        public DispatcherTimer MainTimer = new DispatcherTimer(DispatcherPriority.Send);

        /// <summary>
        /// List of enabled songs displayed in songs_listbox.
        /// </summary>
        public ObservableCollection<rdata> enabled_songs = new ObservableCollection<rdata>();
        /// <summary>
        /// List of enabled images displayed in images_listbox.
        /// </summary>
        public ObservableCollection<rdata> enabled_images = new ObservableCollection<rdata>();


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
                    State = RPM.allSongs[current_song_ind].title,
                    Assets = new Assets()
                    {
                        LargeImageKey = "hues_csharp_main3",
                        LargeImageText = "",

                    }
                });
            }
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (rhythm_pos > 0)
            {
                if (beat_length * rhythm_pos < Player.GetPosOfStream(Player.Stream_Loop) | beat_length * (rhythm_pos - 1) > Player.GetPosOfStream(Player.Stream_Loop))
                    TimeLine_Move();
            }
            else if (rhythm_pos == 0 & build_is_playing)
            {
                if (Player.GetPosOfStream(Player.Stream_Loop) > 0)
                {
                    TimeLine_Move();
                    build_is_playing = false;
                }
            }
            else if (rhythm_pos == 0)
            {
                if (Player.GetPosOfStream(Player.Stream_Loop) < beat_length)
                    TimeLine_Move();
            }
            else
            {
                if (beat_length * b_rhythm_pos < Player.GetPosOfStream(Player.Stream_Buildup))
                    TimeLine_Move();
            }
        }

        /// <summary> Check if displayed rhythm is too short and fills it if neccessary </summary>
        private void TimelineCheckAndFill()
        {
            if (current_timeline.Length < 250) current_timeline += loop_rhythm;
        }

        private void TimeLine_Move()
        {
            Beat(current_timeline[0]);
            current_timeline = current_timeline.Remove(0, 1);
            TimelineCheckAndFill();
            UIHandler.UpdateTimeline(current_timeline);
            rhythm_pos += 1;
            if (rhythm_pos < 0) b_rhythm_pos++;
            if (rhythm_pos == loop_rhythm.Length) rhythm_pos = 0;
        }

        /// <summary> Plays the event according to char </summary>
        private void Beat(char c)
        {
            if (MainWin.Blackout_Rectangle.Opacity != 0 & c != '.')
            {
                MainWin.SB_Blackout.Stop();
                MainWin.blackouted = false;
                MainWin.Blackout_Rectangle.Opacity = 0;
            }
            if (c != '.') switch (c)
                {
                    case 'o':
                        MainWin.timeline_o();
                        break;
                    case 'O':
                        MainWin.timeline_blur_hor();
                        break;
                    case 'x':
                        MainWin.timeline_x();
                        break;
                    case 'X':
                        MainWin.timeline_blur_vert();
                        break;
                    case '-':
                        MainWin.timeline_pic_and_color();
                        break;
                    case '‑'://YES THATS A DIFFERENT ONE. Thanks to tylup RP.
                        MainWin.timeline_pic_and_color();
                        break;
                    case '~':
                        MainWin.timeline_fade();
                        break;
                    case ':':
                        MainWin.timeline_color_change();
                        break;
                    case '*':
                        MainWin.timeline_image_change();
                        break;
                    case '|':
                        MainWin.timeline_blackout_short();
                        break;
                    case '+':
                        MainWin.timeline_blackout();
                        break;
                    case 'i':
                        MainWin.timeline_invert();
                        break;
                    case 'I':
                        MainWin.timeline_invert_w_image();
                        break;
                    case '=':
                        MainWin.timeline_fade_image();
                        break;
                    case '¤':
                        MainWin.timeline_whiteout();
                        break;
                    case 's':
                        MainWin.timeline_slice_hor();
                        break;
                    case 'S':
                        MainWin.timeline_slice_hor_w_im();
                        break;
                    case 'v':
                        MainWin.timeline_slice_ver();
                        break;
                    case 'V':
                        MainWin.timeline_slice_ver_w_im();
                        break;
                    case '#':
                        MainWin.timeline_slice_double();
                        break;
                    default:
                        MainWin.timeline_pic_and_color();
                        break;
                }
        }

        public void Change_Song(int SelectedSong)
        {
            int i = enabled_songs[SelectedSong].Ind;
            current_song_ind = SelectedSong;

            MainWin.Events_Stop();
            Player.build_mem = null;
            Player.loop_mem = null;
            if (RPM.allSongs[i].buffer != null) Player.loop_mem = RPM.allSongs[i].buffer;
            else Player.loop_mem = RPM.GetAudioFromZip(RPM.ResPacks[RPM.Get_rp_of_song(i)].path, RPM.allSongs[i].filename);
            if (Player.loop_mem.Length != 0)
            {
                loop_rhythm = RPM.allSongs[i].rhythm;
                build_rhythm = "";
                current_timeline = loop_rhythm;

                rhythm_pos = 0;
                b_rhythm_pos = 0;
                if (RPM.allSongs[i].buildup_filename != null & ((BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.On | ((BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.Once & !RPM.allSongs[i].buildup_played)))
                {
                    if ((BuildUpMode)Properties.Settings.Default.buildUpMode == BuildUpMode.Once) RPM.allSongs[i].buildup_played = true;
                    if (RPM.allSongs[i].buffer != null) Player.build_mem = RPM.allSongs[i].buildup_buffer;
                    else Player.build_mem = RPM.GetAudioFromZip(RPM.ResPacks[RPM.Get_rp_of_song(i)].path, RPM.allSongs[i].buildup_filename);
                    Player.Play_With_Buildup();
                    build_rhythm = RPM.allSongs[i].buildupRhythm;
                    int expected_size = Convert.ToInt32(Math.Round(Audio.GetTimeOfStream(Player.Stream_Buildup) / (Audio.GetTimeOfStream(Player.Stream_Loop) / loop_rhythm.Length)));
                    if (build_rhythm == null) //In case there is buildup music without beat string
                    {
                        build_rhythm = new string('.', expected_size);
                    }
                    else if (build_rhythm.Length < expected_size)
                    {
                        build_rhythm += new string('.', expected_size - build_rhythm.Length);
                    }
                    current_timeline = string.Concat(build_rhythm, loop_rhythm);
                    rhythm_pos = -expected_size;
                    build_is_playing = true;
                }
                else Player.Play_Without_Buildup();
                UIHandler.UpdateSongInfo(RPM.allSongs[i]);

                beat_length = Audio.GetTimeOfStream(Player.Stream_Loop) / loop_rhythm.Length;

                TimelineCheckAndFill();
                MainWin.ShortBlackoutTimer.Interval = TimeSpan.FromSeconds(beat_length);

                if (Properties.Settings.Default.discordMode)
                {
                    discordRpcClient.SetPresence(new RichPresence()
                    {
                        Details = "Playing song",
                        State = RPM.allSongs[i].title,
                        Assets = new Assets()
                        {
                            LargeImageKey = "hues_csharp_main3",
                            LargeImageText = "",
                        }
                    }
                    );
                }

                Player.Play();
                MainTimer.Start();
                UIHandler.AudioTimer.Start();
            }
            else StopSong();

        }
        public void StopSong()
        {
            MainTimer.Stop();
            if ((UIStyle)Properties.Settings.Default.uiStyle == UIStyle.Retro) UIHandler.AudioTimer.Stop();
            Player.Stop();
            beat_length = 0;
            UIHandler.UpdateSongInfo(RPM.allSongs[0], true);
            if (Properties.Settings.Default.discordMode)
            {
                discordRpcClient.SetPresence(new RichPresence()
                {
                    Details = "No song selected",
                    State = "¯\\_(ツ)_/¯",
                    Assets = new Assets()
                    {
                        LargeImageKey = "hues_csharp_main3",
                        LargeImageText = "",
                    }
                }
                );
            }
        }


        public void next_song()
        {
            if (enabled_songs.Count > 1)
            {
                if (current_song_ind == enabled_songs.Count - 1) Change_Song(0);
                else Change_Song(enabled_songs[current_song_ind + 1].Ind);
            }
        }
        public void prev_song()
        {
            if (enabled_songs.Count > 1)
            {
                if (current_song_ind == 0) Change_Song(enabled_songs[enabled_songs.Count - 1].Ind);
                else Change_Song(enabled_songs[current_song_ind - 1].Ind);
            }
        }

        public void random_song()
        {
            if (enabled_songs.Count != 0)
            {
                Change_Song((current_song_ind + rnd.Next(1, enabled_songs.Count - 1)) % enabled_songs.Count);
            }
        }

        public void next_image()
        {
            if (enabled_images.Count > 1)
            {
                if (current_image_pos == enabled_images.Count - 1) MainWin.ImageChange(0);
                else MainWin.ImageChange(current_image_pos + 1);
                MainWin.full_auto_mode = false;
            }
        }
        public void prev_image()
        {
            if (enabled_images.Count > 1)
            {
                if (current_image_pos == 0) MainWin.ImageChange(enabled_images.Count - 1);
                else MainWin.ImageChange(current_image_pos - 1);
                MainWin.full_auto_mode = false;
            }
        }

        public void StartAgain()
        {
            MainTimer.Stop();
            rhythm_pos = 0;
            b_rhythm_pos = 0;
            if (Player.build_mem != null)
            {
                Player.Play_With_Buildup();
                int expected_size = Convert.ToInt32(Math.Round(Audio.GetTimeOfStream(Player.Stream_Buildup) / (Audio.GetTimeOfStream(Player.Stream_Loop) / loop_rhythm.Length)));
                current_timeline = string.Concat(build_rhythm, loop_rhythm);
                rhythm_pos = -expected_size;
                build_is_playing = true;
            }
            else
            {
                Player.Play_Without_Buildup();
                current_timeline = loop_rhythm;
            }
            TimelineCheckAndFill();
            MainTimer.Start();
            Player.Play();
        }

        public void ChangeVolume(int Delta)
        {
            if (muted) toggle_mute();
            if (Delta < 0 & current_volume != 0)
            {
                current_volume -= 10;
                Player.Volume = current_volume;
                UIHandler.UpdateVolumeDisplayed(Player.Volume);
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
            }
            if (Delta > 0 & current_volume != 100)
            {
                if (muted) toggle_mute();
                current_volume += 10;
                Player.Volume = current_volume;
                UIHandler.UpdateVolumeDisplayed(Player.Volume);
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
            }
        }
        public void SetVolume(int Volume)
        {
            current_volume = Volume;
            Player.Volume = current_volume;
            Player.SetVolumeToStream(Player.Channel, Player.Volume);
            UIHandler.UpdateVolumeDisplayed(Player.Volume);
        }
        public void toggle_mute()
        {
            if (muted)
            {
                current_volume = muted_volume;
                Player.Volume = muted_volume;
                UIHandler.UpdateVolumeDisplayed(Player.Volume);
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
                muted = false;
            }
            else
            {
                muted_volume = current_volume;
                UIHandler.UpdateVolumeDisplayed(Player.Volume);
                current_volume = 0;
                Player.Volume = 0;
                Player.SetVolumeToStream(Player.Channel, Player.Volume);
                muted = true;
            }
            UIHandler.UpdateVolumeDisplayed(current_volume);
        }

        public int CountDots()
        {
            int Count = 0;
            int limit = build_rhythm.Length + (loop_rhythm.Length * 2);
            for (int i = 3; i < limit; i++)
            {
                if (current_timeline[i] != '.') break;
                else
                {
                    Count++;
                    if (current_timeline.Length - 1 == i) current_timeline += loop_rhythm;
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

    /// <summary>
    ///     The selected hue blend mode for drawing the image.
    /// </summary>
    public enum BlendMode
    {
        /// <summary>
        /// Image is alpha-blended over the hue.
        /// </summary>
        Plain = 0,
        /// <summary>
        /// Image is alpha-blended over the hue at 70% opacity.
        /// </summary>
        Alpha = 1,
        /// <summary>
        /// Image is alpha-blended over a white background.The hue is blended over the image with "hard light" mode at 70% opacity.
        /// </summary>
        HardLight = 2
    }
    public enum BuildUpMode
    {
        Off = 0,
        On = 1,
        Once = 2
    }
    public enum ColorSet
    {
        Normal = 0,
        Pastel = 1,
        Weed = 2
    }
    public enum BlurAmount
    {
        Low = 0,
        Medium = 1,
        High = 2
    }
    public enum BlurDecay
    {
        Slow = 0,
        Medium = 1,
        Fast = 2,
        Fastest = 3
    }
    public enum BlurQuality
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }
    public enum UIStyle
    {
        Alpha = 0,
        Mini = 1,
        Retro = 2,
        Weed = 3,
        Modern = 4,
    }
    public enum anisotropicBlurPower
    {
        Off = 0,
        Low = 1, 
        Medium = 2,
        High = 3
    }
}

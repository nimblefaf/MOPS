using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using System.Runtime.InteropServices;
using Un4seen.Bass.AddOn.Mix;


namespace MOPS
{
    public class Audio
    {
        
        public static string err;
        public static bool check;
        
        /// <summary>
        /// Частота дискретизации
        /// </summary>
        private static readonly int HZ = 44100;
        /// <summary>
        /// Состояние инициалиации
        /// </summary>
        public static bool InitDefaultDevice;
        /// <summary>
        /// Канал
        /// </summary>
        public int Stream_L;
        /// <summary>
        /// Громкость
        /// </summary>
        public int Volume = 50;

        public int Channel;
        public int Stream_B;

        long build_len;
        long loop_len;
        public double time_of_build;



        GCHandle point_B = new GCHandle();
        GCHandle point_L = new GCHandle();

        public byte[] loop_mem;
        public byte[] build_mem;



        private static bool InitBass(int hz)
        {
            if (!InitDefaultDevice)
                InitDefaultDevice = Bass.BASS_Init(-1, hz, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            return InitDefaultDevice;
        }
        
        public void Play_With_Buildup()
        {
            Stop();
            if (InitBass(HZ))
            {
                Channel = BassMix.BASS_Mixer_StreamCreate(HZ, 2, BASSFlag.BASS_MIXER_END);

                point_B = GCHandle.Alloc(build_mem, GCHandleType.Pinned);
                point_L = GCHandle.Alloc(loop_mem, GCHandleType.Pinned);

                Stream_B = Bass.BASS_StreamCreateFile(point_B.AddrOfPinnedObject(), 0, build_mem.LongLength, BASSFlag.BASS_STREAM_DECODE);
                build_len = Bass.BASS_ChannelGetLength(Stream_B, BASSMode.BASS_POS_BYTES);
                time_of_build = GetTimeOfStream(Stream_B);

                Stream_L = Bass.BASS_StreamCreateFile(point_L.AddrOfPinnedObject(), 0, loop_mem.LongLength, BASSFlag.BASS_STREAM_DECODE);
                loop_len = Bass.BASS_ChannelGetLength(Stream_L);

                BassMix.BASS_Mixer_StreamAddChannel(Channel, Stream_B, BASSFlag.BASS_DEFAULT);
                BassMix.BASS_Mixer_StreamAddChannelEx(Channel, Stream_L, BASSFlag.BASS_MIXER_NORAMPIN, build_len, 0);
                
                _loopSync = BassMix.BASS_Mixer_ChannelSetSync(Stream_L, BASSSync.BASS_SYNC_POS | BASSSync.BASS_SYNC_MIXTIME, loop_len, _loopSyncCallback, new IntPtr(1));
            }
        }

        private int _loopSync = 0;
        private SYNCPROC _loopSyncCallback;
        public Audio()
        {
            _loopSyncCallback = new SYNCPROC(EndSync);
        }
        private void EndSync(int syncHandle, int channel, int data, IntPtr user)
        {
            BassMix.BASS_Mixer_ChannelSetPosition(Stream_L, user.ToInt64());
        }

        public void Play_Without_Buildup()
        {
            Stop();
            if (InitBass(HZ))
            {
                Stream_L = Bass.BASS_SampleLoad(loop_mem, 0, loop_mem.Length, 1, BASSFlag.BASS_SAMPLE_LOOP);
                Channel = Bass.BASS_SampleGetChannel(Stream_L, false);
            }
        }

        public void Play()
        {
            Bass.BASS_ChannelSetAttribute(Channel, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100f);
            Bass.BASS_ChannelPlay(Channel, false);
        }
        

        /// <summary>
        /// Длительность канала в секундах
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static double GetTimeOfStream(int stream)
        {
            long TimeBytes = Bass.BASS_ChannelGetLength(stream);
            double Time = Bass.BASS_ChannelBytes2Seconds(stream, TimeBytes);
            //return (int)Time;
            return Time;
        }

        /// <summary>
        /// Текущая позиция в секундах
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public double GetPosOfStream(int stream)
        {
            long pos;
            if (point_L.IsAllocated) pos = BassMix.BASS_Mixer_ChannelGetPosition(stream, BASSMode.BASS_POS_BYTES);
            else pos = Bass.BASS_ChannelGetPosition(stream);
            return Bass.BASS_ChannelBytes2Seconds(stream, pos);
        }

        /// <summary>
        /// Стоп
        /// </summary>
        public void Stop()
        {
            Bass.BASS_ChannelStop(Channel);
            if (point_L.IsAllocated) point_L.Free();
            if (point_B.IsAllocated) point_B.Free();
            Bass.BASS_StreamFree(Channel);
            Bass.BASS_SampleFree(Channel);

            Bass.BASS_StreamFree(Stream_B);
            Bass.BASS_StreamFree(Stream_L);

            ////Uncomment this in case of unsolvable memory leak.
            ////The whole BASS module will be freed, creating a small pause before new song is played
            //Bass.BASS_Free();
            //InitDefaultDevice = false;
        }


        public static void SetPosOfScroll(int stream, int pos)
        {
            Bass.BASS_ChannelSetPosition(stream, (double)pos);
        }
        /// <summary>
        /// Установка громкости
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="vol"></param>
        public void SetVolumeToStream(int stream, int vol)
        {
            Volume = vol;
            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100F);
            Bass.BASS_ChannelPlay(stream, false);
        }

    }
}

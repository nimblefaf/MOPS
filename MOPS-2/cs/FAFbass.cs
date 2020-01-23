using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace MOPS_2
{
    public static class FAFbass
    {
        public static string err;

        /// <summary>
        /// Частота дискретизации
        /// </summary>
        private static int HZ = 44100;
        /// <summary>
        /// Состояние инициалиации
        /// </summary>
        public static bool InitDefaultDevice;
        /// <summary>
        /// Канал
        /// </summary>
        public static int Stream;
        /// <summary>
        /// Громкость
        /// </summary>
        public static int Volume = 50;

        public static int Channel;

        /// <summary>
        /// Ининциализация Bass.dll
        /// </summary>
        /// <param name="hz"></param>
        /// <returns></returns>
        private static bool InitBass(int hz)
        {
            if (!InitDefaultDevice)
                InitDefaultDevice = Bass.BASS_Init(-1, hz, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            return InitDefaultDevice;
        }
        
        /// <summary>
        /// Воспроизвидение
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="vol"></param>
        public static void Play(string filename, int vol)
        {
            Stop();
            if (InitBass(HZ))
            {
                //Bass.BASS_SampleLoad();
                
                Stream = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_SAMPLE_LOOP);
                
                if (Stream != 0)
                {
                    Volume = vol;
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100f);
                    Bass.BASS_ChannelPlay(Stream, false);
                }
            }
        }
        /// <summary>
        /// Воспроизвидение
        /// </summary>
        /// <param name="mem"></param>
        /// <param name="vol"></param>
        public static void PlayLoop(byte[] mem, int vol)
        {
            Stop();
            if (InitBass(HZ))
            {
                //Bass.BASS_SampleLoad();
                Stream = Bass.BASS_SampleLoad(mem, 0, mem.Length, 1, BASSFlag.BASS_SAMPLE_LOOP);
                Channel = Bass.BASS_SampleGetChannel(Stream, false);



                if (Stream != 0)
                {
                    Volume = vol;
                    Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100f);
                    Bass.BASS_ChannelPlay(Channel, false);
                    err = Bass.BASS_ErrorGetCode().ToString();
                }
            }
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
        public static int GetPosOfStream(int stream)
        {
            long pos = Bass.BASS_ChannelGetPosition(stream);
            int posSec = (int)Bass.BASS_ChannelBytes2Seconds(stream, pos);
            return posSec;
        }

        /// <summary>
        /// Стоп
        /// </summary>
        public static void Stop()
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            Bass.BASS_SampleFree(Stream);
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
        public static void SetVolumeToStream(int stream, int vol)
        {
            Volume = vol;
            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100F);
            Bass.BASS_ChannelPlay(Stream, false);
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;

namespace HuesSharp
{
    public class RP
    {
        public string name;
        public string author;
        public string description;
        public string link;
        public string path;
        public int pics_start;
        public int pics_count;
        public int songs_start;
        public int songs_count;
        public bool enabled;

        public RP()
        {
            name = "";
            author = "";
            description = "";
            link = "";
            path = "";
        }
    }



    public struct Pics
    {
        public string name;
        public string fullname;
        public string source;
        public string source_other;
        public string align;
        public BitmapSource pic;
        public BitmapSource[] animation;
        public int[] frameDuration;
        public double beatsPerAnim;
        public double syncOffset;
        public bool enabled;
    }

    public struct Songs
    {
        public string title;
        public string filename;
        public string source;
        public string rhythm;
        public string buildup_filename;
        public string buildupRhythm;
        public byte[] buffer;
        public byte[] buildup_buffer;
        public bool enabled;
        public bool buildup_played;
    }

    public class RPManager
    {
        public PicConverter picConverter = new PicConverter();
        public byte[] GetAudioFromZip(string RP_path, string filename)
        {
            byte[] res = new byte[0];
            try
            {
                using (FileStream zipToOpen = new FileStream(RP_path, FileMode.Open))
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read, false, Encoding.Default))
                    foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains(filename)))
                        using (var stream = entry.Open())
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            memoryStream.Position = 0;
                            using (BinaryReader br = new BinaryReader(memoryStream))
                            {
                                res = br.ReadBytes((int)memoryStream.Length);
                                break;
                            }
                        }
            }
            catch (IOException e)
            {
                MessageBox.Show("Oops, error occured!\n" + e.Message);
            }

            return res;
        }


        static string RemoveDiacritics(string text) //Because German names are suffering and 'ä' can break everything
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        static int GetNumOfZipEntries(string filePath)
        {
            using (FileStream zipToOpen = new FileStream(filePath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read, false, Encoding.Default))
                {
                    return archive.Entries.Count;
                }
            }
        }

        public int Get_rp_of_song(int ind)
        {
            for (int i = ResPacks.Length - 1; i >= 0; i--)
            {
                if (ind >= ResPacks[i].songs_start) return i;
            }
            return -1;
        }
        public int Get_rp_of_image(int ind)
        {
            for (int i = ResPacks.Length - 1; i >= 0; i--)
            {
                if (ind > ResPacks[i].pics_start) return i;
            }
            return -1;
        }

        public static string status = "Loader Idle";
        /// <summary>
        /// Collection of loaded Resource Packs
        /// </summary>
        public RP[] ResPacks = new RP[0];
        /// <summary>
        /// Collection of all loaded songs
        /// </summary>
        public Songs[] allSongs = new Songs[0];
        /// <summary>
        /// Collection of all loaded images
        /// </summary>
        public Pics[] allPics = new Pics[0];

        /// <summary>
        /// Loads and parses a local resource pack file in .zip format
        /// </summary>
        /// <param name="respacks"></param>
        /// <returns></returns>
        public Pics[] SupremeReader(string[] respacks, BackgroundWorker worker, DoWorkEventArgs e)
        {
            Pics[] Transfer = new Pics[0];
            XmlDocument info_xml = new XmlDocument();
            XmlDocument songs_xml = new XmlDocument();
            XmlDocument images_xml = new XmlDocument();
            Dictionary<string, BitmapImage> PicsBuffer = new Dictionary<string, BitmapImage> { };
            Dictionary<string, byte[]> SongsBuffer = new Dictionary<string, byte[]> { };
            int TotalEntries = 0;
            int ProccessedEntries = 0;
            foreach (string arch in respacks) TotalEntries += GetNumOfZipEntries(arch);


            XmlReaderSettings readerSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                DtdProcessing = DtdProcessing.Ignore,
                CheckCharacters = false
            };
            foreach (string target_path in respacks)
            {
                using (FileStream zipToOpen = new FileStream(target_path, FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read, false, Encoding.Default))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (entry.Name.ToLower() == "info.xml")
                                    using (var stream = entry.Open())
                                    using (var reader = new StreamReader(stream))
                                    using (var xmlread = XmlReader.Create(reader, readerSettings))
                                        info_xml.Load(xmlread);
                                if (entry.Name.ToLower() == "songs.xml")
                                    using (var stream = entry.Open())
                                    using (var reader = new StreamReader(stream))
                                    {
                                        string doc = reader.ReadToEnd();
                                        doc = doc.Replace("&", "&amp;");
                                        doc = doc.Replace("&amp;amp;", "&amp;");
                                        using (var inner = new MemoryStream(Encoding.UTF8.GetBytes(doc)))
                                        using (var xmlread = XmlReader.Create(inner, readerSettings))
                                        {
                                            songs_xml.Load(xmlread);
                                        }
                                    }
                                if (entry.Name.ToLower() == "images.xml")
                                    using (var stream = entry.Open())
                                    using (var reader = new StreamReader(stream))
                                    {
                                        using (var xmlread = XmlReader.Create(reader, readerSettings))
                                            images_xml.Load(xmlread);
                                    }
                            }
                            if (entry.FullName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase)/* | entry.FullName.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase)*/)
                            {
                                using (var stream = entry.Open())
                                using (var memoryStream = new MemoryStream())
                                {
                                    stream.CopyTo(memoryStream);
                                    memoryStream.Position = 0;
                                    using (BinaryReader br = new BinaryReader(memoryStream))
                                    {
                                        SongsBuffer.Add(entry.Name.Substring(0, entry.Name.Length/* - 4*/), br.ReadBytes((int)memoryStream.Length));
                                    }
                                }
                            }
                            if (entry.FullName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) | entry.FullName.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) | entry.FullName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase))
                            {
                                using (var stream = entry.Open())
                                using (var memoryStream = new MemoryStream())
                                {
                                    stream.CopyTo(memoryStream);
                                    memoryStream.Position = 0;

                                    var pic = new BitmapImage();

                                    pic.BeginInit();
                                    pic.CacheOption = BitmapCacheOption.OnLoad;
                                    pic.StreamSource = memoryStream;
                                    pic.EndInit();
                                    string name = entry.Name.Substring(0, entry.Name.Length - 4);
                                    PicsBuffer.Add(name, pic);
                                }
                            }
                            ProccessedEntries++;
                            int percentComplete = (int)((float)ProccessedEntries / TotalEntries * 100);
                            if (worker.WorkerReportsProgress) worker.ReportProgress(percentComplete);
                        }
                    }
                }

                if (info_xml.HasChildNodes)
                {
                    Array.Resize(ref ResPacks, ResPacks.Length + 1);
                    ResPacks[ResPacks.Length - 1] = new RP();

                    foreach (XmlNode node in info_xml.DocumentElement)
                    {
                        if (node.Name == "name") ResPacks[ResPacks.Length - 1].name = node.InnerText;
                        else if (node.Name == "author") ResPacks[ResPacks.Length - 1].author = node.InnerText;
                        else if (node.Name == "description") ResPacks[ResPacks.Length - 1].description = node.InnerText;
                        else if (node.Name == "link") ResPacks[ResPacks.Length - 1].link = node.InnerText;
                    }

                    ResPacks[ResPacks.Length - 1].songs_start = allSongs.Length;

                    ResPacks[ResPacks.Length - 1].enabled = true;
                    ResPacks[ResPacks.Length - 1].path = target_path;

                    if (songs_xml.HasChildNodes) parseSongs(songs_xml, SongsBuffer, false);
                    else ResPacks[ResPacks.Length - 1].songs_count = 0;

                    if (images_xml.HasChildNodes) parsePics(images_xml, PicsBuffer, ref Transfer);
                    else ResPacks[ResPacks.Length - 1].pics_count = 0;
                    if (ResPacks.Length - 1 == 0) ResPacks[ResPacks.Length - 1].pics_start = 0;
                    else ResPacks[ResPacks.Length - 1].pics_start = ResPacks[ResPacks.Length - 2].pics_start + ResPacks[ResPacks.Length - 2].pics_count;

                    PicsBuffer.Clear();
                    SongsBuffer.Clear();
                    info_xml = new XmlDocument();
                    songs_xml = new XmlDocument();
                    images_xml = new XmlDocument();
                    //return Transfer;
                }
                else
                {
                    //MainWindow.set.Status_textBlock.Text = "info.xml not found";
                    PicsBuffer.Clear();
                    SongsBuffer.Clear();
                    info_xml = new XmlDocument();
                    songs_xml = new XmlDocument();
                    images_xml = new XmlDocument();
                    //return null;
                }
            }
            for (int j = 0; j < Transfer.Length; j++)
            {
                Transfer[j].pic.Freeze();
                if (Transfer[j].animation != null)
                {
                    foreach (BitmapSource i in Transfer[j].animation) i.Freeze();
                }
            }
            //foreach (Pics p in Transfer)
            //{
            //    p.pic.Freeze();
            //    if (p.animation != null)
            //    {
            //        foreach (BitmapSource i in p.animation) i.Freeze();
            //    }
            //}
            return Transfer;
        }

        /// <summary>
        /// Parses a remote resource pack file in .zip format
        /// </summary>
        /// <param name="target_path"></param>
        /// <returns></returns>
        public Pics[] WebReader(byte[] data, BackgroundWorker worker, DoWorkEventArgs e)
        {
            Pics[] Transfer = new Pics[0];
            XmlDocument info_xml = new XmlDocument();
            XmlDocument songs_xml = new XmlDocument();
            XmlDocument images_xml = new XmlDocument();
            Dictionary<string, BitmapImage> PicsBuffer = new Dictionary<string, BitmapImage> { };
            Dictionary<string, byte[]> SongsBuffer = new Dictionary<string, byte[]> { };

            XmlReaderSettings readerSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                DtdProcessing = DtdProcessing.Ignore,
                CheckCharacters = false
            };

            //MainWindow.set.Status_textBlock.Text = "Loading ZIP...";
            using (MemoryStream zipToOpen = new MemoryStream(data))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read, false, Encoding.Default))
                {
                    int ProgressCount = 0;
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        ProgressCount++;
                        if (entry.FullName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (entry.Name.ToLower() == "info.xml")
                            {
                                using (var stream = entry.Open())
                                using (var reader = new StreamReader(stream))
                                using (var xmlread = XmlReader.Create(reader, readerSettings))
                                    info_xml.Load(xmlread);
                            }



                            if (entry.Name.ToLower() == "songs.xml")
                                using (var stream = entry.Open())
                                using (var reader = new StreamReader(stream))
                                {
                                    string doc = reader.ReadToEnd();
                                    doc = doc.Replace("&", "&amp;");
                                    doc = doc.Replace("&amp;amp;", "&amp;");
                                    using (var inner = new MemoryStream(Encoding.UTF8.GetBytes(doc)))
                                    using (var xmlread = XmlReader.Create(inner, readerSettings))
                                    {
                                        songs_xml.Load(xmlread);
                                    }
                                }
                            if (entry.Name.ToLower() == "images.xml")
                                using (var stream = entry.Open())
                                using (var reader = new StreamReader(stream))
                                {
                                    using (var xmlread = XmlReader.Create(reader, readerSettings))
                                        images_xml.Load(xmlread);
                                }
                        }
                        if (entry.FullName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase) | entry.FullName.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var stream = entry.Open())
                            using (var memoryStream = new MemoryStream())
                            {
                                stream.CopyTo(memoryStream);
                                memoryStream.Position = 0;
                                using (BinaryReader br = new BinaryReader(memoryStream))
                                {
                                    SongsBuffer.Add(entry.Name.Substring(0, entry.Name.Length - 4), br.ReadBytes((int)memoryStream.Length));
                                }
                            }
                        }
                        if (entry.FullName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) | entry.FullName.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) | entry.FullName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var stream = entry.Open())
                            using (var memoryStream = new MemoryStream())
                            {
                                stream.CopyTo(memoryStream);
                                memoryStream.Position = 0;

                                var pic = new BitmapImage();

                                pic.BeginInit();
                                pic.CacheOption = BitmapCacheOption.OnLoad;
                                pic.StreamSource = memoryStream;
                                pic.EndInit();
                                string name = entry.Name.Substring(0, entry.Name.Length - 4);
                                PicsBuffer.Add(name, pic);
                            }
                        }
                        int percentComplete = (int)((float)ProgressCount / (float)archive.Entries.Count * 100);
                        if (worker.WorkerReportsProgress) worker.ReportProgress(percentComplete);
                    }
                }
            }

            if (info_xml.HasChildNodes)
            {
                //MainWindow.set.Status_textBlock.Text = "Parsing data...";
                Array.Resize(ref ResPacks, ResPacks.Length + 1);
                ResPacks[ResPacks.Length - 1] = new RP();

                foreach (XmlNode node in info_xml.DocumentElement)
                {
                    if (node.Name == "name") ResPacks[ResPacks.Length - 1].name = node.InnerText;
                    else if (node.Name == "author") ResPacks[ResPacks.Length - 1].author = node.InnerText;
                    else if (node.Name == "description") ResPacks[ResPacks.Length - 1].description = node.InnerText;
                    else if (node.Name == "link") ResPacks[ResPacks.Length - 1].link = node.InnerText;
                }

                ResPacks[ResPacks.Length - 1].songs_start = allSongs.Length;
                ResPacks[ResPacks.Length - 1].pics_start = allPics.Length;
                ResPacks[ResPacks.Length - 1].enabled = true;

                if (songs_xml.HasChildNodes) parseSongs(songs_xml, SongsBuffer, true);
                else ResPacks[ResPacks.Length - 1].songs_count = 0;

                if (images_xml.HasChildNodes) parsePics(images_xml, PicsBuffer, ref Transfer);
                else ResPacks[ResPacks.Length - 1].pics_count = 0;
                PicsBuffer.Clear();
                SongsBuffer.Clear();
                foreach (Pics p in Transfer)
                {
                    p.pic.Freeze();
                    if (p.animation != null)
                    {
                        foreach (BitmapSource i in p.animation) i.Freeze();
                    }
                }
                return Transfer;
            }
            else
            {
                //MainWindow.set.Status_textBlock.Text = "info.xml not found";
                PicsBuffer.Clear();
                SongsBuffer.Clear();
                return null;
            }
        }

        private void parseSongs(XmlDocument xml, Dictionary<string, byte[]> Buffer, bool remotePack)
        {
            XmlElement xRoot = xml.DocumentElement;
            foreach (XmlNode node in xRoot)
            {
                if (node.NodeType == XmlNodeType.Comment) continue;
                Array.Resize(ref allSongs, allSongs.Length + 1);
                if (remotePack) allSongs[allSongs.Length - 1].buffer = Buffer[node.Attributes[0].Value];
                allSongs[allSongs.Length - 1].filename = node.Attributes[0].Value + ".mp3";
                allSongs[allSongs.Length - 1].enabled = true;
                allSongs[allSongs.Length - 1].buildup_played = false;
                foreach (XmlNode childnode in node)
                {
                    if (childnode.Name == "title")
                    {
                        allSongs[allSongs.Length - 1].title = childnode.InnerText;
                    }
                    if (childnode.Name == "source")
                    {
                        allSongs[allSongs.Length - 1].source = childnode.InnerText;
                    }
                    if (childnode.Name == "rhythm")
                    {
                        allSongs[allSongs.Length - 1].rhythm = childnode.InnerText;
                        allSongs[allSongs.Length - 1].rhythm.Replace("\n", "");
                    }
                    if (childnode.Name == "buildup")
                    {
                        allSongs[allSongs.Length - 1].buildup_filename = childnode.InnerText;
                        if (remotePack) allSongs[allSongs.Length - 1].buildup_buffer = Buffer[childnode.InnerText];
                    }
                    if (childnode.Name == "buildupRhythm")
                    {
                        allSongs[allSongs.Length - 1].buildupRhythm = childnode.InnerText;
                        allSongs[allSongs.Length - 1].buildupRhythm.Replace("\n", "");
                    }
                }
            }
            ResPacks[ResPacks.Length - 1].songs_count = allSongs.Length - ResPacks[ResPacks.Length - 1].songs_start;
        }

        private void parsePics(XmlDocument xml, Dictionary<string, BitmapImage> Buffer, ref Pics[] Transfer)
        {
            XmlElement xRoot = xml.DocumentElement;
            Pics tempPic;
            int count = 0;
            foreach (XmlNode node in xRoot)
            {
                tempPic = new Pics();
                if (node.NodeType == XmlNodeType.Comment) continue; //WHY DOES IT EVEN PARSE COMMENTS?!
                tempPic.name = node.Attributes[0].Value;

                if (Buffer.ContainsKey(RemoveDiacritics(node.Attributes[0].Value)))
                    tempPic.pic = Buffer[RemoveDiacritics(node.Attributes[0].Value)];
                else if (Buffer.ContainsKey(RemoveDiacritics(node.ChildNodes[1].InnerText)))
                    tempPic.pic = Buffer[RemoveDiacritics(node.ChildNodes[1].InnerText)];

                tempPic.pic = picConverter.ImageOptimize(tempPic.pic);

                tempPic.enabled = true;
                tempPic.source = "";
                tempPic.source_other = "";
                tempPic.fullname = tempPic.name;

                tempPic.align = "center";

                foreach (XmlNode childnode in node)
                {
                    if (childnode.Name == "source")
                    {
                        tempPic.source = childnode.InnerText;
                    }
                    else if (childnode.Name == "source_other")
                    {
                        tempPic.source_other = childnode.InnerText;
                    }
                    else if (childnode.Name == "fullname")
                    {
                        tempPic.fullname = childnode.InnerText;
                    }
                    else if (childnode.Name == "align")
                    {
                        tempPic.align = childnode.InnerText;
                    }
                    else if (childnode.Name == "frameDuration")
                    {
                        string MysteriousZero = "";
                        if (Buffer.ContainsKey(node.Attributes[0].Value + "_1")) MysteriousZero = "";
                        else if (Buffer.ContainsKey(node.Attributes[0].Value + "_01")) MysteriousZero = "0";
                        else continue; //https://youtu.be/5FjWe31S_0g

                        //TEST IT
                        //tempPic.pic = Buffer[node.Attributes[0].Value + "_01"];
                        //tempPic.pic = picConverter.ImageOptimize(tempPic.pic);
                        tempPic.pic = picConverter.ImageOptimize(Buffer[node.Attributes[0].Value + "_" + MysteriousZero + "1"]);

                        tempPic.animation = new BitmapImage[0];
                        for (int i = 1; i < Buffer.Count; i++)
                        {
                            string end;
                            if (i < 10) end = "_" + MysteriousZero + Convert.ToString(i);
                            else end = "_" + Convert.ToString(i);
                            if (Buffer.ContainsKey(node.Attributes[0].Value + end))
                            {
                                Array.Resize(ref tempPic.animation, tempPic.animation.Length + 1);
                                tempPic.animation[tempPic.animation.Length - 1] = Buffer[node.Attributes[0].Value + end];
                            }
                            else break;
                        }

                        string text = childnode.InnerText;
                        tempPic.frameDuration = new int[tempPic.animation.Length];
                        int tempInt;
                        if (int.TryParse(text, out tempInt))
                            for (int i = 0; i < tempPic.frameDuration.Length; i++) tempPic.frameDuration[i] = tempInt;
                        else
                        {
                            string[] s_nums = text.Split(',');
                            if (tempPic.animation.Length > s_nums.Length)
                            {
                                for (int i = 0; i < s_nums.Length; i++)
                                    tempPic.frameDuration[i] = int.Parse(s_nums[i]);
                                for (int i = s_nums.Length; i < tempPic.frameDuration.Length; i++)
                                    tempPic.frameDuration[i] = int.Parse(s_nums[s_nums.Length - 1]);
                            }
                            else
                            {
                                for (int i = 0; i < tempPic.frameDuration.Length; i++)
                                    tempPic.frameDuration[i] = int.Parse(s_nums[i]);
                            }
                        }

                        tempPic.animation = picConverter.ImageArrayOptimize(tempPic.animation);
                    }
                }
                Array.Resize(ref Transfer, Transfer.Length + 1);
                Transfer[Transfer.Length - 1] = tempPic;
                count++;
            }
            ResPacks[ResPacks.Length - 1].pics_count = count;
        }
    }
}

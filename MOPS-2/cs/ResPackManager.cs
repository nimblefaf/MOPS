﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace MOPS
{
    public struct RP
    {
        public string name;
        public string author;
        public string description;
        public string link;
        public int pics_start;
        public int pics_count;
        public int songs_start;
        public int songs_count;
        public bool enabled;
    }

    

    public struct Pics
    {
        public string name;
        public string fullname;
        public string source;
        public string source_other; 
        public string align;
        public BitmapImage pic;
        public BitmapSource invertedPic;
        public BitmapImage[] animation;
        public BitmapSource[] invertedAnimation;
        public int frameDuration;
        public bool enabled;
    }

    public struct Songs
    {
        public string title;
        public string source;
        public string rhythm;
        public string buildup_filename;
        public string buildup_rhythm;
        public byte[] buffer;
        public byte[] buildup_buffer;
        public bool enabled;
        public bool buildup_played;
    }

    public class RPManager
    {
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

        private BitmapSource InvertPic(BitmapSource ImageToInvert)
        {
            Color[] ColorArray = new Color[2] { Color.FromArgb(0, 0, 0, 0), Color.FromArgb(255, 255, 255, 255) };
            BitmapPalette WhitePalette = new BitmapPalette(ColorArray);
            if (ImageToInvert == null) return null;
            if (ImageToInvert.Format == PixelFormats.Indexed1) return null;
            //if (ImageToInvert.Format == PixelFormats.Indexed1)
            //{
            //    int stride = (ImageToInvert.PixelWidth * ImageToInvert.Format.BitsPerPixel + 7) / 8;
            //    int length = stride * ImageToInvert.PixelHeight;
            //    byte[] data = new byte[length];
            //    ImageToInvert.CopyPixels(data, stride, 0);
            //    return BitmapSource.Create(ImageToInvert.PixelWidth, ImageToInvert.PixelHeight, ImageToInvert.DpiX, ImageToInvert.DpiY, PixelFormats.Indexed1, WhitePalette, data, stride);
            //}
            else
            {
                if (ImageToInvert.Format != PixelFormats.Bgra32)
                {
                    FormatConvertedBitmap ConvertedPic = new FormatConvertedBitmap();
                    ConvertedPic = new FormatConvertedBitmap();
                    ConvertedPic.BeginInit();
                    ConvertedPic.Source = ImageToInvert;
                    ConvertedPic.DestinationFormat = PixelFormats.Bgra32;
                    ConvertedPic.DestinationPalette = BitmapPalettes.BlackAndWhiteTransparent;
                    ConvertedPic.EndInit();

                    ImageToInvert = ConvertedPic;
                }
                int stride = (ImageToInvert.PixelWidth * ImageToInvert.Format.BitsPerPixel + 7) / 8;
                int length = stride * ImageToInvert.PixelHeight;
                byte[] data = new byte[length];
                ImageToInvert.CopyPixels(data, stride, 0);

                if (ImageToInvert.Format.BitsPerPixel != 1)
                    for (int i = 0; i < length; i += 4)
                    {
                        data[i] = (byte)(255 - data[i]); //R
                        data[i + 1] = (byte)(255 - data[i + 1]); //G
                        data[i + 2] = (byte)(255 - data[i + 2]); //B
                                                                 //data[i + 3] = (byte)(255 - data[i + 3]); //A
                    }

                return BitmapSource.Create(
                    ImageToInvert.PixelWidth, ImageToInvert.PixelHeight,
                    ImageToInvert.DpiX, ImageToInvert.DpiY, ImageToInvert.Format,
                    ImageToInvert.Palette, data, stride);
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
        /// Loads and parses a single resource pack in .zip format
        /// </summary>
        /// <param name="target_path"></param>
        /// <returns></returns>
        public Pics[] SupremeReader(string target_path, BackgroundWorker worker, DoWorkEventArgs e)
        {
            Pics[] Transfer = new Pics[0];
            XDocument info_xml = new XDocument();
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
                                {
                                    using (var xmlread = XmlReader.Create(reader, readerSettings))
                                        info_xml = XDocument.Load(xmlread);
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
                        if (entry.FullName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var stream = entry.Open())
                            using (var memoryStream = new MemoryStream())
                            {
                                stream.CopyTo(memoryStream);
                                memoryStream.Position = 0;
                                using (BinaryReader br = new BinaryReader(memoryStream))
                                {
                                    SongsBuffer.Add(entry.Name, br.ReadBytes((int)memoryStream.Length));
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
                    }
                }
            }

            if (info_xml.Root != null)
            {
                //MainWindow.set.Status_textBlock.Text = "Parsing data...";
                Array.Resize(ref ResPacks, ResPacks.Length + 1);

                ResPacks[ResPacks.Length - 1].name = info_xml.XPathSelectElement("//info/name").Value;
                ResPacks[ResPacks.Length - 1].author = info_xml.XPathSelectElement("//info/author").Value;
                ResPacks[ResPacks.Length - 1].description = info_xml.XPathSelectElement("//info/description").Value;
                ResPacks[ResPacks.Length - 1].link = info_xml.XPathSelectElement("//info/link").Value;
                ResPacks[ResPacks.Length - 1].songs_start = allSongs.Length;
                ResPacks[ResPacks.Length - 1].pics_start = allPics.Length;
                ResPacks[ResPacks.Length - 1].enabled = true;

                if (songs_xml.HasChildNodes)
                {
                    XmlElement xRoot = songs_xml.DocumentElement;
                    foreach (XmlNode node in xRoot)
                    {
                        if (node.NodeType == XmlNodeType.Comment) continue;
                        Array.Resize(ref allSongs, allSongs.Length + 1);
                        allSongs[allSongs.Length - 1].buffer = SongsBuffer[node.Attributes[0].Value + ".mp3"];
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
                                allSongs[allSongs.Length - 1].buildup_buffer = SongsBuffer[childnode.InnerText + ".mp3"];
                            }
                            if (childnode.Name == "buildupRhythm")
                            {
                                allSongs[allSongs.Length - 1].buildup_rhythm = childnode.InnerText;
                                allSongs[allSongs.Length - 1].buildup_rhythm.Replace("\n", "");
                            }
                        }
                    }
                    ResPacks[ResPacks.Length - 1].songs_count = allSongs.Length - ResPacks[ResPacks.Length - 1].songs_start;
                }
                else ResPacks[ResPacks.Length - 1].songs_count = 0;

                if (images_xml.HasChildNodes)
                {
                    XmlElement xRoot = images_xml.DocumentElement;
                    foreach (XmlNode node in xRoot)
                    {
                        if (node.NodeType == XmlNodeType.Comment) continue; //WHY DOES IT EVEN PARSE COMMENTS?!
                        Array.Resize(ref Transfer, Transfer.Length + 1);
                        Transfer[Transfer.Length - 1].name = node.Attributes[0].Value;

                        if (PicsBuffer.ContainsKey(RemoveDiacritics(node.Attributes[0].Value)))
                            Transfer[Transfer.Length - 1].pic = PicsBuffer[RemoveDiacritics(node.Attributes[0].Value)];
                        else if (PicsBuffer.ContainsKey(RemoveDiacritics(node.ChildNodes[1].InnerText)))
                            Transfer[Transfer.Length - 1].pic = PicsBuffer[RemoveDiacritics(node.ChildNodes[1].InnerText)];

                        Transfer[Transfer.Length - 1].invertedPic = InvertPic(Transfer[Transfer.Length - 1].pic);

                        Transfer[Transfer.Length - 1].enabled = true;
                        Transfer[Transfer.Length - 1].source = "";
                        Transfer[Transfer.Length - 1].source_other = "";
                        Transfer[Transfer.Length - 1].fullname = Transfer[Transfer.Length - 1].name;

                        Transfer[Transfer.Length - 1].align = "center";

                        foreach (XmlNode childnode in node)
                        {
                            if (childnode.Name == "source")
                            {
                                Transfer[Transfer.Length - 1].source = childnode.InnerText;
                            }
                            if (childnode.Name == "source_other")
                            {
                                Transfer[Transfer.Length - 1].source_other = childnode.InnerText;
                            }
                            if (childnode.Name == "fullname")
                            {
                                Transfer[Transfer.Length - 1].fullname = childnode.InnerText;
                            }
                            if (childnode.Name == "align")
                            {
                                Transfer[Transfer.Length - 1].align = childnode.InnerText;
                            }
                            if (childnode.Name == "frameDuration")
                            {
                                Transfer[Transfer.Length - 1].frameDuration = Convert.ToInt32(childnode.InnerText);
                                Transfer[Transfer.Length - 1].pic = PicsBuffer[node.Attributes[0].Value + "_01"];
                                Transfer[Transfer.Length - 1].invertedPic = InvertPic(Transfer[Transfer.Length - 1].pic);
                                Transfer[Transfer.Length - 1].animation = new BitmapImage[0];
                                Transfer[Transfer.Length - 1].invertedAnimation = new BitmapSource[0];
                                for (int i = 1; i < PicsBuffer.Count; i++)
                                {
                                    string end = "";
                                    if (i < 10) end = "_0" + Convert.ToString(i);
                                    else end = "_" + Convert.ToString(i);
                                    if (PicsBuffer.ContainsKey(node.Attributes[0].Value + end))
                                    {
                                        Array.Resize(ref Transfer[Transfer.Length - 1].animation, Transfer[Transfer.Length - 1].animation.Length + 1);
                                        Transfer[Transfer.Length - 1].animation[Transfer[Transfer.Length - 1].animation.Length - 1] = PicsBuffer[node.Attributes[0].Value + end];

                                        Array.Resize(ref Transfer[Transfer.Length - 1].invertedAnimation, Transfer[Transfer.Length - 1].invertedAnimation.Length + 1);
                                        Transfer[Transfer.Length - 1].invertedAnimation[Transfer[Transfer.Length - 1].invertedAnimation.Length - 1] = InvertPic(Transfer[Transfer.Length - 1].animation[Transfer[Transfer.Length - 1].animation.Length - 1]);
                                    }
                                    else break;
                                }
                            }
                        }
                    }
                    ResPacks[ResPacks.Length - 1].pics_count = Transfer.Length;
                }
                else ResPacks[ResPacks.Length - 1].pics_count = 0;
                //MainWindow.set.Status_textBlock.Text = "Loaded";
                PicsBuffer.Clear();
                SongsBuffer.Clear();
                foreach (Pics p in Transfer)
                {
                    p.pic.Freeze();
                    if (p.invertedPic != null) p.invertedPic.Freeze();
                    if (p.animation != null)
                    {
                        foreach (BitmapImage i in p.animation) i.Freeze();
                        foreach (BitmapSource i in p.invertedAnimation) i.Freeze();
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
    }
}

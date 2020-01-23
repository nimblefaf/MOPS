using System;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MOPS_2
{
    public struct ResPack
    {
        public string name;
        public string author;
        public string description;
        public string link;
        public int pics_start;
        public int songs_start;
    }

    

    public struct Pics
    {
        public string name;
        public string fullname;
        public string source;
        public string source_other; 
        public string align;
        public BitmapImage png;
        public BitmapImage[] animation;
        public bool still;
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
    }

    public class ResPackManager
    {
        static string RemoveDiacritics(string text)
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

        public static ResPack[] resPacks = new ResPack[0];
        public static Songs[] allSongs = new Songs[0];
        public static Pics[] allPics = new Pics[0];

        public static void SupremeReader(string target_path)
        {
            XDocument info_xml = new XDocument();
            XmlDocument songs_xml = new XmlDocument();
            XmlDocument images_xml = new XmlDocument();
            Dictionary<string, BitmapImage> PicsBuffer = new Dictionary<string, BitmapImage> { };
            Dictionary<string, byte[]> SongsBuffer = new Dictionary<string, byte[]> { };

            Array.Resize(ref resPacks, resPacks.Length + 1);


            using (FileStream zipToOpen = new FileStream(target_path, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read, false, Encoding.Default))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (entry.Name == "info.xml")
                                using (var stream = entry.Open())
                                using (var reader = new StreamReader(stream))
                                {
                                    info_xml = XDocument.Load(reader);
                                }
                            if (entry.Name == "songs.xml")
                                using (var stream = entry.Open())
                                using (var reader = new StreamReader(stream))
                                {
                                    songs_xml.Load(reader);
                                }
                            if (entry.Name == "images.xml")
                                using (var stream = entry.Open())
                                using (var reader = new StreamReader(stream))
                                {
                                    images_xml.Load(reader);
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
                        if (entry.FullName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
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

                                PicsBuffer.Add(entry.Name, pic);
                            }
                        }
                    }
                }

            }

            resPacks[resPacks.Length - 1].name = info_xml.XPathSelectElement("//info/name").Value;
            resPacks[resPacks.Length - 1].author = info_xml.XPathSelectElement("//info/author").Value;
            resPacks[resPacks.Length - 1].description = info_xml.XPathSelectElement("//info/description").Value;
            resPacks[resPacks.Length - 1].link = info_xml.XPathSelectElement("//info/link").Value;
            resPacks[resPacks.Length - 1].songs_start = allSongs.Length;
            resPacks[resPacks.Length - 1].pics_start = allPics.Length;

            if (songs_xml.HasChildNodes)
            {
                XmlElement xRoot = songs_xml.DocumentElement;
                foreach (XmlNode node in xRoot)
                {
                    if (node.NodeType == XmlNodeType.Comment) continue;
                    Array.Resize(ref allSongs, allSongs.Length + 1);
                    allSongs[allSongs.Length - 1].buffer = SongsBuffer[node.Attributes[0].Value + ".mp3"];
                    allSongs[allSongs.Length - 1].enabled = true;
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
                        }
                        if (childnode.Name == "buildup")
                        {
                            allSongs[allSongs.Length - 1].buildup_filename = childnode.InnerText;
                            allSongs[allSongs.Length - 1].buildup_buffer = SongsBuffer[childnode.InnerText + ".mp3"];
                        }
                        if (childnode.Name == "buildupRhythm")
                        {
                            allSongs[allSongs.Length - 1].buildup_rhythm = childnode.InnerText;
                        }
                    }
                }
            }

            if (images_xml.HasChildNodes)
            {
                XmlElement xRoot = images_xml.DocumentElement;
                foreach (XmlNode node in xRoot)
                {
                    if (node.NodeType == XmlNodeType.Comment) continue; //WHY DOES IT EVEN PARSE COMMENTS?!
                    Array.Resize(ref allPics, allPics.Length + 1);
                    allPics[allPics.Length - 1].name = node.Attributes[0].Value;
                    if (node.LastChild.Name == "frameDuration")
                    {
                        allPics[allPics.Length - 1].png = PicsBuffer[node.Attributes[0].Value + "_01.png"];
                        allPics[allPics.Length - 1].still = false;
                    }
                    else
                    {
                        string test = node.Attributes[0].Value + ".png";
                        if (PicsBuffer.ContainsKey(RemoveDiacritics(node.Attributes[0].Value) + ".png"))
                            allPics[allPics.Length - 1].png = PicsBuffer[RemoveDiacritics(node.Attributes[0].Value) + ".png"];
                        else if (PicsBuffer.ContainsKey(RemoveDiacritics(node.ChildNodes[1].InnerText + ".png")))
                            allPics[allPics.Length - 1].png = PicsBuffer[RemoveDiacritics(node.ChildNodes[1].InnerText + ".png")];
                    }
                    allPics[allPics.Length - 1].enabled = true;
                    foreach (XmlNode childnode in node)
                    {
                        if (childnode.Name == "source")
                        {
                            allPics[allPics.Length - 1].source = childnode.InnerText;
                        }
                        if (childnode.Name == "source_other")
                        {
                            allPics[allPics.Length - 1].source_other = childnode.InnerText;
                        }
                        if (childnode.Name == "fullname")
                        {
                            allPics[allPics.Length - 1].fullname = childnode.InnerText;
                        }
                        if (childnode.Name == "align")
                        {
                            allPics[allPics.Length - 1].align = childnode.InnerText;
                        }
                        if (childnode.Name == "frameDuration")
                        {

                        }
                    }
                }
            }
        }
    }
}

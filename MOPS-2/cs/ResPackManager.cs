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

    public class RPManager
    {
        static string RemoveDiacritics(string text) //Because German names are suffering
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

        public static int get_rp_of_song(int ind)
        {
            for (int i = ResPacks.Length - 1; i >= 0; i--)
            {
                if (ind >= ResPacks[i].songs_start) return i;
            }
            return -1;
        }
        public static int get_rp_of_image(int ind)
        {
            for (int i = ResPacks.Length - 1; i >= 0; i--)
            {
                if (ind > ResPacks[i].pics_start) return i;
            }
            return -1;
        }

        public static string status = "Loader Idle";

        public static RP[] ResPacks = new RP[0];
        public static Songs[] allSongs = new Songs[0];
        public static Pics[] allPics = new Pics[0];

        public static bool SupremeReader(string target_path)
        {
            XDocument info_xml = new XDocument();
            XmlDocument songs_xml = new XmlDocument();
            XmlDocument images_xml = new XmlDocument();
            Dictionary<string, BitmapImage> PicsBuffer = new Dictionary<string, BitmapImage> { };
            Dictionary<string, byte[]> SongsBuffer = new Dictionary<string, byte[]> { };
            


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
                                    info_xml = XDocument.Load(reader);
                                }
                            if (entry.Name.ToLower() == "songs.xml")
                                using (var stream = entry.Open())
                                using (var reader = new StreamReader(stream))
                                {
                                    XmlParserContext xml;
                                    songs_xml.Load(reader);
                                    
                                }
                            if (entry.Name.ToLower() == "images.xml")
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
                        if (entry.FullName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) | entry.FullName.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase))
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

            if (info_xml.Root != null)
            {
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
                    ResPacks[ResPacks.Length - 1].songs_count = allSongs.Length - ResPacks[ResPacks.Length - 1].songs_start;
                }
                else ResPacks[ResPacks.Length - 1].songs_count = 0;

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
                            allPics[allPics.Length - 1].png = PicsBuffer[node.Attributes[0].Value + "_01.png"]; //Animation TBD
                            allPics[allPics.Length - 1].still = false;
                        }
                        else
                        {
                            string test = node.Attributes[0].Value + ".png";
                            if (PicsBuffer.ContainsKey(RemoveDiacritics(node.Attributes[0].Value) + ".png"))
                                allPics[allPics.Length - 1].png = PicsBuffer[RemoveDiacritics(node.Attributes[0].Value) + ".png"];
                            else if (PicsBuffer.ContainsKey(RemoveDiacritics(node.ChildNodes[1].InnerText + ".png")))
                                allPics[allPics.Length - 1].png = PicsBuffer[RemoveDiacritics(node.ChildNodes[1].InnerText + ".png")];
                            else if (PicsBuffer.ContainsKey(RemoveDiacritics(node.Attributes[0].Value) + ".gif"))
                                allPics[allPics.Length - 1].png = PicsBuffer[RemoveDiacritics(node.Attributes[0].Value) + ".gif"];
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
                    ResPacks[ResPacks.Length - 1].pics_count = allPics.Length - ResPacks[ResPacks.Length - 1].pics_start;
                }
                else ResPacks[ResPacks.Length - 1].pics_count = 0;
                PicsBuffer.Clear();
                SongsBuffer.Clear();
                return true;
            }
            else
            {
                System.Windows.MessageBox.Show("Error: info.xml not found");
                PicsBuffer.Clear();
                SongsBuffer.Clear();
                return false;
            }
        }
    }
}

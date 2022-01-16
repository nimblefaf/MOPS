using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Compression;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace MOPS.UI
{
    /// <summary>
    /// Interaction logic for Preloader.xaml
    /// </summary>
    public partial class Preloader : UserControl
    {
        private MainWindow main;
        private BackgroundWorker backgroundLoader;
        public DoubleAnimation PreloaderFade = new DoubleAnimation();
        private int Percentage;

        public Preloader()
        {
            InitializeComponent();
            RespackCheck();
            Percentage = 0;

            backgroundLoader = new BackgroundWorker();
            backgroundLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(load_completed);
            backgroundLoader.ProgressChanged += new ProgressChangedEventHandler(BGWorker_ProgressChanged);

            PreloaderFade.From = 1;
            PreloaderFade.To = 0;
            PreloaderFade.Duration = TimeSpan.FromSeconds(0.4);
            PreloaderFade.Completed += delegate (object sender, EventArgs e)
            {
                Visibility = Visibility.Hidden;
                ((Panel)this.Parent).Children.Remove(this); //removing UC, since we don't need it anymore
            };
        }
        public void SetReference(MainWindow window)
        {
            main = window;
            backgroundLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(main.InnerWin.resources_TabPanel.load_completed);
            backgroundLoader.DoWork += new DoWorkEventHandler(main.InnerWin.resources_TabPanel.load_dowork);
        }

        private void load_completed(object sender, RunWorkerCompletedEventArgs e) 
        {
            Cursor = Cursors.Arrow;
            StartBlock.Text = "Completed";
            this.BeginAnimation(OpacityProperty, PreloaderFade);
        }

        private void BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Percentage = e.ProgressPercentage;
            PacksBlock.Text = e.ProgressPercentage.ToString();
            ProgressBarMove(e.ProgressPercentage);
        }

        private void ProgressBarMove(int Perc)
        {
            ProgressBGBar.Width = this.ActualWidth / 100 * Perc;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ProgressBarMove(Percentage);
        }

        List<string> packs = new List<string>();
        private void RespackCheck()
        {
            if (Directory.Exists("Packs"))
            {
                string[] files = Directory.GetFiles("Packs");
                List<string> zips = new List<string>();
                foreach (string filename in files) if (filename.EndsWith(".zip")) zips.Add(filename);

                if (zips.Count == 0) PacksBlock.Text = "No local respacks detected";
                else
                {
                    foreach (string zip in zips)
                        using (FileStream zipToOpen = new FileStream(zip, FileMode.Open))
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read, false, Encoding.Default))
                            foreach (ZipArchiveEntry entry in archive.Entries)
                                if (entry.FullName.EndsWith("info.xml", StringComparison.InvariantCultureIgnoreCase)) packs.Add(zip);

                    if (packs.Count == 0) PacksBlock.Text = "No local respacks detected: info.xml not found";
                    else if (packs.Count == 1)
                    {
                        PacksBlock.Text = "\"" + packs[0].Substring(6) + "\" will be loaded";
                    }
                    else
                    {
                        PacksBlock.Text = packs.Count.ToString() + " local respacks will be loaded";
                        if (packs.Contains("Packs\\Defaults_v5.0.zip") & packs.IndexOf("Packs\\Defaults_v5.0.zip") != 0)
                        {
                            packs.Remove("Packs\\Defaults_v5.0.zip");
                            packs.Insert(0, "Packs\\Defaults_v5.0.zip");
                        }
                    }
                }
            }
            else
            {
                Directory.CreateDirectory("Packs");
                PacksBlock.Text = "No local respack detected";
            }
        }

        private void StartBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartLoading();
        }

        private void StartLoading()
        {
            Cursor = Cursors.Wait;
            WarningBlock.Visibility = Visibility.Hidden;
            StartBlock.IsEnabled = false;
            StartBlock.Visibility = Visibility.Hidden;
            LoadingBlock.Visibility = Visibility.Visible;

            if (packs.Count == 0) this.BeginAnimation(OpacityProperty, PreloaderFade);
            else backgroundLoader.RunWorkerAsync(packs.ToArray());
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.skipPreloadWarn) StartLoading();
        }
    }
}

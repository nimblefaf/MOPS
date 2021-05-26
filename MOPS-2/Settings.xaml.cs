using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MOPS
{
    
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

    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Settings : Window
    {
        MainWindow main;

        UI.Options_TabPanel options_TabPanel;
        public UI.Resources_TabPanel resources_TabPanel;

        public Settings()
        {
            InitializeComponent();
            resources_TabPanel = (UI.Resources_TabPanel)res_tab.Content;
            options_TabPanel = (UI.Options_TabPanel)opt_tab.Content;
        }

        public void SetReference(MainWindow window)
        {
            main = window;
            resources_TabPanel.SetReference(main);
            options_TabPanel.SetReference(main);
        }


        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TO DO: Resize the window for Editor
        }

        private void SettingsWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right) Hide();
        }

        private void hide_button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            main.Focus();
        }
    }
}

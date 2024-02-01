using System.Windows;
using System.Windows.Controls;

namespace HuesSharp.UI
{
    /// <summary>
    /// Interaction logic for InnerWindow.xaml
    /// </summary>
    public partial class InnerWindow : UserControl
    {
        MainWindow main;

        public Options_TabPanel options_TabPanel;
        public Resources_TabPanel resources_TabPanel;

        public InnerWindow()
        {
            InitializeComponent();
            resources_TabPanel = (Resources_TabPanel)res_tab.Content;
            options_TabPanel = (Options_TabPanel)opt_tab.Content;
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

        private void hide_button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

    }
}

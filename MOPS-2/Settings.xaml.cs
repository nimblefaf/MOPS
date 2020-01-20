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
using System.Windows.Shapes;

namespace MOPS_2
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void SettingsWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right) Hide();
        }

        private void hide_button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        public void rp_display()
        {
            respack_listbox.Items.Add(ResPackManager.resPacks[ResPackManager.resPacks.Length - 1].name);
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            respack_listbox.ItemsSource = ResPackManager.resPacks;
        }

        private void respack_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int ind = respack_listbox.SelectedIndex;
            rp_name_label.Content = ResPackManager.resPacks[ind].name;
            rp_author_label.Content = ResPackManager.resPacks[ind].author;
            rp_description_textbox.Text = ResPackManager.resPacks[ind].description;
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

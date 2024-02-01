using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HuesSharp.UI
{
    /// <summary>
    /// Interaction logic for PicPicker.xaml
    /// </summary>
    public partial class PicPicker : UserControl
    {
        MainWindow main;
        public PicPicker()
        {
            InitializeComponent();
            main = (MainWindow)Application.Current.MainWindow;
            images_listbox.ItemsSource = main.Core.enabled_images;
        }

        private void Images_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (images_listbox.SelectedIndex != -1)
            {
                main.PicPicker_SelectionChanged(images_listbox.SelectedIndex);
                images_listbox.SelectedIndex = -1;
            }
        }

        private void Images_listbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}

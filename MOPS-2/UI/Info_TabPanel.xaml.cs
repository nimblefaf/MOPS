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
using System.Diagnostics;
using System.Reflection;

namespace HuesSharp.UI
{
    /// <summary>
    /// Interaction logic for Info_TabPanel.xaml
    /// </summary>
    public partial class Info_TabPanel : UserControl
    {
        public Info_TabPanel()
        {
            InitializeComponent();
            InfoTextBlock1.Text = "0x40 Hues of C#, V" + Assembly.GetExecutingAssembly().GetName().Version.ToString().Remove(3);
            glossaryTextBox.Text =
                "x  Vertical blur, image and colour (snare)\r\n" +
                "o  Horizontal blur, image and colour (bass)\r\n" +
                "-  Change image and colour (no blur)\r\n" +
                ":  Colour only\r\n" +
                "*  Image only\r\n" +
                "X  Vertical blur only\r\n" +
                "O  Horizontal blur only\r\n" +
                "+¤ Blackout/whiteout (classic)\r\n" +
                "|! Blackout/whiteout (short)\r\n" +
                //"┊¡ Blackout/whiteout (instant)\r\n" +
                //"▼▽ Blackout/whiteout (fade out)\r\n" +
                //"▲△ Blackout/whiteout (fade in)\r\n" +
                //")( Trippy cirle in/out and change image\r\n" +
                //">< Trippy cirle in/out\r\n" +
                "~  Fade colour\r\n" +
                "=  Fade and change image\r\n" +
                "i  Invert all colours\r\n" +
                "I  Invert & change image\r\n";
                //"ı  Fade invert\r\n" +
                //"sv Horizontal/vertical slice\r\n" +
                //"SV Horizontal/vertical slice and change image\r\n" +
                //"# Double slice\r\n" +
                //"@ Double slice and change image\r\n" +
                //"←↓↑→ Shutter\r\n" +
                //"¯ Stop all effects in bank immediately\r\n" +
                //"_ Stop timed effects (fade, slice etc)";
            shortcutsTextBox.Text =
                "↑↓ Change song\r\n" +
                "←→ Change image\r\n" +
                "[N] Random song\r\n" +
                "-+ Change volume\r\n" +
                "[M] Toggle mute\r\n" +
                "[B] Restart song from build\r\n" +
                //"[F] Toggle automode\r\n" +
                "[H] Toggle UI hide\r\n" +
                "[C] Character list\r\n" +
                "[S] Song list\r\n" +
                "[W] Toggle window\r\n" +
                "[R] Resource packs\r\n" +
                "[L] Load local zip\r\n" +
                "[O] Options\r\n" +
                "[I] Information\r\n" +
                "[1-6] Change UI\r\n" +
                "[Alt+Enter] Fullscreen";
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // for .NET Core you need to add UseShellExecute = true
            // see https://docs.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace HuesSharp.UI
{
    /// <summary>
    /// Interaction logic for UI_Mini.xaml
    /// </summary>
    public partial class UI_Mini : UserControl
    {
        public UI_Mini()
        {
            InitializeComponent();
            InitHideAnimation();
        }


        #region HideUI
        private bool HideAnimationInProgress = false;
        public void ToggleHideUI()
        {
            if (!HideAnimationInProgress)
            {
                if (TimelineGrid.Opacity == 1)
                {
                    LilButtonGrid.Visibility = Visibility.Visible;
                    TimelineGrid.BeginAnimation(OpacityProperty, HideMainGridAnimation);
                    LilButtonGrid.BeginAnimation(OpacityProperty, ShowLilButtonAnimation);
                    TimelineGrid.Opacity = 0;
                    TimelineGrid.IsEnabled = false;
                    LilButtonGrid.Opacity = 1;
                    HideAnimationInProgress = true;
                }
                else
                {
                    TimelineGrid.Visibility = Visibility.Visible;
                    TimelineGrid.BeginAnimation(OpacityProperty, ShowMainGridAnimation);
                    LilButtonGrid.BeginAnimation(OpacityProperty, HideLilButtonAnimation);
                    TimelineGrid.Opacity = 1;
                    LilButtonGrid.IsEnabled = false;
                    LilButtonGrid.Opacity = 0;
                    HideAnimationInProgress = true;
                }
            }
        }

        private DoubleAnimation HideMainGridAnimation = new DoubleAnimation();
        private DoubleAnimation ShowMainGridAnimation = new DoubleAnimation();
        private DoubleAnimation HideLilButtonAnimation = new DoubleAnimation();
        private DoubleAnimation ShowLilButtonAnimation = new DoubleAnimation();

        private void InitHideAnimation()
        {
            HideMainGridAnimation.From = 1;
            HideMainGridAnimation.To = 0;
            HideMainGridAnimation.Duration = TimeSpan.FromSeconds(0.6);
            HideMainGridAnimation.Completed += delegate (object sender, EventArgs e)
            {
                TimelineGrid.Visibility = Visibility.Hidden;
            };

            ShowMainGridAnimation.From = 0;
            ShowMainGridAnimation.To = 1;
            ShowMainGridAnimation.Duration = TimeSpan.FromSeconds(0.6);
            ShowMainGridAnimation.Completed += delegate (object sender, EventArgs e)
            {
                TimelineGrid.IsEnabled = true;
                HideAnimationInProgress = false;
            };

            HideLilButtonAnimation.From = 1;
            HideLilButtonAnimation.To = 0;
            HideLilButtonAnimation.Duration = TimeSpan.FromSeconds(0.6);
            HideLilButtonAnimation.Completed += delegate (object sender, EventArgs e)
            {
                LilButtonGrid.Visibility = Visibility.Hidden;
            };

            ShowLilButtonAnimation.From = 0;
            ShowLilButtonAnimation.To = 1;
            ShowLilButtonAnimation.Duration = TimeSpan.FromSeconds(0.6);
            ShowLilButtonAnimation.Completed += delegate (object sender, EventArgs e)
            {
                LilButtonGrid.IsEnabled = true;
                HideAnimationInProgress = false;
            };
        }
        #endregion

        private void showUI_TBB_Click(object sender, RoutedEventArgs e)
        {
            ToggleHideUI();
        }
    }
}

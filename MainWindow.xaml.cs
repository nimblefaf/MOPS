using HuesSharp.Shaders;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HuesSharp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rnd = new Random();

        internal Core Core = new Core();

        DispatcherTimer AnimTimer = new DispatcherTimer(DispatcherPriority.Render);
        public DispatcherTimer ShortBlackoutTimer = new DispatcherTimer(DispatcherPriority.Render);

        Shaders.InvertColorEffect invertColorEffect = new Shaders.InvertColorEffect();
        Shaders.HardLightEffect HardLightEffect = new Shaders.HardLightEffect();
        Shaders.HuesYBlur8Effect YBlur8 = new Shaders.HuesYBlur8Effect();
        Shaders.HuesXBlur8Effect XBlur8 = new Shaders.HuesXBlur8Effect();
        Shaders.HuesYBlur14Effect YBlur14 = new Shaders.HuesYBlur14Effect();
        Shaders.HuesXBlur14Effect XBlur14 = new Shaders.HuesXBlur14Effect();
        Shaders.HuesYBlur26Effect YBlur26 = new Shaders.HuesYBlur26Effect();
        Shaders.HuesXBlur26Effect XBlur26 = new Shaders.HuesXBlur26Effect();
        Shaders.SliceOmniV1Effect SliceOmni = new Shaders.SliceOmniV1Effect();
        Shaders.SliceTestHorV1Effect SliceHorizontal = new Shaders.SliceTestHorV1Effect();
        Shaders.SliceTestVertV1Effect SliceVertical = new Shaders.SliceTestVertV1Effect();

        public Hues.Palette[] hues;

        public int CurrentColorInd = 0;
        private int anim_ind = 0;

        public bool Colors_Inverted = false;
        public bool full_auto_mode
        {
            get { return _Full_auto_mode; }
            set
            {
                _Full_auto_mode = value;
                Core.UIHandler.UpdateMiscInfo();
            }
        }
        private bool _Full_auto_mode = true;
        public bool blackouted = false;

        public MainWindow()
        {
            InitializeComponent();

            AnimTimer.Tick += new EventHandler(AnimTimer_Tick);
            ShortBlackoutTimer.Tick += new EventHandler(ShortBlackoutTimer_Tick);
            VisualBrush blendBrush = new VisualBrush(ColorGrid);
            HardLightEffect.Blend = blendBrush;

            PreloaderWin.SetReference(this);
            InnerWin.SetReference(this);


            switch ((ColorSet)Properties.Settings.Default.colorSet)
            {
                case ColorSet.Normal:
                    hues = Hues.hues_normal;
                    break;
                case ColorSet.Pastel:
                    hues = Hues.hues_pastel;
                    break;
                case ColorSet.Weed:
                    hues = Hues.hues_weed;
                    break;
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Core.SetReferences();
            Init_Animations();
            ColorBlend_Graphics_Update();
            UIStyle_Graphics_Update();
            BlurDecay_Upd();
            BlurAmount_Upd();
            timeline_color_change();
        }

        private Storyboard BlurAnimSB = new Storyboard();
        public DoubleAnimation BlurAnim = new DoubleAnimation();

        private Storyboard AnisBlurAnimSB = new Storyboard();
        private DoubleAnimation AnisBlurAnim = new DoubleAnimation();

        private Storyboard SlideAnimSB = new Storyboard();
        private DoubleAnimation SlideAnim = new DoubleAnimation();

        public Storyboard SB_Blackout = new Storyboard();
        private DoubleAnimation Blackout = new DoubleAnimation();
        private Storyboard SB_Blackout_Short = new Storyboard();
        private DoubleAnimation Blackout_Short = new DoubleAnimation();
        private DoubleAnimation Blackout_Blur = new DoubleAnimation();
        public ColorAnimation Fade = new ColorAnimation();

        private void Init_Animations()
        {
            BlurAnim.From = 0.02;
            BlurAnim.To = 0;
            BlurAnim.Duration = TimeSpan.FromSeconds(0.5);
            Storyboard.SetTargetProperty(BlurAnim, new PropertyPath("Effect.BlurAmount"));
            Storyboard.SetTarget(BlurAnim, ImageGrid);
            BlurAnimSB.Children.Add(BlurAnim);
            BlurAnimSB.FillBehavior = FillBehavior.Stop;
            BlurAnimSB.DecelerationRatio = 1;

            AnisBlurAnim.From = 0.005;
            AnisBlurAnim.To = 0;
            AnisBlurAnim.Duration = TimeSpan.FromSeconds(0.5);
            Storyboard.SetTargetProperty(AnisBlurAnim, new PropertyPath("Effect.BlurAmount"));
            Storyboard.SetTarget(AnisBlurAnim, AnisotropicGrid);
            AnisBlurAnimSB.Children.Add(AnisBlurAnim);
            AnisBlurAnimSB.FillBehavior = FillBehavior.Stop;
            AnisBlurAnimSB.DecelerationRatio = 1;

            SlideAnim.From = 0.04;
            SlideAnim.To = 0;
            SlideAnim.Duration = TimeSpan.FromSeconds(0.3);
            Storyboard.SetTargetProperty(SlideAnim, new PropertyPath("Effect.BlurAmount"));
            Storyboard.SetTarget(SlideAnim, ImageGrid);
            SlideAnimSB.Children.Add(SlideAnim);
            SlideAnimSB.FillBehavior = FillBehavior.Stop;
            SlideAnimSB.DecelerationRatio = 1;



            Fade.FillBehavior = FillBehavior.HoldEnd;
            Fade.BeginTime = TimeSpan.FromSeconds(0);
            //Fade.Completed += delegate (object sender, EventArgs e)
            //{
            //    ColorOverlap_Rectangle.Fill = hues[CurrentColorInd].brush;
            //    HardLightEffect.Blend = Color.FromArgb(179, hues[CurrentColorInd].brush.Color.R, hues[CurrentColorInd].brush.Color.G, hues[CurrentColorInd].brush.Color.B);
            //};

            Blackout_Rectangle.Opacity = 0;
            Blackout_Short.BeginTime = new TimeSpan(0);
            Blackout_Short.From = 0;
            Blackout_Short.To = 1;
            Blackout_Short.Duration = TimeSpan.FromSeconds(0.2);
            Blackout_Short.FillBehavior = FillBehavior.Stop;
            SB_Blackout_Short.FillBehavior = FillBehavior.Stop;
            Storyboard.SetTargetProperty(Blackout_Short, new PropertyPath(OpacityProperty));
            SB_Blackout_Short.Children.Add(Blackout_Short);
            Storyboard.SetTarget(Blackout_Short, Blackout_Rectangle);

            Blackout.BeginTime = new TimeSpan(0);
            Blackout.FillBehavior = FillBehavior.Stop;
            Blackout.From = 0;
            Blackout.To = 1;
            Blackout.Duration = TimeSpan.FromSeconds(0.15);
            Blackout.Completed += delegate (object sender, EventArgs e)
            {
                Blackout_Rectangle.Opacity = 1;
                blackouted = true;
            };
            Storyboard.SetTargetProperty(Blackout, new PropertyPath(OpacityProperty));

            Blackout_Blur.BeginTime = new TimeSpan(0);
            Blackout_Blur.FillBehavior = FillBehavior.Stop;
            Blackout_Blur.From = 0;
            Blackout_Blur.To = 0.02;
            Blackout_Blur.Duration = TimeSpan.FromSeconds(0.15);
            Storyboard.SetTargetProperty(Blackout_Blur, new PropertyPath("Effect.BlurAmount"));
            SB_Blackout.Children.Add(Blackout);
            SB_Blackout.Children.Add(Blackout_Blur);
            Storyboard.SetTarget(Blackout, Blackout_Rectangle);
            Storyboard.SetTarget(Blackout_Blur, ImageGrid);
            SB_Blackout.FillBehavior = FillBehavior.Stop;
        }

        public void ColorBlend_Graphics_Update()
        {
            switch ((BlendMode)Properties.Settings.Default.blendMode)
            {
                case BlendMode.Plain:
                    image0.Opacity = 1;
                    ColorGrid.Opacity = 0.8;
                    ImageGrid_Rectangle.Fill = Brushes.Transparent;
                    RenderGrid.Effect = null;
                    break;
                case BlendMode.Alpha:
                    image0.Opacity = 0.7;
                    ColorGrid.Opacity = 0.8;
                    ImageGrid_Rectangle.Fill = Brushes.Transparent;
                    RenderGrid.Effect = null;
                    break;
                case BlendMode.HardLight:
                    image0.Opacity = 1;
                    ColorGrid.Opacity = 1;
                    ImageGrid_Rectangle.Fill = Brushes.White;
                    RenderGrid.Effect = HardLightEffect;
                    break;
            }
        }
        public void UIStyle_Graphics_Update()
        {
            switch ((UIStyle)Properties.Settings.Default.uiStyle)
            {
                case UIStyle.Alpha:
                    DisplayGrid.Children.Add(Core.UIHandler.Display_Alpha);
                    break;
                case UIStyle.Mini:
                    DisplayGrid.Children.Add(Core.UIHandler.Display_Mini);
                    break;
                case UIStyle.Retro:
                    DisplayGrid.Children.Add(Core.UIHandler.Display_Retro);
                    if (Core.MainTimer.IsEnabled) Core.UIHandler.AudioTimer.Start();
                    break;
                case UIStyle.Weed:
                    DisplayGrid.Children.Add(Core.UIHandler.Display_Weed);
                    if (Core.MainTimer.IsEnabled) Core.UIHandler.AudioTimer.Start();
                    break;
                case UIStyle.Modern:
                    DisplayGrid.Children.Add(Core.UIHandler.Display_Modern);
                    if (Core.MainTimer.IsEnabled) Core.UIHandler.AudioTimer.Start();
                    break;
            }
            if (DisplayGrid.Children.Count == 2)
            {
                DisplayGrid.Children.RemoveAt(0);
                Core.UIHandler.UpdateEverything();
                if ((UIStyle)Properties.Settings.Default.uiStyle == UIStyle.Mini & (UIStyle)Properties.Settings.Default.uiStyle == UIStyle.Alpha) Core.UIHandler.AudioTimer.Stop();
            }
        }
        public void BlurDecay_Upd()
        {
            switch ((BlurDecay)Properties.Settings.Default.blurDecay)
            {
                case BlurDecay.Slow:
                    BlurAnim.Duration = TimeSpan.FromSeconds(0.35);
                    AnisBlurAnim.Duration = TimeSpan.FromSeconds(0.35);
                    break;
                case BlurDecay.Medium:
                    BlurAnim.Duration = TimeSpan.FromSeconds(0.25);
                    AnisBlurAnim.Duration = TimeSpan.FromSeconds(0.25);
                    break;
                case BlurDecay.Fast:
                    BlurAnim.Duration = TimeSpan.FromSeconds(0.15);
                    AnisBlurAnim.Duration = TimeSpan.FromSeconds(0.15);
                    break;
                case BlurDecay.Fastest:
                    BlurAnim.Duration = TimeSpan.FromSeconds(0.10);
                    AnisBlurAnim.Duration = TimeSpan.FromSeconds(0.10);
                    break;
            }

        }
        public void BlurAmount_Upd()
        {
            switch ((BlurAmount)Properties.Settings.Default.blurAmount)
            {
                case BlurAmount.Low:
                    BlurAnim.From = 0.005 / (Properties.Settings.Default.blurQuality + 1);
                    break;
                case BlurAmount.Medium:
                    BlurAnim.From = 0.01 / (Properties.Settings.Default.blurQuality + 1);
                    break;
                case BlurAmount.High:
                    BlurAnim.From = 0.05 / (Properties.Settings.Default.blurQuality + 1);
                    break;
            }
        }

        public void AnisotropicState_Upd()
        {
            if (Properties.Settings.Default.anisotropicBlurEnabled)
            {
                if (ImageGrid.Effect == XBlur8 | ImageGrid.Effect == XBlur14 | ImageGrid.Effect == XBlur26)
                    AnisotropicGrid.Effect = YBlur8;
                else if (ImageGrid.Effect == YBlur8 | ImageGrid.Effect == YBlur14 | ImageGrid.Effect == YBlur26)
                    AnisotropicGrid.Effect = XBlur8;
            }
            else
            {
                AnisBlurAnimSB.Stop();
                AnisotropicGrid.Effect = null;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                ToggleInnerWindow();
            }
        }

        public void ToggleInnerWindow()
        {
            if (InnerWin.IsVisible) InnerWin.Visibility = Visibility.Hidden;
            else InnerWin.Visibility = Visibility.Visible;
        }

        private void FileDrop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                InnerWin.resources_TabPanel.load_localRP_from_path(files[0]);
            }
        }

        #region KeyControls

        //
        //Key Controls
        //

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Core.ChangeVolume(e.Delta);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.M:
                    Core.toggle_mute();
                    break;
                case Key.Up:
                    Core.next_song();
                    break;
                case Key.Down:
                    Core.prev_song();
                    break;
                case Key.Right:
                    Core.next_image();
                    break;
                case Key.Left:
                    Core.prev_image();
                    break;
                case Key.Space:
                    timeline_pic_and_color();
                    break;
                case Key.H:
                    Core.UIHandler.ToggleHideUI();
                    break;
                case Key.W:
                    ToggleInnerWindow();
                    break;
                case Key.R:
                    if (InnerWin.Visibility == Visibility.Hidden) ToggleInnerWindow();
                    InnerWin.tabControl.SelectedIndex = 0;
                    break;
                case Key.E:
                    if (InnerWin.Visibility == Visibility.Hidden) ToggleInnerWindow();
                    InnerWin.tabControl.SelectedIndex = 1;
                    break;
                case Key.O:
                    if (InnerWin.Visibility == Visibility.Hidden) ToggleInnerWindow();
                    InnerWin.tabControl.SelectedIndex = 2;
                    break;
                case Key.I:
                    show_info_page();
                    break;
                case Key.D0:
                    if (Properties.Settings.Default.uiStyle != (int)UIStyle.Alpha)
                    {
                        Properties.Settings.Default.uiStyle = (int)UIStyle.Alpha;
                        UIStyle_Graphics_Update();
                        InnerWin.options_TabPanel.Options_UI_Update();
                    }
                    break;
                case Key.D1:
                    if (Properties.Settings.Default.uiStyle != (int)UIStyle.Retro)
                    {
                        Properties.Settings.Default.uiStyle = (int)UIStyle.Retro;
                        UIStyle_Graphics_Update();
                        InnerWin.options_TabPanel.Options_UI_Update();
                    }
                    break;
                case Key.D2:
                    if (Properties.Settings.Default.uiStyle != (int)UIStyle.Weed)
                    {
                        Properties.Settings.Default.uiStyle = (int)UIStyle.Weed;
                        UIStyle_Graphics_Update();
                        InnerWin.options_TabPanel.Options_UI_Update();
                    }
                    break;
                case Key.D3:
                    if (Properties.Settings.Default.uiStyle != (int)UIStyle.Modern)
                    {
                        Properties.Settings.Default.uiStyle = (int)UIStyle.Modern;
                        UIStyle_Graphics_Update();
                        InnerWin.options_TabPanel.Options_UI_Update();
                    }
                    break;
                case Key.D4:
                    //XMAS
                    break;
                case Key.D5:
                    //HLWN
                    break;
                case Key.D6:
                    if (Properties.Settings.Default.uiStyle != (int)UIStyle.Mini)
                    {
                        Properties.Settings.Default.uiStyle = (int)UIStyle.Mini;
                        UIStyle_Graphics_Update();
                        InnerWin.options_TabPanel.Options_UI_Update();
                    }
                    break;
                case Key.B://PLAY FROM BUILDUP
                    Core.StartAgain();
                    break;
                case Key.F://TOGGLE AUTO MODE
                    if (full_auto_mode) full_auto_mode = false;
                    else full_auto_mode = true;
                    Core.UIHandler.UpdateMiscInfo();
                    break;
                case Key.N://RANDOM SONG
                    Core.random_song();
                    break;
                case Key.C:
                    Core.UIHandler.ToggleCharList();
                    break;
                case Key.S:
                    Core.UIHandler.ToggleSongList();
                    break;
                case Key.L:
                    InnerWin.resources_TabPanel.load_local_RP();
                    break;
                case Key.F11:
                    toggle_fullscreen();
                    break;
                case Key.Escape:
                    if (this.WindowState == WindowState.Maximized)
                        toggle_fullscreen();
                    break;
                case Key.Add:
                    Core.ChangeVolume(1);
                    break;
                case Key.Subtract: 
                    Core.ChangeVolume(-1);
                    break;

            }
        }

        private void toggle_fullscreen()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.ResizeMode = ResizeMode.CanResize;
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.Visibility = Visibility.Visible;
            }
        }
        public void show_info_page()
        {
            if (InnerWin.Visibility == Visibility.Hidden) ToggleInnerWindow();
            InnerWin.tabControl.SelectedIndex = 3;
        }

        #endregion



        #region Timeline events

        // Vertical blur (snare)
        public void timeline_x()
        {
            timeline_pic_and_color();
            timeline_blur_vert();
        }
        // Horizontal blur (bass)
        public void timeline_o()
        {
            timeline_pic_and_color();
            timeline_blur_hor();
        }

        // For '-' in the timeline
        public void timeline_pic_and_color()
        {
            timeline_color_change();
            timeline_image_change();
        }
        // '+'
        public void timeline_blackout()
        {
            Blackout_Rectangle.Fill = Brushes.Black;
            ImageGrid.Effect = XBlur8;
            SB_Blackout.Begin();
        }
        // '¤'
        public void timeline_whiteout()
        {
            Blackout_Rectangle.Fill = Brushes.White;
            ImageGrid.Effect = XBlur8;
            SB_Blackout.Begin();
        }
        // '|'
        public void timeline_blackout_short()
        {
            Blackout_Rectangle.Fill = Brushes.Black;
            Blackout_Rectangle.Opacity = 1;
            timeline_pic_and_color();
            ShortBlackoutTimer.Start();
        }
        private void ShortBlackoutTimer_Tick(object sender, EventArgs e)
        {
            Blackout_Rectangle.Opacity = 0;
            ShortBlackoutTimer.Stop();
        }
        // ':'
        public void timeline_color_change()
        {
            RandomHue();
            ColorOverlap_Rectangle.Fill = hues[CurrentColorInd].brush;
            Core.UIHandler.UpdateColorName(hues[CurrentColorInd].name);
        }
        // '*'
        public void timeline_image_change()
        {
            if (full_auto_mode & Core.enabled_images.Count != 1)
            {
                if (Core.enabled_images.Count != 0)
                {
                    if (Properties.Settings.Default.shuffleImages) ImageChange((Core.current_image_pos + rnd.Next(1, Core.enabled_images.Count - 1)) % Core.enabled_images.Count);
                    else ImageChange(++Core.current_image_pos % Core.enabled_images.Count);
                }
                else ImageChange(-1);
            }
        }





        // 'X' Vertical blur only
        public void timeline_blur_vert()
        {
            if (Core.enabled_images.Count != 0)
            { 
                switch ((BlurQuality)Properties.Settings.Default.blurQuality)
                {
                    case BlurQuality.Low:
                        ImageGrid.Effect = YBlur8;
                        break;
                    case BlurQuality.Medium:
                        ImageGrid.Effect = YBlur14;
                        break;
                    case BlurQuality.High:
                        ImageGrid.Effect = YBlur26;
                        break;
                }
                if (Properties.Settings.Default.anisotropicBlurEnabled)
                    AnisotropicGrid.Effect = XBlur8;
                else
                    AnisotropicGrid.Effect = null;
            }
            BlurAnimSB.Begin();
            if (Properties.Settings.Default.anisotropicBlurEnabled) AnisBlurAnimSB.Begin();

            Core.UIHandler.TBAnimStart(true);
        }

        // 'O' Vertical blur only
        public void timeline_blur_hor()
        {
            if (Core.enabled_images.Count != 0)
            {
                switch ((BlurQuality)Properties.Settings.Default.blurQuality)
                {
                    case BlurQuality.Low:
                        ImageGrid.Effect = XBlur8;
                        break;
                    case BlurQuality.Medium:
                        ImageGrid.Effect = XBlur14;
                        break;
                    case BlurQuality.High:
                        ImageGrid.Effect = XBlur26;
                        break;
                }
                if (Properties.Settings.Default.anisotropicBlurEnabled)
                    AnisotropicGrid.Effect = YBlur8;
                else
                    AnisotropicGrid.Effect = null;
            }
            
            BlurAnimSB.Begin();
            if (Properties.Settings.Default.anisotropicBlurEnabled) AnisBlurAnimSB.Begin();

            Core.UIHandler.TBAnimStart(false);
        }

        // ')' Trippy cirle in
        private void timeline_circle_in()
        {

        }

        // '(' Trippy cirle out
        private void timeline_circle_out()
        {

        }

        // '~' Fade color
        public void timeline_fade()
        {
            SolidColorBrush oldColor = hues[CurrentColorInd].brush;
            RandomHue();
            Fade.Duration = TimeSpan.FromSeconds((Core.CountDots() + 1) * Core.beat_length - (Core.beat_length / 100));
            Fade.From = oldColor.Color;
            Fade.To = hues[CurrentColorInd].brush.Color;
            Core.UIHandler.UpdateColorName(hues[CurrentColorInd].name);
            ColorOverlap_Rectangle.Fill.BeginAnimation(SolidColorBrush.ColorProperty, Fade);
            //ColorOverlap_Rectangle.Fill = hues[CurrentColorInd].brush;
        }

        // '=' Fade and change image
        public void timeline_fade_image()
        {
            timeline_image_change();
            timeline_fade();
        }

        /// <summary> 'i' - inverts colors of the window </summary>
        public void timeline_invert()
        {
            if (Colors_Inverted)
            {
                Effect = null;

                Colors_Inverted = false;
            }
            else
            {
                Effect = invertColorEffect;

                Colors_Inverted = true;
            }
        }

        // 'I' Invert & change image
        public void timeline_invert_w_image()
        {
            timeline_pic_and_color();
            timeline_invert();
        }

        // 's' Horizontal slice
        public void timeline_slice_hor()
        {
            ImageGrid.Effect = SliceHorizontal;
            SlideAnimSB.Begin();
            Core.UIHandler.TBAnimStart(false);
        }
        // 'S' Horizontal slice & change image
        public void timeline_slice_hor_w_im()
        {
            timeline_image_change();
            timeline_slice_hor();
        }
        // 'v' Vertical slice
        public void timeline_slice_ver()
        {
            ImageGrid.Effect = SliceVertical;
            SlideAnimSB.Begin();
            Core.UIHandler.TBAnimStart(true);
        }
        // 'V' Vertical slice & change image
        public void timeline_slice_ver_w_im()
        {
            timeline_image_change();
            timeline_slice_ver();
        }
        // '#' Double slice
        public void timeline_slice_double()
        {
            ImageGrid.Effect = SliceOmni;
            SlideAnimSB.Begin();
            Core.UIHandler.TBAnimStart(true);
        }
        // '@' Double slice and change image
        public void timeline_scice_double_w_im()
        {
            timeline_image_change();
            timeline_slice_double();
        }

        #endregion


        private void Smart_Stretch()
        {
            if (ActualWidth / ActualHeight > Core.RPM.allPics[Core.current_image_pos].pic.Width / Core.RPM.allPics[Core.current_image_pos].pic.Height) image0.Stretch = Stretch.Uniform;
            else image0.Stretch = Stretch.UniformToFill;
        }


        public void Events_Stop()
        {
            if (Colors_Inverted)
            {
                timeline_invert();
            }
            if (Blackout_Rectangle.Opacity != 0)
            {
                SB_Blackout.Stop();
                Blackout_Rectangle.Opacity = 0;
            }
        }

        public void PicPicker_SelectionChanged(int ind)
        {
            ImageChange(ind);
            full_auto_mode = false;
        }

        public void ImageChange(int p)
        {
            BlurAnimSB.Stop();
            Core.current_image_pos = p;
            if (p != -1)
            {
                int index = Core.enabled_images[p].Ind;
                image0.Source = Core.RPM.allPics[index].pic;

                if (Core.RPM.allPics[index].animation == null)
                {
                    AnimTimer.Stop();
                    Core.UIHandler.UpdatePicName(Core.RPM.allPics[index]);
                    switch (Core.RPM.allPics[index].align)
                    {
                        case "left":
                            image0.HorizontalAlignment = HorizontalAlignment.Left;
                            break;
                        case "center":
                            image0.HorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        case "right":
                            image0.HorizontalAlignment = HorizontalAlignment.Right;
                            break;
                    }
                    Smart_Stretch();
                }
                else
                {
                    anim_ind = 1;
                    AnimTimer.Interval = TimeSpan.FromMilliseconds(Core.RPM.allPics[Core.enabled_images[Core.current_image_pos].Ind].frameDuration[0]);
                    AnimTimer.Start();

                    switch (Core.RPM.allPics[index].align)
                    {
                        case "left":
                            image0.HorizontalAlignment = HorizontalAlignment.Left;
                            break;
                        case "center":
                            image0.HorizontalAlignment = HorizontalAlignment.Center;
                            break;
                        case "right":
                            image0.HorizontalAlignment = HorizontalAlignment.Right;
                            break;
                    }
                    Smart_Stretch();
                }
            }
            else
            {
                if (Core.enabled_images.Count == 0)
                {
                    image0.Source = null;
                    Core.UIHandler.UpdatePicName(new Pics());
                }
            }
        }

        private void image_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            Smart_Stretch();
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            image0.Source = Core.RPM.allPics[Core.enabled_images[Core.current_image_pos].Ind].animation[anim_ind];
            if (Core.RPM.allPics[Core.enabled_images[Core.current_image_pos].Ind].animation.Length == anim_ind + 1) anim_ind = 0;
            else anim_ind++;
        }

        private void RandomHue()
        {
            CurrentColorInd = (CurrentColorInd + rnd.Next(1, hues.Length - 2)) % hues.Length;
        }



        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            Core.discordRpcClient.Dispose();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Core.RPM.allPics.Length > 0) Smart_Stretch();
        }
    }

    /// <summary>
    /// Template for the contents of images&songs listboxes
    /// </summary>
    public class rdata
    {
        public string Name { get; set; }
        public int Ind { get; set; }
    }
}
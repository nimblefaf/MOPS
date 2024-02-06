using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows.Threading;

namespace HuesSharp
{
    internal class EffectsManager
    {


        public DispatcherTimer ShortBlackoutTimer = new DispatcherTimer(DispatcherPriority.Render);
        public EffectsManager() 
        {
            SetReferences();
            ShortBlackoutTimer.Tick += new EventHandler(ShortBlackoutTimer_Tick);

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

        public MainWindow MainWin;

        public void SetReferences()
        {
            MainWin = (MainWindow)Application.Current.MainWindow;
            Init_Animations();
        }

        bool Colors_Inverted = false;
        Random rnd = new Random();
        public Hues.Palette[] hues;
        public int CurrentColorInd = 0;

        Shaders.InvertColorEffect invertColorEffect = new Shaders.InvertColorEffect();
        public Shaders.HardLightEffect HardLightEffect = new Shaders.HardLightEffect();
        Shaders.HuesYBlur8Effect YBlur8 = new Shaders.HuesYBlur8Effect();
        Shaders.HuesXBlur8Effect XBlur8 = new Shaders.HuesXBlur8Effect();
        Shaders.HuesYBlur14Effect YBlur14 = new Shaders.HuesYBlur14Effect();
        Shaders.HuesXBlur14Effect XBlur14 = new Shaders.HuesXBlur14Effect();
        Shaders.HuesYBlur26Effect YBlur26 = new Shaders.HuesYBlur26Effect();
        Shaders.HuesXBlur26Effect XBlur26 = new Shaders.HuesXBlur26Effect();
        Shaders.SliceOmniV1Effect SliceOmni = new Shaders.SliceOmniV1Effect();
        Shaders.SliceTestHorV1Effect SliceHorizontal = new Shaders.SliceTestHorV1Effect();
        Shaders.SliceTestVertV1Effect SliceVertical = new Shaders.SliceTestVertV1Effect();


        public Storyboard BlurAnimSB = new Storyboard();
        public DoubleAnimation BlurAnim = new DoubleAnimation();

        private Storyboard SlideAnimSB = new Storyboard();
        private DoubleAnimation SlideAnim = new DoubleAnimation();

        public Storyboard SB_Blackout = new Storyboard();
        private DoubleAnimation Blackout = new DoubleAnimation();
        private Storyboard SB_Blackout_Short = new Storyboard();
        private DoubleAnimation Blackout_Short = new DoubleAnimation();
        private DoubleAnimation Blackout_Blur = new DoubleAnimation();
        public ColorAnimation Fade = new ColorAnimation();



        public void Init_Animations()
        {
            BlurAnim.From = 0.02;
            BlurAnim.To = 0;
            BlurAnim.Duration = TimeSpan.FromSeconds(0.5);
            Storyboard.SetTargetProperty(BlurAnim, new PropertyPath("Effect.BlurAmount"));
            Storyboard.SetTarget(BlurAnim, MainWin.ImageGrid);
            BlurAnimSB.Children.Add(BlurAnim);
            BlurAnimSB.FillBehavior = FillBehavior.Stop;
            BlurAnimSB.DecelerationRatio = 1;

            SlideAnim.From = 0.04;
            SlideAnim.To = 0;
            SlideAnim.Duration = TimeSpan.FromSeconds(0.3);
            Storyboard.SetTargetProperty(SlideAnim, new PropertyPath("Effect.BlurAmount"));
            Storyboard.SetTarget(SlideAnim, MainWin.ImageGrid);
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

            MainWin.Blackout_Rectangle.Opacity = 0;
            Blackout_Short.BeginTime = new TimeSpan(0);
            Blackout_Short.From = 0;
            Blackout_Short.To = 1;
            Blackout_Short.Duration = TimeSpan.FromSeconds(0.2);
            Blackout_Short.FillBehavior = FillBehavior.Stop;
            SB_Blackout_Short.FillBehavior = FillBehavior.Stop;
            Storyboard.SetTargetProperty(Blackout_Short, new PropertyPath(UIElement.OpacityProperty));
            SB_Blackout_Short.Children.Add(Blackout_Short);
            Storyboard.SetTarget(Blackout_Short, MainWin.Blackout_Rectangle);

            Blackout.BeginTime = new TimeSpan(0);
            Blackout.FillBehavior = FillBehavior.Stop;
            Blackout.From = 0;
            Blackout.To = 1;
            Blackout.Duration = TimeSpan.FromSeconds(0.15);
            Blackout.Completed += delegate (object sender, EventArgs e)
            {
                MainWin.Blackout_Rectangle.Opacity = 1;
                MainWin.blackouted = true;
            };
            Storyboard.SetTargetProperty(Blackout, new PropertyPath(UIElement.OpacityProperty));

            Blackout_Blur.BeginTime = new TimeSpan(0);
            Blackout_Blur.FillBehavior = FillBehavior.Stop;
            Blackout_Blur.From = 0;
            Blackout_Blur.To = 0.02;
            Blackout_Blur.Duration = TimeSpan.FromSeconds(0.15);
            Storyboard.SetTargetProperty(Blackout_Blur, new PropertyPath("Effect.BlurAmount"));
            SB_Blackout.Children.Add(Blackout);
            SB_Blackout.Children.Add(Blackout_Blur);
            Storyboard.SetTarget(Blackout, MainWin.Blackout_Rectangle);
            Storyboard.SetTarget(Blackout_Blur, MainWin.ImageGrid);
            SB_Blackout.FillBehavior = FillBehavior.Stop;
        }

        private void RandomHue()
        {
            CurrentColorInd = (CurrentColorInd + rnd.Next(1, hues.Length - 2)) % hues.Length;
        }

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
            MainWin.Blackout_Rectangle.Fill = Brushes.Black;
            MainWin.ImageGrid.Effect = XBlur8;
            SB_Blackout.Begin();
        }
        // '¤'
        public void timeline_whiteout()
        {
            MainWin.Blackout_Rectangle.Fill = Brushes.White;
            MainWin.ImageGrid.Effect = XBlur8;
            SB_Blackout.Begin();
        }
        // '|'
        public void timeline_blackout_short()
        {
            MainWin.Blackout_Rectangle.Fill = Brushes.Black;
            MainWin.Blackout_Rectangle.Opacity = 1;
            timeline_pic_and_color();
            ShortBlackoutTimer.Start();
        }
        private void ShortBlackoutTimer_Tick(object sender, EventArgs e)
        {
            MainWin.Blackout_Rectangle.Opacity = 0;
            ShortBlackoutTimer.Stop();
        }
        // ':'
        public void timeline_color_change()
        {
            RandomHue();
            MainWin.ColorOverlap_Rectangle.Fill = hues[CurrentColorInd].brush;
            MainWin.Core.UIHandler.UpdateColorName(hues[CurrentColorInd].name);
        }
        // '*'
        public void timeline_image_change()
        {
            if (MainWin.full_auto_mode & MainWin.Core.enabled_images.Count != 1)
            {
                if (MainWin.Core.enabled_images.Count != 0)
                {
                    if (Properties.Settings.Default.shuffleImages) MainWin.ImageChange((MainWin.Core.current_image_pos + rnd.Next(1, MainWin.Core.enabled_images.Count - 1)) % MainWin.Core.enabled_images.Count);
                    else MainWin.ImageChange(++MainWin.Core.current_image_pos % MainWin.Core.enabled_images.Count);
                }
                else MainWin.ImageChange(-1);
            }
        }





        // 'X' Vertical blur only
        public void timeline_blur_vert()
        {
            if (MainWin.Core.enabled_images.Count != 0) switch ((BlurQuality)Properties.Settings.Default.blurQuality)
                {
                    case BlurQuality.Low:
                        MainWin.ImageGrid.Effect = YBlur8;
                        break;
                    case BlurQuality.Medium:
                        MainWin.ImageGrid.Effect = YBlur14;
                        break;
                    case BlurQuality.High:
                        MainWin.ImageGrid.Effect = YBlur26;
                        break;
                }
            BlurAnimSB.Begin();
            MainWin.Core.UIHandler.TBAnimStart(true);
        }

        // 'O' Vertical blur only
        public void timeline_blur_hor()
        {
            if (MainWin.Core.enabled_images.Count != 0) switch ((BlurQuality)Properties.Settings.Default.blurQuality)
                {
                    case BlurQuality.Low:
                        MainWin.ImageGrid.Effect = XBlur8;
                        break;
                    case BlurQuality.Medium:
                        MainWin.ImageGrid.Effect = XBlur14;
                        break;
                    case BlurQuality.High:
                        MainWin.ImageGrid.Effect = XBlur26;
                        break;
                }
            BlurAnimSB.Begin();
            MainWin.Core.UIHandler.TBAnimStart(false);
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
            Fade.Duration = TimeSpan.FromSeconds((MainWin.Core.CountDots() + 1) * MainWin.Core.beat_length - (MainWin.Core.beat_length / 100));
            Fade.From = oldColor.Color;
            Fade.To = hues[CurrentColorInd].brush.Color;
            MainWin.Core.UIHandler.UpdateColorName(hues[CurrentColorInd].name);
            MainWin.ColorOverlap_Rectangle.Fill.BeginAnimation(SolidColorBrush.ColorProperty, Fade);
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
                MainWin.Effect = null;

                Colors_Inverted = false;
            }
            else
            {
                MainWin.Effect = invertColorEffect;

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
            MainWin.ImageGrid.Effect = SliceHorizontal;
            SlideAnimSB.Begin();
            MainWin.Core.UIHandler.TBAnimStart(false);
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
            MainWin.ImageGrid.Effect = SliceVertical;
            SlideAnimSB.Begin();
            MainWin.Core.UIHandler.TBAnimStart(true);
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
            MainWin.ImageGrid.Effect = SliceOmni;
            SlideAnimSB.Begin();
            MainWin.Core.UIHandler.TBAnimStart(true);
        }
        // '@' Double slice and change image
        public void timeline_scice_double_w_im()
        {
            timeline_image_change();
            timeline_slice_double();
        }

        #endregion



    }
}

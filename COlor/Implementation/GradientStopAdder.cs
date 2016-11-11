﻿using System;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ColorBox
{
    class GradientStopAdder : Button
    {
        protected Point startPoint;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (Mouse.LeftButton == MouseButtonState.Pressed && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                //проверим что нажата кнопка мыши
                //далее
                Point p = e.GetPosition(this);
                //теперь проверим куда движение
                var shift = p.X - startPoint.X;
                double offset = shift / this.ActualWidth;
                //теперь сдвигаем все Gradients на offset относительно прежних позиций
                var gradients = this.ColorBox.GradientsAll;
                var minGrOff = this.ColorBox.GradientsInit.Min((c) => c.Offset);
                var maxGrOff = this.ColorBox.GradientsInit.Max((c) => c.Offset);

                if (gradients.Min(c => c.Offset) + offset > minGrOff && gradients.Max(c => c.Offset) + offset < maxGrOff)
                {
                    foreach (var grad in gradients)
                    {
                        grad.Offset += offset;
                    }
                }
                startPoint = p;
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (this.ColorBox != null && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                //считаем начальную точку
                startPoint = e.GetPosition(this);
            }

            if (e.Source is GradientStopAdder && this.ColorBox != null && Keyboard.Modifiers != ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control)
            {
                Button btn = e.Source as Button;
                var minGrOff = this.ColorBox.GradientsInit.Min((c) => c.Offset);
                var maxGrOff = this.ColorBox.GradientsInit.Max((c) => c.Offset);
                var offset = Mouse.GetPosition(btn).X / btn.ActualWidth;

                if (offset > minGrOff && offset < maxGrOff)
                {
                    GradientStop _gs = new GradientStop();

                    _gs.Offset = Mouse.GetPosition(btn).X/btn.ActualWidth;
                    _gs.Color = GetColorFromImage(e.GetPosition(this));
                    this.ColorBox.Gradients.Add(_gs);
                    this.ColorBox.GradientsAll.Add(_gs);
                    this.ColorBox.SelectedGradient = _gs;
                    this.ColorBox.Color = _gs.Color;
                    this.ColorBox.SetBrush();
                }
            }
        }

        Color GetColorFromImage(Point p)
        {
            try
            {
                Rect bounds = VisualTreeHelper.GetDescendantBounds(this);
                //+20 за счет добавления еще одного ROw в грид!!!
                RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height + 20, 96, 96, PixelFormats.Default);
                rtb.Render(this);

                byte[] arr;
                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));
                using (var stream = new System.IO.MemoryStream())
                {
                    png.Save(stream);
                    arr = stream.ToArray();
                }

                BitmapSource bitmap = BitmapFrame.Create(new System.IO.MemoryStream(arr));

                byte[] pixels = new byte[4];
                CroppedBitmap cb = new CroppedBitmap(bitmap, new Int32Rect((int)p.X, (int)p.Y + 20, 1, 1));
                cb.CopyPixels(pixels, 4, 0);
                return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
            }
            catch (Exception)
            {
                return this.ColorBox.Color;
            }
        }
        
        public ColorBox ColorBox
        {
            get { return (ColorBox)GetValue(ColorBoxProperty); }
            set { SetValue(ColorBoxProperty, value); }
        }
        public static readonly DependencyProperty ColorBoxProperty =
            DependencyProperty.Register("ColorBox", typeof(ColorBox), typeof(GradientStopAdder));       
    }
}

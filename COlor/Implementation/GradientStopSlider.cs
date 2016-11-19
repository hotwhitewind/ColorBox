using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorBox
{
    class GradientStopSlider : Slider
    {
        protected Point startPoint;
        private static double initMinGrOff;
        private static double initMaxGrOff;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point p = e.GetPosition(this);

            if (Mouse.LeftButton == MouseButtonState.Pressed && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var shift = p.X - startPoint.X;
                double offset = shift / this.ActualWidth;

                var gradients = this.ColorBox.GradientsAll;
                var selectedGr = this.ColorBox.SelectedGradient;
                var minGrOff = this.ColorBox.GradientsInit.Min((c) => c.Offset);
                var maxGrOff = this.ColorBox.GradientsInit.Max((c) => c.Offset);
                var minGr = this.ColorBox.GradientsInit.First(c => c.Offset == minGrOff);
                var maxGr = ColorBox.GradientsInit.First(c => c.Offset == maxGrOff);

                if (!selectedGr.Equals(minGr) && !selectedGr.Equals(maxGr))
                {
                    //не крайние границы
                    if (offset + selectedGr.Offset >= maxGrOff)
                    {
                        selectedGr.Offset = maxGrOff;
                    }
                    else if (offset + selectedGr.Offset <= minGrOff)
                    {
                        selectedGr.Offset = minGrOff;
                    }
                }

                if (selectedGr.Offset >= minGrOff && selectedGr.Offset <= maxGrOff)
                {
                    foreach (var grad in gradients)
                    {
                        //все лежащие справа или слева
                        //все справа
                        if (offset > 0)
                        {
                            if (grad.Offset >= selectedGr.Offset && grad.Offset < maxGrOff && grad.Offset + offset < maxGrOff)
                            {
                                //в зависимости от расстояния до исходного корректируем offset
                                var corr = offset * (selectedGr.Offset - grad.Offset);
                                grad.Offset += offset + corr;
                            }
                            if (grad.Offset < selectedGr.Offset && grad.Offset > minGrOff && grad.Offset + offset > minGrOff)
                            {
                                //в зависимости от расстояния до исходного корректируем offset
                                var corr = offset * (selectedGr.Offset - grad.Offset);
                                grad.Offset += offset - corr;
                            }
                        }
                        else
                        {
                            //все слева
                            if (offset < 0)
                            {
                                if (grad.Offset <= selectedGr.Offset && grad.Offset > minGrOff && grad.Offset + offset > minGrOff)
                                {
                                    var corr = offset * (selectedGr.Offset - grad.Offset);
                                    grad.Offset += offset - corr;
                                }
                                if (grad.Offset > selectedGr.Offset && grad.Offset < maxGrOff && grad.Offset + offset < maxGrOff)
                                {
                                    var corr = offset * (selectedGr.Offset - grad.Offset);
                                    grad.Offset += offset + corr;
                                }
                            }
                        }
                    }
                }
            }
            else if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var shift = p.X - startPoint.X;
                double offset = shift / this.ActualWidth;

                var gradients = this.ColorBox.GradientsAll;
                var selectedGr = this.ColorBox.SelectedGradient;
                var minGrOff = this.ColorBox.GradientsInit.Min((c) => c.Offset);
                var maxGrOff = this.ColorBox.GradientsInit.Max((c) => c.Offset);
                var minGr = this.ColorBox.GradientsInit.First(c => c.Offset == minGrOff);
                var maxGr = ColorBox.GradientsInit.First(c => c.Offset == maxGrOff);

                if (!selectedGr.Equals(minGr) && !selectedGr.Equals(maxGr))
                {
                    //не крайние границы
                    if (offset + selectedGr.Offset > maxGrOff)
                    {
                        selectedGr.Offset = maxGrOff;
                    }
                    else if (offset + selectedGr.Offset < minGrOff)
                    {
                        selectedGr.Offset = minGrOff;
                    }
                }

                if (offset == 0)
                {
                    this.ColorBox.GradientsAllInit = new ObservableCollection<GradientStop>();
                    foreach (var grad in ColorBox.GradientsAll)
                    {
                        this.ColorBox.GradientsAllInit.Add(new GradientStop(grad.Color, grad.Offset));
                    }
                    initMinGrOff = this.ColorBox.GradientsInit.Min((c) => c.Offset);
                    initMaxGrOff = this.ColorBox.GradientsInit.Max((c) => c.Offset);
                }
                if (selectedGr.Equals(minGr) || selectedGr.Equals(maxGr))
                {
                    for (int i = 0; i < gradients.Count; i++)
                    {
                        if (offset != 0 && !gradients[i].Equals(minGr) && !gradients[i].Equals(maxGr))
                        {
                            gradients[i].Offset = minGrOff +
                                                  (ColorBox.GradientsAllInit[i].Offset - initMinGrOff)*(maxGrOff - minGrOff)/
                                                  (initMaxGrOff - initMinGrOff);
                        }
                        //if (offset < 0 && !gradients[i].Equals(maxGr))
                        //{
                        //    gradients[i].Offset = minGrOff +
                        //                          (ColorBox.GradientsAllInit[i].Offset - initMinGrOff) * (maxGrOff - minGrOff) /
                        //                          (initMaxGrOff - initMinGrOff);
                        //}
                    }
                }
                //if (selectedGr.Equals(minGr))
                //{
                //    foreach (var grad in gradients)
                //    {
                //        //все лежащие справа или слева
                //        //все справа
                //        if (offset >= 0 && !grad.Equals(maxGr))
                //        {
                //            if (grad.Offset >= selectedGr.Offset && grad.Offset < maxGrOff && grad.Offset + offset < maxGrOff)
                //            {
                //                //в зависимости от расстояния до исходного корректируем offset
                //                var corr = offset * (selectedGr.Offset - grad.Offset);
                //                grad.Offset += offset + corr;
                //            }
                //            if (grad.Offset < selectedGr.Offset)
                //            {
                //                grad.Offset = selectedGr.Offset;
                //            }
                //        }
                //        else
                //        {
                //            if (offset <= 0 && !grad.Equals(maxGr))
                //            {
                //                if (grad.Offset > selectedGr.Offset && grad.Offset < maxGrOff && grad.Offset + offset < maxGrOff)
                //                {
                //                    var corr = offset * (selectedGr.Offset - grad.Offset);
                //                    grad.Offset += offset + corr;
                //                }
                //            }
                //        }
                //    }
                //}

                //if (selectedGr.Equals(maxGr))
                //{
                //    foreach (var grad in gradients)
                //    {
                //        //все лежащие справа или слева
                //        //все справа
                //        if (offset > 0 && !grad.Equals(minGr))
                //        {
                //            if (grad.Offset < selectedGr.Offset && grad.Offset > minGrOff && grad.Offset + offset > minGrOff)
                //            {
                //                //в зависимости от расстояния до исходного корректируем offset
                //                var corr = offset * (selectedGr.Offset - grad.Offset);
                //                grad.Offset += offset - corr;
                //            }
                //        }
                //        else
                //        {
                //            if (offset < 0 && !grad.Equals(minGr))
                //            {
                //                if (grad.Offset < selectedGr.Offset && grad.Offset > minGrOff && grad.Offset + offset > minGrOff)
                //                {
                //                    var corr = offset * (selectedGr.Offset - grad.Offset);
                //                    grad.Offset += offset - corr;
                //                }
                //                if (grad.Offset > selectedGr.Offset)
                //                {
                //                    grad.Offset = selectedGr.Offset;
                //                }
                //            }
                //        }
                //    }
                //}
            }
            startPoint = p;
            base.OnMouseMove(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            var p = e.GetPosition(this);

            var shift = p.X - startPoint.X;
            double offset = shift / this.ActualWidth;
            
            var selectedGr = this.ColorBox.SelectedGradient;
            var minGrOff = this.ColorBox.GradientsInit.Min((c) => c.Offset);
            var maxGrOff = this.ColorBox.GradientsInit.Max((c) => c.Offset);
            var minGr = this.ColorBox.GradientsInit.First(c => c.Offset == minGrOff);
            var maxGr = ColorBox.GradientsInit.First(c => c.Offset == maxGrOff);

            if (!selectedGr.Equals(minGr) && !selectedGr.Equals(maxGr))
            {
                //не крайние границы
                if (offset + selectedGr.Offset >= maxGrOff)
                {
                    selectedGr.Offset = maxGrOff;
                }
                else if (offset + selectedGr.Offset <= minGrOff)
                {
                    selectedGr.Offset = minGrOff;
                }
            }
            base.OnPreviewMouseLeftButtonUp(e);
        }


        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (this.ColorBox != null)
            {
                startPoint = e.GetPosition(this);
            }
            //initMinGrOff = this.ColorBox.GradientsInit.Min((c) => c.Offset);
            //initMaxGrOff = this.ColorBox.GradientsInit.Max((c) => c.Offset);

            //this.ColorBox.GradientsAllInit = new ObservableCollection<GradientStop>();
            //foreach (var grad in ColorBox.GradientsAll)
            //{
            //    this.ColorBox.GradientsAllInit.Add(new GradientStop(grad.Color, grad.Offset));
            //}

            if (this.ColorBox != null)
            {
                this.ColorBox._BrushSetInternally = true;
                this.ColorBox._UpdateBrush = false;

                this.ColorBox.SelectedGradient = this.SelectedGradient;
                this.ColorBox.Color = this.SelectedGradient.Color;

                this.ColorBox._UpdateBrush = true;
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            if (this.ColorBox != null && this.ColorBox.GradientsInit != null && this.ColorBox.SelectedGradient != null)
            {
                var selectedGr = this.ColorBox.SelectedGradient;
                var minGrOff = this.ColorBox.GradientsInit.Min((c) => c.Offset);
                var maxGrOff = this.ColorBox.GradientsInit.Max((c) => c.Offset);
                var minGr = this.ColorBox.GradientsInit.First(c => c.Offset == minGrOff);
                var maxGr = ColorBox.GradientsInit.First(c => c.Offset == maxGrOff);

                if (!selectedGr.Equals(minGr) && !selectedGr.Equals(maxGr))
                {
                    //не крайние границы
                    if (newValue >= maxGrOff)
                    {
                        newValue = maxGrOff;
                    }
                    if (newValue <= minGrOff)
                    {
                        newValue = minGrOff;
                    }
                }
            }

            if (this.ColorBox != null)
            {
                this.ColorBox._BrushSetInternally = true;
                this.ColorBox.SetBrush();
                this.ColorBox._HSBSetInternally = false;
            }
            base.OnValueChanged(oldValue, newValue);
        }

        public ColorBox ColorBox
        {
            get { return (ColorBox)GetValue(ColorBoxProperty); }
            set { SetValue(ColorBoxProperty, value); }
        }
        public static readonly DependencyProperty ColorBoxProperty =
            DependencyProperty.Register("ColorBox", typeof(ColorBox), typeof(GradientStopSlider));

        public GradientStop SelectedGradient
        {
            get { return (GradientStop)GetValue(SelectedGradientProperty); }
            set { SetValue(SelectedGradientProperty, value); }
        }
        public static readonly DependencyProperty SelectedGradientProperty =
            DependencyProperty.Register("SelectedGradient", typeof(GradientStop), typeof(GradientStopSlider));

    }
}

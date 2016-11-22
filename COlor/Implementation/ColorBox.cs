using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace ColorBox
{
    [TemplatePart(Name = PART_CurrentColor, Type = typeof (TextBox))]

    public class ColorBox : Control
    {
        internal const string PART_CurrentColor = "PART_CurrentColor";

        //internal bool _GradientStopSetInternally = false;
        internal bool _HSBSetInternally = false;
        internal bool _RGBSetInternally = false;
        internal bool _BrushSetInternally = false;
        internal bool _BrushTypeSetInternally = false;
        internal bool _UpdateBrush = true;

        internal TextBox CurrentColorTextBox
        {
            get;
            private set;
        }

        static ColorBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ColorBox), new FrameworkPropertyMetadata(typeof (ColorBox)));

        }

        public static RoutedCommand RemoveGradientStop = new RoutedCommand();
        public static RoutedCommand ReverseGradientStop = new RoutedCommand();

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            CurrentColorTextBox = GetTemplateChild(PART_CurrentColor) as TextBox;

            if (CurrentColorTextBox != null)
            {
                CurrentColorTextBox.PreviewKeyDown += CurrentColorTextBox_PreviewKeyDown;
            }

            this.CommandBindings.Add(new CommandBinding(ColorBox.RemoveGradientStop, RemoveGradientStop_Executed));
            this.CommandBindings.Add(new CommandBinding(ColorBox.ReverseGradientStop, ReverseGradientStop_Executed));

            if (!this._BrushSetInternally && this.Brush != null)
            {
                this._BrushTypeSetInternally = true;

                LinearGradientBrush lgb = this.Brush as LinearGradientBrush;
                //c.Opacity = lgb.Opacity;
                StartX = lgb.StartPoint.X;
                StartY = lgb.StartPoint.Y;
                EndX = lgb.EndPoint.X;
                EndY = lgb.EndPoint.Y;
                MappingMode = lgb.MappingMode;
                SpreadMethod = lgb.SpreadMethod;
                GradientsAll = new ObservableCollection<GradientStop>(lgb.GradientStops);
                GradientsInit = new ObservableCollection<GradientStop>();
                Gradients = new ObservableCollection<GradientStop>();
                //var minOffset = lgb.GradientStops.Min(c => c.Offset);
                //var maxOffset = lgb.GradientStops.Max(c => c.Offset);
                //GradientsInit.Add(lgb.GradientStops.First(c => c.Offset == minOffset));
                //GradientsInit.Add(lgb.GradientStops.First(c => c.Offset == maxOffset));


                GradientsInit.Add(LowInitGradient);
                GradientsInit.Add(HighInitGradient);
                //для вычитания границ из общего градиента
                //GradientsAll.Remove(lgb.GradientStops.First(c => c.Offset == minOffset));
                //GradientsAll.Remove(lgb.GradientStops.First(c => c.Offset == maxOffset));
                ///////////////////////////////////////////
                foreach (var grad in lgb.GradientStops)
                {
                    Gradients.Add(grad);
                }
                _BrushTypeSetInternally = false;
            }

            if (this.GradientsAll == null)
            {
                if (!this._BrushTypeSetInternally)
                {
                    this._BrushTypeSetInternally = true;

                    if (this.GradientsAll == null)
                    {
                        this.GradientsAll = new ObservableCollection<GradientStop>();
                        //вычитание границ из общего градиента
                        this.GradientsAll.Add(new GradientStop(Colors.Green, 0.5));
                        //////////////////////////////////////////////////////////
                        this.GradientsInit = new ObservableCollection<GradientStop>();
                        LowInitGradient = new GradientStop(Colors.Transparent, 0);
                        HighInitGradient = new GradientStop(Colors.Transparent, 1);
                        this.GradientsInit.Add(LowInitGradient);
                        this.GradientsInit.Add(HighInitGradient);
                        this.Gradients = new ObservableCollection<GradientStop>();
                        foreach (var grad in GradientsAll)
                        {
                            Gradients.Add(grad);
                        }
                    }
                    this._BrushTypeSetInternally = false;
                }
            }

            var gradients = this.GradientsAll;
            var minGrOff = this.GradientsInit.Min((c) => c.Offset);
            var maxGrOff = this.GradientsInit.Max((c) => c.Offset);

            var initMinGrOff = GradientsAll.Min((c) => c.Offset);
            var initMaxGrOff = GradientsAll.Max((c) => c.Offset);

            for (int i = 0; i < gradients.Count; i++)
            {
                gradients[i].Offset = minGrOff +
                                      (gradients[i].Offset - initMinGrOff) * (maxGrOff - minGrOff) /
                                      (initMaxGrOff - initMinGrOff);
            }

            SetBrush();
        }

        private void CurrentColorTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression be = CurrentColorTextBox.GetBindingExpression(TextBox.TextProperty);
                if (be != null)
                {
                    be.UpdateSource();
                }
            }
        }

        private void RemoveGradientStop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.GradientsAll != null && this.GradientsAll.Count > 1)
            {
                var tempsel = this.SelectedGradient;
                if (!this.GradientsInit.Contains(tempsel))
                {
                    //это не граничный элемент
                    this.Gradients.Remove(this.SelectedGradient);
                    this.GradientsAll.Remove(tempsel);
                }
                this.SetBrush();
            }
        }

        private void ReverseGradientStop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this._UpdateBrush = false;
            this._BrushSetInternally = true;
            foreach (GradientStop gs in GradientsAll)
            {
                gs.Offset = 1.0 - gs.Offset;
            }
            this._UpdateBrush = true;
            this._BrushSetInternally = false;
            this.SetBrush();
        }

        private void InitTransform()
        {
            if (this.Brush.Transform == null || this.Brush.Transform.Value.IsIdentity)
            {
                this._BrushSetInternally = true;

                TransformGroup _tg = new TransformGroup();
                _tg.Children.Add(new RotateTransform());
                _tg.Children.Add(new ScaleTransform());
                _tg.Children.Add(new SkewTransform());
                _tg.Children.Add(new TranslateTransform());
                this.Brush.Transform = _tg;

                this._BrushSetInternally = false;
            }
        }

        #region Private Properties

        private double StartX
        {
            get { return (double) GetValue(StartXProperty); }
            set { SetValue(StartXProperty, value); }
        }

        private static readonly DependencyProperty StartXProperty =
            DependencyProperty.Register("StartX", typeof (double), typeof (ColorBox),
                new PropertyMetadata(0.5, new PropertyChangedCallback(StartXChanged)));

        private static void StartXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is LinearGradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as LinearGradientBrush).StartPoint = new Point((double) args.NewValue,
                    (cp.Brush as LinearGradientBrush).StartPoint.Y);
                cp._BrushSetInternally = false;
            }
        }

        private double StartY
        {
            get { return (double) GetValue(StartYProperty); }
            set { SetValue(StartYProperty, value); }
        }

        private static readonly DependencyProperty StartYProperty =
            DependencyProperty.Register("StartY", typeof (double), typeof (ColorBox),
                new PropertyMetadata(0.0, new PropertyChangedCallback(StartYChanged)));

        private static void StartYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is LinearGradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as LinearGradientBrush).StartPoint = new Point(
                    (cp.Brush as LinearGradientBrush).StartPoint.X, (double) args.NewValue);
                cp._BrushSetInternally = false;
            }
        }

        private double EndX
        {
            get { return (double) GetValue(EndXProperty); }
            set { SetValue(EndXProperty, value); }
        }

        private static readonly DependencyProperty EndXProperty =
            DependencyProperty.Register("EndX", typeof (double), typeof (ColorBox),
                new PropertyMetadata(0.5, new PropertyChangedCallback(EndXChanged)));

        private static void EndXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is LinearGradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as LinearGradientBrush).EndPoint = new Point((double) args.NewValue,
                    (cp.Brush as LinearGradientBrush).EndPoint.Y);
                cp._BrushSetInternally = false;
            }
        }

        private double EndY
        {
            get { return (double) GetValue(EndYProperty); }
            set { SetValue(EndYProperty, value); }
        }

        private static readonly DependencyProperty EndYProperty =
            DependencyProperty.Register("EndY", typeof (double), typeof (ColorBox),
                new PropertyMetadata(1.0, new PropertyChangedCallback(EndYChanged)));

        private static void EndYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is LinearGradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as LinearGradientBrush).EndPoint = new Point((cp.Brush as LinearGradientBrush).EndPoint.X,
                    (double) args.NewValue);
                cp._BrushSetInternally = false;
            }
        }

        private double GradientOriginX
        {
            get { return (double) GetValue(GradientOriginXProperty); }
            set { SetValue(GradientOriginXProperty, value); }
        }

        private static readonly DependencyProperty GradientOriginXProperty =
            DependencyProperty.Register("GradientOriginX", typeof (double), typeof (ColorBox),
                new PropertyMetadata(0.5, new PropertyChangedCallback(GradientOriginXChanged)));

        private static void GradientOriginXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).GradientOrigin = new Point((double) args.NewValue,
                    (cp.Brush as RadialGradientBrush).GradientOrigin.Y);
                cp._BrushSetInternally = false;
            }
        }

        private double GradientOriginY
        {
            get { return (double) GetValue(GradientOriginYProperty); }
            set { SetValue(GradientOriginYProperty, value); }
        }

        private static readonly DependencyProperty GradientOriginYProperty =
            DependencyProperty.Register("GradientOriginY", typeof (double), typeof (ColorBox),
                new PropertyMetadata(0.5, new PropertyChangedCallback(GradientOriginYChanged)));

        private static void GradientOriginYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).GradientOrigin =
                    new Point((cp.Brush as RadialGradientBrush).GradientOrigin.X, (double) args.NewValue);
                cp._BrushSetInternally = false;
            }
        }

        private double CenterX
        {
            get { return (double) GetValue(CenterXProperty); }
            set { SetValue(CenterXProperty, value); }
        }

        private static readonly DependencyProperty CenterXProperty =
            DependencyProperty.Register("CenterX", typeof (double), typeof (ColorBox),
                new PropertyMetadata(0.5, new PropertyChangedCallback(CenterXChanged)));

        private static void CenterXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).Center = new Point((double) args.NewValue,
                    (cp.Brush as RadialGradientBrush).Center.Y);
                cp._BrushSetInternally = false;
            }
        }

        private double CenterY
        {
            get { return (double) GetValue(CenterYProperty); }
            set { SetValue(CenterYProperty, value); }
        }

        private static readonly DependencyProperty CenterYProperty =
            DependencyProperty.Register("CenterY", typeof (double), typeof (ColorBox),
                new PropertyMetadata(0.5, new PropertyChangedCallback(CenterYChanged)));

        private static void CenterYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).Center = new Point((cp.Brush as RadialGradientBrush).Center.X,
                    (double) args.NewValue);
                cp._BrushSetInternally = false;
            }
        }

        private double RadiusX
        {
            get { return (double) GetValue(RadiusXProperty); }
            set { SetValue(RadiusXProperty, value); }
        }

        private static readonly DependencyProperty RadiusXProperty =
            DependencyProperty.Register("RadiusX", typeof (double), typeof (ColorBox),
                new PropertyMetadata(0.5, new PropertyChangedCallback(RadiusXChanged)));

        private static void RadiusXChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).RadiusX = (double) args.NewValue;
                cp._BrushSetInternally = false;
            }
        }

        private double RadiusY
        {
            get { return (double) GetValue(RadiusYProperty); }
            set { SetValue(RadiusYProperty, value); }
        }

        private static readonly DependencyProperty RadiusYProperty =
            DependencyProperty.Register("RadiusY", typeof (double), typeof (ColorBox),
                new PropertyMetadata(0.5, new PropertyChangedCallback(RadiusYChanged)));

        private static void RadiusYChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is RadialGradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as RadialGradientBrush).RadiusY = (double) args.NewValue;
                cp._BrushSetInternally = false;
            }
        }

        private double BrushOpacity
        {
            get { return (double) GetValue(BrushOpacityProperty); }
            set { SetValue(BrushOpacityProperty, value); }
        }

        private static readonly DependencyProperty BrushOpacityProperty =
            DependencyProperty.Register("BrushOpacity", typeof (double), typeof (ColorBox), new PropertyMetadata(1.0));

        private GradientSpreadMethod SpreadMethod
        {
            get { return (GradientSpreadMethod) GetValue(SpreadMethodProperty); }
            set { SetValue(SpreadMethodProperty, value); }
        }

        private static readonly DependencyProperty SpreadMethodProperty =
            DependencyProperty.Register("SpreadMethod", typeof (GradientSpreadMethod), typeof (ColorBox),
                new PropertyMetadata(GradientSpreadMethod.Pad, new PropertyChangedCallback(SpreadMethodChanged)));

        private static void SpreadMethodChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is GradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as GradientBrush).SpreadMethod = (GradientSpreadMethod) args.NewValue;
                cp._BrushSetInternally = false;
            }
        }

        private BrushMappingMode MappingMode
        {
            get { return (BrushMappingMode) GetValue(MappingModeProperty); }
            set { SetValue(MappingModeProperty, value); }
        }

        private static readonly DependencyProperty MappingModeProperty =
            DependencyProperty.Register("MappingMode", typeof (BrushMappingMode), typeof (ColorBox),
                new PropertyMetadata(BrushMappingMode.RelativeToBoundingBox,
                    new PropertyChangedCallback(MappingModeChanged)));

        private static void MappingModeChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ColorBox cp = property as ColorBox;
            if (cp.Brush is GradientBrush)
            {
                cp._BrushSetInternally = true;
                (cp.Brush as GradientBrush).MappingMode = (BrushMappingMode) args.NewValue;
                cp._BrushSetInternally = false;
            }
        }

        #endregion

        #region Internal Properties


        internal ObservableCollection<GradientStop> Gradients
        {
            get { return (ObservableCollection<GradientStop>)GetValue(GradientsProperty); }
            set { SetValue(GradientsProperty, value); }
        }

        internal static readonly DependencyProperty GradientsProperty =
            DependencyProperty.Register("Gradients", typeof(ObservableCollection<GradientStop>), typeof(ColorBox));

        internal ObservableCollection<GradientStop> GradientsInit
        {
            get { return (ObservableCollection<GradientStop>)GetValue(GradientsInitProperty); }
            set { SetValue(GradientsInitProperty, value); }
        }

        internal static readonly DependencyProperty GradientsInitProperty =
            DependencyProperty.Register("GradientsInit", typeof(ObservableCollection<GradientStop>), typeof(ColorBox));

        internal GradientStop SelectedGradient
        {
            get { return (GradientStop) GetValue(SelectedGradientProperty); }
            set { SetValue(SelectedGradientProperty, value); }
        }

        internal static readonly DependencyProperty SelectedGradientProperty =
            DependencyProperty.Register("SelectedGradient", typeof (GradientStop), typeof (ColorBox));


        #endregion

        #region Public Properties

        public ObservableCollection<GradientStop> GradientsAll;
        public ObservableCollection<GradientStop> GradientsAllInit;

        public GradientStop LowInitGradient
        {
            get { return (GradientStop)GetValue(LowInitGradientProperty); }
            set { SetValue(LowInitGradientProperty, value); }
        }

        public static readonly DependencyProperty LowInitGradientProperty = DependencyProperty.Register("LowInitGradient",
            typeof(GradientStop), typeof(ColorBox));

        public GradientStop HighInitGradient
        {
            get { return (GradientStop)GetValue(HighInitGradientProperty); }
            set { SetValue(HighInitGradientProperty, value); }
        }

        public static readonly DependencyProperty HighInitGradientProperty = DependencyProperty.Register("HighInitGradient",
            typeof(GradientStop), typeof(ColorBox));

        public Brush Brush
        {
            get { return (Brush) GetValue(BrushProperty); }
            set
            {
                SetValue(BrushProperty, value);
            }
        }

        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("Brush", typeof (Brush), typeof (ColorBox));

        public Color Color
        {
            get { return (Color) GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof (Color), typeof (ColorBox),
                new UIPropertyMetadata(Colors.Black, OnColorChanged));

        public static void OnColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ColorBox c = (ColorBox) o;

            if (e.NewValue is Color)
            {
                Color color = (Color) e.NewValue;

                if (!c._HSBSetInternally)
                {
                    // update HSB value based on new value of color

                    double H = 0;
                    double S = 0;
                    double B = 0;
                    ColorHelper.HSBFromColor(color, ref H, ref S, ref B);

                    c._HSBSetInternally = true;

                    c.Alpha = (double) (color.A/255d);
                    c.Hue = H;
                    c.Saturation = S;
                    c.Brightness = B;

                    c._HSBSetInternally = false;
                }

                if (!c._RGBSetInternally)
                {
                    // update RGB value based on new value of color

                    c._RGBSetInternally = true;

                    c.A = color.A;
                    c.R = color.R;
                    c.G = color.G;
                    c.B = color.B;

                    c._RGBSetInternally = false;
                }

                c.RaiseColorChangedEvent((Color) e.NewValue);
            }
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ColorBox), new PropertyMetadata(Orientation.Horizontal));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #endregion


        #region Color Specific Properties

        private double Hue
        {
            get { return (double) GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        private static readonly DependencyProperty HueProperty =
            DependencyProperty.Register("Hue", typeof (double), typeof (ColorBox),
                new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(UpdateColorHSB),
                    new CoerceValueCallback(HueCoerce)));

        private static object HueCoerce(DependencyObject d, object Hue)
        {
            double v = (double) Hue;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private double Brightness
        {
            get { return (double) GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }

        private static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register("Brightness", typeof (double), typeof (ColorBox),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(UpdateColorHSB),
                    new CoerceValueCallback(BrightnessCoerce)));

        private static object BrightnessCoerce(DependencyObject d, object Brightness)
        {
            double v = (double) Brightness;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private double Saturation
        {
            get { return (double) GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }

        private static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register("Saturation", typeof (double), typeof (ColorBox),
                new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(UpdateColorHSB),
                    new CoerceValueCallback(SaturationCoerce)));

        private static object SaturationCoerce(DependencyObject d, object Saturation)
        {
            double v = (double) Saturation;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private double Alpha
        {
            get { return (double) GetValue(AlphaProperty); }
            set { SetValue(AlphaProperty, value); }
        }

        private static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register("Alpha", typeof (double), typeof (ColorBox),
                new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(UpdateColorHSB),
                    new CoerceValueCallback(AlphaCoerce)));

        private static object AlphaCoerce(DependencyObject d, object Alpha)
        {
            double v = (double) Alpha;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        private int A
        {
            get { return (int) GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }

        private static readonly DependencyProperty AProperty =
            DependencyProperty.Register("A", typeof (int), typeof (ColorBox),
                new FrameworkPropertyMetadata(255, new PropertyChangedCallback(UpdateColorRGB),
                    new CoerceValueCallback(RGBCoerce)));


        private int R
        {
            get { return (int) GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }

        private static readonly DependencyProperty RProperty =
            DependencyProperty.Register("R", typeof (int), typeof (ColorBox),
                new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(UpdateColorRGB),
                    new CoerceValueCallback(RGBCoerce)));


        private int G
        {
            get { return (int) GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }

        private static readonly DependencyProperty GProperty =
            DependencyProperty.Register("G", typeof (int), typeof (ColorBox),
                new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(UpdateColorRGB),
                    new CoerceValueCallback(RGBCoerce)));


        private int B
        {
            get { return (int) GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        private static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof (int), typeof (ColorBox),
                new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(UpdateColorRGB),
                    new CoerceValueCallback(RGBCoerce)));


        private static object RGBCoerce(DependencyObject d, object value)
        {
            int v = (int) value;
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }


        #endregion

        /// <summary>
        /// Shared property changed callback to update the Color property
        /// </summary>
        public static void UpdateColorHSB(object o, DependencyPropertyChangedEventArgs e)
        {
            ColorBox c = (ColorBox) o;
            Color n = ColorHelper.ColorFromAHSB(c.Alpha, c.Hue, c.Saturation, c.Brightness);

            c._HSBSetInternally = true;

            c.Color = n;

            if (c.SelectedGradient != null)
                c.SelectedGradient.Color = n;

            c.SetBrush();

            c._HSBSetInternally = false;
        }

        /// <summary>
        /// Shared property changed callback to update the Color property
        /// </summary>
        public static void UpdateColorRGB(object o, DependencyPropertyChangedEventArgs e)
        {
            ColorBox c = (ColorBox) o;
            Color n = Color.FromArgb((byte) c.A, (byte) c.R, (byte) c.G, (byte) c.B);

            c._RGBSetInternally = true;

            c.Color = n;

            if (c.SelectedGradient != null)
                c.SelectedGradient.Color = n;

            c.SetBrush();

            c._RGBSetInternally = false;
        }


        #region ColorChanged Event

        public delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs e);

        public static readonly RoutedEvent ColorChangedEvent =
            EventManager.RegisterRoutedEvent("ColorChanged", RoutingStrategy.Bubble, typeof (ColorChangedEventHandler),
                typeof (ColorBox));

        public event ColorChangedEventHandler ColorChanged
        {
            add { AddHandler(ColorChangedEvent, value); }
            remove { RemoveHandler(ColorChangedEvent, value); }
        }

        private void RaiseColorChangedEvent(Color color)
        {
            ColorChangedEventArgs newEventArgs = new ColorChangedEventArgs(ColorBox.ColorChangedEvent, color);
            RaiseEvent(newEventArgs);
        }

        #endregion


        internal void SetBrush()
        {
            if (!_UpdateBrush)
            {
                return;
            }

            this._BrushSetInternally = true;

            // retain old opacity
            double opacity = 1;
            TransformGroup tempTG = null;
            if (this.Brush != null)
            {
                opacity = this.Brush.Opacity;
                tempTG = Brush.Transform as TransformGroup;
            }

            var brush = new LinearGradientBrush();

            foreach (GradientStop g in GradientsAll)
            {
                brush.GradientStops.Add(new GradientStop(g.Color, g.Offset));
            }
            brush.StartPoint = new Point(this.StartX, this.StartY);
            brush.EndPoint = new Point(this.EndX, this.EndY);
            brush.MappingMode = this.MappingMode;
            brush.SpreadMethod = this.SpreadMethod;
            Brush = brush;

            this.Brush.Opacity = opacity; // retain old opacity
            if (tempTG != null)
                this.Brush.Transform = tempTG;

            this._BrushSetInternally = false;
        }
    }
}

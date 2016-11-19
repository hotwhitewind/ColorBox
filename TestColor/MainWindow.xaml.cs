using System.Windows;
using System.Windows.Media;

namespace TestColor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CB.HighInitGradient.Offset = 1;
            CB.LowInitGradient.Offset = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var grad = new GradientStopCollection
            {
                new GradientStop(Colors.Blue, 0),
                new GradientStop(Colors.BlueViolet, 0.5),
                new GradientStop(Colors.Red, 0.75),
                new GradientStop(Colors.Yellow, 1)
            };

            CB.Brush = new LinearGradientBrush(grad);
            CB.OnApplyTemplate();
        }
    }
}

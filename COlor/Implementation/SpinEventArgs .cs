using System.Windows;

namespace ColorBox
{
    class SpinEventArgs : RoutedEventArgs
    {
        public SpinDirection Direction
        {
            get;
            private set;
        }
      
        public SpinEventArgs(SpinDirection direction)
            : base()
        {
            Direction = direction;
        }
    }
}



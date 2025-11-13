using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace ScreenRecorderTray
{
    public partial class RegionSelectionWindow : Window
    {
        private Point _start;
        private bool _isDragging;

        public Rect SelectedRegion { get; private set; }

        public RegionSelectionWindow()
        {
            InitializeComponent();
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            Cursor = Cursors.Cross;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _start = e.GetPosition(this);
            SelectionRect.Visibility = Visibility.Visible;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;
            var pos = e.GetPosition(this);

            var x = System.Math.Min(pos.X, _start.X);
            var y = System.Math.Min(pos.Y, _start.Y);
            var w = System.Math.Abs(pos.X - _start.X);
            var h = System.Math.Abs(pos.Y - _start.Y);

            Canvas.SetLeft(SelectionRect, x);
            Canvas.SetTop(SelectionRect, y);
            SelectionRect.Width = w;
            SelectionRect.Height = h;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging) return;
            _isDragging = false;

            var pos = e.GetPosition(this);
            var x = System.Math.Min(pos.X, _start.X);
            var y = System.Math.Min(pos.Y, _start.Y);
            var w = System.Math.Abs(pos.X - _start.X);
            var h = System.Math.Abs(pos.Y - _start.Y);

            SelectedRegion = new Rect(x + Left, y + Top, w, h);
            DialogResult = true;
            Close();
        }
    }
}

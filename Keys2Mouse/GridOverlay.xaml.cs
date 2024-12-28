using System.Configuration;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Point = System.Windows.Point;
using System.Runtime.InteropServices;


namespace Keys2Mouse
{
    /// <summary>
    /// Interaction logic for GridOverlay.xaml
    /// </summary>
    public partial class GridOverlay : Window
    {
        private int rows;
        private int columns;
        private int subgridRows;
        private int subgridColumns;
        private double opacity;
        private Color fontColor;
        private Color background;
        private Color drag1Value;
        private Color drag2Value;
        private List<int> input = new List<int>();
        private (int x, int y) heldOrigin = (-1, -1);
        private bool isHold;
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private Grid currentSubGrid;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public GridOverlay()
        {
            InitializeComponent();
            this.IsHitTestVisible = false;
            DynamicGrid.IsHitTestVisible = false;
            LoadGridConfiguration();
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle; 
            SetForegroundWindow(hwnd);
        }

        private void SetClickThrough()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        }

        private void LoadGridConfiguration()
        {
            rows = GetConfigurationValue("NumberOfRows", 26);
            columns = GetConfigurationValue("NumberOfColumns", 26);
            subgridRows = GetConfigurationValue("SubGridRows", 4);
            subgridColumns = GetConfigurationValue("SubGridColumns", 6);
            opacity = GetConfigurationValue("Opacity", 0.2);
            fontColor = (Color)ColorConverter.ConvertFromString(GetConfigurationValue("FontColor", "#FFFFFFFF"));
            background = (Color)ColorConverter.ConvertFromString(GetConfigurationValue("Background", "#FF000000"));
            drag1Value = (Color)ColorConverter.ConvertFromString(GetConfigurationValue("Drag1Value", "#FF00FF00"));
            drag2Value = (Color)ColorConverter.ConvertFromString(GetConfigurationValue("Drag2Value", "#FFFF0000"));
            DynamicGrid.Children.Clear();
            DynamicGrid.RowDefinitions.Clear();
            DynamicGrid.ColumnDefinitions.Clear();
            heldOrigin = (-1, -1);
            SetBackGroundColor();
            currentSubGrid = null;
            input.Clear();
            for (int i = 0; i < rows; i++)
            {
                DynamicGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < columns; j++)
            {
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    var border = new Border
                    {
                        BorderBrush = new SolidColorBrush(fontColor),
                        BorderThickness = new Thickness(0.5)
                    };

                    var textBlock = new TextBlock
                    {
                        Text = $"{GetLetterFromIndex(i)} {GetLetterFromIndex(j)}",
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(fontColor)
                    };
                    var viewbox = new Viewbox
                    {
                        Child = textBlock
                    };
                    border.Child = viewbox;

                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    DynamicGrid.Children.Add(border);
                }
            }
        }

        private dynamic GetConfigurationValue(string key, dynamic defaultValue)
        {
            string? value = ConfigurationManager.AppSettings[key];
            if (defaultValue is int)
            {
                if (int.TryParse(value, out int result))
                {
                    return result;
                }
            } 
            else if (defaultValue is double)
            {
                if (double.TryParse(value, out double result))
                {
                    return result;
                }
            }
            else if (defaultValue is string)
            {
                if (!String.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return defaultValue;
        }

        static string GetLetterFromIndex(int index)
        {
            var value = "";

            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];

            value += letters[index % letters.Length];

            return value;
        }

        static int GetIndexFromLetter(string letter)
        {
            return letters.IndexOf(letter);
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
                return;
            }
            else if (e.Key == Key.Back)
            {
                isHold = false;
                LoadGridConfiguration();
                return;
            } else if (e.Key == Key.OemQuestion)
            {
                var app = (App)Application.Current;
                app.ShowSettings();
                this.Close();
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift))
            {
                isHold = true;
                SetBackGroundColor();
                return;
            }
            else if (e.Key == Key.LeftCtrl && Keyboard.IsKeyUp(Key.LeftShift))
            {
                SetClickThrough();
                if (Keyboard.IsKeyDown(Key.D2))
                {
                    SimulateMouseClick();
                    Thread.Sleep(100);
                    SimulateMouseClick();
                }
                else if (Keyboard.IsKeyDown(Key.D3))
                {
                    SimulateMouseClick();
                    Thread.Sleep(100);
                    SimulateMouseClick();
                    Thread.Sleep(100);
                    SimulateMouseClick();
                }
                else
                    SimulateMouseClick();
                this.Close();
                return;
            }
            else if (e.Key == Key.Tab)
            {
                SetClickThrough();
                SimulateMouseRightClick();
                this.Close();
                return;
            }

            int index = GetIndexFromLetter(e.Key.ToString());
            if (index != -1) input.Add(index);
            if (input.Count == 2)
            {
                var coords = (MoveMouseToGridCell(input[0], input[1]));
                SetCursorPos(coords.x, coords.y);
                CreateSubGrid(input[0], input[1]);
            }
            else if (input.Count == 3)
            {
                if (isHold == false || heldOrigin.x == -1)
                {
                    int row = input[2] / subgridColumns;
                    int col = input[2] % subgridColumns;
                    var coords = MoveMouseToSubGridCell(row, col);
                    SetCursorPos(coords.x, coords.y);
                    LoadGridConfiguration();
                    heldOrigin = coords;
                    SetBackGroundColor();
                }
                else
                {
                    SetClickThrough();
                    SetCursorPos(heldOrigin.x, heldOrigin.y);
                    Thread.Sleep(100);
                    SimulateMouseClickAndHold();
                    int row = input[2] / subgridColumns;
                    int col = input[2] % subgridColumns;
                    var coords = MoveMouseToSubGridCell(row, col);
                    SetCursorPos(coords.x, coords.y);
                    Thread.Sleep(100);
                    SimulateMouseRelease();
                    this.Close();
                }
            }
        }

        private (int x, int y) MoveMouseToGridCell(int row, int column)
        {
            double cellWidth = DynamicGrid.ActualWidth / DynamicGrid.ColumnDefinitions.Count;
            double cellHeight = DynamicGrid.ActualHeight / DynamicGrid.RowDefinitions.Count;
            double x = cellWidth * column + cellWidth / 2;
            double y = cellHeight * row + cellHeight / 2;

            var point = DynamicGrid.PointToScreen(new Point(x, y));

            return ((int)point.X, (int)point.Y);
        }

        private void CreateSubGrid(int row, int column)
        {
            var existingChildren = DynamicGrid.Children.Cast<UIElement>()
                .Where(c => Grid.GetRow(c) == row && Grid.GetColumn(c) == column).ToList();

            foreach (var child in existingChildren)
            {
                DynamicGrid.Children.Remove(child);
            }

            Grid subGrid = new Grid
            {
                ShowGridLines = false,
                Background = Brushes.Transparent
            };

            for (int i = 0; i < subgridRows; i++)
            {
                subGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < subgridColumns; j++)
            {
                subGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < subgridRows; i++)
            {
                for (int j = 0; j < subgridColumns; j++)
                {
                    var border = new Border
                    {
                        BorderBrush = new SolidColorBrush(fontColor),
                        BorderThickness = new Thickness(0.5)
                    };

                    var textBlock = new TextBlock
                    {
                        Text = $"{GetLetterFromIndex((i * subgridColumns) + j)}",
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(fontColor)
                    };
                    var viewbox = new Viewbox
                    {
                        Child = textBlock
                    };
                    border.Child = viewbox;

                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    subGrid.Children.Add(border);
                }
            }

            Grid.SetRow(subGrid, row);
            Grid.SetColumn(subGrid, column);
            DynamicGrid.Children.Add(subGrid);
            currentSubGrid = subGrid;
        }

        private (int x, int y) MoveMouseToSubGridCell(int row, int column)
        {
            if (currentSubGrid == null) return (-1, -1);

            double cellWidth = currentSubGrid.ActualWidth / currentSubGrid.ColumnDefinitions.Count;
            double cellHeight = currentSubGrid.ActualHeight / currentSubGrid.RowDefinitions.Count;
            double x = cellWidth * column + cellWidth / 2;
            double y = cellHeight * row + cellHeight / 2;

            var point = currentSubGrid.PointToScreen(new Point(x, y));

            return ((int)point.X, (int)point.Y);
        }

        private void SetBackGroundColor()
        {
            if (isHold && heldOrigin.x != -1)
                this.Background = new SolidColorBrush(Color.FromArgb((byte)(255.0 * opacity), drag2Value.R, drag2Value.G, drag2Value.B));
            else if (isHold)
                this.Background = new SolidColorBrush(Color.FromArgb((byte)(255.0 * opacity), drag1Value.R, drag1Value.G, drag1Value.B));
            else
                this.Background = new SolidColorBrush(Color.FromArgb((byte)(255.0 * opacity), background.R, background.G, background.B));
        }

        private void SimulateMouseClick() { mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0); }
        private void SimulateMouseClickAndHold() { mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0); }
        private void SimulateMouseRelease() { mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0); }
        private void SimulateMouseRightClick() { mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0); }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
    }
}
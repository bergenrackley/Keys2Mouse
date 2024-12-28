using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MouselessWindows
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
        private List<int> input = new List<int>();
        private (int x, int y) heldOrigin = (-1, -1);
        private bool isHold;
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private Grid currentSubGrid;

        public GridOverlay()
        {
            InitializeComponent();
            this.IsHitTestVisible = false;
            DynamicGrid.IsHitTestVisible = false;
            LoadGridConfiguration();
            //Loaded += OverlayWindow_Loaded;
        }

        private void OverlayWindow_Loaded()
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
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5)
                    };

                    var textBlock = new TextBlock
                    {
                        Text = $"{GetLetterFromIndex(i)}   {GetLetterFromIndex(j)}",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    border.Child = textBlock;

                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    DynamicGrid.Children.Add(border);
                }
            }
        }

        private int GetConfigurationValue(string key, int defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (int.TryParse(value, out int result))
            {
                return result;
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
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift))
            {
                isHold = true;
                SetBackGroundColor();
                return;
            }
            else if (e.Key == Key.LeftCtrl && Keyboard.IsKeyUp(Key.LeftShift))
            {
                SimulateMouseClick();
                return;
            }

            int index = GetIndexFromLetter(e.Key.ToString());
            if (index != -1) input.Add(index);
            if (input.Count == 2)
            {
                var coords = (MoveMouseToGridCell(input[0], input[1]));
                SetCursorPos(coords.x, coords.y);
                CreateSubGrid(input[0], input[1]);
            } else if (input.Count == 3)
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
                } else
                {
                    OverlayWindow_Loaded();
                    SetCursorPos(heldOrigin.x, heldOrigin.y);
                    SimulateMouseClickAndHold();
                    int row = input[2] / subgridColumns;
                    int col = input[2] % subgridColumns;
                    var coords = MoveMouseToSubGridCell(row, col);
                    SetCursorPos(coords.x, coords.y);
                    Thread.Sleep(100);
                    SimulateMouseRelease();
                    //isHold = false;
                    //LoadGridConfiguration();
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
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5)
                    };

                    var textBlock = new TextBlock
                    {
                        Text = $"{GetLetterFromIndex((i*subgridColumns)+j)}",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    border.Child = textBlock;

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
                this.Background = new SolidColorBrush(Color.FromArgb((int)(255.0 * 0.1), 0, 255, 0));
            else if (isHold)
                this.Background = new SolidColorBrush(Color.FromArgb((int)(255.0 * 0.1), 255, 0, 0));
            else
                this.Background = new SolidColorBrush(Color.FromArgb((int)(255.0 * 0.1), 255, 255, 255));
        }

        private void SimulateMouseClick() { mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0); }
        private void SimulateMouseClickAndHold() { mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0); }
        private void SimulateMouseRelease() { mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0); }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000; 
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002; 
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
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
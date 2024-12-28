using System;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using System.IO; // Add reference to System.Windows.Forms
using IWshRuntimeLibrary;
using File = System.IO.File; // Add this at the top of your file
namespace Keys2Mouse
{
    public partial class SettingsWindow : Window
    {
        private Color fontColor;
        private Color backgroundColor;
        private Color drag1Color;
        private Color drag2Color;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            NumberOfRowsTextBox.Text = ConfigurationManager.AppSettings["NumberOfRows"];
            NumberOfColumnsTextBox.Text = ConfigurationManager.AppSettings["NumberOfColumns"];
            SubGridRowsTextBox.Text = ConfigurationManager.AppSettings["SubGridRows"];
            SubGridColumnsTextBox.Text = ConfigurationManager.AppSettings["SubGridColumns"];
            RunOnStartCheckBox.IsChecked = bool.Parse(ConfigurationManager.AppSettings["RunOnStart"]);
            OpacitySlider.Value = double.Parse(ConfigurationManager.AppSettings["Opacity"], CultureInfo.InvariantCulture);

            fontColor = (Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["FontColor"]); 
            FontColorPicker.SelectedColor = fontColor; 
            backgroundColor = (Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["Background"]); 
            BackgroundColorPicker.SelectedColor = backgroundColor; 
            drag1Color = (Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["Drag1Value"]); 
            Drag1ColorPicker.SelectedColor = drag1Color; 
            drag2Color = (Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["Drag2Value"]); 
            Drag2ColorPicker.SelectedColor = drag2Color;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["NumberOfRows"].Value = NumberOfRowsTextBox.Text;
            config.AppSettings.Settings["NumberOfColumns"].Value = NumberOfColumnsTextBox.Text;
            config.AppSettings.Settings["SubGridRows"].Value = SubGridRowsTextBox.Text;
            config.AppSettings.Settings["SubGridColumns"].Value = SubGridColumnsTextBox.Text;
            config.AppSettings.Settings["RunOnStart"].Value = RunOnStartCheckBox.IsChecked.ToString();
            config.AppSettings.Settings["Opacity"].Value = OpacitySlider.Value.ToString(CultureInfo.InvariantCulture);
            config.AppSettings.Settings["FontColor"].Value = fontColor.ToString();
            config.AppSettings.Settings["Background"].Value = backgroundColor.ToString();
            config.AppSettings.Settings["Drag1Value"].Value = drag1Color.ToString();
            config.AppSettings.Settings["Drag2Value"].Value = drag2Color.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            if (RunOnStartCheckBox.IsChecked == true)
            {
                AddApplicationToStartup();
            }
            else
            {
                RemoveApplicationFromStartup();
            }

            System.Windows.MessageBox.Show("Settings Saved!");
            this.Close();
        }


        private void NumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void FontColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) { 
            if (e.NewValue.HasValue) { 
                fontColor = e.NewValue.Value;
            }
        }
        private void BackgroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) { 
            if (e.NewValue.HasValue) {
                backgroundColor = e.NewValue.Value;
            } 
        }
        private void Drag1ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) { 
            if (e.NewValue.HasValue) { 
                drag1Color = e.NewValue.Value; 
            } 
        }

        private void Drag2ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                drag2Color = e.NewValue.Value;
            }
        }

        private Color PickColor(Color currentColor)
        {
            using (var colorDialog = new ColorDialog())
            {
                // Initialize the dialog with the current color
                colorDialog.Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B);

                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var color = colorDialog.Color;
                    return Color.FromArgb(color.A, color.R, color.G, color.B);
                }
            }
            return currentColor;
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OpacityValueTextBlock.Text = OpacitySlider.Value.ToString("F2");
        }

        private void AddApplicationToStartup()
        {
            string shortcutLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Keys2Mouse.lnk");
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");

            if (!File.Exists(shortcutLocation))
            {
                WshShell wshShell = new WshShell();
                IWshShortcut shortcut = wshShell.CreateShortcut(shortcutLocation) as IWshShortcut;
                shortcut.Description = "Mouseless Windows Application";
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                shortcut.Save();
            }
        }

        private void RemoveApplicationFromStartup()
        {
            string shortcutLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Keys2Mouse.lnk");

            if (File.Exists(shortcutLocation))
            {
                File.Delete(shortcutLocation);
            }
        }

    }
}
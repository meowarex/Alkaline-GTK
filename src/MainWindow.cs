using System;
using Gtk;
using AlkalineGTK.Utils;
using System.Threading.Tasks;

namespace AlkalineGTK
{
    public class MainWindow : Window
    {
        private FileChooserButton _fileChooser;
        private ComboBoxText _formatDropdown;
        private Button _convertButton;
        private Entry _outputDirectory;

        public MainWindow() : base("AlkalineGTK - File Converter")
        {
            SetDefaultSize(500, 300);
            SetPosition(WindowPosition.Center);
            DeleteEvent += OnDeleteEvent;

            var vbox = new VBox(false, 10)
            {
                BorderWidth = 10
            };
            Add(vbox);

            // File chooser
            _fileChooser = new FileChooserButton("Select a file", FileChooserAction.Open);
            _fileChooser.FileSet += OnFileSelected;
            vbox.PackStart(_fileChooser, false, false, 5);

            // Output directory chooser
            var hboxDir = new HBox(false, 5);
            var lblDir = new Label("Output Directory:");
            _outputDirectory = new Entry();
            var dirChooser = new Button("Browse");
            dirChooser.Clicked += OnBrowseOutputDirectory;
            hboxDir.PackStart(lblDir, false, false, 0);
            hboxDir.PackStart(_outputDirectory, true, true, 0);
            hboxDir.PackStart(dirChooser, false, false, 0);
            vbox.PackStart(hboxDir, false, false, 5);

            // Format dropdown
            _formatDropdown = new ComboBoxText();
            _formatDropdown.AppendText("Select format");
            _formatDropdown.Active = 0;
            vbox.PackStart(_formatDropdown, false, false, 5);

            // Convert button
            _convertButton = new Button("Convert");
            _convertButton.Clicked += OnConvertClicked;
            vbox.PackStart(_convertButton, false, false, 5);

            ShowAll();
        }

        private void OnDeleteEvent(object sender, DeleteEventArgs args)
        {
            Application.Quit();
        }

        private void OnBrowseOutputDirectory(object sender, EventArgs e)
        {
            var dialog = new FileChooserDialog("Select Output Directory", this, FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel,
                "Select", ResponseType.Accept);
            if (dialog.Run() == (int)ResponseType.Accept)
            {
                _outputDirectory.Text = dialog.Filename;
            }
            dialog.Destroy();
        }

        private async void OnFileSelected(object sender, EventArgs e)
        {
            string inputFile = _fileChooser.Filename;
            if (!string.IsNullOrEmpty(inputFile))
            {
                await LoadSupportedFormats(inputFile);
            }
        }

        private async void OnConvertClicked(object sender, EventArgs e)
        {
            string inputFile = _fileChooser.Filename;
            string outputDir = _outputDirectory.Text;
            string selectedFormat = _formatDropdown.ActiveText;

            if (string.IsNullOrEmpty(inputFile))
            {
                ShowMessage("Please select a file to convert.");
                return;
            }

            if (string.IsNullOrEmpty(outputDir))
            {
                ShowMessage("Please select an output directory.");
                return;
            }

            if (selectedFormat == "Select format")
            {
                ShowMessage("Please select a target format.");
                return;
            }

            try
            {
                var api = new CloudConvertApi();
                var supportedFormats = await api.GetSupportedFormatsAsync(inputFile);

                if (supportedFormats.Contains(selectedFormat, StringComparer.OrdinalIgnoreCase))
                {
                    // Implement the actual conversion logic here
                    // For demonstration, we'll initiate a conversion job with CloudConvert
                    string newFilePath = System.IO.Path.Combine(outputDir, $"{System.IO.Path.GetFileNameWithoutExtension(inputFile)}.{selectedFormat}");
                    bool success = await api.ConvertFileAsync(inputFile, newFilePath, selectedFormat);

                    if (success)
                    {
                        ShowMessage($"File converted and saved to {newFilePath}");
                    }
                    else
                    {
                        ShowMessage("File conversion failed. Please check the logs for more details.");
                    }
                }
                else
                {
                    ShowMessage($"Conversion to {selectedFormat} is not supported.");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error: {ex.Message}");
            }
        }

        private async Task LoadSupportedFormats(string inputFile)
        {
            try
            {
                var api = new CloudConvertApi();
                var supportedFormats = await api.GetSupportedFormatsAsync(inputFile);

                _formatDropdown.RemoveAll();
                _formatDropdown.AppendText("Select format");
                foreach (var format in supportedFormats)
                {
                    _formatDropdown.AppendText(format);
                }
                _formatDropdown.Active = 0;
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to load supported formats: {ex.Message}");
            }
        }

        private void ShowMessage(string message)
        {
            var md = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, message);
            md.Run();
            md.Destroy();
        }
    }
}
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
        private CloudConvertApi _api;
        private Label _statusLabel;

        public MainWindow() : base("AlkalineGTK - File Converter")
        {
            Log("MainWindow constructor started");
            SetDefaultSize(500, 300);
            SetPosition(WindowPosition.Center);
            DeleteEvent += OnDeleteEvent;

            Log("Initializing components");
            InitializeComponents();
            ShowAll();

            // Ask for API key on startup
            GLib.Idle.Add(() =>
            {
                Log("Asking for API key");
                AskForApiKey();
                return false;
            });

            Log("MainWindow constructor completed");
        }

        private void InitializeComponents()
        {
            Log("InitializeComponents started");
            // File chooser
            _fileChooser = new FileChooserButton("Select a file", FileChooserAction.Open);
            _fileChooser.FileSet += OnFileSelected;
            var vbox = new VBox(false, 10)
            {
                BorderWidth = 10
            };
            Add(vbox);
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

            // Add status label
            _statusLabel = new Label("Ready");
            vbox.PackStart(_statusLabel, false, false, 5);

            Log("InitializeComponents completed");
        }

        private void AskForApiKey()
        {
            Log("AskForApiKey method started");
            var dialog = new Dialog("Enter API Key", this, DialogFlags.Modal)
            {
                BorderWidth = 10
            };

            var content = dialog.ContentArea;
            var label = new Label("Please enter your CloudConvert API key:");
            content.Add(label);

            var entry = new Entry
            {
                WidthRequest = 300,
                Visibility = false
            };
            content.Add(entry);

            dialog.AddButton("OK", ResponseType.Ok);
            dialog.AddButton("Cancel", ResponseType.Cancel);

            dialog.Response += (sender, e) =>
            {
                if (e.ResponseId == ResponseType.Ok)
                {
                    string apiKey = entry.Text;
                    if (!string.IsNullOrWhiteSpace(apiKey))
                    {
                        Log("Setting API key");
                        Environment.SetEnvironmentVariable("CLOUDCONVERT_API_TOKEN", apiKey);
                        try
                        {
                            _api = new CloudConvertApi();
                            ShowMessage("API key set successfully");
                        }
                        catch (Exception ex)
                        {
                            ShowMessage($"Error setting API key: {ex.Message}");
                        }
                    }
                    else
                    {
                        ShowMessage("API key is empty, asking again");
                        AskForApiKey(); // Ask again if the key is empty
                    }
                }
                else
                {
                    ShowMessage("API key not provided, closing application");
                    Application.Quit();
                }
                dialog.Destroy();
            };

            dialog.ShowAll();
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
            Log("File selected");
            try
            {
                string inputFile = _fileChooser.Filename;
                Log($"File selected: {inputFile}");
                if (!string.IsNullOrEmpty(inputFile))
                {
                    Log("Loading supported formats");
                    await LoadSupportedFormats(inputFile);
                }
                else
                {
                    Log("No file selected");
                }
            }
            catch (Exception ex)
            {
                Log($"Error in OnFileSelected: {ex.Message}");
            }
        }

        private async void OnConvertClicked(object sender, EventArgs e)
        {
            ShowMessage("Convert button clicked");
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
                if (_api == null)
                {
                    ShowMessage("API is not initialized");
                    return;
                }

                ShowMessage("Getting supported formats");
                var supportedFormats = await _api.GetSupportedFormatsAsync(inputFile);

                if (supportedFormats.Contains(selectedFormat, StringComparer.OrdinalIgnoreCase))
                {
                    ShowMessage("Starting file conversion");
                    string newFilePath = System.IO.Path.Combine(outputDir, $"{System.IO.Path.GetFileNameWithoutExtension(inputFile)}.{selectedFormat}");
                    bool success = await _api.ConvertFileAsync(inputFile, newFilePath, selectedFormat);

                    if (success)
                    {
                        ShowMessage($"File converted successfully: {newFilePath}");
                    }
                    else
                    {
                        ShowMessage("File conversion failed");
                    }
                }
                else
                {
                    ShowMessage($"Conversion to {selectedFormat} is not supported");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error during conversion: {ex.Message}");
            }
        }

        private async Task LoadSupportedFormats(string inputFile)
        {
            try
            {
                if (_api == null)
                {
                    ShowMessage("API is null, asking for API key");
                    AskForApiKey();
                    return;
                }

                ShowMessage("Getting supported formats from API");
                var supportedFormats = await _api.GetSupportedFormatsAsync(inputFile);
                
                if (supportedFormats == null)
                {
                    ShowMessage("Supported formats is null");
                    return;
                }

                ShowMessage($"Supported formats: {string.Join(", ", supportedFormats)}");

                Application.Invoke((sender, args) =>
                {
                    try
                    {
                        ShowMessage("Updating format dropdown");
                        _formatDropdown.RemoveAll();
                        _formatDropdown.AppendText("Select format");
                        foreach (var format in supportedFormats)
                        {
                            if (!string.IsNullOrWhiteSpace(format))
                            {
                                _formatDropdown.AppendText(format);
                            }
                            else
                            {
                                ShowMessage("Skipping empty or null format");
                            }
                        }
                        _formatDropdown.Active = 0;
                        ShowMessage("Format dropdown updated");
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"Error updating format dropdown: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Error in LoadSupportedFormats: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            Program.Log($"MainWindow: {message}");
        }

        private void ShowMessage(string message)
        {
            Log($"Status: {message}");
            Application.Invoke((sender, args) =>
            {
                if (_statusLabel != null)
                {
                    _statusLabel.Text = message;
                }
                else
                {
                    Log("Error: _statusLabel is null");
                }
            });
        }
    }
}
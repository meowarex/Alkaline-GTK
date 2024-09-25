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
            SetDefaultSize(500, 300);
            SetPosition(WindowPosition.Center);
            DeleteEvent += OnDeleteEvent;

            InitializeComponents();
            ShowAll();

            // Ask for API key on startup
            GLib.Idle.Add(() =>
            {
                AskForApiKey();
                return false;
            });
        }

        private void InitializeComponents()
        {
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
        }

        private void AskForApiKey()
        {
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
                        Environment.SetEnvironmentVariable("CLOUDCONVERT_API_TOKEN", apiKey);
                        try
                        {
                            _api = new CloudConvertApi();
                            ShowMessage("API key set successfully!");
                        }
                        catch (Exception ex)
                        {
                            ShowMessage($"Error setting API key: {ex.Message}");
                        }
                    }
                    else
                    {
                        ShowMessage("API key cannot be empty. Please try again.");
                        AskForApiKey(); // Ask again if the key is empty
                    }
                }
                else
                {
                    ShowMessage("API key is required to use the application. The app will now close.");
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
            try
            {
                string inputFile = _fileChooser.Filename;
                Console.WriteLine($"File selected: {inputFile}");
                if (!string.IsNullOrEmpty(inputFile))
                {
                    await LoadSupportedFormats(inputFile);
                }
                else
                {
                    Console.WriteLine("No file selected.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnFileSelected: {ex}");
                ShowMessage($"Error selecting file: {ex.Message}");
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
                if (_api == null)
                {
                    ShowMessage("API key not set. Please restart the application.");
                    return;
                }

                var supportedFormats = await _api.GetSupportedFormatsAsync(inputFile);

                if (supportedFormats.Contains(selectedFormat, StringComparer.OrdinalIgnoreCase))
                {
                    // Implement the actual conversion logic here
                    // For demonstration, we'll initiate a conversion job with CloudConvert
                    string newFilePath = System.IO.Path.Combine(outputDir, $"{System.IO.Path.GetFileNameWithoutExtension(inputFile)}.{selectedFormat}");
                    bool success = await _api.ConvertFileAsync(inputFile, newFilePath, selectedFormat);

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
                if (_api == null)
                {
                    Console.WriteLine("API is null. Asking for API key.");
                    ShowMessage("API key not set. Please set the API key.");
                    AskForApiKey();
                    return;
                }

                Console.WriteLine("Loading supported formats...");
                ShowMessage("Loading supported formats...");
                var supportedFormats = await _api.GetSupportedFormatsAsync(inputFile);
                
                if (supportedFormats == null)
                {
                    Console.WriteLine("Supported formats is null");
                    ShowMessage("Failed to retrieve supported formats.");
                    return;
                }

                Console.WriteLine($"Supported formats: {string.Join(", ", supportedFormats)}");

                Application.Invoke((sender, args) =>
                {
                    try
                    {
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
                                Console.WriteLine("Skipping empty or null format");
                            }
                        }
                        _formatDropdown.Active = 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating format dropdown: {ex}");
                    }
                });

                ShowMessage("Supported formats loaded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadSupportedFormats: {ex}");
                ShowMessage($"Failed to load supported formats. Error: {ex.Message}");
            }
        }

        private void ShowMessage(string message)
        {
            Console.WriteLine($"Status message: {message}");
            Application.Invoke((sender, args) =>
            {
                try
                {
                    if (_statusLabel != null)
                    {
                        _statusLabel.Text = message;
                    }
                    else
                    {
                        Console.WriteLine("Error: _statusLabel is null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating status label: {ex.Message}");
                }
            });
        }
    }
}
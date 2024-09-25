using System;
using Gtk;
using System.IO;

namespace AlkalineGTK
{
    public class Program
    {
        private static string logPath = Path.Combine(Environment.CurrentDirectory, "alkalinegtk_log.txt");

        public static void Main(string[] args)
        {
            try
            {
                Log("Application started");

                Application.Init();
                Log("Gtk Application initialized");

                var win = new MainWindow();
                Log("MainWindow instance created");

                win.Show();
                Log("MainWindow shown");

                Application.Run();
            }
            catch (Exception ex)
            {
                Log($"Unhandled exception: {ex}");
            }
            finally
            {
                Log("Application ended");
            }
        }

        public static void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logMessage = $"[{timestamp}] {message}";
            File.AppendAllText(logPath, logMessage + Environment.NewLine);
        }
    }
}
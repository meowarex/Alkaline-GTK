using System;
using Gtk;
using System.IO;

namespace AlkalineGTK
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Set up console redirection to a file
            string logPath = Path.Combine(Environment.CurrentDirectory, "alkalinegtk_log.txt");
            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                Console.SetOut(writer);
                Console.SetError(writer);

                Console.WriteLine($"Application started at {DateTime.Now}");

                Application.Init();
                var win = new MainWindow();
                win.Show();
                Application.Run();

                Console.WriteLine($"Application ended at {DateTime.Now}");
            }
        }
    }
}
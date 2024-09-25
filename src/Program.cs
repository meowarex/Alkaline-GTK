using System;
using Gtk;

namespace AlkalineGTK
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Application.Init();
            var win = new MainWindow();
            win.Show();
            Application.Run();
        }
    }
}
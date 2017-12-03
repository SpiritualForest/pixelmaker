using System;
using System.Diagnostics; // For sub process creation
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using Gui;

// TODO: Config file?

class PixelMaker {

    private static Dictionary<string, string> ParseArguments(string[] args) {
        // Parse the arguments and return a formatted dictionary
        // of ArgName key, ArgValue value
        var argsDict = new Dictionary<string, string>();
        return argsDict;
    }

    private static void Main(string[] args) {
        // TODO: accept arguments for grid size, square size, etc.
        // Command line arguments should override options outlined in the config file
        Dictionary<string, string> arguments = ParseArguments(args);
        
        // PixelMaker version
        string version = "0.1 pre-alpha";

        /* Check our environment */
        var os = Environment.OSVersion;
        PlatformID pid = os.Platform;

        if (pid == PlatformID.Unix) {
            Console.WriteLine("Unix environment.");
        }
        // Create a new main window object
        MainWindow mainWindow = new MainWindow();
        mainWindow.Text = string.Format("PixelMaker v{0}", version);

        // Run the application
        Application.Run(mainWindow);
    }
}

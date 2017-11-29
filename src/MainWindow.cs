using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using Gui;

/* TODO:
 * 0. Extend GUI to allow for colour selection.
 * 1. Extend GUI to allow for resizing of squares.
 */

namespace Gui {

    class MainWindow : Form {
        
        // Border size in pixels
        private int BorderInformation = SystemInformation.Border3DSize.Width * (int)Math.Pow(SystemInformation.Border3DSize.Width, 2);

        internal MainWindow() {
            /* TODO: Dynamic sizing of the window. */
            // Set Size and center the window
            Size = new Size(1000 + BorderInformation, 600);
            CenterToScreen();

            // Create a new menu strip and assign it to our MainMenuStrip property
            MenuStrip mainMenu = new MenuStrip();
            MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);

            // Create a new GridBox control
            this.CreateGridBox();

            // Assign an event handler to a ResizeEnd event
            ResizeEnd += new EventHandler(HandleResizeEnd);

            // Now we actually populate the menu
            this.PopulateMenubar();
        }
        
        private void CreateGridBox(int sideLength = 10) {
            /* Creates a new GridBox control.
             * Its size is based on the window size,
             * the Main menu strip's height, and the window border size. */
            
            GridBox gridBox = new GridBox();
            gridBox.Location = new Point(0, this.MainMenuStrip.Height-1);
            gridBox.SquareSideLength = sideLength;

            // The window size might not be divisible by SquareSideLength.
            // We must perform a modulus operation on it to find the remainder,
            // and then subtract that remainder from the width.
            // The final result is the width we can safely set.
            int fullWidth = this.Width - BorderInformation;
            int remainder = fullWidth % sideLength;
            fullWidth -= remainder;
            // Now the height
            int fullHeight = this.Height - (this.MainMenuStrip.Height * 2) - BorderInformation;
            remainder = fullHeight % sideLength;
            fullHeight -= remainder;

            gridBox.ClientSize = new Size(fullWidth, fullHeight);
            // Add the newly created GridBox control to the MainWindow's Controls list.
            this.Controls.Add(gridBox);
        }
        
        private void PopulateMenubar() {
            // First we have to obtain a reference to the GridBox object, which contains the methods we want to use as EventHandlers.
            GridBox gridBox = (GridBox)this.Controls.Find("gridbox", true).FirstOrDefault();
            if (gridBox == null) {
                Console.WriteLine("Fatal error. A reference to the GridBox object could not be obtained.");
                Console.WriteLine("The application will exit.");
                this.Close();
            }
            // Top menus
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem viewMenu = new ToolStripMenuItem("View");
            // Sub menu items.
            // For file menu
            //                                  string  img   event handler
            var loadMap = new ToolStripMenuItem("Load", null, new EventHandler(gridBox.LoadMap)); // Load map
            var saveMap = new ToolStripMenuItem("Save", null, new EventHandler(gridBox.SaveMap)); // Save map
            var exportBitmap = new ToolStripMenuItem("Export bitmap", null, new EventHandler(gridBox.ExportBitmap));
            var exitApp = new ToolStripMenuItem("Exit", null, new EventHandler(ExitApplication)); // Exit app
            ToolStripMenuItem[] items = new ToolStripMenuItem[] { loadMap, saveMap, exportBitmap, exitApp };
            fileMenu.DropDownItems.AddRange(items);
            MainMenuStrip.Items.Add(fileMenu);
            // View menu
            var setColor = new ToolStripMenuItem("Paint colour");
            string[] colors = new string[] { "Red", "Blue", "Green", "Yellow", "Pink" };
            foreach(string color in colors) {
                var colorItem = new ToolStripMenuItem(color, null, gridBox.SetColorFromMenu, color);
                setColor.DropDownItems.Add(colorItem);
            }
            viewMenu.DropDownItems.Add(setColor);
            MainMenuStrip.Items.Add(viewMenu);
        }

        protected void HandleResizeEnd(object sender, EventArgs e) {
            /* We need to resize the grid when the window size changes.
             * We delete the old GridBox control
             * and then call the function to create a new one. */
            GridBox gridBox = (GridBox)this.Controls.Find("gridbox", true).FirstOrDefault();
            int sideLength = 10; // The default value is 10
            if (gridBox != null) {
                // Remove the old control
                sideLength = gridBox.SquareSideLength;
                this.Controls.Remove(gridBox);
            }
            // Create the new one
            this.CreateGridBox(sideLength);
        }

        private void ExitApplication(object sender, EventArgs e) {
            Console.WriteLine("Exiting application...");
            Close();
        }
    } // END OF MAINWINDOW CLASS

    class Slider : Control {
        //private Square Handle;
    }
}

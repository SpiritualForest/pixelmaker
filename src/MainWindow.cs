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
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            var exitApp = new ToolStripMenuItem("Exit", null, new EventHandler(ExitApplication));
            fileMenu.DropDownItems.Add(exitApp);
            MainMenuStrip.Items.Add(fileMenu);
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

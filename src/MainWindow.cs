using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using Gui;

/* TODO:
 * 0. Extend GUI to allow for colour selection.
 * 1. Extend GUI to allow for resizing of squares.
 * 2. Implement the ability to move the entire drawing around by a given number of square (left, right, down, up)
 * 3. Implement saving and loading of maps.
 * 4. FIXME: Use the built in ColorDialog for colour selection support. Fuck this menu shit.
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

            // Create a new GridBox control and resize it
            GridBox gridBox = new GridBox();
            this.Controls.Add(gridBox);
            this.ResizeGridBox();

            // Assign an event handler to a ResizeEnd event
            ResizeEnd += new EventHandler(HandleResizeEnd);

            // Now we actually populate the menu
            this.PopulateMenubar();
        }
        
        private void ResizeGridBox() {
            /* Creates a new GridBox control.
             * Its size is based on the window size,
             * the Main menu strip's height, and the window border size.
             * The amount of squares is based on the SquareSideLength property. */
             
            int horizontalAxisPosition = 0; // The top-left x axis position.
            GridBox gridBox = GetGridBox();
            gridBox.Location = new Point(horizontalAxisPosition, this.MainMenuStrip.Height-1);

            // The window size might not be divisible by SquareSideLength.
            // We must perform a modulus operation on it to find the remainder,
            // and then subtract that remainder from the width.
            // The final result is the width we can safely set.
            int fullWidth = this.Width - BorderInformation;
            int remainder = fullWidth % gridBox.SquareSideLength;
            fullWidth -= remainder;
            fullWidth -= horizontalAxisPosition; // We need to subtract <x> from the width so the Grid size won't overflow beyond the window boundary
            // Now the height
            int fullHeight = this.Height - (this.MainMenuStrip.Height * 2) - BorderInformation;
            remainder = fullHeight % gridBox.SquareSideLength;
            fullHeight -= remainder;
            
            // Structs are value types, so we have to create a new one,
            // store it in a temporary struct, and then assign the temporary struct to ClientSize. 
            Size tempSize = new Size(fullWidth, fullHeight);
            Console.WriteLine("The new size is: {0}", tempSize);
            gridBox.ClientSize = tempSize;
        }
        
        private void PopulateMenubar() {
            // FIXME: there's a big grey area on the grid after the menu disappears.
            // FIXME: Also, this function is fucking ugly. Seriously. Refactor this crap ASAP.
            // Find a way to redraw the grid when that happens.
            // Top menus
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem viewMenu = new ToolStripMenuItem("View");
            // Sub menu items.
            // For file menu
            //                                  string  img   event handler
            GridBox gridBox = GetGridBox();
            var loadMap = new ToolStripMenuItem("Load", null, new EventHandler(gridBox.LoadMap)); // Load map
            var saveMap = new ToolStripMenuItem("Save", null, new EventHandler(gridBox.SaveMap)); // Save map
            var exportBitmap = new ToolStripMenuItem("Export bitmap", null, new EventHandler(gridBox.ExportBitmap));
            var exitApp = new ToolStripMenuItem("Exit", null, (sender, e) => { this.Close(); }); // Exit app
            ToolStripMenuItem[] items = new ToolStripMenuItem[] { loadMap, saveMap, exportBitmap, exitApp };
            fileMenu.DropDownItems.AddRange(items);
            MainMenuStrip.Items.Add(fileMenu);

            // Now the View menu
            // FIXME: ShrinkSquares() and ExtendSquares() should account for SquareSideLength!
            ToolStripMenuItem[] squareSizes = new ToolStripMenuItem[] {
                new ToolStripMenuItem("5px", null, (sender, e) => {
                        gridBox.SquareSideLength = 5;
                        }),
                new ToolStripMenuItem("10px", null, (sender, e) => {
                        gridBox.SquareSideLength = 10;
                        }),
                new ToolStripMenuItem("20px", null, (sender, e) => {
                        gridBox.SquareSideLength = 20;
                        }),
                new ToolStripMenuItem("25px", null, (sender, e) => {
                        gridBox.SquareSideLength = 25;
                        }),
            };
            var squareSizeItem = new ToolStripMenuItem("Square Size");
            squareSizeItem.DropDownItems.AddRange(squareSizes);
            viewMenu.DropDownItems.Add(squareSizeItem);

            viewMenu.DropDownItems.Add(new ToolStripMenuItem("Open color dialog", null,
                        (sender, e) => {
                            ColorDialog cd = new ColorDialog();
                            cd.ShowDialog();
                        })
                    );
            MainMenuStrip.Items.Add(viewMenu);
        }

        protected void HandleResizeEnd(object sender, EventArgs e) {
            // Resize the GridBox control
            this.ResizeGridBox();
        }

        internal GridBox GetGridBox() {
            return (GridBox)this.Controls.Find("gridbox", true).FirstOrDefault();
        }
    }
}

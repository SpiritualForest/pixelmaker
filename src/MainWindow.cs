using System;
using System.Collections.Generic;
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
             
            int horizontalAxisPosition = 150; // The top-left x axis position.
            GridBox gridBox = new GridBox();
            gridBox.Location = new Point(horizontalAxisPosition, this.MainMenuStrip.Height-1);
            gridBox.SquareSideLength = sideLength;

            // The window size might not be divisible by SquareSideLength.
            // We must perform a modulus operation on it to find the remainder,
            // and then subtract that remainder from the width.
            // The final result is the width we can safely set.
            int fullWidth = this.Width - BorderInformation;
            int remainder = fullWidth % sideLength;
            fullWidth -= remainder;
            fullWidth -= horizontalAxisPosition; // We need to subtract <x> from the width so the Grid size won't overflow beyond the window boundary
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
            // FIXME: there's a big grey area on the grid after the menu disappears.
            // Find a way to redraw the grid when that happens.
            // Top menus
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem viewMenu = new ToolStripMenuItem("View");
            // Sub menu items.
            // For file menu
            //                                  string  img   event handler
            var loadMap = new ToolStripMenuItem("Load", null, new EventHandler(gridBox.LoadMap)); // Load map
            var saveMap = new ToolStripMenuItem("Save", null, new EventHandler(gridBox.SaveMap)); // Save map
            var exportBitmap = new ToolStripMenuItem("Export bitmap", null, new EventHandler(gridBox.ExportBitmap));
            var exitApp = new ToolStripMenuItem("Exit", null, (sender, e) => { this.Close(); }); // Exit app
            ToolStripMenuItem[] items = new ToolStripMenuItem[] { loadMap, saveMap, exportBitmap, exitApp };
            fileMenu.DropDownItems.AddRange(items);
            MainMenuStrip.Items.Add(fileMenu);
            // View menu
            var setColor = new ToolStripMenuItem("Square Colour");
            // Since AddRange() doesn't support a generic list, we have to loop over it ourselves.
            foreach(ToolStripMenuItem colorItem in this.MenuColorsList()) {
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

        private Dictionary<string, Color[]> GetColors() {
            // Meta-function to construct a dictionary of all colours
            // sorted alphabetically.
            // Used for menu functions.
            // FIXME: This is perhaps a somewhat inefficient way of doing things.
            // Find a better way.
            Dictionary<string, Color[]> colors = new Dictionary<string, Color[]> {
                { "A", new Color[] { Color.AliceBlue, Color.AntiqueWhite, Color.Aqua, Color.Aquamarine, Color.Azure } },
                { "B", new Color[] { Color.Beige, Color.Bisque, Color.Black, Color.BlanchedAlmond, Color.Blue, Color.BlueViolet, Color.Brown, Color.BurlyWood } },
                { "C", new Color[] { Color.CadetBlue, Color.Chartreuse, Color.Chocolate, Color.Coral, Color.CornflowerBlue, 
                                      Color.Cornsilk, Color.Crimson, Color.Cyan } },
                { "D", new Color[] { Color.DarkBlue, Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray, Color.DarkGreen, Color.DarkKhaki, Color.DarkMagenta,
                                      Color.DarkOliveGreen, Color.DarkOrange, Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkSeaGreen,
                                      Color.DarkSlateBlue, Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepPink,
                                      Color.DeepSkyBlue, Color.DimGray, Color.DodgerBlue } },
                { "F", new Color[] { Color.Firebrick, Color.FloralWhite, Color.ForestGreen, Color.Fuchsia } },
                { "G", new Color[] { Color.Gainsboro, Color.GhostWhite, Color.Gold, Color.Goldenrod, Color.Gray, Color.Green, Color.GreenYellow } },
                { "H", new Color[] { Color.Honeydew, Color.HotPink } },
                { "I", new Color[] { Color.IndianRed, Color.Indigo, Color.Ivory } },
                { "K", new Color[] { Color.Khaki } },
                { "L", new Color[] { Color.Lavender, Color.LavenderBlush, Color.LawnGreen, Color.LemonChiffon, Color.LightBlue, Color.LightCoral,
                                     Color.LightCyan, Color.LightGoldenrodYellow, Color.LightGray, Color.LightGreen, Color.LightPink, Color.LightSalmon,
                                     Color.LightSeaGreen, Color.LightSkyBlue, Color.LightSlateGray, Color.LightSteelBlue, Color.LightYellow,
                                     Color.Lime, Color.LimeGreen, Color.Linen } },
                { "M", new Color[] { Color.Magenta, Color.Maroon, Color.MediumAquamarine, Color.MediumBlue, Color.MediumOrchid, Color.MediumPurple,
                                     Color.MediumSeaGreen, Color.MediumSlateBlue, Color.MediumSpringGreen, Color.MediumTurquoise, 
                                     Color.MediumVioletRed, Color.MidnightBlue, Color.MintCream, Color.MistyRose, Color.Moccasin } },
                { "N", new Color[] { Color.NavajoWhite, Color.Navy } },
                { "O", new Color[] { Color.OldLace, Color.Olive, Color.OliveDrab, Color.Orange, Color.OrangeRed, Color.Orchid } },
                { "P", new Color[] { Color.PaleGoldenrod, Color.PaleGreen, Color.PaleTurquoise, Color.PaleVioletRed, Color.PapayaWhip,
                                     Color.PeachPuff, Color.Peru, Color.Pink, Color.Plum, Color.PowderBlue, Color.Purple } },
                { "R", new Color[] { Color.Red, Color.RosyBrown, Color.RoyalBlue, } },
                { "S", new Color[] { Color.SaddleBrown, Color.Salmon, Color.SandyBrown, Color.SeaGreen, Color.SeaShell, Color.Sienna,
                                     Color.Silver, Color.SkyBlue, Color.SlateBlue, Color.SlateGray, Color.Snow, Color.SpringGreen, Color.SteelBlue } },
                { "T", new Color[] { Color.Tan, Color.Teal, Color.Thistle, Color.Tomato, Color.Turquoise } },
                { "V", new Color[] { Color.Violet } },
                { "W", new Color[] { Color.Wheat, Color.White, Color.WhiteSmoke } },
                { "Y", new Color[] { Color.Yellow, Color.YellowGreen } },
            };
            return colors;
        }

        private List<ToolStripMenuItem> MenuColorsList() {
            GridBox gridBox = GetGridBox();
            if (gridBox == null) {
                Console.WriteLine("Fatal error. Could not obtain a reference to the GridBox object.");
                this.Close();
            }
            // Create a list of menu items
            var colorItems = new List<ToolStripMenuItem>();
            // Iterate over the dictionary.
            foreach(KeyValuePair<string, Color[]> entry in GetColors()) {
                // Create the parent item, which is a menu entry with the first letter for each colour
                var parentItem = new ToolStripMenuItem(entry.Key);
                parentItem.MouseEnter += (sender, e) => { gridBox.Invalidate(); };
                foreach(Color color in entry.Value) {
                    // Create the child item, which is the one that actually sets the colour.
                    // Its name comes from the Color.Name property, which is a string.
                    var childItem = new ToolStripMenuItem(color.Name, null, 
                            // Anonymous function (or delegate in C# terms) to set 
                            // the GridBox's SelectedColor property to this item's colour.
                            (sender, e) => { 
                            gridBox.SelectedColor = color;
                            gridBox.Invalidate();
                            });
                    // Add the child item to the parent item
                    parentItem.DropDownItems.Add(childItem);
                }
                // Add the parent item to the list of items
                colorItems.Add(parentItem);
            }
            return colorItems;
        }
        
        private GridBox GetGridBox() {
            GridBox gridBox = (GridBox)this.Controls.Find("gridbox", true).FirstOrDefault();
            return gridBox;
        }
    } // END OF MAINWINDOW CLASS

    class Slider : Control {
        //private Square Handle;
    }

    class ColorSelector : Control {
    }
}

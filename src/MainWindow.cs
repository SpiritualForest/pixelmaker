using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using Gui; // For GridBox control

namespace Gui {

    class MainWindow : Form {
        
        // Border size in pixels
	internal int BorderInformation = SystemInformation.Border3DSize.Width * (int)Math.Pow(SystemInformation.Border3DSize.Width, 2);
        
        // Current map file (loaded or created)
        private string _currentWorkingFile;

        // Hexadecimal values for all the colours we support out of the box.
        private string[] PredefinedColors = new string[] {
            "#ffff8080", "#ffffff80", "#ff80ff80", "#ff00ff80", "#ff80ffff", "#ff0080ff", "#ffff80c0", "#ffff80ff",
            "#FFFF0000", "#FFFFFF00", "#ff80ff00", "#ff00ff40", "#FF00FFFF", "#ff0080c0", "#ff8080c0", "#FFFF00FF",
            "#ff804040", "#ffff8040", "#FF00FF00", "#FF008080", "#ff004080", "#ff8080ff", "#ff800040", "#ffff0080", 
            "#FF800000", "#ffff8000", "#FF008000", "#ff008040", "#FF0000FF", "#ff0000a0", "#FF800080", "#ff8000ff", 
            "#ff400000", "#ff804000", "#ff004000", "#ff004040", "#FF000080", "#ff000040", "#ff400040", "#ff400080", 
            "#FF000000", "#FF808000", "#ff808040", "#FF808080", "#ff408080", "#FFC0C0C0", "#ff400040", "#FFFFFFFF",
        };

        private ColorDialog colorDialog = new ColorDialog(); // Used primarily for creating custom colours
        
        internal string CurrentWorkingFile {
            get { return _currentWorkingFile; }
            set {
                _currentWorkingFile = value;
                SetTitle("");
            }
        }
        
        internal string Version { get; set; } // Our version

        internal void SetTitle(string title) {
            // Sets the Window title.
            Text = string.Format("PixelMaker v{0} - {1}", Version, title);
        }

        internal MainWindow() {
            // Set Size and center the window
            Size = new Size(1200 + BorderInformation, 600);
            CenterToScreen();

            // Create a new menu strip and assign it to our MainMenuStrip property
            MenuStrip mainMenu = new MenuStrip();
            MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);

            // Create a new GridBox control and resize it
            GridBox gridBox = new GridBox();
            gridBox.ParentWindow = this;
            gridBox.Location = new Point((15*3)+(5*3)+20, MainMenuStrip.Height);
            this.Controls.Add(gridBox);
            gridBox.ResizeGridBox();

            // Assign an event handler to a ResizeEnd event
            ResizeEnd += new EventHandler(HandleResizeEnd);

            // Now we actually populate the menu
            this.PopulateMenubar();

            // Draw the color boxes (for color selection)
            Label colorsLabel = new Label();
            colorsLabel.Location = new Point(5, MainMenuStrip.Height+1);
            colorsLabel.Text = "Colors:";
            this.Controls.Add(colorsLabel);

            int y = MainMenuStrip.Height + colorsLabel.Height+3, x = 5; // x,y topleft point. Pixels.
            for(int i = 1; i <= PredefinedColors.Length; i++) {
                // i == 1 initially because 0 % N is 0,
                // which causes a logic problem later on.
                // We'll just always pull the color at i-1 instead of i.
                Color color = ColorTranslator.FromHtml(PredefinedColors[i-1]);
                ColorBox colorBox = new ColorBox(this);
                colorBox.Location = new Point(x, y);
                colorBox.Size = new Size(15, 15);
                colorBox.BackColor = color;
                this.Controls.Add(colorBox);
                x += colorBox.Width+10;
                if (i % 3 == 0) {
                    /* On every 3 ColorBoxes drawn we move downwards by the ColorBox's height plus 10 pixels.
                     * We also have to reset x to its original position. */
                    x = 5;
                    y += colorBox.Height+10;
                }
                colorBox.Invalidate();
            }
            // Now the big ColorBox which shows the currently selected color.
            y += 10;
            Label selected = new Label();
            selected.Text = "Selected:";
            selected.Location = new Point(5, y);
            this.Controls.Add(selected);
            
            ColorBox bigBox = new ColorBox(this);
            bigBox.Location = new Point(5, y+25);
            bigBox.Size = new Size((15*3)+(5*3)+5, 50);
            bigBox.BackColor = gridBox.SelectedColor; // Should be white.
            bigBox.Name = "BigBox"; // Required for updating the BackColor of the box
            this.Controls.Add(bigBox);
            bigBox.Invalidate();
        }
        
        private void PopulateMenubar() {
            // Populates the main menu bar.
            GridBox gridBox = GetGridBox();
            // Top menus
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem viewMenu = new ToolStripMenuItem("View");
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Help");
            // Sub menus.
            // File menu first.
            var loadMap = new ToolStripMenuItem("Load", null, (sender, e) => { gridBox.LoadMap(); }); // Load map
            var saveMap = new ToolStripMenuItem("Save", null, (sender, e) => { gridBox.SaveMap(CurrentWorkingFile); }); // Save map
            // We call SaveMap with null to indicate that we want to select a new file to save into.
            var saveMapAs = new ToolStripMenuItem("Save as", null, (sender, e) => { gridBox.SaveMap(null); }); // Save map as
            var exportBitmap = new ToolStripMenuItem("Export bitmap", null, (sender, e) => { gridBox.ExportBitmap(); }); // Export map into bitmap image
            var exitApp = new ToolStripMenuItem("Exit", null, (sender, e) => { ExitApplication(); }); // Exit app
            ToolStripMenuItem[] items = new ToolStripMenuItem[] { loadMap, saveMap, saveMapAs, exportBitmap, exitApp };
            fileMenu.DropDownItems.AddRange(items);
            MainMenuStrip.Items.Add(fileMenu);

            // Now the View menu
            // Square sizes
            ToolStripMenuItem[] squareSizes = new ToolStripMenuItem[] {
                new ToolStripMenuItem("5px", null, (sender, e) => {
                        gridBox.SquareSideLength = 5;
                        gridBox.ResizeGridBox();
                        }),
                new ToolStripMenuItem("10px", null, (sender, e) => {
                        gridBox.SquareSideLength = 10;
                        gridBox.ResizeGridBox();
                        }),
                new ToolStripMenuItem("20px", null, (sender, e) => {
                        gridBox.SquareSideLength = 20;
                        gridBox.ResizeGridBox();
                        }),
                new ToolStripMenuItem("25px", null, (sender, e) => {
                        gridBox.SquareSideLength = 25;
                        gridBox.ResizeGridBox();
                        }),
            };
            var squareSizeItem = new ToolStripMenuItem("Square Size");
            squareSizeItem.DropDownItems.AddRange(squareSizes);
            viewMenu.DropDownItems.Add(squareSizeItem);
            
            // Background clearing functionality
            viewMenu.DropDownItems.Add(
                    new ToolStripMenuItem("Clear grid", null, (sender, e) => {
                        // Sets all the squares' background colour to the default colour.
                        gridBox.SetBackgroundColor(gridBox.DefaultBackgroundColor);
                    })
            );

            // Set background colour, which does NOT affect the already painted squares.
            viewMenu.DropDownItems.Add(
                    new ToolStripMenuItem("Set background colour", null, (sender, e) => {
                        if (colorDialog.ShowDialog() == DialogResult.OK) {
                            // A "false" boolean parameter indicates that we don't want to set it for all squares,
                            // only for the non painted ones.
                            gridBox.SetBackgroundColor(colorDialog.Color, false);
                        }
                    })
            );
            
            // ColorDialog for custom colours
            viewMenu.DropDownItems.Add(
                    // FIXME: "Select color to draw" is a fucking HORRIBLE way to describe this item's functionality.
                    new ToolStripMenuItem("Select color to draw", null, (sender, e) => {
                        if (colorDialog.ShowDialog() == DialogResult.OK) {
                            gridBox.SelectedColor = colorDialog.Color;
                            this.Controls.Find("BigBox", true).FirstOrDefault().BackColor = colorDialog.Color;
                        }
                    })
            );
            MainMenuStrip.Items.Add(viewMenu);
            MainMenuStrip.Items.Add(helpMenu); // TODO: Make documentation and display it via the help menu
        }

        protected void HandleResizeEnd(object sender, EventArgs e) {
            // Resize the GridBox control
            GetGridBox().ResizeGridBox();
        }

        internal GridBox GetGridBox() {
            return (GridBox)this.Controls.Find("gridbox", true).FirstOrDefault();
        }

        internal void ExitApplication() {
            GridBox gridBox = GetGridBox();
            if (gridBox.GridModified) {
                // Grid was modified. Save changes?
                DialogResult saveChanges = MessageBox.Show("Save changes?", "Save Changes", MessageBoxButtons.YesNoCancel);
                if (saveChanges == DialogResult.Yes) {
                    gridBox.SaveMap(CurrentWorkingFile);
                }
                else if (saveChanges == DialogResult.Cancel) {
                    // Do not quit.
                    return;
                }
            }
            // Actually close the form.
            Close();
        }
    } // END OF MAINWINDOW CLASS
} // END OF GUI NAMESPACE

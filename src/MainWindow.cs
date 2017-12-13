using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using Gui; // For GridBox control

/* TODO:
 * 2. Implement the ability to move the entire drawing around by a given number of square (left, right, down, up)
 * 3. Implement saving and loading of maps.
 */

namespace Gui {
    class ColorBox : Control {
        
        MainWindow ParentWindow { get; set; }
        
        public ColorBox(MainWindow parentWindow) {
            this.SetStyle(ControlStyles.UserPaint, true);
            ParentWindow = parentWindow;
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics graphicsObj = e.Graphics;
            /* Two dark grey lines should be painted at
             * x: 0 - this.Width,
             * y: 0 - this.Height. */
            // Rectangle at Location 0,0, fill Width,Height
            Rectangle areaRectangle = new Rectangle(0, 0, Width, Height);
            SolidBrush paintBrush = new SolidBrush(BackColor);
            graphicsObj.FillRectangle(paintBrush, areaRectangle);
            // Now draw the grey lines
            // First the outer, brighter ones
            Pen greyPen = new Pen(Color.FromArgb(169, 169, 169));

            // Horizontal (x axis)
            graphicsObj.DrawLine(greyPen, 0, 0, Width, 0);
            // Vertical (y axis)
            graphicsObj.DrawLine(greyPen, 0, 0, 0, Height);

            // Inner darker lines
            greyPen.Color = Color.FromArgb(105, 105, 105);

            // Horizontal (x axis)
            graphicsObj.DrawLine(greyPen, 1, 1, Width, 1);
            // Vertical (y axis)
            graphicsObj.DrawLine(greyPen, 1, 1, 1, Height);
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            // A mouse click sets the GridBox's SelectedColor to this square's BackColor
            if (Name != "BigBox") {
                ParentWindow.GetGridBox().SelectedColor = BackColor;
                ColorBox bigBox = (ColorBox)ParentWindow.Controls.Find("BigBox", true).FirstOrDefault();
                if (bigBox == null) {
                    // For some reason the big color box is null.
                    Console.WriteLine("The big color box is null.");
                    return;
                }
                bigBox.BackColor = BackColor;
                bigBox.Invalidate();
            }
        }
    }

    class MainWindow : Form {
        
        // Border size in pixels
	internal int BorderInformation = SystemInformation.Border3DSize.Width * (int)Math.Pow(SystemInformation.Border3DSize.Width, 2);
	private string[] PredefinedColors = new string[] {
            "#ffff8080", "#ffffff80", "#ff80ff80", "#ff00ff80", "#ff80ffff", "#ff0080ff", "#ffff80c0", "#ffff80ff",
            "#FFFF0000", "#FFFFFF00", "#ff80ff00", "#ff00ff40", "#FF00FFFF", "#ff0080c0", "#ff8080c0", "#FFFF00FF",
            "#ff804040", "#ffff8040", "#FF00FF00", "#FF008080", "#ff004080", "#ff8080ff", "#ff800040", "#ffff0080", 
            "#FF800000", "#ffff8000", "#FF008000", "#ff008040", "#FF0000FF", "#ff0000a0", "#FF800080", "#ff8000ff", 
            "#ff400000", "#ff804000", "#ff004000", "#ff004040", "#FF000080", "#ff000040", "#ff400040", "#ff400080", 
            "#FF000000", "#FF808000", "#ff808040", "#FF808080", "#ff408080", "#FFC0C0C0", "#ff400040", "#FFFFFFFF",
        };

        internal MainWindow() {
            // Set Size and center the window
            Size = new Size(1000 + BorderInformation, 600);
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
            colorsLabel.Location = new Point(1, MainMenuStrip.Height+1);
            colorsLabel.Text = "Colors:";
            this.Controls.Add(colorsLabel);

            int y = MainMenuStrip.Height + colorsLabel.Height+3, x = 5;
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
                    /* On every 3 ColorBoxes drawn, we move down by 10 pixels. */
                    x = 5;
                    y += colorBox.Height+10;
                }
                colorBox.Invalidate();
            }
            // Now the big ColorBox which shows the currently selected color.
            y += 10;
            Label selected = new Label();
            selected.Text = "Selected:";
            selected.Location = new Point(1, y);
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
            // Sub menus.
            // File menu first.
            var loadMap = new ToolStripMenuItem("Load", null, new EventHandler(gridBox.LoadMap));
            var saveMap = new ToolStripMenuItem("Save", null, new EventHandler(gridBox.SaveMap)); // Save map
            var exportBitmap = new ToolStripMenuItem("Export bitmap", null, new EventHandler(gridBox.ExportBitmap));
            var exitApp = new ToolStripMenuItem("Exit", null, (sender, e) => { this.Close(); }); // Exit app
            ToolStripMenuItem[] items = new ToolStripMenuItem[] { loadMap, saveMap, exportBitmap, exitApp };
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
                        // Sets all the squares' background colour to White.
                        // TODO: The user should be given a choice of which colour to use.
                        gridBox.SetBackgroundColor(Color.White);
                    })
            );
            MainMenuStrip.Items.Add(viewMenu);
            foreach(ToolStripMenuItem menuItem in MainMenuStrip.Items) {
                /* This just adds a GridBox.Invalidate() call
                 * on every menu item, when its MouseEnter event has occurred.
                 * We have to do this because otherwise the menu overwrites the GridBox,
                 * so we have to redraw the GridBox. */

                // FIXME: Find a different way to solve this problem. This solution is not efficient.
                AddInvalidateCallToMenuItem(menuItem);
            }
        }

        private void AddInvalidateCallToMenuItem(ToolStripMenuItem menuItem) {
            // Recursively adds a GridBox.Invalidate() call to the menu item
            // and all of its children, until no more children are found.
            menuItem.MouseEnter += (sender, e) => { GetGridBox().Invalidate(); };
            foreach(ToolStripMenuItem childItem in menuItem.DropDownItems) {
                AddInvalidateCallToMenuItem(childItem);
            }
        }

        protected void HandleResizeEnd(object sender, EventArgs e) {
            // Resize the GridBox control
            GetGridBox().ResizeGridBox();
        }

        internal GridBox GetGridBox() {
            return (GridBox)this.Controls.Find("gridbox", true).FirstOrDefault();
        }
    } // END OF MAINWINDOW CLASS
} // END OF GUI NAMESPACE

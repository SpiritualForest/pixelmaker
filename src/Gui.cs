using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

/* NOTE: use Invalidate(new Region(Square.AreaRectangle)) when updating individual squares.
 * https://msdn.microsoft.com/en-us/library/wtzka3b5(v=vs.110).aspx
 * https://msdn.microsoft.com/en-us/library/system.drawing.region(v=vs.110).aspx */

namespace Gui {
    class Square {
        internal Rectangle AreaRectangle { get; set; } // The Rectangle to fill with BackColor (Graphics.FillRectangle() method)
        internal Color BackColor { get; set; }
        internal Point Location { get; set; } // Only required for Slider control
        private GridBox Parent { get; set; }

        internal Square(GridBox parent) {
            this.Parent = parent;
        }

        internal void OnMouseEnter() {
            /* Called by the GridBox parent of the square
             * when the mouse cursor enters the square's area */
            Console.WriteLine("OnMouseEnter called. My area: {0}", AreaRectangle);
        }

        internal void OnMouseLeave() {
            /* Called by the GridBox parent of the square
             * when the mouse cursor leaves the square's area. */
            Console.WriteLine("OnMouseLeave called. My area: {0}", AreaRectangle);
        }

        internal void OnLeftMouseClick() {
            /* Called by the GridBox parent of the square
             * when the left mouse button is clicked while in the square's area. */
            Console.WriteLine("Left click on square at {0}", AreaRectangle);
        }

        internal void OnRightMouseClick() {
            /* Called by the GridBox parent of the square
             * when the right mouse button is clicked while in the square's area. */
            Console.WriteLine("Right click on square at {0}", AreaRectangle);
        }
    } // END OF SQUARE CLASS

    class GridBox : Control {
        /* Fields */
        private Size _size;
        private int _squareSideLength;

        /* Properties */

        // The Squares list is a list-of-list of squares.
        // Each sublist represents a y-axis location, with its elements representing
        // the squares whose y-axis location is at that particular index,
        //  when it's divided by SideLength.
        private List<List<Square>> Squares { get; set; }

        private Square ActiveSquare { get; set; } // Refers to the square over which the mouse is currently hovering
        internal Color SelectedColor { get; set; } // The color to which a square is set when it's clicked upon
        
        // The default background colour to which a square should be reset if not set to SelectedColor, or deleted by a right mouse click.
        internal Color DefaultBackgroundColor { get; set; }

        internal int SquareSideLength {
            // The length of a square's side (or edge) in pixels.
            // Minimum is 5. There is no maximum size currently.
            get { return this._squareSideLength; }
            set { 
                if (value < 5) {
                    /* The smallest possible square size is 5x5.
                     * Why? Because that's what I want. Got a problem? Suck it.
                     *
                     * Anyway, squares of less than 5x5 pixels look really small and annoying. */
                    throw new ArgumentException("Side length must be at least 5.");
                }
                else if ((this.Width % value != 0) || (this.Height % value != 0)) {
                    /* The grid's width or height is not divisible by the given value.
                     * Abort operation. */
                    throw new ArgumentException(string.Format("Length error: {0}. Side length must be divisible by {1} and {2}.", value, this.Width, this.Height));
                }
                else {
                    this._squareSideLength = value;
                    if (Squares != null) {
                        /* Since the Squares list is not null,
                         * it means that the resizing took place
                         * after the grid had already been drawn at least once.
                         * Therefore we have to repopulate the squares list
                         * and redraw the grid based on the new side length value. */
                        this.PopulateSquares();
                    }
                }
            }
        }

        internal new Size ClientSize {
            get { return this._size; }
            set {
                /* Before we update the value, we must make sure
                 * that its width and height parameters are evenly
                 * divisible by our SideLength.
                 * Otherwise, the GridBox will have incomplete squares on its edges. */

                if ((value.Width % SquareSideLength != 0) || (value.Height % SquareSideLength != 0)) {
                    /* At least one of the parameters supplied by the new size value
                     * is not divisible by SideLength.
                     * We cannot set it. Abort the operation. */
                    throw new ArgumentException(string.Format("Invalid size: {0}x{1}. Width and height must be divisible by {2}.", 
                                value.Width, value.Height, SquareSideLength));
                }
                else {
                    // The new size is divisible, and therefore safe to set.
                    // After setting it, we repopulate the squares list based on the new size.
                    this._size = value;
                    this.Height = value.Height;
                    this.Width = value.Width;
                    this.PopulateSquares();
                }
            }
        }

        // Constructor
        internal GridBox() {
            // Set UserPaint to true, to indicate that we'll draw the control manually
            this.SetStyle(ControlStyles.UserPaint, true);
            this.Name = "gridbox";
            this.DefaultBackgroundColor = Color.White;
        }

        internal void SetBackgroundColor(Color backColor) {
            /* Set the BackColor property of all the squares to <backColor>.
             * We use this method when we want to change the background color of the grid,
             * which means that all squares should be set to the same color. */
            foreach(var squareList in Squares) {
                foreach(Square squareObj in squareList) {
                    squareObj.BackColor = backColor;
                }
            }
            // Redraw the grid
            this.Invalidate();
        }

        private void PopulateSquares() {
            /* Called when the Size property is changed.
             *
             * This method will populate the private list of squares.
             * It will only be called when the Size property is being set,
             * regardless of when, or where, it's being set from.
             *
             * SquareSideLength is the length of each square's side in pixels. */
            var topList = new List<List<Square>>();
            for(int y = 0; y < this.Height; y += SquareSideLength) {
                var sublist = new List<Square>();
                for(int x = 0; x < this.Width; x += SquareSideLength) {
                    /* We create a new Square object with this GridBox as its parent argument,
                     * set its area rectangle, and background color */
                    Square squareObj = new Square(this);
                    squareObj.AreaRectangle = new Rectangle(x+1, y+1, SquareSideLength-1, SquareSideLength-1);
                    
                    // The default background color of a square is the default background color of the grid
                    squareObj.BackColor = this.DefaultBackgroundColor;

                    // Add the square object to the list
                    sublist.Add(squareObj);
                }
                // Add the sublist to the top list
                topList.Add(sublist);
            }
            /* Set the Squares list of lists to the newly populated top list
             * and redraw the entire GridBox */
            this.Squares = topList;
            this.Invalidate();
        }
        
        private void DrawSquares(Graphics graphicsObj) {
            /* Draws all the squares on the GridBox.
             * This is only called by the OnPaint() event handler method. */
            foreach(var squareList in Squares) {
                foreach(Square squareObj in squareList) {
                    /* Create a new paint brush with the square's color */
                    SolidBrush paintBrush = new SolidBrush(squareObj.BackColor);

                    // Now we fill the square's area rectangle with its background color
                    graphicsObj.FillRectangle(paintBrush, squareObj.AreaRectangle);
                }
            }
        }
        
        // Paint Event Handler 
        protected override void OnPaint(PaintEventArgs e) {
            /* Redraw the entire grid. */
            Graphics graphicsObj = e.Graphics;
            Rectangle clipRectangle = e.ClipRectangle;
            if (clipRectangle.Width == this.Width) {
                /* We must redraw the entire grid. */
                Console.WriteLine("Redrawing entire grid...");
                DrawSquares(graphicsObj);
            }
            else {
                /* Redraw only the specific rectangle. */
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            int x = e.X / SquareSideLength, y = e.Y / SquareSideLength;
            Square squareObj = Squares[y][x];
            if (squareObj != ActiveSquare) {
                /* Either ActiveSquare is null, or there's a previously active square.
                 * In the case of a previously active on, we must call OnMouseLeave() on it.
                 * We also have to call OnMouseEnter() on the new square. */
                if (ActiveSquare != null) {
                    // There is a previously active on
                    ActiveSquare.OnMouseLeave();
                    ActiveSquare = squareObj;
                }
                // Call OnMouseEnter() on the newly entered square
                squareObj.OnMouseEnter();
            }
            // Set our current square object as the active square
            ActiveSquare = squareObj;
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            /* We delegate the handling of a click event onto the square object itself. */
            int x = e.X / SquareSideLength, y = e.Y / SquareSideLength;
            Square squareObj = Squares[y][x];
            if (e.Button == MouseButtons.Left) {
                // Left click will set the colour to SelectedColor
                squareObj.OnLeftMouseClick();
            }
            else if (e.Button == MouseButtons.Right) {
                // Right click will reset the square's colour back to the background colour of the grid
                squareObj.OnRightMouseClick();
            }
        }
    } // END OF GRIDBOX CLASS

    class MainWindow : Form {
        
        // Border size in pixels
        private int BorderInformation = SystemInformation.Border3DSize.Width * (int)Math.Pow(SystemInformation.Border3DSize.Width, 2);

        internal MainWindow() {
            Size = new Size(1000 + BorderInformation, 600);
            CenterToScreen();
            this.CreateGridBox();
            ResizeEnd += new EventHandler(HandleResizeEnd);
        }
        private void CreateGridBox(int sideLength = 10) {
            /* Creates a new GridBox control.
             * Its size is based on the window and window border size.
             * TODO: SquareSideLength should be dynamically set. */
            GridBox gridBox = new GridBox();
            gridBox.Location = new Point(0, 0);
            gridBox.SquareSideLength = sideLength;
            try {
                // Try setting the GridBox's size
                gridBox.ClientSize = new Size(this.Width - BorderInformation, 500);
            }
            catch (ArgumentException) {
                // The new window width is not divisible by SquareSideLength.
                // We must perform a modulus operation on it to find the remainder,
                // and then subtract that remainder from the width.
                // The final result is the width we can safely set.
                int fullWidth = this.Width - BorderInformation;
                int remainder = fullWidth % gridBox.SquareSideLength;
                fullWidth -= remainder;
                gridBox.ClientSize = new Size(fullWidth, 500);
            }
            // Add the newly created GridBox control to the MainWindow's Controls list.
            this.Controls.Add(gridBox);
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

        private void ExitApplication() {
            Close();
        }
    } // END OF MAINWINDOW CLASS

    class Slider : Control {
        //private Square Handle;
    }
}

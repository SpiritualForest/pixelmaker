using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Gui {
    // Required for redrawing the grid.
    enum MouseEventType { Enter, Leave };

    class Square {
        internal Rectangle AreaRectangle { get; set; } // The Rectangle to fill with BackColor (Graphics.FillRectangle() method)
        internal Color BackColor { get; set; }
        internal Point Location { get; set; } // Only required for Slider control
        private GridBox Parent { get; set; }

        /* The only two events which cause the square's rectangle to be redrawn
         * are MouseEnter and MouseLeave.
         * MouseEnter always uses the Parent's SelectedColor color,
         * whereas MouseLeave always uses the square's BackColor color.
         * Because calling Parent.Invalidate() from both results in a call
         * to the GridBox's OnPaint method, we must signify which method invoked the call,
         * so that we can know which colour should be used to draw the square.
         * MouseEventType Enter (or 0) means enter, whereas Leave (or 1) means MouseLeave. */
        internal MouseEventType MouseEvent { get; set; }

        internal Square(GridBox parent) {
            this.Parent = parent;
        }

        internal void OnMouseEnter() {
            /* Called by the GridBox parent of the square
             * when the mouse cursor enters the square's area.
             * We set MouseEventType to 0, to indicate that
             * Parent.SelectedColor should be used to draw the square. */
            Console.WriteLine("Entering area: {0}", this.AreaRectangle);
            MouseEvent = MouseEventType.Enter;
            Parent.Invalidate(this.AreaRectangle);
        }

        internal void OnMouseLeave() {
            /* Called by the GridBox parent of the square
             * when the mouse cursor leaves the square's area.
             * We set MouseEventType to 1, to indicate that
             * Square.BackColor should be used to draw the square. */
            Console.WriteLine("Leaving area: {0}", this.AreaRectangle);
            MouseEvent = MouseEventType.Leave;
            Parent.Invalidate(this.AreaRectangle);
            Parent.Update();
        }

        internal void OnLeftMouseClick() {
            /* Called by the GridBox parent of the square
             * when the left mouse button is clicked while in the square's area.
             * We change the square's BackColor property to Parent.SelectedColor. */
            BackColor = Parent.SelectedColor;
        }

        internal void OnRightMouseClick() {
            /* Called by the GridBox parent of the square
             * when the right mouse button is clicked while in the square's area.
             * Resets the square's BackColor property to the
             * parent's default background color.
             * Basically, "deletes" the color from the square. */
            BackColor = Parent.DefaultBackgroundColor;
        }
    } // END OF SQUARE CLASS

    class GridBox : Control {
#region Fields
        private Size _size;
        private int _squareSideLength;
#endregion
#region Properties
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
#endregion
#region Constructor
        internal GridBox() {
            // Set UserPaint to true, to indicate that we'll draw the control manually
            this.SetStyle(ControlStyles.UserPaint, true);
            this.Name = "gridbox";
            this.DefaultBackgroundColor = Color.White;
            // For now, SelectedColor will default to Color.Blue
            this.SelectedColor = Color.Blue;
        }
#endregion
#region Methods
        internal void SetBackgroundColor(Color backColor) {
            /* Set the BackColor property of all the squares to <backColor>.
             * We use this method when we want to change the background color of the grid,
             * which means that all squares should be set to the same color. */
            
            // First we set the grid's default background colour to <backColor>
            this.DefaultBackgroundColor = backColor;

            // Now we set each square's BackColor property to <backColor>
            foreach(var squareList in Squares) {
                foreach(Square squareObj in squareList) {
                    squareObj.BackColor = backColor;
                }
            }
            // Redraw the grid
            this.Invalidate();
        }

        private void PopulateSquares() {
            /* Called when the Size property or SquareSideLength property is changed.
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
        
        private Square GetSquare(int x, int y) {
            // x and y are pixels.
            // Returns the Square object found at index [y][x] in this.Squares
            x = x / SquareSideLength;
            y = y / SquareSideLength;
            Square squareObj;
            try {
                squareObj = Squares[y][x];
                return squareObj;
            }
            catch(IndexOutOfRangeException) {
                return null;
            }
        }
#endregion
#region PaintEventHandler
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
                /* Redraw only the specific rectangle.
                 * TODO: Construct a SolidBrush object using the square object's
                 * BackColor property, or the GridBox object's DefaultBackgroundColor property. */
                Square squareObj = GetSquare(clipRectangle.X, clipRectangle.Y);
                if (squareObj != null) {
                    Console.WriteLine("Redrawing square at: {0}", squareObj.AreaRectangle.ToString());
                    SolidBrush paintBrush;
                    if (squareObj.MouseEvent == MouseEventType.Enter) {
                        // Enter
                        paintBrush = new SolidBrush(this.SelectedColor);
                    }
                    else {
                        // Leave
                        Console.WriteLine("Leave event detected.");
                        paintBrush = new SolidBrush(squareObj.BackColor);
                    }
                    graphicsObj.FillRectangle(paintBrush, squareObj.AreaRectangle);
                }
            }
        }
#endregion
#region MouseEventHandlers
        protected override void OnMouseMove(MouseEventArgs e) {
            int x = e.X / SquareSideLength, y = e.Y / SquareSideLength;
            Square squareObj = Squares[y][x];
            if (squareObj != ActiveSquare) {
                /* Either ActiveSquare is null, or there's a previously active square.
                 * In the case of a previously active on, we must call OnMouseLeave() on it.
                 * We also have to call OnMouseEnter() on the new square. */
                if (ActiveSquare != null) {
                    // There is a previously active square
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
#endregion
    } // END OF GRIDBOX CLASS
}

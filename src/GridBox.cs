using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Gui; // For Square class and MouseEvent enum

namespace Gui {

    class GridBox : Control {
#region Fields
        private Size _size;
        private int _squareSideLength = 10;
#endregion
#region Properties
        private bool LeftMouseDown { get; set; } // Left button mouse is down (pressed)
        private bool RightMouseDown { get; set; } // Right mouse button is down (pressed)

        // The Squares list is a list-of-list of squares.
        // Each sublist represents a y-axis location, with its elements representing
        // the squares whose y-axis location is at that particular index,
        //  when it's divided by SideLength.
        private List<List<Square>> Squares { get; set; }
        internal Dictionary<Point, Square> PaintedSquares { get; set; } // Only holds squares on which the user clicked

        private Square ActiveSquare { get; set; } // Refers to the square over which the mouse is currently hovering
        internal Color SelectedColor { get; set; } // The color to which a square is set when it's clicked upon
        
        // The default background colour to which a square should be reset if not set to SelectedColor, or deleted by a right mouse click.
        // I didn't use the built in BackColor property because that causes the intermittent lines between the squares to disappear.
        internal Color DefaultBackgroundColor { get; set; }

        internal int SquareSideLength {
            // The length of a square's side (or edge) in pixels.
            // Minimum is 5. There is no maximum size currently.
            get { return this._squareSideLength; }
            set { 
                if (value < 5) {
                    // Minimum value is 5
                    throw new ArgumentException("Side length must be at least 5 pixels.");
                }
                // TODO: ShrinkSquares() and ExtendSquares() should account for side length!
                int oldLength = this._squareSideLength;
                this._squareSideLength = value;
                if (Squares != null) { 
                    this.RecalculateSquareRectangles();
                    if (oldLength > value) {
                        // The new length is smaller than the old one.
                        // This means we need to add more squares to the list.
                        this.ExtendSquares();
                    }
                    else {
                        // The new length is larger.
                        // This time we need to remove squares from the list.
                        this.ShrinkSquares();
                    }
                }
            }
        }

        internal new Size ClientSize {
            get { return this._size; }
            set {
                Size oldSize = this._size; // Required for comparison
                this._size = value;
                this.Width = value.Width;
                this.Height = value.Height;
                // Now we determine whether to extend or shrink the squares list
                if (value.Width > oldSize.Width || value.Height > oldSize.Height) {
                    // New width or height is larger than the old one. Extend.
                    this.ExtendSquares();
                }
                else {
                    // New width or height is smaller than the old one. Shrink.
                    this.ShrinkSquares();
                }
            }
        }
        internal MainWindow ParentWindow { get; set; }
#endregion
#region Constructor
        internal GridBox() {
            // Set UserPaint to true, to indicate that we'll draw the control manually
            this.SetStyle(ControlStyles.UserPaint, true);
            this.Name = "gridbox";
            // Default background and selected colours are white
            this.DefaultBackgroundColor = Color.White;
            this.SelectedColor = Color.White;
            this.PaintedSquares = new Dictionary<Point, Square>();
        }
#endregion
#region SquareMethods
        internal void SetBackgroundColor(Color backColor, bool setAll = true) {
            /* Set the BackColor property of all the squares to <backColor>.
             * We use this method when we want to change the background color of the grid,
             * which means that all squares should be set to the same color.
             * If setAll is true, we set the background colour for all the squares.
             * If it's false, we only set the background colour for squares
             * which were not painted. */
            
            // First we set the grid's default background colour to <backColor>
            this.DefaultBackgroundColor = backColor;

            // Now we set each square's BackColor property to <backColor>
            foreach(var squareList in Squares) {
                foreach(Square squareObj in squareList) {
                    if (!setAll) {
                        // Not all squares should be set.
                        // This means that we want to set the background colour 
                        // only for squares which were not painted by the user
                        if (PaintedSquares.ContainsKey(squareObj.Location)) {
                            // This square is painted. We skip it.
                            continue;
                        }
                    }
                    else {
                        // Remove the square from PaintedSquares because we want a clean grid
                        if (PaintedSquares.ContainsKey(squareObj.Location)) {
                            PaintedSquares.Remove(squareObj.Location);
                        }
                    }
                    // Set the background colour the square
                    squareObj.BackColor = backColor;
                }
            }
            // Redraw the grid
            this.Invalidate();
        }

        private void ShrinkSquares() {
            /* Remove squares from the list of squares */
            // First we remove entirely deleted lists.
            // This happens when the height was changed.

            // We loop backwards from the previous count of squares,
            // because it's larger than the new size (either height or width).
            for(int y = Squares.Count - 1; y > this.Height / SquareSideLength; y--) {
                // Height is in pixels, so we divide it by SquareSideLength to get the list index.
                foreach(Square squareObj in Squares[y]) {
                    if (PaintedSquares.ContainsKey(squareObj.Location)) {
                        // A non existent square cannot be painted ;)
                        PaintedSquares.Remove(squareObj.Location);
                    }
                }
                // Now we can remove the entire sublist.
                Squares.RemoveAt(y);
            }
            // Now we remove any extra squares on each sublist.
            // This happens when the width was changed.
            for(int y = 0; y < Squares.Count; y++) {
                // Once again we loop backwards starting at Count - 1 until we hit the new width.
                for(int x = Squares[y].Count - 1; x > this.Width / SquareSideLength; x--) {
                    Square squareObj = Squares[y][x];
                    Squares[y].RemoveAt(x);
                    if (PaintedSquares.ContainsKey(squareObj.Location)) {
                        // Remove this square from the PaintedSquares dictionary as well
                        PaintedSquares.Remove(squareObj.Location);
                    }
                }
            }
            // Redraw the grid
            this.Invalidate();
        }

        private void ExtendSquares() {
            /* Called when the Size property or SquareSideLength property is changed,
             * and the resulting change is a higher window size, 
             * or a larger amount of squares.
             *
             * Extends (or initializes) the squares list. */
            if (Squares == null) {
                // First-time initialization of the entire grid.
                var topList = new List<List<Square>>();
                for(int y = 0; y < this.Height; y += SquareSideLength) {
                    var sublist = new List<Square>();
                    for(int x = 0; x < this.Width; x += SquareSideLength) {
                        /* We create a new Square object with this GridBox as its parent,
                         * and x and y as its location and rectangle parameters.
                         * The actual location and rectangle are created and set
                         * in the square's constructor. */
                        Square squareObj = new Square(this, x, y);
                        sublist.Add(squareObj);
                    }
                    topList.Add(sublist);
                }
                /* Set the Squares list of lists to the newly populated top list. */
                this.Squares = topList;
            }
            else {
                // Modify the existing squares list
                // First extend the existing sublists.
                for(int y = 0; y < Squares.Count; y++) {
                    // y is an index value
                    for(int x = Squares[y].Count * SquareSideLength; x < this.Width; x += SquareSideLength) {
                        Square squareObj = new Square(this, x, y * SquareSideLength);
                        Squares[y].Add(squareObj);
                    }
                }
                // Now add the potentially new lists
                for(int y = Squares.Count * SquareSideLength; y < this.Height; y += SquareSideLength) {
                    // y is a pixel point
                    List<Square> sublist = new List<Square>();
                    for(int x = 0; x < this.Width; x += SquareSideLength) {
                        Square squareObj = new Square(this, x, y);
                        sublist.Add(squareObj);
                    }
                    Squares.Add(sublist);
                }
            }
            // Redraw the grid.
            this.Invalidate();
        }
        
        private void DrawSquares(Graphics graphicsObj) {
            /* Draws all the squares on the GridBox.
             * This is only called by the OnPaint() event handler method. */

            // Create a new SolidBrush object with the default background colour.
            // We'll change the colour to the square object's BackColor on each iteration.
            SolidBrush paintBrush = new SolidBrush(this.DefaultBackgroundColor);
            Console.WriteLine("DrawSquares total squares: {0}x{1} (y, x)", Squares.Count, Squares[0].Count);
            foreach(var squareList in Squares) {
                foreach(Square squareObj in squareList) {
                    /* Change the paint brush's colour to the square's BackColor */
                    paintBrush.Color = squareObj.BackColor;

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
            try {
                return Squares[y][x];
            }
            catch(ArgumentOutOfRangeException) {
                return null;
            }
        }

        private void RecalculateSquareRectangles() {
            /* Iterates over the list of squares
             * and recalculates the area rectangle and top-left location point
             * of each square based on the current SquareSideLength */
            for(int y = 0; y < Squares.Count; y++) {
                for(int x = 0; x < Squares[y].Count; x++) {
                    Square squareObj = Squares[y][x];
                    Rectangle AreaRectangle = new Rectangle(x*SquareSideLength+1, y*SquareSideLength+1, SquareSideLength-1, SquareSideLength-1);
                    Point Location = new Point(x*SquareSideLength, y*SquareSideLength);
                    squareObj.Location = Location;
                    squareObj.AreaRectangle = AreaRectangle;
                }
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
            else if (clipRectangle.Width <= SquareSideLength) {
                /* Redraw only the specific rectangle (individual square). */
                Square squareObj = GetSquare(clipRectangle.X, clipRectangle.Y);
                if (squareObj != null) {
                    SolidBrush paintBrush;
                    if (squareObj.MouseEvent == MouseEventType.Enter) {
                        // Enter
                        paintBrush = new SolidBrush(this.SelectedColor);
                    }
                    else {
                        // Leave
                        paintBrush = new SolidBrush(squareObj.BackColor);
                    }
                    // Fill the rectangle with the specified colour
                    graphicsObj.FillRectangle(paintBrush, squareObj.AreaRectangle);
                }
            }
            else {
                // Redraw an area that contains several squares, but not the entire grid.
                // Sample: {X=0,Y=0,Width=95,Height=87}
                // Starting at the square found at X,Y, we have to redraw Width*Height squares.
                int y = clipRectangle.Y / SquareSideLength, x = clipRectangle.X / SquareSideLength;
                int originalX = x;
                for(int verticalSquares = -1; verticalSquares <= clipRectangle.Height / SquareSideLength; verticalSquares++) {
                    for(int horizontalSquares = -1; horizontalSquares <= clipRectangle.Width / SquareSideLength; horizontalSquares++) {
                        try {
                            Square squareObj = Squares[y][x];
                            SolidBrush paintBrush = new SolidBrush(squareObj.BackColor);
                            e.Graphics.FillRectangle(paintBrush, squareObj.AreaRectangle);
                        }
                        catch(ArgumentOutOfRangeException) {
                            Console.WriteLine("OnPaintBackground on non existing square.");
                        }
                        finally {
                            x++;
                        }
                    }
                    // After each vertical (y axis) completion, x must be set to its original value
                    x = originalX;
                    y++;
                }
            } 
        }
#endregion
#region MouseEventHandlers
        protected override void OnMouseMove(MouseEventArgs e) {
            /* We have to check the position of the mouse on the grid (in pixels),
             * and obtain the Square object which resides in that position.
             * If that square is not also the currently set ActiveSquare,
             * it means that we've left the previously active square, and we
             * have to reset its colour to its own BackColor. */

            Square squareObj = GetSquare(e.X, e.Y);
            if (squareObj == null) {
                // No such square. We must be out of bounds.
                Console.WriteLine("MouseMove error. No such square. Out of bounds.");
                return;
            }
            if (squareObj != ActiveSquare) {
                /* Either ActiveSquare is null, or there's a previously active square.
                 * In the case of a previously active on, we must call OnMouseLeave() on it.
                 * We also have to call OnMouseEnter() on the new square. */
                if (ActiveSquare != null) {
                    // There is a previously active square
                    ActiveSquare.OnMouseLeave();
                    ActiveSquare = squareObj;
                }
                /* Call OnMouseEnter() on the newly entered square.
                 * We do this here to avoid calling OnMouseEnter()
                 * whenever the mouse moves inside the square
                 * after having already entered its area.
                 *
                 * However, before calling OnMouseEnter(), we must check whether
                 * one of the mouse buttons is currently pressed.
                 * If it's pressed, it means that the square is caught in a mouse drag,
                 * and therefore should be notified that this is happening. */
                if (LeftMouseDown) {
                    squareObj.MouseEvent = MouseEventType.LeftButtonDrag;
                }
                else if (RightMouseDown) {
                    squareObj.MouseEvent = MouseEventType.RightButtonDrag;
                }
                squareObj.OnMouseEnter();
            }
            // Set our current square object as the active square
            ActiveSquare = squareObj;
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            /* We delegate the handling of a click event onto the square object itself. */
            Square squareObj = GetSquare(e.X, e.Y);
            if (squareObj == null) {
                // No such square.
                return;
            }
            if (e.Button == MouseButtons.Left) {
                // Left click will set the colour to SelectedColor
                squareObj.OnLeftMouseClick();
            }
            else if (e.Button == MouseButtons.Right) {
                // Right click will reset the square's colour back to the background colour of the grid
                squareObj.OnRightMouseClick();
            }
        }

        protected override void OnMouseLeave(EventArgs e) {
            /* Called when the mouse leaves the GridBox altogether.
             * We must reset the colour of the last active square
             * to its own BackColor.
             *
             * We don't need to handle a specific MouseEnter() event for the GridBox
             * because the MouseMove handler will already pick it up. */
            if (ActiveSquare != null) {
                ActiveSquare.OnMouseLeave();
                ActiveSquare = null;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            // We need to handle this event to detect a mouse drag
            Square squareObj = GetSquare(e.X, e.Y);
            if (squareObj == null) {
                // No such square;
                Console.WriteLine("MouseDown occurred on non existing square.");
                return;
            }
            if (e.Button == MouseButtons.Left) {
                // Left button is pressed.
                // We have to set this square's BackColor to the selected color,
                // otherwise it will be ignored, since its OnMouseEnter() event
                // will NOT fire, because it was already entered when this MouseDown
                // event took place.
                squareObj.BackColor = this.SelectedColor;
                LeftMouseDown = true;
            }
            else if (e.Button == MouseButtons.Right) {
                // Right button is pressed.
                // We have to reset this square's BackColor to DefaultBackgroundColor,
                // otherwise it will be ignored.
                squareObj.BackColor = this.DefaultBackgroundColor;
                RightMouseDown = true;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                // Left button is released.
                LeftMouseDown = false;
            }
            else if (e.Button == MouseButtons.Right) {
                // Right button is released.
                RightMouseDown = false;
            }
        }
#endregion
#region MenuEventHandlers
        internal void LoadMap(object sender, EventArgs e) {
            /* Loads a map file into the grid. */
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "PixelMaker files (*.pxl)|*.pxl";
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                // A file was successfully selected by the user.
                string fileName = openFileDialog.FileName;
            }
        }

        internal void SaveMap(object sender, EventArgs e) {
            /* Saves the grid into a map file. */
            // TODO: Use the build in SaveFileDialog();
            Console.WriteLine("SaveMap called.");
        }

        internal void ExportBitmap(object sender, EventArgs e) {
            /* Exports the map as a bitmap image */
            // TODO: Use the built in SaveFileDialog()
            Console.WriteLine("ExportBitmap called.");
        }
#endregion
	internal void ResizeGridBox() {
            /* Resizes the GridBox.
             * Its size is based on the parent window size,
             * the Main menu strip's height, and the parent window border size.
             * The amount of squares is based on the SquareSideLength property. */
            
            // Each ColorBox is 15 pixels in size (width and height).
            // There are three adjacent ColorBoxes, spaced 5 pixels apart from one another. 
            // Another 20 pixels serve as padding.
            int horizontalAxisPosition = (15*3)+(5*3)+20; // The top-left x axis position.
            int BorderInformation = ParentWindow.BorderInformation;

            // The window size might not be divisible by SquareSideLength.
            // We must perform a modulus operation on it to find the remainder,
            // and then subtract that remainder from the width.
            // The final result is the width we can safely set.
            int fullWidth = ParentWindow.Width - BorderInformation;
            int remainder = fullWidth % this.SquareSideLength;
            fullWidth -= remainder;
            fullWidth -= horizontalAxisPosition; // We need to subtract <x> from the width so the Grid size won't overflow beyond the window boundary
            // Now the height
            int fullHeight = ParentWindow.Height - (ParentWindow.MainMenuStrip.Height * 2) - BorderInformation;
            remainder = fullHeight % this.SquareSideLength;
            fullHeight -= remainder;
            
            // Structs are value types, so we have to create a new one,
            // store it in a temporary struct, and then assign the temporary struct to ClientSize. 
            Size tempSize = new Size(fullWidth, fullHeight);
            this.ClientSize = tempSize;
        }
    } // END OF GRIDBOX CLASS
} // END OF GUI NAMESPACE

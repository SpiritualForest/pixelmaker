using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO; // For BinaryReader and BinaryWriter classes (required for saving and loading)
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
                    // When a mouse move event is detected on the GridBox,
                    // it will call the square object's MouseEnter or MouseLeave event,
                    // depending on whether the square's area was just entered or left.
                    // The square sets its own MouseEvent property to the appropriate event type.
                    // These two events determine which colour we use to draw the square
                    // with which we're interacting.
                    SolidBrush paintBrush;
                    if (squareObj.MouseEvent == MouseEventType.Enter) {
                        // Enter event. The square should be repainted to the GridBox's SelectedColor colour.
                        paintBrush = new SolidBrush(this.SelectedColor);
                    }
                    else {
                        // Leave event. The square should be repainted to its own BackColor colour.
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
                int y = clipRectangle.Y / SquareSideLength, x = clipRectangle.X / SquareSideLength; // Index numbers to locate squares
                int originalX = x;
                for(int verticalSquares = -1; verticalSquares <= clipRectangle.Height / SquareSideLength; verticalSquares++) {
                    for(int horizontalSquares = -1; horizontalSquares <= clipRectangle.Width / SquareSideLength; horizontalSquares++) {
                        try {
                            Square squareObj = Squares[y][x];
                            SolidBrush paintBrush = new SolidBrush(squareObj.BackColor);
                            e.Graphics.FillRectangle(paintBrush, squareObj.AreaRectangle);
                        }
                        catch(ArgumentOutOfRangeException) {
                            Console.WriteLine("Tried to repaint non existent square as part of the area: {0}", clipRectangle);
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
                ParentWindow.CurrentWorkingFile = fileName;
                // TOCTTOU
                if (!File.Exists(fileName)) {
                    MessageBox.Show("No such file: {0}", fileName);
                    return;
                }
                
                // Now we can proceed with reading and loading the file
                try {
                    using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open))) {
                        if (reader.BaseStream.Length < 13) {
                            // Malformed or otherwise erroneus file. Can't even read header.
                            MessageBox.Show("Error. Malformed file. Cannot read header.");
                            return;
                        }
                        /* The first 13 bytes are the header, in the following order:
                        * The string "PXL",
                        * MainWindow width, MainWindow height,
                        * GridBox width, GridBox height,
                        * Square side length.
                        * Each one is an unsigned int16 type.
                        * We cast them to an int, because in the program we use int, rather than uint16.
                        * We only used uint16 to save space on the disk. */
                        string pxl = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(3));
                        if (pxl != "PXL") {
                            // This is not a valid PixelMaker file.
                            MessageBox.Show("Error. The file is not a valid PixelMaker map file.");
                            return;
                        }
                        // MainWindow size
                        int mainWindowWidth = (int)reader.ReadUInt16();
                        int mainWindowHeight = (int)reader.ReadUInt16();
                        // GridBox size
                        int gridBoxWidth = (int)reader.ReadUInt16();
                        int gridBoxHeight = (int)reader.ReadUInt16();
                        // Square side length
                        int squareSideLength = (int)reader.ReadUInt16();

                        // Now we make sure everything is fine before going any further.
                        // First we make sure the GridBox size we just read is divisible by the square side length we read.
                        if (gridBoxWidth % squareSideLength != 0 || gridBoxHeight % squareSideLength != 0) {
                            // Error. GridBox size is not divisible by the given square side length.
                            MessageBox.Show("Cannot open file: GridBox size does not match required amount of squares.");
                            return;
                        }
                        // Now we make sure that the total number of squares 
                        // matches the number of remaining bytes in the file.
                        // 4 total bytes per square.
                        int totalSquares = (gridBoxWidth / squareSideLength) * (gridBoxHeight / squareSideLength);
                        if (totalSquares != (reader.BaseStream.Length - reader.BaseStream.Position) / 4) {
                            // No match. Error.
                            // FIXME: Change these error message descriptions to something better
                            MessageBox.Show("Cannot open file: File size does not match required amount of squares.");
                            return;
                        }
                        // Checks passed. We can proceed.
                        // Set the window, gridbox, and square sizes
                        Size mainWindowSize = new Size(mainWindowWidth, mainWindowHeight);
                        ParentWindow.Size = mainWindowSize;
                        Size gridBoxSize = new Size(gridBoxWidth, gridBoxHeight);
                        this.Size = gridBoxSize;
                        SquareSideLength = squareSideLength;

                        // Now read the colours and form a list of squares.
                        List<List<Square>> squareObjects = new List<List<Square>>();
                        for(int y = 0; y < gridBoxHeight; y += squareSideLength) {
                            List<Square> sublist = new List<Square>();
                            for(int x = 0; x < gridBoxWidth; x += squareSideLength) {
                                Square squareObj = new Square(this, x, y);
                                // Read 4 bytes from the map file.
                                // These bytes represent ARGB values, from which we will then form the square's colour code.
                                byte A = reader.ReadByte();
                                byte R = reader.ReadByte();
                                byte G = reader.ReadByte();
                                byte B = reader.ReadByte();
                                squareObj.BackColor = Color.FromArgb(A, R, G, B);
                                sublist.Add(squareObj);
                            }
                            squareObjects.Add(sublist);
                        }
                        Squares = squareObjects;
                        // Redraw the grid.
                        Invalidate();
                    }
                }
                catch(Exception ex) when(ex is UnauthorizedAccessException || ex is EndOfStreamException) {
                    MessageBox.Show(string.Format("Could not open file: {0}", ex.Message));
                }
            }
        }

        internal void SaveMap(string fileName) {
            /* Saves the grid into a map file. */
            if (fileName == null) {
                // No filename given.
                // We must therefore prompt the user to select a file into which we should save the map.
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.Filter = "PixelMaker files (*.pxl)|*.pxl";
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    // A file was successfully selected.
                    fileName = saveFileDialog.FileName;
                }
                else {
                    // No file was selected. Abort.
                    return;
                }
            }
            // If execution reaches this stage, it means that a file was successfully selected.
            ParentWindow.CurrentWorkingFile = fileName;
            /* File structure.
             * Header (in order of writing):
             * 3 byte string, which forms the word "PXL",
             * Window width and height (pixels), GridBox width and height (pixels), square side length (pixels),
             * numbers of y axis squares (List indices), number of x axis squares (List indices).
             * All headers are UInt16 in order to save some space. In practice, we could probably go even lower, say 12 bits,
             * but C# does not natively provide this kind of data type (as far as I know).
             *
             * File contents:
             * A, R, G, and B colour values of each square. (byte data type. 1 byte per value, 4 bytes total per square)
             * We do not write the square's position. This will be calculated when loading the map file.
             */
            UInt16[] headers = new UInt16[] { 
                // MainWindow size
                (UInt16)ParentWindow.Width, (UInt16)ParentWindow.Height, 
                // GridBox size
                (UInt16)this.Width, (UInt16)this.Height, 
                // Square size
                (UInt16)this.SquareSideLength,
            };
            try {
                using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create))) {
                    // Write the headers first. A total of 13 bytes. (a 3 byte "PXL" string, 5 headers, 2 bytes each)
                    writer.Write(System.Text.Encoding.UTF8.GetBytes("PXL"));
                    foreach(UInt16 header in headers) {
                        writer.Write(header);
                    }
                    // Write the squares.
                    foreach(List<Square> sublist in Squares) {
                        foreach(Square squareObj in sublist) {
                            /* Each colour consists of A, R, G, and B values.
                            * Each of these values is a single byte (0 - 255), totalling 4 bytes per square. */
                            Color color = squareObj.BackColor;
                            byte[] argb = new byte[] { color.A, color.R, color.G, color.B };
                            foreach(byte byteValue in argb) {
                                writer.Write(byteValue);
                            }
                        }
                    }
                }
            }
            catch(UnauthorizedAccessException) {
                MessageBox.Show("Cannot save file. Permission denied.");
            }
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

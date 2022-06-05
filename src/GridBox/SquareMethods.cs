/* SquareMethods.cs.
 * This file contains the methods that handle square objects.
 * Drawing the squares, extending and shrinking the squares list, and so forth. */

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Gui; // For Square class

namespace Gui {
    partial class GridBox : Control {

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

        private void MovePaintedSquares(Keys direction) {
            // FIXME: Handle errors properly.
            /* This function "moves" all the squares one step in the given direction.
             * In reality, it doesn't actually move the squares themselves, but rather
             * paints the adjacent squares with the current squares' background colours.
             * The adjacent squares are of course based on the direction in which we're moving. */
            Dictionary<Square, Color> pendingSquares = new Dictionary<Square, Color>();
            foreach(KeyValuePair<Point, Square> pair in PaintedSquares) {
                Square squareObj = pair.Value;
                Point location = pair.Key;
                int xIndex = location.X / SquareSideLength;
                int yIndex = location.Y / SquareSideLength;
                /* We perform collision detection using exceptions.
                 * Since we only have to detect GridBox border collision,
                 * but not individual square collisions (these can't ever happen, this isn't Tetris),
                 * then we simply obtain the adjacent square object based on the direction of movement.
                 * If the next square doesn't exist, because it's out of bounds, then we get an exception
                 * and everything stops.
                 *
                 * Pros: no explicit conditional checks, simpler code, and far better performance in case of no collisions.
                 * Cons: a huge amount of potentially wasted operations in case an exception DOES occur
                 * after processing a large amount of squares already. */
                try {
                    Square adjacentSquare;
                    if (direction == Keys.Left) {
                        // x-1
                        adjacentSquare = Squares[yIndex][xIndex-1];
                    }
                    else if (direction == Keys.Right) {
                        // x+1
                        adjacentSquare = Squares[yIndex][xIndex+1];
                    }
                    else if (direction == Keys.Up) {
                        // y-1
                        adjacentSquare = Squares[yIndex-1][xIndex];
                    }
                    else {
                        // Down. y+1.
                        adjacentSquare = Squares[yIndex+1][xIndex];
                    }
                    // Add the adjacent square to the pendingSquares dictionary,
                    pendingSquares.Add(adjacentSquare, squareObj.BackColor);
                }
                catch(Exception ex) {
                    Console.WriteLine("Could not move squares: {0}", ex.Message);
                    return;
                }
            }
            // Now clear the grid.
            foreach(KeyValuePair<Point, Square> pair in PaintedSquares) {
                // We reset all the currently painted squares' BackColor
                // to the default background colour.
                Square squareObj = pair.Value;
                squareObj.BackColor = DefaultBackgroundColor;
            }
            PaintedSquares = new Dictionary<Point, Square>();
            // Now draw the new squares
            foreach(KeyValuePair<Square, Color> pair in pendingSquares) {
                Square pendingSquare = pair.Key;
                Color backColor = pair.Value;
                pendingSquare.BackColor = backColor;
                PaintedSquares.Add(pendingSquare.Location, pendingSquare);
            }
            // Call Invalidate() to redraw the entire grid
            Invalidate();
            // Set the GridBox's state to modified, because we changed it :)
            GridModified = true;
        }
    }
}

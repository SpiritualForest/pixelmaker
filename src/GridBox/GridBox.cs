/* GridBox.cs.
 * This is the main GridBox class definition file.
 * It contains all the fields and properties, as well as the constructor. */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Gui; // For Square class and MouseEvent enum

namespace Gui {

    partial class GridBox : Control {

        private Size _size;
        private int _squareSideLength = 10;

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
        
        internal bool GridModified { get; set; }

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

        internal GridBox() {
            // Set UserPaint to true, to indicate that we'll draw the control manually
            this.SetStyle(ControlStyles.UserPaint, true);
            this.Name = "gridbox";
            // Default background and selected colours are white
            this.DefaultBackgroundColor = Color.White;
            this.SelectedColor = Color.White;
            this.PaintedSquares = new Dictionary<Point, Square>();
        }

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

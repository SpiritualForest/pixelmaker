using System;
using System.Drawing; // For Rectangle, Point, and Color structures
using Gui; // For GridBox definition

namespace Gui {
/* Required for redrawing the grid.
     * 0. Mouse enters square area.
     * 1. Mouse leaves square area.
     * 2. Mouse dragged with left button down.
     * 3. Mouse dragged with right button down. */
    enum MouseEventType { Enter, Leave, LeftButtonDrag, RightButtonDrag };

    class Square {
        internal Rectangle AreaRectangle { get; set; } // The Rectangle to fill with BackColor (Graphics.FillRectangle() method)
        internal Color BackColor { get; set; }
        internal Point Location { get; set; } // Top-left x,y point of the square on the grid
        private GridBox Parent { get; set; }

        /* The events which cause the square's rectangle to be redrawn
         * are MouseEnter and MouseLeave, and RightMouseClick.
         * MouseEnter always uses the Parent's SelectedColor color,
         * whereas MouseLeave always uses the square's BackColor color.
         * Because calling Parent.Invalidate() from all three methods results in a call
         * to the GridBox's OnPaint method, we must signify which method invoked the call,
         * so that we can know which colour should be used to draw the square.
         * MouseEventType Enter (or 0) means enter, whereas Leave (or 1) means MouseLeave. */
        internal MouseEventType MouseEvent { get; set; }

        internal Square(GridBox parent, int x, int y) {
            this.Parent = parent;
            this.BackColor = parent.DefaultBackgroundColor;
            this.Location = new Point(x, y);
            this.AreaRectangle = new Rectangle(x+1, y+1, parent.SquareSideLength-1, parent.SquareSideLength-1);
        }

        internal void OnMouseEnter() {
            /* Called by the GridBox parent of the square
             * when the mouse cursor enters the square's area.
             * We set MouseEventType to 0, to indicate that
             * Parent.SelectedColor should be used to draw the square. */
            
            // Check for dragging
            if (MouseEvent == MouseEventType.LeftButtonDrag) {
                // Painting multiple squares
                BackColor = Parent.SelectedColor;
                if (!Parent.PaintedSquares.ContainsKey(this.Location)) {
                    // Add the square to the PaintedSquares dictionary
                    Parent.PaintedSquares.Add(this.Location, this);
                }
            }
            else if (MouseEvent == MouseEventType.RightButtonDrag) {
                // "Deleting" multiple squares
                BackColor = Parent.DefaultBackgroundColor;
                if (Parent.PaintedSquares.ContainsKey(this.Location)) {
                    // Remove the square from the PaintedSquares dictionary
                    Parent.PaintedSquares.Remove(this.Location);
                }
            }
            
            // Now we indicate that the mouse event is Enter, and update our square.
            MouseEvent = MouseEventType.Enter;
            Parent.Invalidate(this.AreaRectangle);
        }

        internal void OnMouseLeave() {
            /* Called by the GridBox parent of the square
             * when the mouse cursor leaves the square's area.
             * We set MouseEventType to 1, to indicate that
             * Square.BackColor should be used to draw the square. */
            MouseEvent = MouseEventType.Leave;
            Parent.Invalidate(this.AreaRectangle);
            Parent.Update();
        }

        internal void OnLeftMouseClick() {
            /* Called by the GridBox parent of the square
             * when the left mouse button is clicked while in the square's area.
             * We change the square's BackColor property to Parent.SelectedColor.
             * We also add the square to the PaintedSquares dictionary. */
            BackColor = Parent.SelectedColor;
            if (!Parent.PaintedSquares.ContainsKey(this.Location)) {
                Parent.PaintedSquares.Add(this.Location, this);
            }
        }

        internal void OnRightMouseClick() {
            /* Called by the GridBox parent of the square
             * when the right mouse button is clicked while in the square's area.
             * Resets the square's BackColor property to the
             * parent's default background color.
             * Basically, "deletes" the color from the square. */
            BackColor = Parent.DefaultBackgroundColor;
            if (Parent.PaintedSquares.ContainsKey(this.Location)) {
                Parent.PaintedSquares.Remove(this.Location);
            }
            MouseEvent = MouseEventType.Leave; // To signal that we should use Square.BackColor.
            Parent.Invalidate(this.AreaRectangle);
        }
    } // END OF SQUARE CLASS
} // END OF GUI NAMESPACE

/* OnPaint.cs.
 * This file only contains the OnPaint event handler,
 * which is in charge of redrawing the GridBox, or areas of it, when necessary. */
using System;
using System.Windows.Forms;
using System.Drawing;

namespace Gui {
    partial class GridBox : Control {
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
                            x++;
                        }
                        catch(ArgumentOutOfRangeException) {
                            Console.WriteLine("Tried to repaint non existent square as part of the area: {0}", clipRectangle);
                        }
                    }
                    // After each vertical (y axis) completion, x must be set to its original value
                    x = originalX;
                    y++;
                }
            } 
        }
#endregion
    }
}

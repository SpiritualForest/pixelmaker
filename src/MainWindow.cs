using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using Gui;

/* NOTE: use Invalidate(new Region(Square.AreaRectangle)) when updating individual squares.
 * https://msdn.microsoft.com/en-us/library/wtzka3b5(v=vs.110).aspx
 * https://msdn.microsoft.com/en-us/library/system.drawing.region(v=vs.110).aspx */

namespace Gui {

    class MainWindow : Form {
        
        // Border size in pixels
        private int BorderInformation = SystemInformation.Border3DSize.Width * (int)Math.Pow(SystemInformation.Border3DSize.Width, 2);

        internal MainWindow() {
            /* TODO: Dynamic sizing of the window. */
            Size = new Size(1000 + BorderInformation, 600);
            CenterToScreen();
            this.CreateGridBox();
            ResizeEnd += new EventHandler(HandleResizeEnd);
        }
        private void CreateGridBox(int sideLength = 10) {
            /* Creates a new GridBox control.
             * Its size is based on the window and window border size.
             * TODO: the Grid Box's height should be based on MainWindow's Height property. */
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

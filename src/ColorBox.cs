using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Gui; // For MainWindow class

namespace Gui {
    class ColorBox : Control {
        
        MainWindow ParentWindow { get; set; }
     
        public ColorBox(MainWindow parentWindow) {
            this.SetStyle(ControlStyles.UserPaint, true);
            ParentWindow = parentWindow;
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics graphicsObj = e.Graphics;
            /* First we fill the entire ColorBox's rectangle with its background colour.
             * Then we draw the 3D border using two grey lines. */
            // Rectangle at Location 0,0, fill Width,Height
            Rectangle areaRectangle = new Rectangle(0, 0, Width, Height);
            SolidBrush paintBrush = new SolidBrush(BackColor);
            graphicsObj.FillRectangle(paintBrush, areaRectangle);
            
            // Now draw the grey border
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
}

/* A color selection window, very similar to the built in ColorDialog.
 * I didn't use ColorDialog because:
 * 1) it would block.
 * 2) it requires the user to press "OK" and close the dialog, 
 * which means that the user would always have to reopen the ColorDialog
 * whenever they want to change the colour. This is inconvenient. */

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

// TODO: This should be completely separate and modular.
// The whole implementation should be applicable to any future program, not just this one.

namespace Gui {

    class ColorBox : Control {
        
        ColorWindow ParentWindow { get; set; }
        
        public ColorBox(ColorWindow parentWindow) {
            this.SetStyle(ControlStyles.UserPaint, true);
            ParentWindow = parentWindow;
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics graphicsObj = e.Graphics;
            /* Two dark grey lines should be painted at
             * x: 0 - this.Width,
             * y: 0 - this.Height. */
            // Rectangle at Location 0,0, fill Width,Height
            Rectangle areaRectangle = new Rectangle(0, 0, Width, Height);
            SolidBrush paintBrush = new SolidBrush(BackColor);
            graphicsObj.FillRectangle(paintBrush, areaRectangle);
            // Now draw the grey lines
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
            // Now we have to dispatch a new ColorSelect event.
            // That particular event is defined in the ColorWindow parent form.
            ParentWindow.DispatchColorSelectEvent(BackColor);
        }
    }

    class ColorWindow : Form {

        // List of predefined colours from which the user can choose.
        // It contains all the predefined colours that are part of ColorDialog.
        // This array will never change.
        private string[] _predefinedColors = new string[] {
            // 8 colours per line
            "#ffff8080", "#ffffff80", "#ff80ff80", "#ff00ff80", "#ff80ffff", "#ff0080ff", "#ffff80c0", "#ffff80ff",
            "#FFFF0000", "#FFFFFF00", "#ff80ff00", "#ff00ff40", "#FF00FFFF", "#ff0080c0", "#ff8080c0", "#FFFF00FF",
            "#ff804040", "#ffff8040", "#FF00FF00", "#FF008080", "#ff004080", "#ff8080ff", "#ff800040", "#ffff0080", 
            "#FF800000", "#ffff8000", "#FF008000", "#ff008040", "#FF0000FF", "#ff0000a0", "#FF800080", "#ff8000ff", 
            "#ff400000", "#ff804000", "#ff004000", "#ff004040", "#FF000080", "#ff000040", "#ff400040", "#ff400080", 
            "#FF000000", "#FF808000", "#ff808040", "#FF808080", "#ff408080", "#FFC0C0C0", "#ff400040", "#FFFFFFFF",
        };

        public string[] PredefinedColors {
            get { return _predefinedColors; }
        }

        private List<string> CustomColors { get; set; } // User-defined colours
        
        public event EventHandler<ColorSelectEventArgs> ColorSelect; 
       
        public ColorWindow() {
            // Size is always the same.
            Size = new Size((8*10) + (8*15) + 20, (6*10) + (6*15) + 40);
            Text = "Color Window";
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            
            // Now create the color boxes
            int x = 10, y = 10;
            for(int i = 1; i <= PredefinedColors.Length; i++) {
                // i is initially 1 because 0 % N is always 0, and that fucks up the logic later on.
                // Create a new ColorBox with this window as its parent window
                ColorBox colorBox = new ColorBox(this);

                // Set color, size, and location within the parent window
                colorBox.BackColor = ColorTranslator.FromHtml(PredefinedColors[i-1]);
                colorBox.Size = new Size(15, 15);
                colorBox.Location = new Point(x, y);
                x += colorBox.Width + 10;
                if (i % 8 == 0) {
                    // Because we want the window to contain 8 color boxes on one row,
                    // when we reach 8 (or any multiple of 8),
                    // we reset x to the starting position (10), and increment y by the colorBox's height
                    // and an additional 10 pixels.
                    x = 10;
                    y += colorBox.Height + 10;
                }
                this.Controls.Add(colorBox);
                colorBox.Invalidate();
            }
        }
#region ColorSelectEvent handlers
        internal void DispatchColorSelectEvent(Color color) {
            ColorSelectEventArgs args = new ColorSelectEventArgs();
            args.Color = color;
            OnColorSelect(args);
        }

        protected virtual void OnColorSelect(ColorSelectEventArgs e) {
            // God damn... sometimes I hate C#.
            // Although most of the time I love it.
            EventHandler<ColorSelectEventArgs> handler = ColorSelect;
            if (handler != null) {
                handler(this, e);
            }
        }
#endregion
    }

    public class ColorSelectEventArgs : EventArgs {
        public Color Color { get; set; }
    }
}

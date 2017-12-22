/* KeyboardEvents.cs
 * Handles all the keyboard interaction with the GridBox */

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace Gui {
    partial class GridBox : Control {
        
        private bool ShiftKeyDown;
        private bool ControlKeyDown; 
        
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            /* By default, keys such as the arrow keys are not considered
             * to be input keys. Therefore we have to handle this event
             * and set IsInputKey to true, to indicate that we want to catch arrow keys as well. */
            e.IsInputKey = true;
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            /* We only handle the arrow keys in this function for now.
             * Other input keys will be handled by the OnKeyPress() method. */
            switch(e.KeyCode) {
                // Arrow keys first
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    MovePaintedSquares(e.KeyCode);
                    break;
                // Modifier keys (Control, Shift, etc) here.
                case Keys.ControlKey:
                    ControlKeyDown = true;
                    break;
                case Keys.ShiftKey:
                    ShiftKeyDown = true;
                    break;
                default:
                    Console.WriteLine(e.KeyCode);
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            Console.WriteLine("Key up: {0}", e.KeyCode);
        }

        protected override void OnKeyPress(KeyPressEventArgs e) {
            Console.WriteLine(e.KeyChar);
        }
    }
}

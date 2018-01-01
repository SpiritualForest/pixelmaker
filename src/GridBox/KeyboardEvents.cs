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
            /* This function handles the arrow and modifier keys. */
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
            // First modifier keys.
            switch(e.KeyCode) {
                case Keys.ControlKey:
                    ControlKeyDown = false;
                    break;
                case Keys.ShiftKey:
                    ShiftKeyDown = false;
                    break;
            }

            // Now we handle the relevant letter keys that are paired with the modifiers.
            if (ControlKeyDown) {
                // Control+Letter shortcuts
                switch(e.KeyCode) {
                    case Keys.S:
                        // Quick save
                        SaveMap(ParentWindow.CurrentWorkingFile);
                        break;
                    case Keys.L:
                        // Quick load
                        LoadMap();
                        break;
                    case Keys.Q:
                        // Exit application.
                        ParentWindow.ExitApplication();
                        break;
                    case Keys.E:
                        // Export bitmap
                        ExportBitmap();
                        break;
                    case Keys.Z:
                        // Undo previous actions
                        // TODO.
                        Console.WriteLine("Ctrl-Z");
                        break;
                }
            }
        }
    }
}

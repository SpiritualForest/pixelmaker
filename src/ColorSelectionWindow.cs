/* A color selection window, very similar to the built in ColorDialog.
 * I didn't use ColorDialog because:
 * 1) it would block.
 * 2) it requires the user to press "OK" and close the dialog, 
 * which means that the user would always have to reopen the ColorDialog
 * whenever they want to change the colour. This is inconvenient. */

using System;
using System.Drawing;
using System.Windows.Forms;
using Gui;

namespace Gui {
    class ColorSelectionWindow : Form {

        // List of predefined colours from which the user can choose.
        // It contains all the predefined colours that are part of ColorDialog.
        private List<string> colorNames = new List<string>() {
            // 8 colours per line
            "#ffff8080", "#ffffff80", "#ff80ff80", "#ff00ff80", "#ff80ffff", "#ff0080ff", "#ffff80c0", "#ffff80ff",
            "#FFFF0000", "#FFFFFF00", "#ff80ff00", "#ff00ff40", "#FF00FFFF", "#ff0080c0", "#ff8080c0", "#FFFF00FF",
            "#ff804040", "#ffff8040", "#FF00FF00", "#FF008080", "#ff004080", "#ff8080ff", "#ff800040", "#ffff0080", 
            "#FF800000", "ffff8000", "#FF008000", "#ff008040", "#FF0000FF", "#ff0000a0", "#FF800080", "#ff8000ff", 
            "#ff400000", "#ff804000", "#ff004000", "#ff004040", "#FF000080", "#ff000040", "#ff400040", "#ff400080", 
            "#FF000000", "#FF808000", "#ff808040", "#FF808080", "#ff408080", "#FFC0C0C0", "#ff400040", "#FFFFFFFF",
        };
    }
}

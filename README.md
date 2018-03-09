## DEVELOPMENT HALTED.

### After the application repeatedly became unresponsive on Windows when built with the Microsoft C# compiler, and
### since Windows Forms are essentially considered dead by now, and Mono will not support WPF, I decided to halt development.

#### Compilation instructions:
##### mcs \*.cs GridBox/\*.cs -r:System.Windows.Forms.dll -r:System.Drawing.dll /out:pixelmaker.exe,
##### or, for Unix-like OS users, simply run the Makefile with "make pixelmaker".

##### Run the resulting pixelmaker.exe file.

##### Left mouse button paints a square.
##### Right mouse button "deletes" a square (resets its colour to the background colour).
##### Arrow buttons move the whole painting around (slow, very buggy, beware).

### You can view HelloWorldScreenshot.png and HelloWorldExported.bmp to see the functionality of the program.

#### PixelMaker is a program that allows the user to draw images made out of squares ("big pixels") and export those drawings into a bitmap image.

#### Compilation instructions:
##### mcs \*.cs GridBox/\*.cs -r:System.Windows.Forms.dll -r:System.Drawing.dll /out:pixelmaker.exe,
##### or, for Unix-like OS users, simply run the Makefile with "make pixelmaker".

##### Run the resulting pixelmaker.exe file.

##### Left mouse button paints a square.
##### Right mouse button "deletes" a square (resets its colour to the background colour).
##### Arrow buttons move the whole painting around (slow, very buggy, beware).

![](https://github.com/SpiritualForest/pixelmaker/blob/master/HelloWorldScreenshot.png)

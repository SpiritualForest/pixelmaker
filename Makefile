# PixelMaker Makefile, used to simplify building the application.

pixelmaker:
	mcs src/*.cs src/GridBox/*.cs -r:System.Windows.Forms.dll -r:System.Drawing.dll /out:pixelmaker.exe

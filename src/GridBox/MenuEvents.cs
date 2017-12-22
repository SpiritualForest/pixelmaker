/* MenuEvents.cs.
 * This file contains the methods that respond to menu events from the MainWindow.
 * It implements the saving and loading of GridBox maps,
 * and the exporting of the GridBox contents into a bitmap image. */

using System;
using System.IO; // For BinaryReader and BinaryWriter
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace Gui {
    partial class GridBox : Control {
        
        // We create new instances of these here
        // so that we can keep the directories chosen by the user.
        private SaveFileDialog saveFileDialog = new SaveFileDialog();
        private OpenFileDialog openFileDialog = new OpenFileDialog();

        internal void LoadMap(object sender, EventArgs e) {
            /* Loads a map file into the grid. */
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.RestoreDirectory = false;
            openFileDialog.Filter = "PixelMaker files (*.pxl)|*.pxl";
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                // A file was successfully selected by the user.
                string fileName = openFileDialog.FileName;
                ParentWindow.CurrentWorkingFile = fileName;
                
                // Now we can proceed with reading and loading the file
                try {
                    using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open))) {
                        if (reader.BaseStream.Length < 13) {
                            // Malformed or otherwise erroneus file. Can't even read header.
                            MessageBox.Show("Error. Malformed file. Cannot read header.");
                            return;
                        }
                        /* The first 13 bytes are the header, in the following order:
                        * The string "PXL",
                        * MainWindow width, MainWindow height,
                        * GridBox width, GridBox height,
                        * Square side length.
                        * Each one is an unsigned int16 type.
                        * We cast them to an int, because in the program we use int, rather than uint16.
                        * We only used uint16 to save space on the disk. */
                        string pxl = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(3));
                        if (pxl != "PXL") {
                            // This is not a valid PixelMaker file.
                            MessageBox.Show("Error. The file is not a valid PixelMaker map file.");
                            return;
                        }
                        // MainWindow size
                        int mainWindowWidth = (int)reader.ReadUInt16();
                        int mainWindowHeight = (int)reader.ReadUInt16();
                        // GridBox size
                        int gridBoxWidth = (int)reader.ReadUInt16();
                        int gridBoxHeight = (int)reader.ReadUInt16();
                        // Square side length
                        int squareSideLength = (int)reader.ReadUInt16();

                        // Now we make sure everything is fine before going any further.
                        // First we make sure the GridBox size we just read is divisible by the square side length we read.
                        if (gridBoxWidth % squareSideLength != 0 || gridBoxHeight % squareSideLength != 0) {
                            // Error. GridBox size is not divisible by the given square side length.
                            MessageBox.Show("Cannot open file: GridBox size does not match required amount of squares.");
                            return;
                        }
                        // Now we make sure that the total number of squares 
                        // matches the number of remaining bytes in the file.
                        // 4 total bytes per square.
                        int totalSquares = (gridBoxWidth / squareSideLength) * (gridBoxHeight / squareSideLength);
                        if (totalSquares != (reader.BaseStream.Length - reader.BaseStream.Position) / 4) {
                            // No match. Error.
                            // FIXME: Change these error message descriptions to something better
                            MessageBox.Show("Cannot open file: File size does not match required amount of squares.");
                            return;
                        }
                        // Checks passed. We can proceed.
                        // Set the window, gridbox, and square sizes
                        Size mainWindowSize = new Size(mainWindowWidth, mainWindowHeight);
                        ParentWindow.Size = mainWindowSize;
                        Size gridBoxSize = new Size(gridBoxWidth, gridBoxHeight);
                        this.Size = gridBoxSize;
                        SquareSideLength = squareSideLength;

                        // Now read the colours and form a list of squares.
                        /* FIXME: Must add squares to PaintedSquares! */
                        List<List<Square>> squareObjects = new List<List<Square>>();
                        for(int y = 0; y < gridBoxHeight; y += squareSideLength) {
                            List<Square> sublist = new List<Square>();
                            for(int x = 0; x < gridBoxWidth; x += squareSideLength) {
                                Square squareObj = new Square(this, x, y);
                                // Read 4 bytes from the map file.
                                // These bytes represent ARGB values, from which we will then form the square's colour code.
                                byte A = reader.ReadByte();
                                byte R = reader.ReadByte();
                                byte G = reader.ReadByte();
                                byte B = reader.ReadByte();
                                squareObj.BackColor = Color.FromArgb(A, R, G, B);
                                sublist.Add(squareObj);
                            }
                            squareObjects.Add(sublist);
                        }
                        Squares = squareObjects;
                        // Redraw the grid.
                        Invalidate();
                        
                        // Set the currently open (working) file.
                        ParentWindow.CurrentWorkingFile = fileName;
                        Console.WriteLine("Loaded map file: {0}", fileName);
                    }
                }
                catch(Exception ex) {
                    MessageBox.Show(string.Format("Could not open file: {0}", ex.Message));
                }
            }
        }

        internal void SaveMap(string fileName) {
            /* Saves the grid into a map file. */
            if (fileName == null) {
                // No filename given.
                // We must therefore prompt the user to select a file into which we should save the map.
                saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
                saveFileDialog.RestoreDirectory = false;
                saveFileDialog.Filter = "PixelMaker files (*.pxl)|*.pxl";
                saveFileDialog.FileName = "Untitled";
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    // A file was successfully selected.
                    fileName = saveFileDialog.FileName;
                }
                else {
                    // No file was selected. Abort.
                    return;
                }
            }
            // If execution reaches this stage, it means that a file was successfully selected.
            /* File structure.
             * Header (in order of writing):
             * 3 byte string, which forms the word "PXL",
             * Window width and height (pixels), GridBox width and height (pixels), square side length (pixels),
             * numbers of y axis squares (List indices), number of x axis squares (List indices).
             * All headers are UInt16 in order to save some space. In practice, we could probably go even lower, say 12 bits,
             * but C# does not natively provide this kind of data type (as far as I know).
             *
             * File contents:
             * A, R, G, and B colour values of each square. (byte data type. 1 byte per value, 4 bytes total per square)
             * We do not write the square's position. This will be calculated when loading the map file.
             */
            UInt16[] headers = new UInt16[] { 
                // MainWindow size
                (UInt16)ParentWindow.Width, (UInt16)ParentWindow.Height, 
                // GridBox size
                (UInt16)this.Width, (UInt16)this.Height, 
                // Square size
                (UInt16)this.SquareSideLength,
            };
            try {
                using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create))) {
                    // Write the headers first. A total of 13 bytes. (a 3 byte "PXL" string, 5 headers, 2 bytes each)
                    writer.Write(System.Text.Encoding.UTF8.GetBytes("PXL"));
                    foreach(UInt16 header in headers) {
                        writer.Write(header);
                    }
                    // Write the squares.
                    foreach(List<Square> sublist in Squares) {
                        foreach(Square squareObj in sublist) {
                            /* Each colour consists of A, R, G, and B values.
                            * Each of these values is a single byte (0 - 255), totalling 4 bytes per square. */
                            Color color = squareObj.BackColor;
                            byte[] argb = new byte[] { color.A, color.R, color.G, color.B };
                            writer.Write(argb);
                        }
                    }
                }
                Console.WriteLine("Saved map file: {0}", fileName);
                ParentWindow.CurrentWorkingFile = fileName;
            }
            catch(Exception ex) {
                MessageBox.Show(string.Format("Cannot save file: {0}", ex.Message));
            }
        }

        internal void ExportBitmap(object sender, EventArgs e) {
            /* Exports the map as a bitmap image */
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bitmap images (*.bmp)|*.bmp";
            saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = "Untitled";
            if (saveFileDialog.ShowDialog() != DialogResult.OK) {
                // A file was not successfully selected.
                return;
            }
            // Now we can start writing the file
            string fileName = saveFileDialog.FileName;
            
            // First we create all the pixel data.
            // TODO: Dialog box with progress bar that shows the file creating progress.
            
            Console.WriteLine("Preparing pixel data...");

            // List of lists of bytes. Each byte represents one part of a colour's ARGB value.
            List<List<byte>> pixelData = new List<List<byte>>();
            for(int y = Squares.Count - 1; y >= 0; y--) {
                List<Square> sublist = Squares[y];
                /* Now we obtain the ARGB colour values of every square on this sublist. */
                List<byte> ARGB = new List<byte>();
                foreach(Square squareObj in sublist) {
                    Color color = squareObj.BackColor;
                    for(int x = 0; x < SquareSideLength; x++) {
                        /* Because each square's width is <SquareSideLength>,
                         * and each pixel in the bitmap file is represented by the square's ARGB colour value,
                         * We have to add those values <SquareSideLength> times for each square,
                         * to account for every individual pixel of it. */
                        // The colour bytes are actually ordered in reverse in bitmap images.
                        ARGB.AddRange(new byte[]{ color.B, color.G, color.R, color.A });
                    }
                }
                /* Now we have to add these colours values to the pixelData list.
                 * Since the square's height in pixels is <SquareSideLength>,
                 * we have to add the row's data that amount of times.
                 * Each ARGB row actually represents the data of ALL squares in the Squares[y] sublist. */
                for(int i = 0; i < SquareSideLength; i++) {
                    pixelData.Add(ARGB);
                }
            }

            // Pixel data creation complete. Now we actually write it to the file.
            try {
                using(BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create))) {
                    /* Write BMP headers */
                    // File header, first 14 bytes.
                    writer.Write(System.Text.Encoding.UTF8.GetBytes("BM")); // "BM"
                    // 54 byte header
                    int totalBytes = 54 + (pixelData.Count * pixelData[0].Count);
                    writer.Write(totalBytes); // Bitmap size in bytes
                    writer.Write(0); // Reserved space. 4 byte integer, must be 0.
                    writer.Write(54); // Offset to actual pixel data

                    // Image data header, must be at least 40 bytes.
                    writer.Write(40); // Size of data header. Must be at least 40.
                    writer.Write(pixelData[0].Count / 4); // Width of bitmap in pixels
                    writer.Write(pixelData.Count); // Height of bitmap in pixels.
                    writer.Write((UInt16)1); // Number of colour planes
                    writer.Write((UInt16)32); // Number of bits per pixel. Sets colour mode. 32 bits (ARGB).
                    writer.Write(0); // Set compression to none.
                    writer.Write(0); // Pixel data size set to 0, because it's uncompressed.
                    writer.Write(0); // Horizontal resolution per meter. 0 indicates no preference.
                    writer.Write(0); // Vertical resolution per meter. 0 indicates no preference.
                    writer.Write(0); // Number of colours used. Very uncommon to encounter non-zero value here.
                    writer.Write(0); // Number of important colours. 0 indicates that all colours are important.

                    // Now write the actual pixel data.
                    foreach(List<byte> bytes in pixelData) {
                        writer.Write(bytes.ToArray());
                    }
                }
                MessageBox.Show("File successfully exported.");
            }
            catch(Exception ex) {
                Console.WriteLine("Error writing bitmap file: {0}", ex.Message);
                MessageBox.Show(string.Format("Could not export file: {0}", ex.Message));
            }
        }
    }
}

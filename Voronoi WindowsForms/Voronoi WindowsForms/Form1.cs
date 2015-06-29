using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Voronoi_WindowsForms
{
    public partial class Form1 : Form
    {
        private int numberOfPoints;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Int32.TryParse(input.Text, out numberOfPoints) == false) return;
            Bitmap bitmap = new Bitmap(pictureBox1.Image);
            Bitmap forRefPoints = new Bitmap(pictureBox1.Image);

            int width = bitmap.Width;
            int height = bitmap.Height;
            
            //Cache pixel data to byte array to increase access spead
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int BytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int ByteCount = bitmapData.Stride * bitmap.Height;
            byte[] Pixels = new byte[ByteCount];
            IntPtr PtrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(PtrFirstPixel, Pixels, 0, Pixels.Length);
            int HeightInPixels = bitmapData.Height;
            int WidthInBytes = bitmapData.Width * BytesPerPixel;

            //Array containing all the reference points
            List<mVector> allRefPoints = findRefPoints(forRefPoints);
            List<mVector> refPoints = new List<mVector>(); //list containing all vectors which will be used as seeds
            Random Rnd = new Random();

            for (int r = 0; r < numberOfPoints; r++)
            {

                int x = Rnd.Next(width);

                int y = Rnd.Next(height);
                Color color = new Color();
                color = Color.FromArgb(255, Pixels[y*bitmapData.Stride + x*4 + 2],
                    Pixels[y*bitmapData.Stride + x*4 + 1],
                    Pixels[y*bitmapData.Stride + x*4]);
                refPoints.Add(new mVector(x, y, color));


                int index = Rnd.Next(allRefPoints.Count);
                if (allRefPoints.Count == 0) break;
                if (allRefPoints[index] != null && r < 500)
                {
                    if (allRefPoints[index].x != 0)
                    {
                        int xIndex = allRefPoints[index].x;
                        int yIndex = allRefPoints[index].y;
                        Color colorIndex = new Color();
                        colorIndex = Color.FromArgb(255, Pixels[yIndex*bitmapData.Stride + xIndex*4 + 2],
                            Pixels[yIndex*bitmapData.Stride + xIndex*4 + 1],
                            Pixels[yIndex*bitmapData.Stride + xIndex*4]);
                        refPoints.Add(new mVector(xIndex, yIndex, colorIndex));
                        allRefPoints.RemoveAt(index);
                    }
                }
            }
            //BEGIN OF ALGORITHM

            Parallel.For(0, HeightInPixels, y =>
            {
                // Currentline in Pixel 
                // Conversion from pixel to array coordinates 
                // array (x,y) = y*bitmap.Stride + x*4 + (0/1/2) (blue, green, red)
                int CurrentLine = y*bitmapData.Stride;
                int interval = 8;

                for (int x = 0; x < WidthInBytes; x = x + BytesPerPixel)
                {
                    //determines closest refpoint to (x, y) specified by index
                    mVector closestRefPoint = new mVector(width*2, height*2, Color.Blue);
                    int distToCurrentPoint,
                        distToRefPoint = ((x/4 - closestRefPoint.x)*(x/4 - closestRefPoint.x) +
                                          (y - closestRefPoint.y)*(y - closestRefPoint.y));

                    /*
                    //Filter list of possible nearest points
                    List<mVector> filteredList = new List<mVector>();
                    do
                    {
                        filteredList = refPoints;
                        filteredList = filteredList.Where(t => t.x < x/4 + width / interval).ToList();
                        filteredList = filteredList.Where(t => t.x > x/4 - width / interval).ToList();
                        filteredList = filteredList.Where(t => t.y < y + height / interval).ToList();
                        filteredList = filteredList.Where(t => t.y > y - height / interval).ToList();
                        interval--;
                    } while (filteredList.Count == 0 && interval > 0);
                    interval = 8;
                     * */

                    foreach (mVector t in refPoints)
                    {
                        distToCurrentPoint = ((x/4 - t.x)*(x/4 - t.x) + (y - t.y)*(y - t.y));
                        if (
                            distToCurrentPoint < distToRefPoint
                            )
                        {
                            closestRefPoint = t;
                            distToRefPoint = distToCurrentPoint;
                        }
                    }
                    Pixels[CurrentLine + x] = closestRefPoint.color.B;
                    Pixels[CurrentLine + x + 1] = closestRefPoint.color.G;
                    Pixels[CurrentLine + x + 2] = closestRefPoint.color.R;
                }
            });
            
            // Copy modified bytes back:
            Marshal.Copy(Pixels, 0, PtrFirstPixel, Pixels.Length);
            bitmap.UnlockBits(bitmapData);
            pictureBox1.Image = bitmap;
        }

        
        public List<mVector> findRefPoints(Bitmap b)
        {
            Bitmap newBP = new Bitmap(b);
            EdgeDetectHomogenity(newBP, 10);
            BitmapData bitmapData = newBP.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, newBP.PixelFormat);
            int BytesPerPixel = Bitmap.GetPixelFormatSize(newBP.PixelFormat) / 8;
            int ByteCount = bitmapData.Stride * newBP.Height;
            byte[] Pixels = new byte[ByteCount];
            IntPtr PtrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(PtrFirstPixel, Pixels, 0, Pixels.Length);
            int HeightInPixels = bitmapData.Height;
            int WidthInBytes = bitmapData.Width * BytesPerPixel;

            //Output List
            List<mVector> refList = new List<mVector>();

            //Calculate Luminance
            for (int y = 0; y < HeightInPixels; y++)
            {
                // Currentline in Pixel 
                // Conversion from pixel to array coordinates 
                // array (x,y) = y*bitmap.Stride + x*4 + (0/1/2) (blue, green, red)
                int currentLine = y * bitmapData.Stride;
                int delay = 0;
                for (int x = 0; x < WidthInBytes; x = x + BytesPerPixel)
                {
                    double luminance = 0.2126 * Pixels[currentLine + x + 2] + 
                        0.7152 * Pixels[currentLine + x + 1] + 0.0722 * Pixels[currentLine + x];
                    if (luminance > 120)
                    {
                        if (delay > 10) 
                        {
                            refList.Add(new mVector(x/4, y, Color.Blue));
                            delay = 0;
                        }
                    }
                    delay++;
                }
            }

            Marshal.Copy(Pixels, 0, PtrFirstPixel, Pixels.Length);
            newBP.UnlockBits(bitmapData);

            return refList;
        }

        public bool EdgeDetectHomogenity(Bitmap b, byte nThreshold)
        {
            // This one works by working out the greatest difference between a 
            // pixel and it's eight neighbours. The threshold allows softer edges to 
            // be forced down to black, use 0 to negate it's effect.
            Bitmap b2 = (Bitmap)b.Clone();

            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                           ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmData2 = b2.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                             ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr Scan02 = bmData2.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                byte* p2 = (byte*)(void*)Scan02;

                int nOffset = stride - b.Width * 3;
                int nWidth = b.Width * 3;

                int nPixel = 0, nPixelMax = 0;

                p += stride;
                p2 += stride;

                for(int y = 1; y < b.Height - 1; y++)
                {
                    p += 3;
                    p2 += 3;

                    for (int x = 3; x < nWidth - 3; ++x)
                    {
                        nPixelMax = Math.Abs(p2[0] - (p2 + stride - 3)[0]);
                        nPixel = Math.Abs(p2[0] - (p2 + stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 + stride + 3)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 + stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride - 3)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        nPixel = Math.Abs(p2[0] - (p2 - stride + 3)[0]);
                        if (nPixel > nPixelMax) nPixelMax = nPixel;

                        if (nPixelMax < nThreshold) nPixelMax = 0;

                        p[0] = (byte) nPixelMax;

                        ++p;
                        ++p2;
                    }

                    p += 3 + nOffset;
                    p2 += 3 + nOffset;
                }
            }
            b.UnlockBits(bmData);
            b2.UnlockBits(bmData2);

            return true;
        }
        
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Images|*.png;*.bmp;*.jpg";
            ImageFormat format = ImageFormat.Png;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(sfd.FileName);
                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                }
                pictureBox1.Image.Save(sfd.FileName, format);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Load(ofd.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            }
            /*
            Bitmap bitmap = new Bitmap(pictureBox1.Image);
            findRefPoints(bitmap);
            EdgeDetectHomogenity(bitmap, 10);
            pictureBox1.Image = bitmap;
             * */
        }
    }
    public class mVector
    {
        public int x;
        public int y;
        public Color color;

        public mVector(int x, int y, Color color)
        {
            this.x = x;
            this.y = y;
            this.color = color;
        }
    }
}

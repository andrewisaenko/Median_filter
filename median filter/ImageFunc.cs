using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace median_filter
{
    public static class ImageFunc
    {
        public static Bitmap CombineImage(Bitmap[] files)
        {
            //read all images into memory
            List<Bitmap> images = new List<Bitmap>();
            Bitmap finalImage = null;

            try
            {
                int width = 0;
                int height = 0;

                foreach (Bitmap image in files)
                {
                    //create a Bitmap from the file and add it to the list
                    Bitmap bitmap = new Bitmap(image);

                    //update the size of the final bitmap
                    width += bitmap.Width;
                    height = bitmap.Height > height ? bitmap.Height : height;

                    images.Add(bitmap);
                }

                //create a bitmap to hold the combined image
                finalImage = new Bitmap(width, height);

                //get a graphics object from the image so we can draw on it
                using (Graphics g = Graphics.FromImage(finalImage))
                {
                    //set background color
                    g.Clear(Color.Black);

                    //go through each image and draw it on the final image
                    int offset = 0;
                    foreach (Bitmap image in images)
                    {
                        g.DrawImage(image,
                          new Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                }

                return finalImage;
            }
            catch (Exception)
            {
                if (finalImage != null)
                    finalImage.Dispose();
                //throw ex;
                throw;
            }
            finally
            {
                //clean up memory
                foreach (Bitmap image in images)
                {
                    image.Dispose();
                }
            }
        }

        public static Bitmap[] DevideImage(Bitmap image, int n)
        {
            int devided_width = (int)((double)image.Width / n + 0.5);

            Bitmap[] bmps = new Bitmap[n];

            for (int i = 0; i < n; i++)
            {
                bmps[i] = new Bitmap(devided_width, image.Height);
                Graphics g = Graphics.FromImage(bmps[i]);
                g.DrawImage(image, new Rectangle(0, 0, devided_width, image.Height), new Rectangle(i * devided_width, 0, devided_width, image.Height), GraphicsUnit.Pixel);
                g.Dispose();
            }
            return bmps;
        }

        public static Bitmap MedianFilter(object obj_input, int matrixSize)
        {

            Bitmap input = (Bitmap)obj_input;

            Bitmap output = new Bitmap(input.Width, input.Height);

            //devide image to R, G, B channels
            uint[,] o_R = new uint[input.Width + matrixSize - 1, input.Height + matrixSize - 1];
            uint[,] o_G = new uint[input.Width + matrixSize - 1, input.Height + matrixSize - 1];
            uint[,] o_B = new uint[input.Width + matrixSize - 1, input.Height + matrixSize - 1];

            uint[,] n_R = new uint[input.Width, input.Height];
            uint[,] n_G = new uint[input.Width, input.Height];
            uint[,] n_B = new uint[input.Width, input.Height];

            //filling R, G, B massives with appropriate value of the color of pixel
            int midOfMatrix = Convert.ToInt32(Math.Floor(matrixSize/2f));

            for (int j = midOfMatrix; j < input.Height + midOfMatrix; j++)
            {
                for (int i = midOfMatrix; i < input.Width + midOfMatrix; i++)
                {
                    UInt32 pixel = (UInt32)input.GetPixel(i - midOfMatrix, j - midOfMatrix).ToArgb();

                    o_R[i, j] = ((pixel & 0x00FF0000) >> 16);
                    o_G[i, j] = ((pixel & 0x0000FF00) >> 8);
                    o_B[i, j] = (pixel & 0x000000FF);
                }
            }

            FillBorder(o_R, midOfMatrix);
            FillBorder(o_G, midOfMatrix);
            FillBorder(o_B, midOfMatrix);

            MakeMedianArr(o_R, n_R, midOfMatrix);
            MakeMedianArr(o_G, n_G, midOfMatrix);
            MakeMedianArr(o_B, n_B, midOfMatrix);



            for (int j = 0; j < input.Height; j++)
            {
                for (int i = 0; i < input.Width; i++)
                {
                    UInt32 newPixel = 0xFF000000 | ((UInt32)n_R[i, j] << 16) | ((UInt32)n_G[i, j] << 8) | (UInt32)n_B[i, j];
                    output.SetPixel(i, j, Color.FromArgb((int)newPixel));
                }
            }
            return output;
        }

        static void MakeMedianArr(uint[,] arr, uint[,] new_arr, int midOfMatrix)
        {
            for (int i = 1; i < arr.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < arr.GetLength(1) - 1; j++)
                {       
                    //going through the matrix with flexible size around the current element 
                    List<uint> matrix = new List<uint>();
                    for(int iInMatrix = i - midOfMatrix; iInMatrix < i + midOfMatrix; iInMatrix++)
                    {
                        for(int jInMatrix = j - midOfMatrix; jInMatrix < j + midOfMatrix; jInMatrix++)
                        {
                            matrix.Add(arr[iInMatrix, jInMatrix]);
                        }
                    }
                    matrix.Sort();
                    arr[i-1, j-1] = matrix[midOfMatrix];
                }
            }
        }

        static void FillBorder(uint[,] arr, int midOfMatrix)
        {
            for (int mof = midOfMatrix; mof > 0; mof--)
            {
                for (int i = 0; i < arr.GetLength(1); i++)
                {
                    arr[mof - 1, i] = arr[mof, i];
                }
            }

            for (int mof = arr.GetUpperBound(0) - midOfMatrix; mof < arr.GetUpperBound(0); mof++)
            {
                for (int i = 0; i < arr.GetLength(1); i++)
                {
                    arr[mof + 1, i] = arr[mof, i];
                }
            }

            for (int mof = midOfMatrix; mof > 0; mof--)
            {
                for (int i = 0; i < arr.GetLength(0); i++)
                {
                    arr[i, mof - 1] = arr[i, mof];
                }
            }

            for (int mof = arr.GetUpperBound(1) - midOfMatrix; mof < arr.GetUpperBound(1); mof++)
            {
                for (int i = 0; i < arr.GetLength(0); i++)
                {
                    arr[i, mof + 1] = arr[i, mof];
                }
            }
        }
       
    }
}

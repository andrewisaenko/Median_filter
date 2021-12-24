using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace median_filter
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "Image Files(*.jpg;*.png)|*.jpg;*.png|All files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pictureBox1.Image = new Bitmap(ofd.FileName);
                }
                catch
                {
                    MessageBox.Show("Невозможно открыть выбранный файл", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error  );
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if(pictureBox1.Image != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Save image as...";
                sfd.OverwritePrompt = true;
                sfd.CheckPathExists = true;

                sfd.Filter = "Image Files(*.jpg)|*.jpg|Image Files(*.png)|*.png|All files (*.*)|*.*";
                sfd.ShowHelp = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBox2.Image.Save(sfd.FileName);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

            }
        }

        private void BWbutton_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Stopwatch time = new Stopwatch();                
                Bitmap image = new Bitmap(pictureBox1.Image);
                Bitmap output;                
                int n = Convert.ToInt32(maskedTextBox1.Text);
                                               
                if (n <= 0)
                {
                    MessageBox.Show("Потоков должно быть больше 0!");
                }
                else if (n == 1)
                {
                    time.Start();
                    output = MedianFilter(image);
                    time.Stop();
                    pictureBox2.Image = output;
                    MessageBox.Show($"t = {time.ElapsedMilliseconds}ms");
                }
                else
                {
                    Bitmap[] img_parts = DevideImage(image, n);

                    Task[] tasks = new Task[n];

                    for (int i = 0; i < n; i++)
                    {
                        time.Start();
                        tasks[i] = new Task(() => { img_parts[i] = MedianFilter(img_parts[i]); });
                        tasks[i].Start();
                        Thread.Sleep(n-1);
                    }

                    Task.WaitAll(tasks);

                    output = CombineImage(img_parts);
                    time.Stop();

                    pictureBox2.Image = output;

                    MessageBox.Show($"t = {time.ElapsedMilliseconds}ms");
                }                
            }                       

            static Bitmap CombineImage(Bitmap[] files)
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

            static Bitmap[] DevideImage(Bitmap image, int n)
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

            static Bitmap MedianFilter(object obj_input)
            {

                Bitmap input = (Bitmap)obj_input;

                Bitmap output = new Bitmap(input.Width, input.Height);

                uint[,] o_R = new uint[input.Width + 2, input.Height + 2];
                uint[,] o_G = new uint[input.Width + 2, input.Height + 2];
                uint[,] o_B = new uint[input.Width + 2, input.Height + 2];

                uint[,] n_R = new uint[input.Width, input.Height];
                uint[,] n_G = new uint[input.Width, input.Height];
                uint[,] n_B = new uint[input.Width, input.Height];

                for (int j = 1; j < input.Height + 1; j++)
                {
                    for (int i = 1; i < input.Width + 1; i++)
                    {
                        UInt32 pixel = (UInt32)input.GetPixel(i - 1, j - 1).ToArgb();

                        o_R[i, j] = ((pixel & 0x00FF0000) >> 16);
                        o_G[i, j] = ((pixel & 0x0000FF00) >> 8);
                        o_B[i, j] = (pixel & 0x000000FF);
                    }
                }

                FillBorder(o_R);
                FillBorder(o_G);
                FillBorder(o_B);

                MakeMedianArr(o_R, n_R);
                MakeMedianArr(o_G, n_G);
                MakeMedianArr(o_B, n_B);


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

            static void MakeMedianArr(uint[,] arr, uint[,] new_arr)
            {
                for (int i = 1; i < arr.GetLength(0) - 1; i++)
                {

                    for (int j = 1; j < arr.GetLength(1) - 1; j++)
                    {
                        uint[] buf = new uint[9];
                        buf[0] = arr[i - 1, j - 1];
                        buf[1] = arr[i, j - 1];
                        buf[2] = arr[i + 1, j - 1];
                        buf[3] = arr[i - 1, j];
                        buf[4] = arr[i, j];
                        buf[5] = arr[i + 1, j];
                        buf[6] = arr[i - 1, j + 1];
                        buf[7] = arr[i, j + 1];
                        buf[8] = arr[i + 1, j + 1];
                        Array.Sort(buf);
                        new_arr[i - 1, j - 1] = buf[4];
                    }
                }
            }

            static void FillBorder(uint[,] arr)
            {
                CopyRange(arr, 1, 0, 0);
                CopyRange(arr, arr.GetUpperBound(0) - 1, arr.GetUpperBound(0), 0);
                CopyRange(arr, 1, 0, 1);
                CopyRange(arr, arr.GetUpperBound(1) - 1, arr.GetUpperBound(1), 1);
            }

            static void CopyRange(uint[,] mas, int num1, int num2, int ColumnOrRow)
            {

                if (ColumnOrRow == 0)
                {
                    for (int i = 0; i < mas.GetLength(1); i++)
                    {
                        mas[num2, i] = mas[num1, i];
                    }
                }
                else if (ColumnOrRow == 1)
                {
                    for (int i = 0; i < mas.GetLength(0); i++)
                    {
                        mas[i, num2] = mas[i, num1];
                    }
                }
            }

        }
            
        
        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            maskedTextBox1.Mask = "00";            
        }
    }
}

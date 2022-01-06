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

                int matrixSize = 3;
                if(matrixSizeTB.Text != "" && Convert.ToInt32(matrixSizeTB.Text) > 3 && Convert.ToInt32(matrixSizeTB.Text)%2 != 0)
                {
                    matrixSize = Convert.ToInt32(matrixSizeTB.Text);
                }
                else if(matrixSizeTB.Text != "" && (Convert.ToInt32(matrixSizeTB.Text) < 3 || Convert.ToInt32(matrixSizeTB.Text) % 2 == 0))
                {
                    MessageBox.Show("Invalid size of manrix!");
                    return;
                }
                
                int numOfThreads = 1;
                if (maskedTextBox1.Text != "" && Convert.ToInt32(maskedTextBox1.Text) > 1)
                {
                    numOfThreads = Convert.ToInt32(maskedTextBox1.Text);
                }
              
                Bitmap image = new Bitmap(pictureBox1.Image);
                Bitmap output;

                if (numOfThreads == 1)
                {
                    //filtering with one thread
                    time.Start();
                    output = ImageFunc.MedianFilter(image, matrixSize);
                    time.Stop();
                }
                else
                {
                    //filtering with N threads
                    Bitmap[] img_parts = ImageFunc.DevideImage(image, numOfThreads);

                    Task[] tasks = new Task[numOfThreads];

                    time.Start();
                    for (int i = 0; i < numOfThreads; i++)
                    {                        
                        tasks[i] = new Task(() => { img_parts[i] = ImageFunc.MedianFilter(img_parts[i], matrixSize); });
                        tasks[i].Start();
                        Thread.Sleep(numOfThreads - 1);
                    }
                    Task.WaitAll(tasks);
                    output = ImageFunc.CombineImage(img_parts);
                    time.Stop();
                }
                
                pictureBox2.Image = output;
                MessageBox.Show($"t = {time.ElapsedMilliseconds}ms");
            }     
        }            
        
        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            maskedTextBox1.Mask = "00";            
        }       
    }
}

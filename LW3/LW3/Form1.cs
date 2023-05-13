using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using OtsuThreshold;

namespace LW3
{
    public partial class Form1 : Form
    {
        int it = -1, w = 0, h = 0;
        private Bitmap image = null, image2 = null;
        string filename1;
        public Bitmap bitmap;
        private Otsu ot = new Otsu();
        private Bitmap org;
        private Bitmap start;
        private Bitmap average;
        private Bitmap finish;
        public Form1()
        {
            InitializeComponent();
            image = new Bitmap(picture.Width, picture.Height);
            picture.Image = image;
            button1.Visible = false;
            button3.Visible = false;
            picture.Visible = false;
            pictureBox2.Visible = false;
            label1.Visible = false;
            textBox1.Visible = false;
            button4.Visible = false;
        }

        //добавить картинку
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.InitialDirectory = Directory.GetCurrentDirectory();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            open.RestoreDirectory = true;
            if (open.ShowDialog() == DialogResult.OK)
            {
                if (image == null)
                {
                    picture.Image = null;
                    image.Dispose();
                    
                }
                filename1 = open.FileName;
                image = new Bitmap(filename1);
                picture.Image = image;
                org = (Bitmap)picture.Image.Clone();
            }
            bitmap = new Bitmap(image);
            pictureBox2.Image = bitmap;
            var NImg = (Bitmap)picture.Image;
            w = NImg.Width;
            h = NImg.Height;

            start = new Bitmap(w, h);
            average = new Bitmap(w, h);
            finish = new Bitmap(w, h);

            //Bitmap finish = new Bitmap(w, h);
            for (int i = 0; i < h; ++i)
            {
                for (int j = 0; j < w; ++j)
                {
                    var pix = NImg.GetPixel(j, i);
                    start.SetPixel(j, i, pix);
                    average.SetPixel(j, i, pix);

                }
            }

            button2.Visible = false;
            button1.Visible = true;
            button3.Visible = true;
            picture.Visible = true;
            pictureBox1.Visible = true;
            pictureBox2.Visible = true;
            button4.Visible = true;
        }

        //критерий Гаврилова
        private void button1_Click(object sender, EventArgs e)
        {
            var w = image.Width;
            var h = image.Height;
            int sum = 0;

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    var pix = image.GetPixel(j, i);
                    int average = (int)(0.2125*pix.R + 0.7154*pix.G + 0.0721*pix.B);
                    sum += average;
                }
            }
            int tmp = sum / (w * h);

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    var pix = image.GetPixel(j, i);
                    int average = (int)(pix.R + pix.B + pix.G) / 3;

                    if (average <= tmp)
                    {
                        bitmap.SetPixel(j, i, Color.Black);
                    }
                    else
                    {
                        bitmap.SetPixel(j, i, Color.White);
                    }
                }
            }
            pictureBox2.Image = bitmap;
        }

        // критерий Отсу
        public void CalculateBarChart()
        {
                // определяем размеры гистограммы. В идеале, ширина должны быть кратна 768 - 
                // по пикселю на каждый столбик каждого из каналов
                int width = pictureBox1.Width, height = pictureBox1.Height;
                // получаем битмап из изображения
                Bitmap bmp = new Bitmap(pictureBox2.Image);
                // создаем саму гистограмму
                Bitmap barChart = new Bitmap(width, height);
                // создаем массивы, в котором будут содержаться количества повторений для каждого из значений каналов.
                // индекс соответствует значению канала
                int[] R = new int[256];
                int[] G = new int[256];
                int[] B = new int[256];
                int i, j;
                Color color;
                // собираем статистику для изображения
                for (i = 0; i < bmp.Width; ++i)
                    for (j = 0; j < bmp.Height; ++j)
                    {
                        color = bmp.GetPixel(i, j);
                        ++R[color.R];
                        ++G[color.G];
                        ++B[color.B];
                    }
                // находим самый высокий столбец, чтобы корректно масштабировать гистограмму по высоте
                int max = 0;
                for (i = 0; i < 256; ++i)
                {
                    if (R[i] > max)
                        max = R[i];
                    if (G[i] > max)
                        max = G[i];
                    if (B[i] > max)
                        max = B[i];
                }
                // определяем коэффициент масштабирования по высоте
                double point = (double)max / height;
                // отрисовываем столбец за столбцом нашу гистограмму с учетом масштаба
                for (i = 0; i < width - 3; ++i)
                {
                    for (j = height - 1; j > height - R[i / 3] / point; --j)
                    {
                        barChart.SetPixel(i, j, Color.Red);
                    }
                    ++i;
                    for (j = height - 1; j > height - G[i / 3] / point; --j)
                    {
                        barChart.SetPixel(i, j, Color.Blue);
                    }
                    ++i;
                    for (j = height - 1; j > height - B[i / 3] / point; --j)
                    {
                        barChart.SetPixel(i, j, Color.Green);
                    }
                }

            pictureBox1.Image = barChart;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            CalculateBarChart();
            label1.Visible = true;
            textBox1.Visible = true;
            Bitmap temp = (Bitmap)org.Clone();
            ot.Convert2GrayScaleFast(temp);
            int otsuThreshold = ot.getOtsuThreshold((Bitmap)temp);
            ot.threshold(temp, otsuThreshold);
            textBox1.Text = otsuThreshold.ToString();
            pictureBox2.Image = temp;

        }

        // критерий Ниблеска

        void nibl(int a, float k, int ir) // ir 3 - ниблеска, ir 4 - сауволы
        {
            int i1, i2;
            int j1, j2;
            int f = a / 2;
            int width = average.Width;
            int height = average.Height;

            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    float s2 = 0, m2 = 0, t = 0;
                    float s = 0, m = 0;
                    float d = 0, q = 0;
                    float kol = 0;
                    if ((i - f) <= 0) { i1 = 0; } else { i1 = i - f; };
                    if ((i + f) >= h) { i2 = h - 1; } else { i2 = i + f; };
                    if ((j - f) <= 0) { j1 = 0; } else { j1 = j - f; };
                    if ((j + f) >= w) { j2 = w - 1; } else { j2 = j + f; };
                    for (int i11 = i1; i11 <= i2; ++i11)
                    {
                        for (int j11 = j1; j11 <= j2; ++j11)
                        {
                            kol++;
                            var pix1 = average.GetPixel(j11, i11);
                            int gd1 = (int)(pix1.R);
                            s += gd1; s2 += (float)Math.Pow(gd1, 2);
                        }
                    }
                    m = s / kol; m2 = s2 / kol;
                    d = m2 - (float)Math.Pow(m, 2);
                    q = (float)Math.Sqrt(d);
                    if (ir == 3) { t = (m + (k * q)); }
                    if (ir == 4)
                    {
                        int r = 0;
                        int clb = checkedListBox1.SelectedIndex;
                        if (clb == 1 || clb == 3) { r = 256; }
                        else { r = 128; }
                        t = m * (1 + k * ((q / r) - 1));
                    }

                    var pix = average.GetPixel(j, i);
                    int gd = (int)(pix.R);
                    if (gd <= t) { gd = 0; }
                    if (gd > t) { gd = 255; }
                    finish.SetPixel(j, i, Color.FromArgb(gd, gd, gd));
                }
            }
            pictureBox2.Image = finish;

        }
        private void button4_Click(object sender, EventArgs e)
        {
            int a = 0;
            float k = 0;
            var NImg = start;
            var w1 = NImg.Width;
            var h1 = NImg.Height;
            for (int i = 0; i < h1; ++i)
            {
                for (int j = 0; j < w1; ++j)
                {
                    var pix = start.GetPixel(j, i);
                    int r = (int)(pix.R);
                    int g = (int)(pix.G);
                    int b = (int)(pix.B);
                    int gd = 0;
                    gd = (int)((0.2125 * r + 0.7154 * g + 0.0721 * b)) / 3;
                    average.SetPixel(j, i, Color.FromArgb(gd, gd, gd));
                }
            }

            //pictureBox2.Image = average;
            nibl(3,4,3);
        }


    }
}

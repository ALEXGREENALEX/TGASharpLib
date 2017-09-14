using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace TGASharpLib
{
    public partial class Form1 : Form
    {
        string[] Files = Directory.GetFiles(@"..\..\..\TGA File Examples\", "*.tga", SearchOption.AllDirectories);
        TGA T;

        public Form1()
        {
            InitializeComponent();

            SetTabWidth(richTextBox1, 2);

            for (int i = 0; i < Files.Length; i++)
                listBox1.Items.Add(Path.GetFileName(Files[i]));
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string TgaFile = Files[listBox1.SelectedIndex];
            if (File.Exists(TgaFile))
            {
                T = new TGA(TgaFile);
                //T.UpdatePostageStampImage();
                ShowTga();
            }
        }

        private void buttonTryConvert_Click(object sender, EventArgs e)
        {
            if (T == null)
                return;

            T = (TGA)((Bitmap)T);
            ShowTga();
        }

        private void buttonSaveSelected_Click(object sender, EventArgs e)
        {
            if (T == null)
                return;

            string OutDir = @"D:\TGA\";
            if (!Directory.Exists(OutDir))
                Directory.CreateDirectory(OutDir);

            T.Save(Path.Combine(OutDir, Path.GetFileName("___T.tga")));
        }

        private void buttonSaveAll_Click(object sender, EventArgs e)
        {
            string OutDir = @"D:\TGA\";
            if (!Directory.Exists(OutDir))
                Directory.CreateDirectory(OutDir);

            for (int i = 0; i < Files.Length; i++)
                new TGA(Files[i]).Save(Path.Combine(OutDir, Path.GetFileName(Files[i])));
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && (e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                e.Effect = DragDropEffects.Move;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.Effect == DragDropEffects.Move)
            {
                string[] Files = (string[])e.Data.GetData(DataFormats.FileDrop);
                T = TGA.FromFile(Files[0]);
                ShowTga();
            }
        }

        #region Additional Functions
        void ShowTga()
        {
            Bitmap BMP = (Bitmap)T;
            Bitmap Thumb = T.GetPostageStampImage();

            // Convert image if Format16bppGrayScale
            if (BMP.PixelFormat == PixelFormat.Format16bppGrayScale)
            {
                BMP = Gray16To8bppIndexed(BMP);
                if (Thumb != null)
                    Thumb = Gray16To8bppIndexed(Thumb);
            }

            pictureBox1.Image = BMP;
            pictureBox2.Visible = (Thumb != null);
            pictureBox2.Image = Thumb;

            richTextBox1.Text = T.GetInfo();
        }

        #region Set Tabulation Size in textBox1
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr h, int msg, int wParam, int[] lParam);
        const int EM_SETTABSTOPS = 0xCB;

        public static void SetTabWidth(Control textbox, int tabWidth)
        {
            SendMessage(textbox.Handle, EM_SETTABSTOPS, 1, new int[] { tabWidth * 4 });
        }
        #endregion

        public Bitmap Gray16To8bppIndexed(Bitmap BmpIn)
        {
            if (BmpIn.PixelFormat != PixelFormat.Format16bppGrayScale)
                throw new BadImageFormatException();

            byte[] ImageData = new byte[BmpIn.Width * BmpIn.Height * 2];
            Rectangle Re = new Rectangle(0, 0, BmpIn.Width, BmpIn.Height);

            BitmapData BmpData = BmpIn.LockBits(Re, ImageLockMode.ReadOnly, BmpIn.PixelFormat);
            Marshal.Copy(BmpData.Scan0, ImageData, 0, ImageData.Length);
            BmpIn.UnlockBits(BmpData);

            byte[] ImageData2 = new byte[BmpIn.Width * BmpIn.Height];
            for (long i = 0; i < ImageData2.LongLength; i++)
                ImageData2[i] = ImageData[i * 2 + 1];
            ImageData = null;

            Bitmap BmpOut = new Bitmap(BmpIn.Width, BmpIn.Height, PixelFormat.Format8bppIndexed);
            BmpData = BmpOut.LockBits(Re, ImageLockMode.WriteOnly, BmpOut.PixelFormat);
            Marshal.Copy(ImageData2, 0, BmpData.Scan0, ImageData2.Length);
            BmpOut.UnlockBits(BmpData);
            ImageData2 = null;
            BmpData = null;

            ColorPalette GrayPalette = BmpOut.Palette;
            Color[] GrayColors = GrayPalette.Entries;
            for (int i = 0; i < GrayColors.Length; i++)
                GrayColors[i] = Color.FromArgb(i, i, i);
            BmpOut.Palette = GrayPalette;

            return BmpOut;
        }
        #endregion

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
            else if (e.KeyCode == Keys.F1)
            {
                if (T != null)
                {
                    T = TGA.FromBytes(T.ToBytes());
                    ShowTga();
                }
            }
        }
    }
}

using BrailleArtist.Common;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BrailleArtist.ViewModel
{
    class MainViewModel : NotifyBase
    {
        public CommandBase ImgChangeCommand { get; set; }
        public CommandBase MyCommand { get; set; }

        private string _brailledraw = null;
        public string BrailleDraw
        {
            get { return _brailledraw; }
            set { _brailledraw = value; this.DoNotify(); }
        }
        private int _width;
        public int Width
        {
            get { return _width; }
            set { _width = value; DoNotify(); }
        }
        private int _height;
        public int Height
        {
            get { return _height; }
            set { _height = value; DoNotify(); }
        }
        private bool _isratiolock = true;
        public bool IsRatioLock
        {
            get { return _isratiolock; }
            set
            {
                _isratiolock = value;
                DoNotify();
                if (IsRatioLock && Width != 0 && Height != 0) GValues.WidthDHeight = (float)Width / (float)Height;
            }
        }
        private string _loadingvisibility = "Collapsed";
        public string LoadingVisibility
        {
            get { return _loadingvisibility; }
            set { _loadingvisibility = value; DoNotify(); }
        }
        private float _middlebright;
        public float MiddleBright
        {
            get { return _middlebright; }
            set { _middlebright = value; DoNotify(); }
        }
        private int _viewfontsize = 10;
        public int ViewFontSize
        {
            get { return _viewfontsize; }
            set { _viewfontsize = value; DoNotify(); }
        }
        private BitmapImage _imgsource;
        public BitmapImage ImgSource
        {
            get { return _imgsource; }
            set { _imgsource = value; this.DoNotify(); }
        }

        public MainViewModel()
        {
            ImgChangeCommand = new CommandBase();
            ImgChangeCommand.DoExecute = new Action<object>(DoImgChange);
            ImgChangeCommand.DoCanExecute = new Func<object, bool>((o) => { return true; });
            ImgChangeCommand.CanExecuteChanged += null;

            MyCommand = new CommandBase();
            MyCommand.DoExecute = new Action<object>(DoMyCommand);
            MyCommand.DoCanExecute = new Func<object, bool>((o) => { return true; });
            MyCommand.CanExecuteChanged += null;
        }
        public async void DoImgChange(object o)
        {
            if (!String.IsNullOrEmpty(GValues.ImgName))
            {
                LoadingVisibility = "Visible";
                if (o.ToString() == "ColorReverse") { InvertBitmap(); }
                else if (o.ToString() == "Clockwise") { GValues.Image.RotateFlip(RotateFlipType.Rotate90FlipNone); }
                else if (o.ToString() == "AntiClockwise") { GValues.Image.RotateFlip(RotateFlipType.Rotate270FlipNone); }
                else if (o.ToString() == "FlipHorizontally") { GValues.Image.RotateFlip(RotateFlipType.RotateNoneFlipX); }
                else if (o.ToString() == "FlipVertically") { GValues.Image.RotateFlip(RotateFlipType.RotateNoneFlipY); }
                await Task.Run(() =>
                {
                    ShowImg();
                    DrawBraille();
                });
                LoadingVisibility = "Collapsed";
            }
        }
        public async void DoMyCommand(object o)
        {
            if (!String.IsNullOrEmpty(GValues.ImgName))
            {
                if (o.ToString() == "Reset")
                {
                    Width = GValues.Image.Width;
                    Height = GValues.Image.Height;
                    if (Width != 0 && Height != 0) GValues.WidthDHeight = Width / Height;
                    MiddleBright = GValues.PixelAverage;
                    ViewFontSize = 10;
                }
                else if (o.ToString() == "Draw")
                {
                    LoadingVisibility = "Visible";
                    await Task.Run(() => DrawBraille());
                    LoadingVisibility = "Collapsed";
                }
                else if (o.ToString() == "Copy")
                {
                    System.Windows.Forms.Clipboard.SetText(BrailleDraw);
                }
            }
        }
        public void CountImg()
        {
            float average = 0;
            int pixelCount = 0;
            PointBitmap bitmap = new PointBitmap(GValues.Image);
            bitmap.LockBits();
            for (int x = 0; x < bitmap.Width / 2; x++)
            {
                for (int y = 0; y < bitmap.Height / 4; y++)
                {
                    float pixelbrightness = bitmap.GetPixel(x, y).GetBrightness();
                    average = average + pixelbrightness;
                    pixelCount++;
                }
            }
            bitmap.UnlockBits();
            GValues.PixelAverage = average / pixelCount;
        }
        public void DrawBraille()
        {
            if (!String.IsNullOrEmpty(GValues.ImgName))
            {
                float average = MiddleBright;
                //Cut down the size of image.
                if (Width > 2000)
                {
                    Width = 1000;
                    Height = Convert.ToInt32((float)Width / GValues.WidthDHeight);
                }
                else if (Height > 2000)
                {
                    Height = 1000;
                    Width = Convert.ToInt32((float)Height * GValues.WidthDHeight);
                }
                else if (Width < 2 || Height < 4)
                {
                    return;
                }
                //The braille is not square, so we need use 8/9 to scale the image to right ratio.
                PointBitmap bitmap = new PointBitmap(new Bitmap(GValues.Image, Width * 8 / 9, Height));
                bitmap.LockBits();
                int width = bitmap.Width;
                int height = bitmap.Height;
                StringBuilder sb = new StringBuilder();
                for (int row = 0; row < height / 4; row++)
                {
                    for (int charno = 0; charno < width / 2; charno++)
                    {
                        int braille = 0x2800;
                        float darkest = 1;
                        int darkestpixel = 0x0;
                        for (int x = 0; x < 2; x++)
                        {
                            for (int y = 0; y < 4; y++)
                            {
                                float thisbrightness = bitmap.GetPixel(charno * 2 + x, row * 4 + y).GetBrightness();
                                //If use homemade function "GetBrightness();" maybe more personalization.
                                if (thisbrightness < average) braille = braille + BrailleDot(x, y);
                                else if (thisbrightness <= darkest)
                                {
                                    darkest = thisbrightness;
                                    darkestpixel = BrailleDot(x, y);
                                }
                            }
                        }
                        //Make sure there will be one dot per braille at least.
                        braille = braille == 0x2800 ? braille + darkestpixel : braille;
                        sb.Append((char)braille);
                    }
                    sb.Append("\n");
                }
                bitmap.UnlockBits();
                BrailleDraw = sb.ToString();
                sb.Clear();
            }
        }
        private int BrailleDot(int col, int row)
        {
            //There calculate one dot at braille.
            int val = 0;
            switch (row)
            {
                case 0:
                    val = 0x1 << (col * 3);
                    break;
                case 1:
                    val = 0x2 << (col * 3);
                    break;
                case 2:
                    val = 0x4 << (col * 3);
                    break;
                case 3:
                    val = 0x40 << (col * 1);
                    break;
            }
            return val;
        }
        public void ShowImg()
        {
            if (!String.IsNullOrEmpty(GValues.ImgName))
            {
                using (Stream ms = new MemoryStream())
                {
                    Bitmap bm = new Bitmap(GValues.Image);
                    bm.Save(ms, ImageFormat.Png);
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    ImgSource = bitmap;
                }
            }
        }
        public void InvertBitmap()
        {
            Bitmap bitmap = GValues.Image;
            Bitmap src = new Bitmap(Image.FromHbitmap(bitmap.GetHbitmap()));
            BitmapData srcdat = src.LockBits(new Rectangle(System.Drawing.Point.Empty, src.Size), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* pix = (byte*)srcdat.Scan0;
                for (int i = 0; i < srcdat.Stride * srcdat.Height; i++)
                    pix[i] = (byte)(255 - pix[i]);
            }
            src.UnlockBits(srcdat);
            GValues.Image = src;
        }
    }
}

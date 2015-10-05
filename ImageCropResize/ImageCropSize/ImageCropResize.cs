#region Using

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using ImageCropResize.Properties;

#endregion Using

namespace ImageCropResize 
{
    public partial class ImageCropForm : Form {

        #region Properties
        Rectangle _cropRect;
        Rectangle _rcLt, _rcRt, _rcLb, _rcRb;
        Rectangle _rcOld, _rcNew;
        Rectangle _rcOriginal;
        SolidBrush _brushRect;
        HatchBrush _brushRectSmall;
        Color _brushColor;

        int _alphaBlend;
        int _nSize;
        int _nWd;
        int _nHt;
        int _nResizeRt;
        int _nResizeBl;
        int _nResizeLt;
        int _nResizeRb;
        int _nThatsIt;
        int _nCropRect;
        int _cropWidth;

        int _imageWidth;
        int _imageHeight;
        readonly int _heightOffset;

        double _cropAspectRatio;
        double _imageAspectRatio;
        double _zoomedRatio;

        Point _ptOld;
        Point _ptNew;

        string _imageStats;
        string _filename;
        #endregion Properties

        #region Initialize
        public ImageCropForm() {
            InitializeComponent();
            SetStyle(
                  ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.UserPaint |
                  ControlStyles.DoubleBuffer, true);
            _cropWidth = 320;
            _heightOffset = panel1.Height + statusStrip1.Height +
                            SystemInformation.CaptionHeight + (SystemInformation.BorderSize.Height * 2);
            UpdateAspectRatio();
            InitializeCropRectangle();
        }

        void InitializeCropRectangle() {
            _alphaBlend = 48;
            _nSize = 8;
            _nWd = _cropWidth = 320;
            _nHt = 1;
            _nThatsIt = 0;
            _nResizeRt = 0;
            _nResizeBl = 0;
            _nResizeLt = 0;
            _nResizeRb = 0;

            _cropAspectRatio = 1.0;

            _brushColor = Color.White;
            _brushRect = new SolidBrush(Color.FromArgb(_alphaBlend, _brushColor.R, _brushColor.G, _brushColor.B));

            _brushColor = Color.Yellow;
            _brushRectSmall = new HatchBrush(HatchStyle.Percent50, Color.FromArgb(192, _brushColor.R, _brushColor.G, _brushColor.B));

            _ptOld = new Point(0, 0);
            _rcOriginal = new Rectangle(0, 0, 0, 0);
            _rcLt = new Rectangle(0, 0, _nSize, _nSize);
            _rcRt = new Rectangle(0, 0, _nSize, _nSize);
            _rcLb = new Rectangle(0, 0, _nSize, _nSize);
            _rcRb = new Rectangle(0, 0, _nSize, _nSize);
            _rcOld = _cropRect = new Rectangle(0, 0, _nWd, _nHt);

            AdjustResizeRects();
        }
        
        #endregion Initialize

        #region MouseEvents
        
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (pictureBox1.Image == null)
                return;

            var pt = new Point(e.X, e.Y);

            if (_rcLt.Contains(pt))
                Cursor = Cursors.SizeNWSE;
            else
                if (_rcRt.Contains(pt))
                    Cursor = Cursors.SizeNESW;
                else
                    if (_rcLb.Contains(pt))
                        Cursor = Cursors.SizeNESW;
                    else
                        if (_rcRb.Contains(pt))
                            Cursor = Cursors.SizeNWSE;
                        else
                            if (_cropRect.Contains(pt))
                                Cursor = Cursors.SizeAll;
                            else
                                Cursor = Cursors.Default;


            if (e.Button == MouseButtons.Left) {
                if (_nResizeRb == 1) {
                    _rcNew.X = _cropRect.X;
                    _rcNew.Y = _cropRect.Y;
                    _rcNew.Width = pt.X - _rcNew.Left;
                    _rcNew.Height = pt.Y - _rcNew.Top;

                    if (_rcNew.X > _rcNew.Right) {
                        _rcNew.Offset(-_nWd, 0);
                        if (_rcNew.X < 0)
                            _rcNew.X = 0;
                    }
                    if (_rcNew.Y > _rcNew.Bottom) {
                        _rcNew.Offset(0, -_nHt);
                        if (_rcNew.Y < 0)
                            _rcNew.Y = 0;
                    }

                    DrawDragRect(e);
                    _rcOld = _cropRect = _rcNew;
                    Cursor = Cursors.SizeNWSE;
                }
                else
                    if (_nResizeBl == 1) {
                        _rcNew.X = pt.X;
                        _rcNew.Y = _cropRect.Y;
                        _rcNew.Width = _cropRect.Right - pt.X;
                        _rcNew.Height = pt.Y - _rcNew.Top;

                        if (_rcNew.X > _rcNew.Right) {
                            _rcNew.Offset(_nWd, 0);
                            if (_rcNew.Right > ClientRectangle.Width)
                                _rcNew.Width = ClientRectangle.Width - _rcNew.X;
                        }
                        if (_rcNew.Y > _rcNew.Bottom) {
                            _rcNew.Offset(0, -_nHt);
                            if (_rcNew.Y < 0)
                                _rcNew.Y = 0;
                        }

                        DrawDragRect(e);
                        _rcOld = _cropRect = _rcNew;
                        Cursor = Cursors.SizeNESW;
                    }
                    else
                        if (_nResizeRt == 1) {
                            _rcNew.X = _cropRect.X;
                            _rcNew.Y = pt.Y;
                            _rcNew.Width = pt.X - _rcNew.Left;
                            _rcNew.Height = _cropRect.Bottom - pt.Y;

                            if (_rcNew.X > _rcNew.Right) {
                                _rcNew.Offset(-_nWd, 0);
                                if (_rcNew.X < 0)
                                    _rcNew.X = 0;
                            }
                            if (_rcNew.Y > _rcNew.Bottom) {
                                _rcNew.Offset(0, _nHt);
                                if (_rcNew.Bottom > ClientRectangle.Height)
                                    _rcNew.Y = ClientRectangle.Height - _rcNew.Height;
                            }

                            DrawDragRect(e);
                            _rcOld = _cropRect = _rcNew;
                            Cursor = Cursors.SizeNESW;
                        }
                        else
                            if (_nResizeLt == 1) {
                                _rcNew.X = pt.X;
                                _rcNew.Y = pt.Y;
                                _rcNew.Width = _cropRect.Right - pt.X;
                                _rcNew.Height = _cropRect.Bottom - pt.Y;

                                if (_rcNew.X > _rcNew.Right) {
                                    _rcNew.Offset(_nWd, 0);
                                    if (_rcNew.Right > ClientRectangle.Width)
                                        _rcNew.Width = ClientRectangle.Width - _rcNew.X;
                                }
                                if (_rcNew.Y > _rcNew.Bottom) {
                                    _rcNew.Offset(0, _nHt);
                                    if (_rcNew.Bottom > ClientRectangle.Height)
                                        _rcNew.Height = ClientRectangle.Height - _rcNew.Y;
                                }

                                DrawDragRect(e);
                                _rcOld = _cropRect = _rcNew;
                                Cursor = Cursors.SizeNWSE;
                            }
                            else
                                if (_nCropRect == 1) //Moving the rectangle
                                {
                                    _ptNew = pt;
                                    int dx = _ptNew.X - _ptOld.X;
                                    int dy = _ptNew.Y - _ptOld.Y;
                                    _cropRect.Offset(dx, dy);
                                    _rcNew = _cropRect;
                                    DrawDragRect(e);
                                    _ptOld = _ptNew;
                                }

                AdjustResizeRects();
                DisplayLocation();
                pictureBox1.Update();
            }

            base.OnMouseMove(e);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            var pt = new Point(e.X, e.Y);
            _rcOriginal = _cropRect;

            if (_rcRb.Contains(pt)) {
                _rcOld = new Rectangle(_cropRect.X, _cropRect.Y, _cropRect.Width, _cropRect.Height);
                _rcNew = _rcOld;
                _nResizeRb = 1;
            }
            else
                if (_rcLb.Contains(pt)) {
                    _rcOld = new Rectangle(_cropRect.X, _cropRect.Y, _cropRect.Width, _cropRect.Height);
                    _rcNew = _rcOld;
                    _nResizeBl = 1;
                }
                else
                    if (_rcRt.Contains(pt)) {
                        _rcOld = new Rectangle(_cropRect.X, _cropRect.Y, _cropRect.Width, _cropRect.Height);
                        _rcNew = _rcOld;
                        _nResizeRt = 1;
                    }
                    else
                        if (_rcLt.Contains(pt)) {
                            _rcOld = new Rectangle(_cropRect.X, _cropRect.Y, _cropRect.Width, _cropRect.Height);
                            _rcNew = _rcOld;
                            _nResizeLt = 1;
                        }
                        else
                            if (_cropRect.Contains(pt)) {
                                _nResizeBl = _nResizeLt = _nResizeRb = _nResizeRt = 0;
                                _nCropRect = 1;
                                _ptNew = _ptOld = pt;
                            }
            _nThatsIt = 1;
            base.OnMouseDown(e);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if (_nThatsIt == 0)
                return;

            _nCropRect = 0;
            _nResizeRb = 0;
            _nResizeBl = 0;
            _nResizeRt = 0;
            _nResizeLt = 0;

            if (_cropRect.Width <= 0 || _cropRect.Height <= 0)
                _cropRect = _rcOriginal;

            if (_cropRect.Right > ClientRectangle.Width)
                _cropRect.Width = ClientRectangle.Width - _cropRect.X;

            if (_cropRect.Bottom > ClientRectangle.Height)
                _cropRect.Height = ClientRectangle.Height - _cropRect.Y;

            if (_cropRect.X < 0)
                _cropRect.X = 0;

            if (_cropRect.Y < 0)
                _cropRect.Y = 0;

            // need to add logic for portrait mode of crop box in this
            // area

            // now that the crop box position is established
            // force it to the proper aspect ratio
            // and scale it

            if (_cropRect.Width > _cropRect.Height) {
                _cropRect.Height = (int)(_cropRect.Width / _cropAspectRatio);
            }
            else {
                _cropRect.Width = (int)(_cropRect.Height * _cropAspectRatio);
            }

            AdjustResizeRects();
            pictureBox1.Refresh();

            base.OnMouseUp(e);

            _nWd = _rcNew.Width;
            _nHt = _rcNew.Height;

            DisplayLocation();
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e) {
            Cursor = Cursors.Default;
        }
        
        #endregion MouseEvents

        #region ButtonEvents
        
        private void btnOpen_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                _filename = openFileDialog1.FileName;
                LoadImage(_filename);
                btnReset.Enabled = true;
                btnOK.Enabled = true;
            }
        }
        
        private void btnOK_Click(object sender, EventArgs e) {
            Bitmap bmp = null;

            var scaledCropRect = new Rectangle {
                X = (int)(_cropRect.X / _zoomedRatio),
                Y = (int)(_cropRect.Y / _zoomedRatio),
                Width = (int)(_cropRect.Width / _zoomedRatio),
                Height = (int)(_cropRect.Height / _zoomedRatio)
            };

            if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
                try {
                    bmp = (Bitmap)CropImage(pictureBox1.Image, scaledCropRect);
                    // 85% quality
                    SaveBmpImg(saveFileDialog1.FileName, bmp, 85);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, "btnOK_Click()");
                }
            }

            if (bmp != null)
                bmp.Dispose();
        }
       
        private void btnReset_Click(object sender, EventArgs e) {
            LoadImage(_filename);
        }

        private void btnCenterCropBox_Click() {
            UpdateAspectRatio();

            _cropRect.X = (pictureBox1.ClientRectangle.Width - _cropRect.Width) / 2;
            _cropRect.Y = (pictureBox1.ClientRectangle.Height - _cropRect.Height) / 2;

            AdjustResizeRects();
            pictureBox1.Refresh();
        }

        #endregion ButtonEvents

        #region ImageOperations
        private void LoadImage(string file) {
            Cursor = Cursors.AppStarting;

            pictureBox1.Image = Image.FromFile(file);

            _imageWidth = pictureBox1.Image.Width;
            _imageHeight = pictureBox1.Image.Height;

            _imageStats = String.Format("{0} | {1}x{2} | Aspect {3:0.0}",
                System.IO.Path.GetFileName(file), _imageWidth, _imageHeight,
                (double)_imageWidth / (double)_imageHeight
                );
            if (_imageWidth > _imageHeight) {
                _imageAspectRatio = _imageWidth / (double)_imageHeight;
                Width = 800 + (SystemInformation.BorderSize.Width * 2);
                Height = (int)((Width / _imageAspectRatio)) + _heightOffset;
            }
            else {
                _imageAspectRatio = _imageHeight / (double)_imageWidth;
                Height = 800;
                Width = (int)((Height / _imageAspectRatio)) + _heightOffset;
            }

            btnCenterCropBox_Click();
            Form1_ResizeEnd(null, null);
            Cursor = Cursors.Default;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e) {
            if (pictureBox1.Image == null) {
                var xGrayBox = true;
                var backgroundX = 0;
                while (backgroundX < pictureBox1.Width) {
                    var backgroundY = 0;
                    var yGrayBox = xGrayBox;
                    while (backgroundY < pictureBox1.Height) {
                        var recWidth = (backgroundX + 50 > pictureBox1.Width) ? pictureBox1.Width - backgroundX : 50;
                        var recHeight = (backgroundY + 50 > pictureBox1.Height) ? pictureBox1.Height - backgroundY : 50;
                        e.Graphics.FillRectangle(yGrayBox ? Brushes.LightGray : Brushes.Gainsboro, backgroundX, backgroundY, recWidth + 2, recHeight + 2);
                        backgroundY += 50;
                        yGrayBox = !yGrayBox;
                    }
                    backgroundX += 50;
                    xGrayBox = !xGrayBox;
                }
            }
            else {
                e.Graphics.FillRectangle((_brushRect), _cropRect);
                e.Graphics.FillRectangle((_brushRectSmall), _rcLt);
                e.Graphics.FillRectangle((_brushRectSmall), _rcRt);
                e.Graphics.FillRectangle((_brushRectSmall), _rcLb);
                e.Graphics.FillRectangle((_brushRectSmall), _rcRb);
                AdjustResizeRects();
            }
            base.OnPaint(e);
        }

        public void AdjustResizeRects() {
            _rcLt.X = _cropRect.Left;
            _rcLt.Y = _cropRect.Top;

            _rcRt.X = _cropRect.Right - _rcRt.Width;
            _rcRt.Y = _cropRect.Top;

            _rcLb.X = _cropRect.Left;
            _rcLb.Y = _cropRect.Bottom - _rcLb.Height;

            _rcRb.X = _cropRect.Right - _rcRb.Width;
            _rcRb.Y = _cropRect.Bottom - _rcRb.Height;
        }

        private void DrawDragRect(MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) {
                return;
            }
            AdjustResizeRects();
            pictureBox1.Invalidate();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e) {
            if (_imageAspectRatio == 0)
                return;
            Height = (int)((Width / _imageAspectRatio)) + _heightOffset;
            UpdateAspectRatio();
            Refresh();
        }

        private void DisplayLocation() {
            if (pictureBox1.Image == null)
                return;

            tsLabelCropboxLocation.Text = String.Format("{0} |  Scale {1:0.00}% | Crop Area {2} x {3} | Crop X, Y {4}, {5}",
                                _imageStats,
                                _zoomedRatio * 100.0,
                                (int)((double)_cropRect.Width / _zoomedRatio),
                                (int)((double)_cropRect.Height / _zoomedRatio),
                                (int)((double)_cropRect.X / _zoomedRatio),
                                (int)((double)_cropRect.Y / _zoomedRatio)
                            );
        }

        private void UpdateAspectRatio() {

            _cropAspectRatio = 1.0;
            var cropHeight = (int)((_cropWidth / _cropAspectRatio));

            try {
                _zoomedRatio = pictureBox1.ClientRectangle.Width / (double)_imageWidth;
            } catch {
                _zoomedRatio = 1.0;
            }
            _cropRect.Width = (int)(_cropWidth * _zoomedRatio);
            _cropRect.Height = (int)(cropHeight * _zoomedRatio);
            _nThatsIt = 1;
            pictureBox1_MouseUp(null, null);
        }

        private static Image CropImage(Image img, Rectangle cropArea) {
            try {
                var bmpImage = new Bitmap(img);
                var bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
                return bmpCrop;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, Resources.ImageCropForm_CropImage_CropImage__);
            }
            return null;
        }

        private static void SaveBmpImg(string path, Image img, long quality) {
            var qualityParam = new EncoderParameter(
              Encoder.Quality, quality);
            var bmpCodec = GetEncoderInfo("image/bmp");

            if (bmpCodec == null) {
                MessageBox.Show(Resources.SaveBmpImgCantfindBitmapEncoder, "SaveBmpImg()");
                return;
            }
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(path, bmpCodec, encoderParams);
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType) {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }
        #endregion ImageOperations

        private void toolTip1_Popup(object sender, PopupEventArgs e) {

        }

    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Windows;
using System.ComponentModel;
using System.Timers;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using Emgu.CV.Face;
using Emgu.CV.Util;
using Microsoft.Win32;
using System.Drawing.Imaging;

namespace FaceDetectionAndRecognition
{

    public partial class WFFaceRecognition : Window, INotifyPropertyChanged
    {
        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;
        private CascadeClassifier haarCascade;
        private Image<Bgr, Byte> bgrFrame = null;
        private Image<Bgr, Byte> detectedFace = null;
        private List<FaceData> faceList = new List<FaceData>();
        private VectorOfMat imageList = new VectorOfMat();
        private List<string> nameList = new List<string>();
        private VectorOfInt labelList = new VectorOfInt();

        private EigenFaceRecognizer recognizer;
        #region CameraCaptureImage
        private Bitmap cameraCapture;
        public Bitmap CameraCapture
        {
            get { return cameraCapture; }
            set
            {
                cameraCapture = value;
                imgCamera.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { imgCamera.Source = BitmapToImageSource(cameraCapture); }));
                NotifyPropertyChanged();
            }
        }
        #endregion
        #endregion

        #region Event
        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetFacesList();
        }
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            WFAbout wfAbout = new WFAbout();
            wfAbout.ShowDialog();
        }
        
        private void OpenImageFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog().Value == true)
            {
                //captureTimer.Stop();
                //videoCapture.Dispose();

                bgrFrame = new Image<Bgr, byte>(openDialog.FileName);
                ProcessFrame();
                //captureTimer.Start();
                this.Title = openDialog.FileName;
                return;
            }
        }
        #endregion

        #region Method
        public void GetFacesList()
        {
            haarCascade = new CascadeClassifier(Config.HaarCascadePath);
        }

        private void ProcessFrame()
        {
            //bgrFrame = videoCapture.QueryFrame().ToImage<Bgr, Byte>();

            if (bgrFrame != null)
            {
                try
                {//for emgu cv bug
                    Image<Gray, byte> grayframe = bgrFrame.Convert<Gray, byte>();

                    Rectangle[] faces = haarCascade.DetectMultiScale(grayframe, 1.01, 2, new System.Drawing.Size(80, 80), new System.Drawing.Size(1000, 1000));

                    int i = 0;
                    foreach (var face in faces)
                    {
                        i++;
                        detectedFace = bgrFrame.Copy(face).Convert<Bgr, byte>();
                        FaceOutput(detectedFace.ToBitmap(), i);
                        //FaceRecognition();
                        //break;
                    }

                    foreach (var face in faces)
                    {
                        bgrFrame.Draw(face, new Bgr(255, 255, 0), 2);
                    }
                    CameraCapture = bgrFrame.ToBitmap();
                }
                catch (Exception ex)
                {

                    //todo log
                }

            }
        }

        private void FaceOutput(Bitmap personFace, int i) {

            string filename = Config.FacePhotosPath + "person" + i + ".jpg";
            personFace.Save(filename, ImageFormat.Jpeg);

        }

        /// <summary>
        /// Convert bitmap to bitmap image for image control
        /// </summary>
        /// <param name="bitmap">Bitmap image</param>
        /// <returns>Image Source</returns>
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        #endregion


    }
}
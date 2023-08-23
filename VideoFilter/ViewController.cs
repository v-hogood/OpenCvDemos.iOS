using AVFoundation;
using MediaPlayer;
using ObjCRuntime;
using OpenCvSdk;

namespace VideoFilter
{
    public partial class ViewController : UIViewController,
        IUINavigationControllerDelegate,
        IUIImagePickerControllerDelegate,
        ICvVideoCameraDelegate2
    {
        public ViewController(NativeHandle handle) : base(handle) { }

        UIImagePickerController imagePicker;

        bool enableFilmGrain = false;
        bool enableInvert = false;
        bool enableRetro = false;
        bool enableSoftFocus = false;
        bool enableCartoon = false;
        bool enableSepia = false;
        bool enableGray = false;
        bool enableYuv = false;
        bool enableBinary = false;

        bool enableProcessing = false;

        bool hasVideo = false;
        bool videoSaved;

        CvVideoCamera2 videoCamera;

        UIInterfaceOrientation startOrientation;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Do any additional setup after loading the view, typically from a nib.

            this.Title = "VideoFilter";

            this.videoCamera = new CvVideoCamera2(imageView);

            this.videoCamera.Delegate = this;
            this.videoCamera.DefaultAVCaptureDevicePosition = AVCaptureDevicePosition.Back;
            this.videoCamera.DefaultAVCaptureSessionPreset = AVCaptureSession.Preset352x288;
            this.videoCamera.DefaultAVCaptureVideoOrientation = AVCaptureVideoOrientation.Portrait;
            this.videoCamera.DefaultFPS = 30;
            this.videoCamera.GrayscaleMode = false;

            this.videoCamera.RecordVideo = true;

            startOrientation = this.InterfaceOrientation;
            switch (this.InterfaceOrientation)
            {
                case UIInterfaceOrientation.LandscapeLeft:
                default:
                    this.videoCamera.DefaultAVCaptureVideoOrientation = AVCaptureVideoOrientation.LandscapeLeft;
                    break;
                case UIInterfaceOrientation.LandscapeRight:
                    this.videoCamera.DefaultAVCaptureVideoOrientation = AVCaptureVideoOrientation.LandscapeRight;
                    break;
                case UIInterfaceOrientation.Portrait:
                    this.videoCamera.DefaultAVCaptureVideoOrientation = AVCaptureVideoOrientation.Portrait;
                    break;
                case UIInterfaceOrientation.PortraitUpsideDown:
                    this.videoCamera.DefaultAVCaptureVideoOrientation = AVCaptureVideoOrientation.PortraitUpsideDown;
                    break;
            }

            this.UpdateView();
        }

        public override void ViewDidUnload()
        {
#pragma warning disable CA1422
            base.ViewDidUnload();
#pragma warning restore CA1422
            // Release any retained subviews of the main view.
        }

        public bool ShouldAutorotateInterfaceOrientation(UIInterfaceOrientation interfaceOrientation)
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                return (interfaceOrientation == startOrientation);
            }
            else
            {
                return true;
            }
        }

        [Export("actionSepia:")]
        public void ActionSepia(UIBarButtonItem button)
        {
            enableSepia = !enableSepia;
            button.Style = (enableSepia) ? UIBarButtonItemStyle.Done : UIBarButtonItemStyle.Bordered;
        }

        [Export("actionInvert:")]
        public void ActionInvert(UIBarButtonItem button)
        {
            enableInvert = !enableInvert;
            button.Style = (enableInvert) ? UIBarButtonItemStyle.Done : UIBarButtonItemStyle.Bordered;
        }

        [Export("actionRetro:")]
        public void ActionRetro(UIBarButtonItem button)
        {
            enableRetro = !enableRetro;
            button.Style = (enableRetro) ? UIBarButtonItemStyle.Done : UIBarButtonItemStyle.Bordered;
        }

        [Export("actionSoftFocus:")]
        public void ActionSoftFocus(UIBarButtonItem button)
        {
            enableSoftFocus = !enableSoftFocus;
            button.Style = (enableSoftFocus) ? UIBarButtonItemStyle.Done : UIBarButtonItemStyle.Bordered;
        }

        [Export("actionCartoon:")]
        public void ActionCartoon(UIBarButtonItem button)
        {
            enableCartoon = !enableCartoon;
            button.Style = (enableCartoon) ? UIBarButtonItemStyle.Done : UIBarButtonItemStyle.Bordered;
        }

        [Export("actionFilmGrain:")]
        public void ActionFilmGrain(UIBarButtonItem button)
        {
            enableFilmGrain = !enableFilmGrain;
            button.Style = (enableFilmGrain) ? UIBarButtonItemStyle.Done : UIBarButtonItemStyle.Bordered;
        }

        [Export("actionGray:")]
        public void ActionGray(UIBarButtonItem button)
        {
            enableGray = !enableGray;
            button.Style = (enableGray) ? UIBarButtonItemStyle.Done : UIBarButtonItemStyle.Bordered;
        }

        [Export("actionYuv:")]
        public void ActionYuv(UIBarButtonItem button)
        {
            enableYuv = !enableYuv;
            button.Style = (enableYuv) ? UIBarButtonItemStyle.Done : UIBarButtonItemStyle.Bordered;
        }

        [Export("actionBinary:")]
        public void ActionBinary(UIBarButtonItem button)
        {
            enableBinary = !enableBinary;
            button.Style = (enableBinary) ? UIBarButtonItemStyle.Done : UIBarButtonItemStyle.Bordered;
        }

        [Export("actionEnableProcessing:")]
        public void ActionEnableProcessing(UIButton sender)
        {
            enableProcessing = !enableProcessing;
            if (enableProcessing)
            {
                this.videoCamera.Start();
                hasVideo = true;
                videoSaved = false;
            }
            else
            {
                this.videoCamera.Stop();
            }
            this.UpdateView();
        }

        [Export("saveVideo:")]
        public void SaveVideo(UIButton sender)
        {
            if (videoSaved == false)
            {
                this.videoCamera.Stop();
                enableProcessing = false;
            }
            videoSaved = true;

            this.videoCamera.SaveVideo();

            this.PresentMoviePlayerViewController(new MPMoviePlayerViewController(url:this.videoCamera.VideoFileURL));
        }

        // delegate method for processing image frames
        public void ProcessImage(Mat image)
        {
            if (enableCartoon)
            {
                ImageFilterController.CartoonMatConversion(image);
            }
            if (enableInvert)
            {
                ImageFilterController.InverseMatConversion(image);
            }
            if (enableSepia)
            {
                ImageFilterController.SepiaConversion(image);
            }
            if (enableFilmGrain)
            {
                ImageFilterController.FilmGrainConversion(image);
            }
            if (enableRetro)
            {
                ImageFilterController.RetroEffectConversion(image);
            }
            if (enableSoftFocus)
            {
                ImageFilterController.SoftFocusConversion(image);
            }
            if (enableGray)
            {
                ImageFilterController.GrayMatConversion(image);
            }
            if (enableYuv)
            {
                ImageFilterController.YuvMatConversion(image);
            }
            if (enableBinary)
            {
                ImageFilterController.BinaryMatConversion(image, 127);
            }
        }

        [Export("showPhotoLibrary:")]
        public void ShowPhotoLibrary(UIButton sender)
        {
            Console.WriteLine("show photo library");

            this.imagePicker = new UIImagePickerController();
            this.imagePicker.Delegate = this;
#pragma warning disable CA1422
            this.imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            this.PresentModalViewController(imagePicker, true);
#pragma warning restore CA1422
        }

        void UpdateView()
        {
            saveLabel.Hidden = !hasVideo;
            saveButton.Hidden = !hasVideo;
        }

        [Export("imagePickerController:didFinishPickingMediaWithInfo:")]
        void DidFinishPickingMedia(UIImagePickerController picker, NSDictionary info)
        {
            UIImage image = (UIImage)info.ObjectForKey(UIImagePickerController.OriginalImage);
            Mat m_image = new(image);
            this.ProcessImage(m_image);
            image = m_image.ToUIImage();
            this.imageView.Image = image;

            this.imageView.Image.SaveToPhotosAlbum(null);

            picker.DismissViewController(true, null);
        }

        [Export("imagePickerControllerDidCancel:")]
        void ImagePickerControllerDidCancel(UIImagePickerController picker)
        {
            picker.DismissViewController(true, null);
        }
    }
}

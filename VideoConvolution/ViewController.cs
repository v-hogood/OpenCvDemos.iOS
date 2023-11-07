using AVFoundation;
using ObjCRuntime;
using OpenCvSdk;

namespace VideoConvolution
{
    enum OpenCvFilterMode
    {
        FiltermodeBlurHomogeneous,
        FiltermodeBlurGaussian,
        FiltermodeBlurMedian,
        FiltermodeBlurBilateral,
        FiltermodeLaplacian,
        FiltermodeSobel,
        FiltermodeCanny,
        FiltermodeHarris
    };

    public partial class ViewController : UIViewController,
        IUIActionSheetDelegate,
        IUINavigationControllerDelegate,
        IUIImagePickerControllerDelegate,
        ICvVideoCameraDelegate2
    {
        // keep these two arrays in sync
        string[] actionSheetBlurTitles = { "Homogeneous", "Gaussian", "Median", "Bilateral" };
        OpenCvFilterMode[] actionSheetBlurModes =
        {
            OpenCvFilterMode.FiltermodeBlurHomogeneous,
            OpenCvFilterMode.FiltermodeBlurGaussian,
            OpenCvFilterMode.FiltermodeBlurMedian,
            OpenCvFilterMode.FiltermodeBlurBilateral,
        };

        // keep these two arrays in sync, don't forget the final nil
        string[] actionSheetEdgeTitles = { "Laplacian", "Sobel", "Canny" };
        OpenCvFilterMode[] actionSheetEdgeModes =
        {
            OpenCvFilterMode.FiltermodeLaplacian,
            OpenCvFilterMode.FiltermodeSobel,
            OpenCvFilterMode.FiltermodeCanny,
        };

        public ViewController(NativeHandle handle) : base(handle) { }

        public ViewController(string nibName, NSBundle bundle) : base(nibName, bundle) { }

        UIImagePickerController imagePicker;

        bool enableProcessing;

        CvVideoCamera2 videoCamera;

        OpenCvFilterMode mode;
	    int kernelSize;
        int threshold;

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (enableProcessing)
            {
                this.videoCamera.Start();
            }
            else
            {
                this.videoCamera.Stop();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            this.videoCamera.Stop();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            this.Title = "VideoConvolution";

            this.videoCamera = new CvVideoCamera2(imageView);
            this.videoCamera.Delegate = this;
            this.videoCamera.DefaultAVCaptureDevicePosition = AVCaptureDevicePosition.Front;
            this.videoCamera.DefaultAVCaptureSessionPreset = AVCaptureSession.Preset352x288;
            this.videoCamera.DefaultAVCaptureVideoOrientation = AVCaptureVideoOrientation.Portrait;
            this.videoCamera.DefaultFPS = 30;
            this.videoCamera.GrayscaleMode = true;

#pragma warning disable CA1422
            this.actionSheetBlurFilters = new UIActionSheet(title:"Blur Filters", this, cancelTitle:"Cancel", destroy:null, other:null);
            for (int i = 0; i < actionSheetBlurTitles.Length; i++)
            {
                this.actionSheetBlurFilters.AddButton(title:actionSheetBlurTitles[i]);
            }

            this.actionSheetEdgeFilters = new UIActionSheet(title:"Edge Filters", this, cancelTitle:"Cancel", destroy:null, other:null);
            for (int i = 0; i < actionSheetEdgeTitles.Length; i++)
            {
                this.actionSheetEdgeFilters.AddButton(title:actionSheetEdgeTitles[i]);
            }
#pragma warning restore CA1422
            enableProcessing = false;

            mode = OpenCvFilterMode.FiltermodeBlurHomogeneous;
            kernelSize = 3;
            threshold = 1;

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
            return true;
        }

        public void ProcessImage(Mat image)
        {
            switch (mode)
            {
                case OpenCvFilterMode.FiltermodeBlurHomogeneous:
                    ConvolutionController.FilterBlurHomogeneous(image, kernelSize);
                    break;

                case OpenCvFilterMode.FiltermodeBlurGaussian:
                    ConvolutionController.FilterBlurGaussian(image, kernelSize);
                    break;

                case OpenCvFilterMode.FiltermodeBlurMedian:
                    ConvolutionController.FilterBlurMedian(image, kernelSize);
                    break;

                case OpenCvFilterMode.FiltermodeBlurBilateral:
                    ConvolutionController.FilterBlurBilateral(image, kernelSize);
                    break;

                case OpenCvFilterMode.FiltermodeLaplacian:
                    ConvolutionController.FilterLaplace(image, kernelSize);
                    break;

                case OpenCvFilterMode.FiltermodeSobel:
                    ConvolutionController.FilterSobel(image, kernelSize);
                    break;

                case OpenCvFilterMode.FiltermodeCanny:
                    ConvolutionController.FilterCanny(image, kernelSize:3, lowThreshold:threshold);
                    break;

                default:
                    break;
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

        [Export("sliderValueChanged:")]
        public void SliderValueChanged(UIButton sender)
        {
            int new_value;
            switch (mode)
            {
                case OpenCvFilterMode.FiltermodeBlurBilateral:
                case OpenCvFilterMode.FiltermodeBlurHomogeneous:
                case OpenCvFilterMode.FiltermodeBlurGaussian:
                case OpenCvFilterMode.FiltermodeBlurMedian:
                case OpenCvFilterMode.FiltermodeLaplacian:
                case OpenCvFilterMode.FiltermodeSobel:
                    new_value = (int)(((slider.Value + 1.0) / 2.0) * 2.0);
                    if (new_value % 2 == 0)
                    {
                        new_value--;
                    }
                    slider.SetValue(new_value, animated:false);
                    kernelSize = (int)slider.Value;

                    break;
                case OpenCvFilterMode.FiltermodeCanny:
                    new_value = (int)slider.Value;
                    threshold = new_value;

                    break;
                default:
                    break;
            }

            this.UpdateView();
        }

        [Export("switchProcessingOnOff:")]
        public void SwitchProcessingOnOff(UIButton sender)
        {
            enableProcessing = !enableProcessing;

            if (enableProcessing)
            {
                videoCamera.Start();
            }
            else
            {
                videoCamera.Stop();
            }
        }

        [Export("switchCamera:")]
        public void SwitchCamera(UIButton sender)
        {
            this.videoCamera.SwitchCameras();
        }

        [Export("showBlurFilters:")]
#pragma warning disable CA1422
        public void ShowBlurFilters(UIButton sender)
        {
            this.actionSheetBlurFilters.ShowInView(this.View);
        }

        [Export("showEdgeFilters:")]
        public void ShowEdgeFilters(UIButton sender)
        {
            this.actionSheetEdgeFilters.ShowInView(this.View);
        }

        [Export("actionSheet:clickedButtonAtIndex:")]
        public void Clicked(UIActionSheet actionSheet, nint buttonIndex)
        {
            if (actionSheet.CancelButtonIndex == buttonIndex)
            {
                return;
            }
#pragma warning restore CA1422
            if (actionSheet == this.actionSheetBlurFilters)
            {
                mode = actionSheetBlurModes[buttonIndex - 1];
            }
            else if (actionSheet == this.actionSheetEdgeFilters)
            {
                mode = actionSheetEdgeModes[buttonIndex - 1];
            }
            this.UpdateView();
        }

        void UpdateView()
        {
            if (mode == OpenCvFilterMode.FiltermodeCanny)
            {
                this.slider.MinValue = 0;
                this.slider.MaxValue = 100;
                this.slider.Value = threshold;
                this.sliderTitleItem.Title = @"Thresh";
                this.sliderValueItem.Title = "" + threshold;
            }
            else
            {
                this.slider.MinValue = 1;
                this.slider.MaxValue = 31;
                this.slider.Value = kernelSize;
                this.sliderTitleItem.Title = "Kernel";
                this.sliderValueItem.Title = "" + kernelSize;
            }
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

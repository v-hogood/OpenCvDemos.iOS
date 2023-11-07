using AVFoundation;
using OpenCvSdk;

namespace FaceDetectVideo;

public partial class ViewController : UIViewController,
	ICvVideoCameraDelegate2
{
    public ViewController(string nibName, NSBundle bundle) : base(nibName, bundle) { }

    CvVideoCamera2 videoCamera;

	UISlider sliderFPS;

	bool enableProcessing;

	CvFaceDetector cvFaceDetector;
	UIImagePickerController imagePicker;

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

		this.Title = "FaceDetectVideo";

		this.cvFaceDetector = new CvFaceDetector();

		this.videoCamera = new CvVideoCamera2(parent: this.imageView);
		this.videoCamera.Delegate = this;
		this.videoCamera.DefaultAVCaptureDevicePosition = AVCaptureDevicePosition.Back;
		this.videoCamera.DefaultAVCaptureVideoOrientation = AVCaptureVideoOrientation.Portrait;
		this.videoCamera.DefaultAVCaptureSessionPreset = AVCaptureSession.Preset352x288;
		this.videoCamera.DefaultFPS = 15;
		this.videoCamera.GrayscaleMode = true;

		enableProcessing = false;
	}

	public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation interfaceOrientation)
	{
		if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
		{
			return interfaceOrientation == UIInterfaceOrientation.Portrait;
		}
		else
		{
			return false;
		}
	}

	[Export("changeFPS:")]
	void ChangeFPS(UISlider slider)
	{
		Console.WriteLine("IBAction changeFPS");

		bool started = this.videoCamera.Running;

		if (started)
		{
			this.videoCamera.Stop();
		}
		this.videoCamera.DefaultFPS = (int)(5 + 10 * slider.Value);
		if (started)
		{
			this.videoCamera.Start();
		}
		this.labelFPS.Text = this.videoCamera.DefaultFPS.ToString();
	}

	[Export("switchProcessingOnOff:")]
	void SwitchProcessingOnOff(UIButton sender)
	{
		enableProcessing = !enableProcessing;
		if (enableProcessing)
		{
			this.videoCamera.Start();
		}
		else
		{
			this.videoCamera.Stop();
		}
	}

	[Export("switchCamera:")]
	public void SwitchCamera(UIButton sender)
	{
		this.videoCamera.SwitchCameras();
	}

	[Export("showCameraImage:")]
	void ShowCameraImage(UIButton sender)
	{
		this.imagePicker = new();
		this.imagePicker.Delegate = this;
		this.imagePicker.SourceType = UIImagePickerControllerSourceType.Camera;
#pragma warning disable CA1422
		this.PresentModalViewController(imagePicker, true);
#pragma warning restore CA1422
	}

	[Export("showPhotoLibrary:")]
	void ShowPhotoLibrary(UIButton sender)
	{
		this.imagePicker = new();
		this.imagePicker.AllowsEditing = true;
		this.imagePicker.Delegate = this;
#pragma warning disable CA1422
		this.imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
		this.PresentModalViewController(imagePicker, true);
#pragma warning restore CA1422
	}

	[Export("showVideoCamera:")]
	void ShowVideoCamera(UIBarButtonItem button)
	{
		Console.WriteLine("show video camera");

		if (this.videoCamera.Running)
		{
			this.videoCamera.Stop();
			button.Title = "Start Cam";
		}
		else
		{
			this.videoCamera.Start();
			button.Title = "Stop Cam";
		}
	}

	public void ProcessImage(Mat image)
	{
		Console.WriteLine(@"Detecting faces...");
		cvFaceDetector.DetectFacesInMat(image);
		Console.WriteLine("done.");
	}

	[Export("imagePickerController:didFinishPickingMediaWithInfo:")]
	void DidFinishPickingMedia(UIImagePickerController picker, NSDictionary info)
	{
		UIImage image = (UIImage)info.ObjectForKey(UIImagePickerController.EditedImage);
		if (image == null)
			image = (UIImage)info.ObjectForKey(UIImagePickerController.OriginalImage);
        Mat m_image = new((int)image.Size.Height, (int)image.Size.Width, CvType.Cv8uc1);
		Imgproc.CvtColor(new(image), m_image, ColorConversionCodes.Bgra2gray);
		this.ProcessImage(m_image);
		image = m_image.ToUIImage();
		this.imageView.Image = image;

		picker.DismissViewController(true, null);
	}

	[Export("imagePickerControllerDidCancel:")]
	void ImagePickerControllerDidCancel(UIImagePickerController picker)
	{
		picker.DismissViewController(true, null);
	}
}

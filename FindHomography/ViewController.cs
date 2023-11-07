using AVFoundation;
using ObjCRuntime;
using OpenCvSdk;
using static FindHomography.CvHomographyController;

namespace FindHomography;

public partial class ViewController : UIViewController,
    ICvVideoCameraDelegate2,
    IUINavigationControllerDelegate,
    IUIActionSheetDelegate
{
    public ViewController(NativeHandle handle) : base(handle) { }

    CvVideoCamera2 videoCamera;

    CvHomographyController homographyController;

    bool enableProcessing;

    UIActionSheet actionSheetDetectors;
    UIActionSheet actionSheetDescriptors;

    CVFeatureDetectorType detector;
	CVFeatureDescriptorType descriptor;

	// keep these two arrays in sync
	string[] actionSheetDetectorTitles = { "FAST", "GDT", "MSER", "ORB", "SIFT" };
	CVFeatureDetectorType[] actionSheetDetectorTypes =
	{
		CVFeatureDetectorType.CV_FEATUREDETECTOR_FAST,
		CVFeatureDetectorType.CV_FEATUREDETECTOR_GOODTOTRACK,
		CVFeatureDetectorType.CV_FEATUREDETECTOR_MSER,
		CVFeatureDetectorType.CV_FEATUREDETECTOR_ORB,
		CVFeatureDetectorType.CV_FEATUREDETECTOR_SIFT,
	};

	// keep these two arrays in sync, don't forget the final nil
	string[] actionSheetDescriptorTitles = { "ORB", "SIFT" };
	CVFeatureDescriptorType[] actionSheetDescriptorTypes =
	{
		CVFeatureDescriptorType.CV_FEATUREDESCRIPTOR_ORB,
		CVFeatureDescriptorType.CV_FEATUREDESCRIPTOR_SIFT,
	};

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
	
		this.Title = "FindHomography";

        this.homographyController = CvHomographyController.SharedInstance();

        this.videoCamera = new CvVideoCamera2(imageView);
        this.videoCamera.Delegate = this;
        this.videoCamera.DefaultAVCaptureDevicePosition = AVCaptureDevicePosition.Back;
        this.videoCamera.DefaultAVCaptureSessionPreset = AVCaptureSession.Preset352x288;
        this.videoCamera.DefaultAVCaptureVideoOrientation = AVCaptureVideoOrientation.Portrait;
        this.videoCamera.DefaultFPS = 15;
        this.videoCamera.GrayscaleMode = true;

#pragma warning disable CA1422
        this.actionSheetDetectors = new(title: "Detector", del: this, cancelTitle: "Cancel", destroy: null, other: null);
		for (int i = 0; i < actionSheetDetectorTitles.Length; i++)
		{
			this.actionSheetDetectors.AddButton(title: actionSheetDetectorTitles[i]);
		}
	
		this.actionSheetDescriptors = new(title: "Descriptor", del: this, cancelTitle: "Cancel", destroy: null, other: null);
		for (int i = 0; i < actionSheetDescriptorTitles.Length; i++)
		{
			this.actionSheetDescriptors.AddButton(title: actionSheetDescriptorTitles[i]);
		}
#pragma warning restore CA1422

        enableProcessing = false;

        this.imageViewObject.Layer.BorderColor = UIColor.Black.CGColor;
        this.imageViewObject.Layer.BorderWidth = 1.5f;

        if (this.imageViewScene != null)
        {
            this.imageViewScene.Layer.BorderColor = UIColor.Black.CGColor;
            this.imageViewScene.Layer.BorderWidth = 1.5f;
        }

        /*
		float rotationAngle = 0.0f;
		CGRect bounds = this.imageView.Bounds;
		CALayer layer = this.imageView.Layer;
	
		rotationAngle = 3 * (float)Math.PI/ 2;
		bounds = new(0, 0, bounds.Size.Height, bounds.Size.Width);
	
		//layer.position = CGPointMake(bounds.size.width/2., bounds.size.height/2.);
		layer.AffineTransform = CGAffineTransform.MakeRotation(rotationAngle);
		layer.Bounds = bounds;
		*/

		this.UpdateView();
	}

    bool ShouldAutorotateInterfaceOrientation(UIInterfaceOrientation interfaceOrientation)
    {
        if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
        {
            return interfaceOrientation == UIInterfaceOrientation.Portrait;
        }
        else
        {
            return true;
        }
    }

	void UpdateView()
	{
		this.imageView.Hidden = !enableProcessing;
		this.labelObject.Hidden = !enableProcessing || this.imageViewObject.Image != null;
		this.labelScene.Hidden = !enableProcessing || this.imageViewScene?.Image != null;
        /*
            this.slider.Hidden = !enableProcessing;
            this.labelSlider.Hidden = !enableProcessing;
            this.labelMin.Hidden = !enableProcessing;
            this.labelMax.Hidden = !enableProcessing;
        */

        this.labelSlider.Text = String.Format("%s: %2.2f", this.homographyController.GetDetectorThresholdName(), this.homographyController.GetDetectorThreshold());
        this.labelMin.Text = String.Format("%3.1f", this.homographyController.thresh_min);
        this.labelMax.Text = String.Format("%3.1f", this.homographyController.thresh_max);
        this.slider.MinValue = this.homographyController.thresh_min;
        this.slider.MaxValue = this.homographyController.thresh_max;
        this.slider.Value = this.homographyController.GetDetectorThreshold();

        this.processingSwitch.Enabled = this.homographyController.object_loaded == true;
	
		if (this.homographyController.object_loaded)
		{
            this.imageViewObject.Image = this.homographyController.ObjectImage;
		}
	
		Console.WriteLine("Slider value: " + this.homographyController.GetDetectorThreshold());
	}

    [Export("changeSlider:")]
    void ChangeSlider(UISlider sender)
    {
        /*
        this.homographyController.DetectorThreshold = this.slider.Value;
        this.UpdateView();
        */

        /*
        bool started = self.videoCamera.Running;

        UISlider slider = (UISlider)sender;
        if (started)
		{
            this.videoCamera.Stop();
        }
        this.videoCamera.Start();
        */
    }

    [Export("switchProcessingOnOff:")]
    void SwitchProcessingOnOff(UISwitch sender)
    {
        enableProcessing = !enableProcessing;
		if (enableProcessing)
		{
			this.videoCamera.Start();
			this.imageView.Hidden = false;
		}
		else
		{
			this.videoCamera.Stop();
			this.imageView.Hidden = true;
		}
	}

    [Export("switchCamera:")]
    void SwitchCamera(UIBarButtonItem button)
    {
		this.videoCamera.SwitchCameras();
		if (this.videoCamera.DefaultAVCaptureDevicePosition == AVCaptureDevicePosition.Front)
		{
			button.Title = "Back";
		}
		else
		{
			button.Title = "Front";
		}
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

#pragma warning disable CA1422
    [Export("showDetectors:")]
    void ShowDetectors(UIButton button)
	{
        this.actionSheetDetectors.ShowInView(this.View);
    }

    [Export("showDescriptors:")]
    void ShowDescriptors(UIButton button)
    {
        this.actionSheetDescriptors.ShowInView(this.View);
    }
#pragma warning restore CA1422

    [Export("actionSheet:clickedButtonAtIndex:")]
    void Clicked(UIActionSheet actionSheet, nint buttonIndex)
    {
        Console.WriteLine("button index: " + buttonIndex);
#pragma warning disable CA1422
        if (actionSheet.CancelButtonIndex == buttonIndex)
#pragma warning restore CA1422
        {
            return;
        }

        bool wasRunning = this.videoCamera.Running;
        this.videoCamera.Stop();

        if (actionSheet == this.actionSheetDetectors)
        {
            detector = actionSheetDetectorTypes[buttonIndex - 1];
			this.homographyController.SetDetector(detector);
        }
        else if (actionSheet == this.actionSheetDescriptors)
        {
            descriptor = actionSheetDescriptorTypes[buttonIndex - 1];
			this.homographyController.SetDescriptor(descriptor);
        }

		if (wasRunning)
		{
            this.videoCamera.Start();
        }

        this.UpdateView();
    }

	public void ProcessImage(Mat image)
	{
		if (enableProcessing)
		{
			Console.WriteLine("Processing (matching)...");
			this.homographyController.SetSceneImage(image);
            this.homographyController.Detect();
            this.homographyController.Descript();
            this.homographyController.Match();
            this.homographyController.DrawScene();
            Console.WriteLine("done.");
		}
	}
}

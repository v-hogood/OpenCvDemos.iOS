using ObjCRuntime;

namespace FindHomography;

public partial class ViewControllerLoadObject : UIViewController,
        IUIImagePickerControllerDelegate,
        IUINavigationControllerDelegate
{
    public ViewControllerLoadObject(NativeHandle handle) : base(handle) { }

    public ViewControllerLoadObject(string nibName, NSBundle bundle) : base(nibName, bundle) { }

    CvHomographyController homographyController;

    bool objectLoaded;

    UIImagePickerController imagePicker;

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
	
		this.Title = "Load Object";

        this.homographyController = CvHomographyController.SharedInstance();

        this.imageViewObject.Layer.BorderColor = UIColor.Black.CGColor;
		this.imageViewObject.Layer.BorderWidth = (float)1.5;
	
		objectLoaded = false;
	
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
		this.buttonContinue.Enabled = objectLoaded == true;
	
		Console.WriteLine("Slider value: " + this.homographyController.GetDetectorThreshold());
	}

	[Export("showCameraImage:")]
	void ShowCameraImage(UIButton sender)
	{
        this.imagePicker = new UIImagePickerController();
        this.imagePicker.Delegate = this;
#pragma warning disable CA1422
        this.imagePicker.SourceType = UIImagePickerControllerSourceType.Camera;
        this.PresentModalViewController(imagePicker, true);
#pragma warning restore CA1422
    }

    [Export("showPhotoLibrary:")]
    void ShowPhotoLibrary(UIButton sender)
    {
        this.imagePicker = new UIImagePickerController();
        this.imagePicker.Delegate = this;
#pragma warning disable CA1422
        this.imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
        this.PresentModalViewController(imagePicker, true);
#pragma warning restore CA1422
    }

    [Export("imagePickerController:didFinishPickingMediaWithInfo:")]
    void DidFinishPickingMedia(UIImagePickerController picker, NSDictionary info)
    {
        UIImage image = (UIImage)info.ObjectForKey(UIImagePickerController.OriginalImage);
        CGSize desiredSize;
        if (image.Size.Width > image.Size.Height)
        {
            desiredSize = new CGSize(352, 288);
        }
        else
        {
            desiredSize = new CGSize(288, 352);
        }
        image = image.Scale(desiredSize);
        Console.WriteLine("imagePickerController didFinish: image info [w,h] = [{0},{1}]", image.Size.Width, image.Size.Height);

        this.homographyController.Reset();
        this.homographyController.SetObjectImage(image);
        this.imageViewObject.Image = this.homographyController.ObjectImage;

        this.homographyController.Match();
        this.homographyController.DrawObject();
        this.imageViewObject.Image = this.homographyController.MatchImage.ToUIImage();

        objectLoaded = true;

        picker.DismissViewController(true, null);

        this.UpdateView();
    }

    [Export("imagePickerControllerDidCancel:")]
    void ImagePickerControllerDidCancel(UIImagePickerController picker)
    {
        picker.DismissViewController(true, null);
    }
}
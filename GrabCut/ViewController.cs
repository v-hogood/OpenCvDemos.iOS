using CoreFoundation;
using MobileCoreServices;
using ObjCRuntime;

namespace GrabCut
{
    public partial class ViewController : UIViewController,
        IUINavigationControllerDelegate,
        IUIImagePickerControllerDelegate
    {
        public ViewController(NativeHandle handle) : base(handle) { }

        UIImagePickerController imagePicker;

        GrabCutController grabCutController;

        bool image_changed;
        bool edit_fg;

        float scale_x;
        float scale_y;

        public void CalculateScale()
        {
            Console.WriteLine("imageView bounds: {0},{1}", imageView.Bounds.Size.Width, imageView.Bounds.Size.Height);
            Console.WriteLine("imageView.image bounds: {0},{1}", imageView.Image.Size.Width, imageView.Image.Size.Height);
    
            scale_x = (float)(imageView.Image.Size.Width / imageView.Bounds.Size.Width);
            scale_y = (float)(imageView.Image.Size.Height / imageView.Bounds.Size.Height);
    
            Console.WriteLine("scale_x: {0}", scale_x);
            Console.WriteLine("scale_y: {0}", scale_y);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Do any additional setup after loading the view, typically from a nib.

            this.Title = "GrabCut";

            edit_fg = true;

            imageView.ExclusiveTouch = true;

            this.imagePicker = new UIImagePickerController();
            this.imagePicker.AllowsEditing = false;
            this.imagePicker.Delegate = this;
#pragma warning disable CA1422
            this.imagePicker.MediaTypes = new string[] { UTType.Image };
            if (UIImagePickerController.IsSourceTypeAvailable(
                UIImagePickerControllerSourceType.PhotoLibrary))
            {
                this.imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            }
#pragma warning restore CA1422

            grabCutController = new GrabCutController();

            grabCutController.SetImage(imageView.Image);
            this.CalculateScale();
            image_changed = false;

            // Gestures
            UITapGestureRecognizer tapGesture = new UITapGestureRecognizer(target: this, action:new Selector("handleTapGesture:"));
            tapGesture.NumberOfTapsRequired = 1;
            imageView.AddGestureRecognizer(tapGesture);

            UIPanGestureRecognizer panGesture = new UIPanGestureRecognizer(target: this, action:new Selector("handlePanGesture:"));
            imageView.AddGestureRecognizer(panGesture);

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
                return (interfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
            }
            else
            {
                return true;
            }
        }

        [Export("imagePickerController:didFinishPickingMediaWithInfo:")]
        void DidFinishPickingMedia(UIImagePickerController picker, NSDictionary info)
        {
            UIImage img = (UIImage)info.ObjectForKey(UIImagePickerController.OriginalImage);
            imageView.Image = img;
            grabCutController.SetImage(img);
            image_changed = false;
            picker.DismissViewController(true, null);
            picker = null;
            this.CalculateScale();
        }

        [Export("imagePickerControllerDidCancel:")]
        void ImagePickerControllerDidCancel(UIImagePickerController picker)
        {
            picker.DismissViewController(true, null);
            picker = null;
        }

        [Export("actionShowPhotoLibrary:")]
        public void ShowPhotoLibrary(UIButton sender)
        {
#pragma warning disable CA1422
            this.PresentModalViewController(this.imagePicker, animated:true);
#pragma warning restore CA1422
        }

        [Export("actionEdit:")]
        public void Edit(UIButton sender)
        {
            this.Editing = !this.Editing;
            this.UpdateView();
        }

        [Export("actionSave:")]
        public void Save(UIButton sender)
        {
            if (image_changed)
            {
                grabCutController.GetSaveImage().SaveToPhotosAlbum(null);
                image_changed = false;
                this.UpdateView();
            }
        }

        [Export("actionToggle:")]
        public void Toggle(UIButton sender)
        {
            edit_fg = !edit_fg;
            if (edit_fg)
            {
                buttonToggle.Title = "FG";
            }
            else
            {
                buttonToggle.Title = "BG";
            }
        }

        [Export("actionGrabCut:")]
        public void GrabCut(UIButton sender)
        {
            this.IndicateActivity(true);

            this.PerformSelector(new Selector("actionGrabCutIteration:"), NSThread.MainThread, withObject:null, waitUntilDone:false);
        }

        [Export("actionGrabCutIteration:")]
        public void GrabCutIteration(UIButton sender)
        {
            DispatchQueue.GetGlobalQueue(DispatchQueuePriority.Default).DispatchAsync(() =>
            {
                grabCutController.NextIteration();
                this.PerformSelector(new Selector("grabCutDone:"), NSThread.MainThread, withObject:null, waitUntilDone:false);
            });
        }

        [Export("grabCutDone:")]
        public void GrabCutDone(UIButton sender)
        {
            label.Text = "Iteration " + grabCutController.IterCount;

            image_changed = true;

            imageView.Image = grabCutController.GetImage();
            this.UpdateView();

            this.IndicateActivity(false);
        }

        public void UpdateView()
        {
            if (this.Editing)
            {
                buttonEdit.Style = UIBarButtonItemStyle.Done;
                imageView.Image = grabCutController.GetImage();
            }
            else
            {
#pragma warning disable CA1422
                buttonEdit.Style = UIBarButtonItemStyle.Bordered;
#pragma warning restore CA1422
                imageView.Image = grabCutController.GetSaveImage();
            }
    
            if (image_changed)
            {
                buttonSave.Style = UIBarButtonItemStyle.Done;
            }
            else
            {
#pragma warning disable CA1422
                buttonSave.Style = UIBarButtonItemStyle.Bordered;
#pragma warning restore CA1422
            }

            this.IndicateActivity(grabCutController.Processing);
    
            buttonToggle.Enabled = this.Editing;
        }

        [Export("handleTapGesture:")]
        public void HandleTapGesture(UIGestureRecognizer sender)
        {
            if (!this.Editing)
                return;
    
            CGPoint tapPoint = sender.LocationInView(sender.View.Superview);
            Console.WriteLine("tap ({0},{1])", tapPoint.X, tapPoint.Y);
            Console.WriteLine("->  ({0},{1})", tapPoint.X * scale_x, tapPoint.Y * scale_y);
            tapPoint = new CGPoint(tapPoint.X * scale_x, tapPoint.Y * scale_y);

            grabCutController.MaskLabel(tapPoint, edit_fg);
    
            imageView.Image = grabCutController.GetImage();
        }

        [Export("handlePanGesture:")]
        public void HandlePanGesture(UIGestureRecognizer sender)
        {
            if (!this.Editing)
                return;

            CGPoint tapPoint = sender.LocationInView(sender.View.Superview);
            Console.WriteLine("tap ({0},{1})", tapPoint.X, tapPoint.Y);

            tapPoint = new CGPoint(tapPoint.X * scale_x, tapPoint.Y * scale_y);

            grabCutController.MaskLabel(tapPoint, edit_fg);

            imageView.Image = grabCutController.GetImage();
        }

        public void IndicateActivity(bool active)
        {
            buttonEdit.Enabled = !active;
            buttonToggle.Enabled = !active;
            buttonGrabCut.Enabled = !active;
            buttonSave.Enabled = !active;

            activityView.Hidden = !active;
            activityIndicatorView.Hidden = !active;
            activityLabel.Hidden = !active;

            if (active)
            {
                activityIndicatorView.StartAnimating();
            }
            else
            {
                activityIndicatorView.StopAnimating();
            }
        }
    }
}

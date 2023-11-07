using UIKit;

namespace FaceDetectVideo
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register ("AppDelegate")]
    public class AppDelegate : UIResponder, IUIApplicationDelegate
    {
        [Export ("window")]
        public UIWindow Window { get; set; }

        ViewController ViewController;

        [Export ("application:didFinishLaunchingWithOptions:")]
        public bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method
            this.Window = new UIWindow(frame: UIScreen.MainScreen.Bounds);
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                this.ViewController = new ViewController(nibName: "ViewController_iPhone", bundle: null);
            }
            else
            {
                this.ViewController = new ViewController(nibName: "ViewController_iPad", bundle: null);
            }
            this.Window.RootViewController = this.ViewController;
            this.Window.MakeKeyAndVisible();
            return true;
        }
    }
}

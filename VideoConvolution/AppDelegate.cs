using UIKit;

namespace VideoConvolution
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register ("AppDelegate")]
    public class AppDelegate : UIResponder, IUIApplicationDelegate
    {
        [Export ("window")]
        public UIWindow Window { get; set; }

        ViewController viewController;

        [Export ("application:didFinishLaunchingWithOptions:")]
        public bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
        {
            this.Window = new UIWindow(UIScreen.MainScreen.Bounds);
            // Override point for customization after application launch.
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                this.viewController = new ViewController(nibName:"ViewController_iPhone", bundle:null);
            } else
            {
                this.viewController = new ViewController(nibName:"ViewController_iPad", bundle:null);
            }
            this.Window.RootViewController = this.viewController;
            this.Window.MakeKeyAndVisible();
            return true;
        }
    }
}

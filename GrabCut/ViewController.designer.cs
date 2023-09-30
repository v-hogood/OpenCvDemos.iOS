// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System.CodeDom.Compiler;

namespace GrabCut
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIImageView imageView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIBarButtonItem buttonEdit { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIBarButtonItem buttonGrabCut { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIBarButtonItem buttonSave { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIBarButtonItem buttonToggle { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UILabel label { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIView activityView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIActivityIndicatorView activityIndicatorView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UILabel activityLabel { get; set; }

        void ReleaseDesignerOutlets()
        {
            imageView?.Dispose();
            imageView = null;

            buttonEdit?.Dispose();
            buttonEdit = null;

            buttonGrabCut?.Dispose();
            buttonGrabCut = null;

            buttonSave?.Dispose();
            buttonSave = null;

            buttonToggle?.Dispose();
            buttonToggle = null;

            label?.Dispose();
            label = null;

            activityView?.Dispose();
            activityView = null;

            activityIndicatorView?.Dispose();
            activityIndicatorView = null;

            activityLabel?.Dispose();
            activityLabel = null;
        }
    }
}

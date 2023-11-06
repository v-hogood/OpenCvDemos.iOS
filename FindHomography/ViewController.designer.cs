// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System.CodeDom.Compiler;

namespace FindHomography
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIImageView imageView { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIImageView imageViewObject { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIImageView imageViewScene { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UILabel labelObject { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UILabel labelScene { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UILabel labelMin { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UILabel labelMax { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UILabel labelSlider { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UISlider slider { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UISwitch processingSwitch { get; set; }

        void ReleaseDesignerOutlets()
        {
            imageView?.Dispose();
            imageView = null;

            imageViewObject?.Dispose();
            imageViewObject = null;

            imageViewScene?.Dispose();
            imageViewScene = null;

            labelObject?.Dispose();
            labelObject = null;

            labelScene?.Dispose();
            labelScene = null;

            labelMin?.Dispose();
            labelMin = null;

            labelMax?.Dispose();
            labelMax = null;

            labelSlider.Dispose();
            labelSlider = null;

            slider?.Dispose();
            slider = null;

            processingSwitch?.Dispose();
            processingSwitch = null;
        }
    }
}

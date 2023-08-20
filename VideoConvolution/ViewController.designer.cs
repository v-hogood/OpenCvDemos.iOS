// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System.CodeDom.Compiler;

namespace VideoConvolution
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIImageView imageView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UISlider slider { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIBarButtonItem sliderTitleItem { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIBarButtonItem sliderValueItem { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIActionSheet actionSheetBlurFilters { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIActionSheet actionSheetEdgeFilters { get; set; }

        void ReleaseDesignerOutlets()
        {
            imageView?.Dispose();
            imageView = null;

            slider?.Dispose();
            slider = null;

            sliderTitleItem?.Dispose();
            sliderTitleItem = null;

            sliderValueItem?.Dispose();
            sliderValueItem = null;

            actionSheetBlurFilters?.Dispose();
            actionSheetBlurFilters = null;

            actionSheetEdgeFilters?.Dispose();
            actionSheetEdgeFilters = null;
        }
    }
}

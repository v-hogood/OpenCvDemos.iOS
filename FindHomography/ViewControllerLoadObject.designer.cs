// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System.CodeDom.Compiler;

namespace FindHomography
{
    [Register ("ViewControllerLoadObject")]
    partial class ViewControllerLoadObject
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIImageView imageViewObject { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIButton buttonContinue { get; set; }

        void ReleaseDesignerOutlets()
        {
            imageViewObject?.Dispose();
            imageViewObject = null;

            buttonContinue?.Dispose();
            buttonContinue = null;
        }
    }
}

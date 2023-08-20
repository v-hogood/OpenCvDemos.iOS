using OpenCvSdk;

namespace VideoConvolution
{
	public static class ConvolutionController
	{
		public static void FilterBlurHomogeneous(Mat image, int kernelSize)
		{
			// process pixel buffer before rendering
			Mat dst = image;
			Imgproc.Blur(image, dst, new Size2i(kernelSize, kernelSize), new Point2i(-1,-1));
		}

		public static void FilterBlurGaussian(Mat image, int kernelSize)
		{
			Mat dst = image;
			Imgproc.GaussianBlur(image, dst, new Size2i(kernelSize, kernelSize), 0, 0);
		}

		public static void FilterBlurMedian(Mat image, int kernelSize)
		{
			Mat dst = image;
			Imgproc.MedianBlur(image, dst, kernelSize);
		}

		public static void FilterBlurBilateral(Mat image, int kernelSize)
		{
			int width = image.Cols;
			int height = image.Rows;
			int bytesPerRow = (int)image.Step1(0);
	
			// we need to copy because src.data != dst.data must hold with bilateral filter
			NSData dataCopy = NSData.FromBytes(image.DataPtr, (nuint)(bytesPerRow * height));
		
			Mat src = new(height, width, CvType.Cv8uc1, dataCopy, bytesPerRow);
	
			Imgproc.BilateralFilter(src, image, kernelSize, kernelSize*2, kernelSize/2);
		}

        // http://docs.opencv.org/doc/tutorials/imgproc/imgtrans/laplace_operator/laplace_operator.html#laplace-operator
        public static void FilterLaplace(Mat image, int kernelSize)
		{
            int scale = 1;
            int delta = 0;
            int ddepth = CvType.Cv16s;

            FilterBlurGaussian(image, kernelSize:3);

            int width = image.Cols;
            int height = image.Rows;
            int bytesPerRow = (int)image.Step1(0);

            // we need to copy because src.data != dst.data must hold with bilateral filter
            NSData dataCopy = NSData.FromBytes(image.DataPtr, (nuint)(bytesPerRow * height));

            Mat src = new(height, width, CvType.Cv8uc1, dataCopy, bytesPerRow);
            Mat tmp = new();

            Imgproc.Laplacian(src, tmp, ddepth, kernelSize, scale, delta, BorderTypes.Default);
            Core.ConvertScaleAbs(tmp, image);
        }

        // http://docs.opencv.org/doc/tutorials/imgproc/imgtrans/sobel_derivatives/sobel_derivatives.html#sobel-derivatives
        public static void FilterSobel(Mat image, int kernelSize)
		{
            int scale = 1;
            int delta = 0;
            int ddepth = CvType.Cv16s;

            FilterBlurGaussian(image, kernelSize:kernelSize);

            int width = image.Cols;
            int height = image.Rows;
            int bytesPerRow = (int)image.Step1(0);

            // we need to copy because src.data != dst.data must hold with bilateral filter
            NSData dataCopy = NSData.FromBytes(image.DataPtr, (nuint)(bytesPerRow * height));

            Mat src = new(height, width, CvType.Cv8uc1, dataCopy, bytesPerRow);

            Mat grad_x = new(), grad_y = new();
            Mat abs_grad_x = new(), abs_grad_y = new();

            /// Gradient X
            //Scharr( src, grad_x, ddepth, 1, 0, scale, delta, BORDER_DEFAULT );
            Imgproc.Sobel(src, grad_x, ddepth, 1, 0, kernelSize, scale, delta, BorderTypes.Default);
            Core.ConvertScaleAbs(grad_x, abs_grad_x);
            /// Gradient Y
            //Scharr( src, grad_y, ddepth, 0, 1, scale, delta, BORDER_DEFAULT );
            Imgproc.Sobel(src, grad_y, ddepth, 0, 1, kernelSize, scale, delta, BorderTypes.Default);
            Core.ConvertScaleAbs(grad_y, abs_grad_y);

            /// Total Gradient (approximate)
            Core.AddWeighted(abs_grad_x, 0.5, abs_grad_y, 0.5, 0, image);
        }

        // http://docs.opencv.org/doc/tutorials/imgproc/imgtrans/canny_detector/canny_detector.html#canny-detector
        public static void FilterCanny(Mat image, int kernelSize, int lowThreshold)
		{
            int ratio = 3;

            int width = image.Cols;
            int height = image.Rows;
            int bytesPerRow = (int)image.Step1(0);

            // we need to copy because src.data != dst.data must hold with bilateral filter
            NSData dataCopy = NSData.FromBytes(image.DataPtr, (nuint)(bytesPerRow * height));

            Mat src = new(height, width, CvType.Cv8uc1, dataCopy, bytesPerRow);

            Mat detected_edges = new();

            /// Reduce noise with a kernel 3x3
            Imgproc.Blur(src, detected_edges, new Size2i(3, 3));

            /// Canny detector
            Imgproc.Canny(detected_edges, detected_edges, lowThreshold, lowThreshold * ratio, kernelSize);

            /// Using Canny's output as a mask, we display our result
            image.SetToScalar(Scalar.All(0));

            src.CopyTo(image, detected_edges);
        }
    }
}

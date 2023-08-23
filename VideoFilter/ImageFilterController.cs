using OpenCvSdk;

namespace VideoFilter
{
	public static class ImageFilterController
	{
		public static void GrayMatConversion(Mat inputMat)
		{
            Mat grayMat = new();
			Imgproc.CvtColor(inputMat, grayMat, ColorConversionCodes.Bgra2gray);
            Imgproc.CvtColor(grayMat, inputMat, ColorConversionCodes.Gray2bgra);
        }

        public static void YuvMatConversion(Mat inputMat)
        {
            Mat yuvMat = new();
            Imgproc.CvtColor(inputMat, yuvMat, ColorConversionCodes.Bgra2yuvI420);
            Imgproc.CvtColor(yuvMat, inputMat, ColorConversionCodes.Yuv2bgraI420);
        }

        public static void InverseMatConversion(Mat inputMat)
		{
            Mat invertMat = new();
            Imgproc.CvtColor(inputMat, invertMat, ColorConversionCodes.Bgra2bgr);

			// invert image
			Core.Bitwise_not(invertMat, invertMat);
			Imgproc.CvtColor(invertMat, inputMat, ColorConversionCodes.Bgr2bgra);
		}

		public static void BinaryMatConversion(Mat inputMat, float threshold)
		{
			Mat grayMat = new();
			Imgproc.CvtColor(inputMat, grayMat, ColorConversionCodes.Bgra2gray);
			Imgproc.Threshold(grayMat, grayMat, threshold, 255, ThresholdTypes.Binary);
            Imgproc.CvtColor(grayMat, inputMat, ColorConversionCodes.Gray2bgra);
        }

        public static void SepiaConversion(Mat inputMat)
		{
            Mat sepia = new Mat(4, 4, CvType.Cv32f);

			sepia.Put(0, 0, new NSNumber[]
				{ 0.189, 0.769, 0.393, 0,
				  0.168, 0.686, 0.349, 0,
				  0.131, 0.534, 0.272, 0,
				  0, 0, 0, 1 });

            Core.Transform(inputMat, inputMat, sepia);
        }

		public static void RetroEffectConversion(Mat inputMat)
		{
            Mat bgrMat = new();
            Imgproc.CvtColor(inputMat, bgrMat, ColorConversionCodes.Bgra2bgr);

            // convert image to YUV color space.
            Mat yuvMat = new();
            Imgproc.CvtColor(bgrMat, yuvMat, ColorConversionCodes.BGR2YCrCb);

            // split the image into separate color planes
            Mat[] planes = {
                new(yuvMat.Size(), CvType.Cv8u),
                new(yuvMat.Size(), CvType.Cv8u),
                new(yuvMat.Size(), CvType.Cv8u) };
            Core.Split(yuvMat, new NSMutableArray<Mat>(planes));

            // fills the matrix with normally distributed random values;
            Mat noise = new(inputMat.Size(), CvType.Cv8u);
            Core.Randn(noise, 150, 20);

            // blur the noise a bit
            Imgproc.GaussianBlur(noise, noise, new Size2i(3, 3), 0.5);

            const double brightness = 0;
            const double contrast = 1.7;
            Core.AddWeighted(planes[0], contrast, noise, 1, -128 + brightness, planes[0]);

            const double color_scale = 0.5;
            planes[1].ConvertTo(planes[1], planes[1].Type,
                                color_scale, 128 * (1 - color_scale));
            planes[2].ConvertTo(planes[2], planes[2].Type,
                                color_scale, 128 * (1 - color_scale));
            planes[0] = planes[0].Mul(planes[0], 1.0 / 255);

            //  merge the results 
            Core.Merge(planes, yuvMat);
            //   output RGB image
            Imgproc.CvtColor(yuvMat, bgrMat, ColorConversionCodes.YCrCb2BGR);
            Imgproc.CvtColor(bgrMat, inputMat, ColorConversionCodes.Bgr2bgra);
        }

        public static void FilmGrainConversion(Mat inputMat)
        {
            Mat bgrMat = new();
            Imgproc.CvtColor(inputMat, bgrMat, ColorConversionCodes.Bgra2bgr);

            // convert image to YUV color space.
            Mat yuvMat = new();
            Imgproc.CvtColor(bgrMat, yuvMat, ColorConversionCodes.BGR2YCrCb);

            // split the image into separate color planes
            Mat[] planes = {
                new(yuvMat.Size(), CvType.Cv8u),
                new(yuvMat.Size(), CvType.Cv8u),
                new(yuvMat.Size(), CvType.Cv8u) };
            Core.Split(yuvMat, new NSMutableArray<Mat>(planes));
            Imgproc.GaussianBlur(planes[0], planes[0], new Size2i(1, 1), 2);

            // normally distributed random values;       
            Mat noise = new(inputMat.Size(), CvType.Cv8u);
            Core.Randu(noise, 0, 255);

            // blur the noise      
            Imgproc.GaussianBlur(noise, noise, new Size2i(5, 5), 1);

            Core.AddWeighted(planes[0], 1, noise, 0.3, 0, planes[0]);

            Core.Merge(planes, yuvMat);
            Imgproc.CvtColor(yuvMat, bgrMat, ColorConversionCodes.YCrCb2BGR);
            Imgproc.CvtColor(bgrMat, inputMat, ColorConversionCodes.Bgr2bgra);
        }

        public static void SoftFocusConversion(Mat inputMat)
        {
            Mat bgrMat = new();
            Imgproc.CvtColor(inputMat, bgrMat, ColorConversionCodes.Bgra2bgr);
            // gaussian blur
            Mat softhMat = new();
            Imgproc.GaussianBlur(bgrMat, softhMat, new Size2i(0, 0), 25);
            Core.AddWeighted(softhMat, 0.6, bgrMat, 0.4, 0, softhMat);
            Imgproc.CvtColor(softhMat, inputMat, ColorConversionCodes.Bgr2bgra);
        }

        public static void CartoonMatConversion(Mat inputMat)
        {
            Mat edge = new();
            Imgproc.CvtColor(inputMat, edge, ColorConversionCodes.Bgra2bgr);

            // Apply bilateral filter to input image.
            Mat cartoonMat = new();
            Imgproc.BilateralFilter(edge, cartoonMat, 5, 150, 150);
            Imgproc.CvtColor(edge, edge, ColorConversionCodes.Bgr2gray);
            Imgproc.Canny(edge, edge, 145, 165);

            Imgproc.CvtColor(edge, edge, ColorConversionCodes.Gray2bgr);
            Core.Subtract(cartoonMat, edge, cartoonMat);

            Imgproc.CvtColor(cartoonMat, inputMat, ColorConversionCodes.Bgr2bgra);
        }
    }
}

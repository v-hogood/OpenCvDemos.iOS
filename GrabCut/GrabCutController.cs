using System.Diagnostics;
using OpenCvSdk;

namespace GrabCut
{
    public class GrabCutController
    {
        static Scalar Red = new Scalar(0, 0, 255);
        static Scalar Pink = new Scalar(230, 130, 255);
        static Scalar Blue = new Scalar(255, 0, 0);
        static Scalar LightBlue = new Scalar(255, 255, 160);
        static Scalar Green = new Scalar(0, 255, 0);

        const int radius = 15;
        const int thickness = -1;

        Mat image = new Mat();

        Mat mask = new Mat();
        Mat bgdModel = new Mat();
        Mat fgdModel = new Mat();

        Mat mFG = new Mat();
        Mat mBG = new Mat();
        Mat m255 = new Mat();

        Rect2i rect;

        Mat fgdPxls;
        Mat bgdPxls;

        bool initialized = false;
        public bool Processing { get; set; } = false;

        public int IterCount { get; set; } = 0;

        public void Reset()
        {
            if (!mask.Empty)
                mask.SetToScalar(Scalar.All((double)GrabCutClasses.Bgd));

            bgdPxls = Mat.Zeros(image.Size(), CvType.Cv8uc1);
            fgdPxls = Mat.Zeros(image.Size(), CvType.Cv8uc1);

            mFG.Create(image.Size(), CvType.Cv8uc3);
            mFG.SetToScalar(Red);
            mBG.Create(image.Size(), CvType.Cv8uc3);
            mBG.SetToScalar(Blue);

            m255.Create(image.Size(), CvType.Cv8uc1);
            m255.SetToScalar(Scalar.All(255));

            int off_x = 1; //image.cols * 0.1;
            int off_y = 1; //image.rows * 0.1;

            rect = new(off_x, off_y, image.Cols - 2 * off_x, image.Rows - 2 * off_y);
            this.SetRectInMask();

            initialized = false;
            IterCount = 0;
        }

        public Mat GetBinMask(Mat comMask)
        {
            if (comMask.Empty || comMask.Type != CvType.Cv8uc1)
                Console.WriteLine(Code.StsBadArg + " comMask is empty or has incorrect type (not CV_8UC1)");
            Mat binMask = new(comMask.Size(), CvType.Cv8uc1);
            binMask.SetToScalar(Scalar.All(1));
            Core.Bitwise_and(comMask, binMask, binMask);

            return binMask;
        }

        public void SetImage(UIImage uiImage)
        {
            image = new(uiImage);
            NSMutableArray<Mat> planes = new NSMutableArray<Mat>(4);
            Mat[] planesRGB = new Mat[3];

            Core.Split(image, planes);
            planesRGB[0] = planes[0];
            planesRGB[1] = planes[1];
            planesRGB[2] = planes[2];
            Core.Merge(planesRGB, image);

            mask.Create(image.Size(), CvType.Cv8uc1);
            this.Reset();
        }

        public void NextIteration()
        {
            if (this.Processing)
                return;
            this.Processing = true;

            Console.WriteLine("nextIter start");
            Console.WriteLine(" initialized: {0}", (initialized == false) ? "NO" : "YES");
            Console.WriteLine(" rect: x,y,w,h: {0}, {1}, {2}, {3}", rect.X, rect.Y, rect.Width, rect.Height);

            if (initialized)
            {
                Console.WriteLine(" grabCut(im, mask, rect, bg, fg, 1) ");
                this.DumpInfo();
                Imgproc.GrabCut(image, mask, rect, bgdModel, fgdModel, 1);
                this.DumpInfo();
            }
            else
            {
                Console.WriteLine(" grabCut(im, mask, rect, bg, fg, 1, GC_INIT_WITH_MASK) ");
                this.DumpInfo();
                Imgproc.GrabCut(image, mask, rect, bgdModel, fgdModel, 1, (int)GrabCutModes.InitWithMask);
                this.DumpInfo();

                initialized = true;
            }
            IterCount++;

            bgdPxls.SetToScalar(Scalar.All(0));
            fgdPxls.SetToScalar(Scalar.All(0));

            this.Processing = false;
        }

        public UIImage GetImage()
        {
            Mat result = new Mat();
            Mat binMask;

            if (!initialized)
            {
                image.CopyTo(result);
            }
            else
            {
                binMask = this.GetBinMask(mask);
                image.CopyTo(result, binMask);
            }

            // TODO: alpha blending with a mask
            mFG.CopyTo(result, fgdPxls);
            mBG.CopyTo(result, bgdPxls);
            Imgproc.Rectangle(result, new Point2i(rect.X, rect.Y), new Point2i(rect.X + rect.Width - 1, rect.Y + rect.Height - 1), Green, 2);

            return result.ToUIImage();
        }

        public UIImage GetSaveImage()
        {
            Mat result = new Mat();
            Mat binMask;

            if (!initialized)
            {
                image.CopyTo(result);
            }
            else
            {
                binMask = this.GetBinMask(mask);
                image.CopyTo(result, binMask);

                // add alpha channel from mask
                Mat alpha = new Mat();
                m255.CopyTo(alpha, binMask);

                Mat[] v = { result, alpha };
                Core.Merge(v, result);
            }

            return result.ToUIImage();
        }

        public void MaskLabel(CGPoint point, bool isForeground)
        {
            Console.WriteLine(" mask foreground begin");

            Point2i p = new((int)point.X, (int)point.Y);

            this.DumpMask();

            if (isForeground)
            {
                Imgproc.Circle(fgdPxls, p, radius, Scalar.All(1), thickness);
                Imgproc.Circle(bgdPxls, p, radius, Scalar.All(0), thickness);
                Imgproc.Circle(mask, p, radius, Scalar.All((double)GrabCutClasses.Fgd), thickness);
            }
            else
            {
                Imgproc.Circle(bgdPxls, p, radius, Scalar.All(1), thickness);
                Imgproc.Circle(fgdPxls, p, radius, Scalar.All(0), thickness);
                Imgproc.Circle(mask, p, radius, Scalar.All((double)GrabCutClasses.Bgd), thickness);
            }

            this.DumpMask();

            Console.WriteLine(" mask foreground end");
        }

        public void DumpMask()
        {
            /*
            NSLog(@" mask: ");
            for (int y=0; y<mask.rows; y++) {
                for (int x=0; x<mask.cols; x++) {
                    if (mask.at<unsigned char>(y,x) == GC_FGD) {
                        cout << "F";
                    } else if (mask.at<unsigned char>(y,x) == GC_BGD) {
                        cout << "B";
                    } else if (mask.at<unsigned char>(y,x) == GC_PR_FGD) {
                        cout << "f";
                    } else if (mask.at<unsigned char>(y,x) == GC_PR_BGD) {
                        cout << "b";
                    } else {
                        cout << "?";
                    }
                }
                cout << endl;
            }
            cout << endl;
            */
        }

        public void DumpInfo()
        {
            /*
            cout << "=== DUMP === " << endl;
            cout << "RECT: " << rect.x << "," << rect.y << "," << rect.width << "," << rect.height << endl;
            cout << "MASK:" << endl;
            for (int y=0; y<mask.rows; y++) {
                for (int x=0; x<mask.cols; x++) {
                    if (mask.at<unsigned char>(y,x) == GC_FGD) {
                        cout << "F";
                    } else if (mask.at<unsigned char>(y,x) == GC_BGD) {
                        cout << "B";
                    } else if (mask.at<unsigned char>(y,x) == GC_PR_FGD) {
                        cout << "f";
                    } else if (mask.at<unsigned char>(y,x) == GC_PR_BGD) {
                        cout << "b";
                    } else {
                        cout << "?";
                    }
                }
                cout << endl;
            }
            cout << "=== ==== === " << endl;
            cout << endl;
            */
        }

        public void SetRectInMask()
        {
            Debug.Assert(!mask.Empty);
            mask.SetToScalar(Scalar.All((double)GrabCutClasses.Bgd));
            rect.X = Math.Max(0, rect.X);
            rect.Y = Math.Max(0, rect.Y);
            rect.Width = Math.Min(rect.Width, image.Cols - rect.X);
            rect.Height = Math.Min(rect.Height, image.Rows - rect.Y);
            new Mat(mask, rect).SetToScalar(Scalar.All((double)GrabCutClasses.PrFgd));
        }
    }
}

using OpenCvSdk;

namespace FindHomography;

public class CvHomographyController : NSObject
{
    static CvHomographyController singleton = null;
	static Object obj = new();

    public static CvHomographyController SharedInstance()
    {
        lock(obj)
		{
			if (singleton == null)
			{
				singleton = new();
			}
			return singleton;
		}
	}

	public enum CVFeatureDetectorType
    {
        CV_FEATUREDETECTOR_FAST,
        CV_FEATUREDETECTOR_GOODTOTRACK,
        CV_FEATUREDETECTOR_MSER,
        CV_FEATUREDETECTOR_ORB,
        CV_FEATUREDETECTOR_SIFT,
    };

    public enum CVFeatureDescriptorType
    {
        CV_FEATUREDESCRIPTOR_SIFT,
        CV_FEATUREDESCRIPTOR_ORB
    };

	double max_dist = 0;
	double min_dist = 100;
		
	int FASTthreshold = 70;
    int ORBnfeatures = 500;
    int GFTTmaxCorners = 1000;
    int SIFTnfeatures = 0;
	double MSERarea_threshold = 1.01;
		
	bool adaptive = true;	
	bool limit_features = false;
	public bool object_loaded = false;

	int numFeaturesObjective = 50;
	int numFeaturesVariance = 15;

    CVFeatureDetectorType currentDetectorType;
	CVFeatureDescriptorType currentDescriptorType;
	
    public float thresh_min;
    public float thresh_max;
    float thresh_delta;

    bool multiscale = true;

    Feature2D detector = null;
    Feature2D descriptor = null;
    DescriptorMatcher matcher = null;

    Mat descriptors_object;
    Mat descriptors_scene;

    Mat img_object;
    Mat img_scene;
    Mat img_matches;

	KeyPoint[] keypoints_object = new KeyPoint[0];
    KeyPoint[] keypoints_scene = new KeyPoint[0];
    DMatch[] matches = new DMatch[0];

    public CvHomographyController()
	{
        /*
        this.SetDetector(CV_FEATUREDETECTOR_FAST);
        this.SetDescriptor(CV_FEATUREDESCRIPTOR_SIFT);
        matcher = new BFMatcher(NormTypes.L2);
        matcher = new FlannBasedMatcher();
        */
        this.UseSIFT();
	}

	public void SetDetector(CVFeatureDetectorType type)
	{
		Feature2D newDetector = null;
	
		switch (type)
		{		
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_FAST:
			{
				int threshold = FASTthreshold;
				bool nonMaxSuppression=true;
				newDetector = FastFeatureDetector.Create(threshold, nonMaxSuppression);
				thresh_min = 0;
				thresh_max = 255;
				thresh_delta = 1;
				break;
			}
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_ORB:
			{
				int nfeatures = ORBnfeatures;
				float scaleFactor = 1.2f;
				int nlevels = multiscale ? 8 : 1;
				int edgeThreshold = 31;
				int firstLevel = 0;
				int WTA_K = 2;
				ScoreType scoreType = ScoreType.HarrisScore;
				int patchSize = 31;
			
				newDetector = ORB.Create(nfeatures,
										 scaleFactor,
										 nlevels,
										 edgeThreshold,
										 firstLevel,
										 WTA_K,
										 scoreType,
										 patchSize
										 );
				thresh_min = numFeaturesObjective - numFeaturesVariance;
				thresh_max = numFeaturesObjective + numFeaturesVariance;
				thresh_min = (thresh_min < 0) ? 0 : thresh_min;
				thresh_delta = -1;
				break;
			}
		
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_GOODTOTRACK:
			{
				int maxCorners = GFTTmaxCorners;
				double qualityLevel = 0.01;
				double minDistance = 1;
				int blockSize = 3;
				bool useHarrisDetector = false;
				double k = 0.04;
				newDetector = GFTTDetector.Create(maxCorners,
												  qualityLevel,
												  minDistance,
												  blockSize,
												  useHarrisDetector,
												  k
												  );
			
				thresh_min = numFeaturesObjective - numFeaturesVariance;
				thresh_max = numFeaturesObjective + numFeaturesVariance;
				thresh_min = (thresh_min < 0) ? 0 : thresh_min;
				thresh_delta = -1;
				break;
			}
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_MSER:
			{
				int delta = 5;
				int min_area = 60;
				int max_area = 14400;
				double max_variation = 0.25;
				double min_diversity =.2;
				int max_evolution = 200;
				double area_threshold = MSERarea_threshold;
				double min_margin = 0.003;
				int edge_blur_size = 5;
			
				newDetector = MSER.Create(delta,
										  min_area,
										  max_area,
										  max_variation,
										  min_diversity,
										  max_evolution,
										  area_threshold,
										  min_margin,
										  edge_blur_size
										  );
				thresh_min = 0.01f;
				thresh_max = 3.00f;
				thresh_delta = 0.01f;
				break;
			}
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_SIFT:
			{
				int nfeatures=SIFTnfeatures;
				int nOctaveLayers = multiscale ? 3 : 1;
				double contrastThreshold = 0.04;
				double edgeThreshold = 10;
				double sigma = 1.6;
				newDetector = SIFT.Create(nfeatures,
										  nOctaveLayers,
										  contrastThreshold,
										  edgeThreshold,
										  sigma
										  );
			
				thresh_min = numFeaturesObjective - numFeaturesVariance;
				thresh_max = numFeaturesObjective + numFeaturesVariance;
				thresh_min = (thresh_min < 0) ? 0 : thresh_min;
				thresh_delta = -1;
			
				break;
			}
			
			default:
				Console.WriteLine(" Error: Unsupported detector chosen");
				break;
		}
	
		if (newDetector != null)
		{
			detector = newDetector;
			currentDetectorType = type;
		}
	}

	public void SetDescriptor(CVFeatureDescriptorType type)
	{
		Feature2D newDescriptor = null;
	
		switch (type)
		{
			case CVFeatureDescriptorType.CV_FEATUREDESCRIPTOR_SIFT:
			{
				int nfeatures = SIFTnfeatures;
				int nOctaveLayers = 3;
				double contrastThreshold = 0.04;
				double edgeThreshold = 10;
				double sigma = 1.6;
				newDescriptor = SIFT.Create(nfeatures,
											nOctaveLayers,
											contrastThreshold,
											edgeThreshold,
											sigma
											);
				break;
			}
			
			case CVFeatureDescriptorType.CV_FEATUREDESCRIPTOR_ORB:
			{
				int nfeatures = ORBnfeatures;
				float scaleFactor = 1.2f;
				int nlevels = 8;
				int edgeThreshold = 31;
				int firstLevel = 0;
				int WTA_K = 2;
				ScoreType scoreType = ScoreType.HarrisScore;
				int patchSize = 31;
			
				newDescriptor = ORB.Create(nfeatures,
										   scaleFactor,
										   nlevels,
										   edgeThreshold,
										   firstLevel,
										   WTA_K,
										   scoreType,
										   patchSize
										   );
				break;
			}
			
			
			default:
				Console.WriteLine(" Error: Unsupported descriptor chosen");
				break;
		}
	
		if (newDescriptor != null)
		{
			descriptor = newDescriptor;
			currentDescriptorType = type;
		}
	}

	public void UseORB()
	{
		multiscale = true;
		this.SetDetector(CVFeatureDetectorType.CV_FEATUREDETECTOR_ORB);
		this.SetDescriptor(CVFeatureDescriptorType.CV_FEATUREDESCRIPTOR_ORB);
		matcher = FlannBasedMatcher.Create();
	}

	public void UseSIFT()
	{
		multiscale = true;
		this.SetDetector(CVFeatureDetectorType.CV_FEATUREDETECTOR_SIFT);
		this.SetDescriptor(CVFeatureDescriptorType.CV_FEATUREDESCRIPTOR_SIFT);
		matcher = FlannBasedMatcher.Create();
	}

	public void Reset()
	{
		descriptors_object = new();
		descriptors_scene = new();

        img_object = new();
		img_scene = new();
	
		Array.Clear(keypoints_object);
		Array.Clear(keypoints_scene);
		Array.Clear(matches);
	}

	public void SetObjectImage(UIImage image)
	{
		// img_object = Mat(image.height, image.width, CV_8UC1, image.data, image.rowBytes);
		Imgproc.CvtColor(new(image), img_object, ColorConversionCodes.Bgra2gray);
		Console.WriteLine("set object image [w,h] = [{0},{1}]", img_object.Cols, img_object.Rows);
	
		// enable multiscale only for reference image
		NSMutableArray<KeyPoint> keypoints = new();
		detector.Detect(img_object, keypoints);
		keypoints_object = keypoints.ToArray<KeyPoint>();
	
		if (limit_features && keypoints_object.Length > numFeaturesObjective + numFeaturesVariance)
		{
			Array.Resize(ref keypoints_object, numFeaturesObjective + numFeaturesVariance);
		}
	
		Console.WriteLine("{0} keypoints detected for reference object", keypoints_object.Length);
	
		descriptor.Compute(img_object, new(keypoints_object), descriptors_object);
		Console.WriteLine("{0} descriptors generated for reference object", descriptors_object.Rows);
	
		// disable multiscale for faster online processing
		// (multiscale features in the reference image are still matched)
		multiscale = false;
		SetDetector(currentDetectorType);
		SetDescriptor(currentDescriptorType);
	
		this.object_loaded = true;
	}

	public void SetSceneImage(Mat image)
	{
		//	img_scene = Mat(image.height, image.width, CV_8UC1, image.data, image.rowBytes);
		img_scene = image;
		Console.WriteLine("set scene image [w,h] = [{0},{1}]", img_scene.Cols, img_scene.Rows);
	}

	public void AdaptThreshold(int numFeatures)
	{
		int n = numFeatures;
	
		if (!adaptive)
			return;
	
		if (Math.Abs(numFeaturesObjective - n) < numFeaturesVariance)
			return;
	
		float threshold = this.GetDetectorThreshold();
	
		if (numFeaturesObjective - n < 0)
		{
			// too many features
			threshold += thresh_delta;
		}
		else
		{
			// too few features
			threshold -= thresh_delta;
		}
	
		threshold = (threshold < thresh_min) ? thresh_min : threshold;
		threshold = (threshold > thresh_max) ? thresh_max : threshold;
	
		this.SetDetectorThreshold(threshold);
	}

	public float GetDetectorThreshold()
	{
		switch (currentDetectorType)
		{
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_FAST:
				return FASTthreshold;
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_ORB:
				return ORBnfeatures;
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_GOODTOTRACK:
				return GFTTmaxCorners;
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_MSER:
				return (float)MSERarea_threshold;
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_SIFT:
				return SIFTnfeatures;
			
			default:
				return 0;
		}
	}

	public string GetDetectorThresholdName()
	{
		switch (currentDetectorType)
		{
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_FAST:
				return "FAST Threshold";
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_ORB:
				return "ORB # Features";
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_GOODTOTRACK:
				return "GFTT Max Corners";
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_MSER:
				return "MSER Area Threshold";
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_SIFT:
				return "SIFT # Features";
			
			default:
				return "undefined";
		}
	}

	public void SetDetectorThreshold(float threshold)
	{
		bool update = true;
	
		switch (currentDetectorType)
		{
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_FAST:
				FASTthreshold = (int)threshold;
				break;
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_ORB:
				ORBnfeatures = (int)threshold;
				break;
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_GOODTOTRACK:
				GFTTmaxCorners = (int)threshold;
				break;
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_MSER:
				MSERarea_threshold = threshold;
				break;
			
			case CVFeatureDetectorType.CV_FEATUREDETECTOR_SIFT:
				SIFTnfeatures = (int)threshold;
				break;
			
			default:
				update = false;
				break;
		}
	
		if (update)
		{
			this.SetDetector(currentDetectorType);
		}
	}

	public void Detect()
	{
		NSMutableArray<KeyPoint> keypoints = new();
		detector.Detect(img_scene, keypoints);
		keypoints_scene = keypoints.ToArray<KeyPoint>();
		Console.WriteLine("{0} keypoints detected for scene image", keypoints_scene.Length);
	
		if (limit_features && keypoints_scene.Length > numFeaturesObjective + numFeaturesVariance)
		{
			Array.Resize(ref keypoints_scene, numFeaturesObjective + numFeaturesVariance);
		}
	
		this.AdaptThreshold(keypoints_scene.Length);
	}

	public void Descript()
	{
		descriptor.Compute(img_scene, new(keypoints_scene), descriptors_scene);
		Console.WriteLine("{0} descriptors generated for scene image", descriptors_scene.Rows);
	}

	public void Match()
	{
		if (descriptors_object.Rows == 0 || descriptors_scene.Rows == 0)
		{
			return;
		}
	
		NSMutableArray<DMatch> match_array = new();
		matcher.Match(descriptors_object, descriptors_scene, match_array);
		matches = match_array.ToArray<DMatch>();
		Console.WriteLine("{0} keypoint matches", matches.Length);
	
		// -- Quick calculation of max and min distances between keypoints
		max_dist = 0;
		min_dist = 100;
		for (int i = 0; i < descriptors_object.Rows; i++)
		{
			double dist = matches[i].Distance;
			if (dist < min_dist) min_dist = dist;
			if (dist > max_dist) max_dist = dist;
		}
	
		Console.WriteLine("-- Max dist : {0}\n", max_dist);
		Console.WriteLine("-- Min dist : {0}\n", min_dist);
	}

	public void DrawMatches()
	{
		// -- Draw only "good" matches (i.e. whose distance is less than 3 * min_dist)
		DMatch[] good_matches = new DMatch[0];
		
		if (matches.Length == descriptors_object.Rows)
		{
			for (int i = 0; i < descriptors_object.Rows; i++)
			{
				if (matches[i].Distance < 3 * min_dist)
				{
					good_matches.Append(matches[i]);
				}
			}
		}
		Console.WriteLine("{0} good matches", good_matches.Length);
	
		img_matches = new Mat();
		
		Features2d.DrawMatches(img_object, keypoints_object, img_scene, keypoints_scene,
							   good_matches, img_matches, Scalar.All(-1), Scalar.All(-1),
							   new(), DrawMatchesFlags.Default);
		Console.WriteLine("draw matches image [w,h] = [{0},{1}]", img_matches.Cols, img_matches.Rows);

		if (good_matches.Length == 0)
			return;
	
		// -- Localize the object
		Point2f[] obj = new Point2f[0];
		Point2f[] scene = new Point2f[0];
	
		for (int i = 0; i < good_matches.Length; i++)
		{
			// -- Get the keypoints from the good matches
			obj.Append(keypoints_object[good_matches[i].QueryIdx].Pt);
			scene.Append(keypoints_scene[good_matches[i].TrainIdx].Pt);
		}
	
		try
		{
			Mat H = Calib3d.FindHomography(new MatOfPoint2f(obj), new MatOfPoint2f(scene), Calib3d.RANSAC);
		
			// -- Get the corners from the image_1 (the object to be "detected")
			Point2f[] obj_corners = new Point2f[4];
			obj_corners[0] = new(0, 0);
			obj_corners[1] = new(img_object.Cols, 0);
			obj_corners[2] = new(img_object.Cols, img_object.Rows);
			obj_corners[3] = new(0, img_object.Rows);
			Point2f[] scene_corners = new Point2f[4];
		
			Core.PerspectiveTransform(new MatOfPoint2f(obj_corners), new MatOfPoint2f(scene_corners), H);
		
			// -- Draw lines between the corners (the mapped object in the scene - image_2)
			Imgproc.Line(img_matches,
				new Point2i((int)scene_corners[0].X + img_object.Cols, (int)scene_corners[0].Y),
                new Point2i((int)scene_corners[1].X + img_object.Cols, (int)scene_corners[1].Y),
				new Scalar(0, 255, 0), 4);
            Imgproc.Line(img_matches,
                new Point2i((int)scene_corners[1].X + img_object.Cols, (int)scene_corners[1].Y),
                new Point2i((int)scene_corners[2].X + img_object.Cols, (int)scene_corners[2].Y),
                new Scalar(0, 255, 0), 4);
            Imgproc.Line(img_matches,
                new Point2i((int)scene_corners[2].X + img_object.Cols, (int)scene_corners[2].Y),
                new Point2i((int)scene_corners[3].X + img_object.Cols, (int)scene_corners[3].Y),
                new Scalar(0, 255, 0), 4);
            Imgproc.Line(img_matches,
                new Point2i((int)scene_corners[3].X + img_object.Cols, (int)scene_corners[3].Y),
                new Point2i((int)scene_corners[0].X + img_object.Cols, (int)scene_corners[0].Y),
                new Scalar(0, 255, 0), 4);
		}
		catch (Exception e)
		{
			Console.WriteLine("Exception in findHomography() and/or perspectiveTransform()");
			Console.WriteLine(e.Message);
		}
	}

	public void DrawObject()
	{
		// -- Draw only "good" matches (i.e. whose distance is less than 3*min_dist)
		DMatch[] good_matches = new DMatch[0];
	
		if (matches.Length == descriptors_object.Rows)
		{
			for (int i = 0; i < descriptors_object.Rows; i++)
			{
				if (matches[i].Distance < 3*min_dist)
				{
					good_matches.Append(matches[i]);
				}
			}
		}
		Console.WriteLine("{0} good matches", good_matches.Length);
	
		img_matches = new Mat();
		Features2d.DrawKeypoints(img_object, keypoints_object, img_matches, Scalar.All(-1), DrawMatchesFlags.Default);
	
		// -- Localize the object
		Point2f[] obj = new Point2f[0];
	
		for (int i = 0; i < good_matches.Length; i++)
		{
			//-- Get the keypoints from the good matches
			obj.Append(keypoints_object[good_matches[i].QueryIdx].Pt);
		}
	}

	public void DrawScene()
	{
		// -- Draw only "good" matches (i.e. whose distance is less than 3*min_dist)
		DMatch[] good_matches = new DMatch[0];
	
		if (matches.Length == descriptors_object.Rows)
		{
			for (int i = 0; i < descriptors_object.Rows; i++)
			{
				if (matches[i].Distance < 3 * min_dist)
				{
					good_matches.Append(matches[i]);
				}
			}
		}
		Console.WriteLine("{0} good matches", good_matches.Length);
	
		Features2d.DrawKeypoints(img_scene, keypoints_scene, img_scene, Scalar.All(-1), DrawMatchesFlags.DrawOverOutimg);

		if (good_matches.Length == 0)
			return;
	
		// -- Localize the object
		Point2f[] obj = new Point2f[0];
		Point2f[] scene = new Point2f[0];
	
		for (int i = 0; i < good_matches.Length; i++)
		{
			// -- Get the keypoints from the good matches
			obj.Append(keypoints_object[good_matches[i].QueryIdx].Pt);
			scene.Append(keypoints_scene[good_matches[i].TrainIdx].Pt);
		}
	
		try
		{
			Mat H = Calib3d.FindHomography(new MatOfPoint2f(obj), new MatOfPoint2f(scene), Calib3d.RANSAC);
		
			// -- Get the corners from the image_1 (the object to be "detected")
			Point2f[] obj_corners = new Point2f[4];
			obj_corners[0] = new(0, 0);
			obj_corners[1] = new(img_object.Cols, 0);
			obj_corners[2] = new(img_object.Cols, img_object.Rows);
			obj_corners[3] = new(0, img_object.Rows);
			Point2f[] scene_corners = new Point2f[4];
		
			Core.PerspectiveTransform(new MatOfPoint2f(obj_corners), new MatOfPoint2f(scene_corners), H);

            // -- Draw lines between the corners (the mapped object in the scene - image_2)
            Imgproc.Line(img_matches,
                new Point2i((int)scene_corners[0].X + img_object.Cols, (int)scene_corners[0].Y),
                new Point2i((int)scene_corners[1].X + img_object.Cols, (int)scene_corners[1].Y),
                new Scalar(0, 255, 0), 4);
            Imgproc.Line(img_matches,
                new Point2i((int)scene_corners[1].X + img_object.Cols, (int)scene_corners[1].Y),
                new Point2i((int)scene_corners[2].X + img_object.Cols, (int)scene_corners[2].Y),
                new Scalar(0, 255, 0), 4);
            Imgproc.Line(img_matches,
                new Point2i((int)scene_corners[2].X + img_object.Cols, (int)scene_corners[2].Y),
                new Point2i((int)scene_corners[3].X + img_object.Cols, (int)scene_corners[3].Y),
                new Scalar(0, 255, 0), 4);
            Imgproc.Line(img_matches,
                new Point2i((int)scene_corners[3].X + img_object.Cols, (int)scene_corners[3].Y),
                new Point2i((int)scene_corners[0].X + img_object.Cols, (int)scene_corners[0].Y),
                new Scalar(0, 255, 0), 4);
		}
		catch (Exception e)
		{
			Console.WriteLine("Exception in findHomography() and/or perspectiveTransform()");
			Console.WriteLine(e.Message);
		}
	}

	public UIImage ObjectImage => img_object.ToUIImage();

	public Mat SceneImage => img_scene;

	public Mat MatchImage => img_matches;

	public void DrawKeypoints()
	{
		foreach (KeyPoint k in keypoints_object)
		{
			Console.WriteLine("object k@(" + k.Pt.X + "," + k.Pt.Y + "), || " + k.Size);
			Imgproc.Circle(img_object, new((int)k.Pt.X, (int)k.Pt.Y), (int)k.Size, new(255,255,0));
		}
	
		for (int i = 0; i < keypoints_scene.Length; i++)
		{
			KeyPoint k = keypoints_scene[i];
            Console.WriteLine("scene k@(" + k.Pt.X + "," + k.Pt.Y + "), || " + k.Size);
			Imgproc.Circle(img_scene, new((int)k.Pt.X, (int)k.Pt.Y), (int)k.Size, new(255,255,0));
		}
	}
}

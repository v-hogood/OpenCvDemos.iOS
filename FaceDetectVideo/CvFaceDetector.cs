using OpenCvSdk;

namespace FaceDetectVideo;

public class CvFaceDetector
{
    CascadeClassifier eyes_cascade = null;
    CascadeClassifier face_cascade = null;

    bool detectEyes = true;

    public CvFaceDetector()
	{
		this.LoadCascades();
	}

	void LoadCascades()
	{
		face_cascade = this.LoadCascade("lbpcascade_frontalface");
		eyes_cascade = this.LoadCascade("haarcascade_eye");
	}

	CascadeClassifier LoadCascade(string filename)
	{
		string real_path = NSBundle.MainBundle.PathForResource(name: filename, ofType:"xml");
		CascadeClassifier cascade = new CascadeClassifier();
	
		if (real_path != null && !cascade.Load(real_path))
		{
			Console.WriteLine("Unable to load cascade file {0}.xml", filename);
		}
		else
		{
            Console.WriteLine("Loaded cascade file {0}.xml", filename);
		}
		return cascade;
	}

	public void DetectFacesInMat(Mat grayMat)
	{
		// haar detect
		float haar_scale = 1.15f;
		int haar_minNeighbors = 3;
		int haar_flags = 0 | Objdetect.CASCADE_SCALE_IMAGE | Objdetect.CASCADE_DO_CANNY_PRUNING;
		Size2i haar_minSize = new(60, 60);

		NSMutableArray<Rect2i> face_array = new();
		face_cascade.DetectMultiScale(grayMat, face_array, haar_scale,
									  haar_minNeighbors, haar_flags, haar_minSize );
		Rect2i[] faces = face_array.ToArray<Rect2i>();
	
		for (int i = 0; i < faces.Length; i++)
		{
			Point2i center = new(faces[i].X + faces[i].Width / 2, faces[i].Y + faces[i].Height / 2);
			Imgproc.Ellipse(grayMat, center, new(faces[i].Width / 2, faces[i].Height / 2), 0, 0, 360, new (255, 0, 255), 4, LineTypes.Line8, 0);
		}
		Console.WriteLine("{0} faces detected", faces.Length);
	}
}

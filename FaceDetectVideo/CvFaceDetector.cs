using OpenCvSdk;

namespace FaceDetectVideo;

public class CvFaceDetector
{
	FaceDetectorYN face_detector = null;

	public CvFaceDetector()
	{
		this.LoadDetector();
	}

	void LoadDetector()
	{
		face_detector = this.LoadDetector("face_detection_yunet_2023mar");
	}

	FaceDetectorYN LoadDetector(string filename)
	{
		string real_path = NSBundle.MainBundle.PathForResource(name: filename, ofType:"onnx");
		FaceDetectorYN detector = FaceDetectorYN.Create(real_path, "", new Size2i(320, 320));
	
		if (real_path != null && detector == null)
		{
			Console.WriteLine("Unable to load onnx file {0}.xml", filename);
		}
		else
		{
			Console.WriteLine("Loaded onnx file {0}.xml", filename);
		}
		return detector;
	}

	public void DetectFacesInMat(Mat grayMat)
	{
		Mat face_array = new();
		face_detector.SetInputSize(grayMat.Size());
		face_detector.Detect(grayMat, face_array);
        Rect2i[] faces = new Rect2i[face_array.Rows];

        for (int i = 0; i < face_array.Rows; i++)
        {
            var face = new Rect2i(
                x: (int)face_array.Get(i, 0)[0],
                y: (int)face_array.Get(i, 1)[0],
                width: (int)face_array.Get(i, 2)[0],
                height: (int)face_array.Get(i, 3)[0]);
            faces[i] = face;
        }

		for (int i = 0; i < faces.Length; i++)
		{
			Point2i center = new(faces[i].X + faces[i].Width / 2, faces[i].Y + faces[i].Height / 2);
			Imgproc.Ellipse(grayMat, center, new(faces[i].Width / 2, faces[i].Height / 2), 0, 0, 360, new (255, 0, 255), 4, LineTypes.Line8, 0);
		}
		Console.WriteLine("{0} faces detected", faces.Length);
	}
}

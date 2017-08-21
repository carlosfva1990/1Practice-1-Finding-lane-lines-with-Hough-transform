using UnityEngine;
using System.Collections;
using UnityEngine.Video;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;

namespace OpenCVForUnityExample
{

    public enum archivoDeVideo
    {
        video,
        video1,
        video2,
        video3
    };
    /// <summary>
    /// HoughLinesP example. (Example of straight line detection using the HoughLinesP class)
    /// </summary>
    /// 
    [RequireComponent(typeof(MeshRenderer))]
    public class HoughLinesVideo : MonoBehaviour
    {

        [Header("Detector de lineas en video")]
        Texture2D imgTexture;
        public bool esVideo = false;
        public bool imagenAColor = true;
        public archivoDeVideo av; // Create a variable 
        string archivo = "video";
        //public MovieTexture movie;
        [SerializeField]
        [Space(10)]
        [Header("Maskara")]
        public bool maskB = true;

        [SerializeField]
        [Space(10)]
        [Header("Gauss")]
        public bool gauss = true;
        [Range(0, 15)]
        public int tamañoKernel = 3;
        [Range(0, 400)]
        public double sigma = 100;

        [Space(10)]
        [Header("Canny")]
        public bool canny = true;
        [Range(0, 600)]
        public int uimbral1 = 250;
        [Range(0, 600)]
        public int uimbral2 = 250;
        /*
        [Space(10)]
        [Header("Sobel")]
        public bool sobel = true;
        public int scale = 1;
        public int delta = 0;
        int ddepth = CvType.CV_16S;
        public int dx = 1;
        public int dy = 0;
        */
        [Space(10)]
        [Header("Trans. Hough")]
        public bool tDeHough = true;
        [Range(0, 400)]
        public int aux = 150;
        [Range(0, 200)]
        public int houghVotes = 100;
        [Range(1, 15)]
        public double rho = 3;
        [Range(0, 1000)]
        public double minLineLength = 200;
        [Range(0, 500)]
        public double maxLineGap = 100;
        Texture2D texture ;
        Texture2D mask;
        VideoPlayer videoPlayer;
        Renderer r;
        Mat imgMat;
        Mat auxMat;
        Mat maskMat;
        Mat grayMat = new Mat();
            Mat lines = new Mat();
        VideoPlayer.FrameReadyEventHandler OnNewFrame = null;
        private Mat maskMat2;
        
        // Use this for initialization
        void Start ()
        {

            switch (av)
            {
                case archivoDeVideo.video:
                    archivo = "video";
                    break;
                case archivoDeVideo.video1:
                    archivo = "video1";
                    break;
                case archivoDeVideo.video2:
                    archivo = "video2";
                    break;
                case archivoDeVideo.video3:
                    archivo = "video3";
                    break;
            }

            mask = new Texture2D(1920, 1080);
            mask = Resources.Load("mascara2") as Texture2D;

            maskMat = new Mat(mask.height, mask.width, CvType.CV_8UC1);


            Utils.texture2DToMat(mask, maskMat);

            //Debug.Log("maskMat" + maskMat);
            // Will attach a VideoPlayer to the main camera.
            GameObject camera = GameObject.Find("Main Camera");

            // VideoPlayer automatically targets the camera backplane when it is added
            // to a camera object, no need to change videoPlayer.targetCamera.
            videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();

            // By default, VideoPlayers added to a camera will use the far plane.
            // Let's target the near plane instead.
            videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.APIOnly;

            // This will cause our scene to be visible through the video being played.
            //videoPlayer.targetCameraAlpha = 1F;

            // Set the video to play. URL supports local absolute or relative paths.
            // Here, using absolute.
            videoPlayer.url = "/Users/carlo/Documents/CIDETEC/vision 3d/hough/Assets/Resources/"+archivo+".mp4";

            // Skip the first 100 frames.
            videoPlayer.frame = 00;

            // Restart from beginning when done.
            videoPlayer.isLooping = true;

            // Each time we reach the end, we slow down the playback by a factor of 10.
            //videoPlayer.loopPointReached += EndReached;

            // Start playback. This means the VideoPlayer may have to prepare (reserve
            // resources, pre-load a few frames, etc.). To better control the delays
            // associated with this preparation one can use videoPlayer.Prepare() along with
            // its prepareCompleted event.
            videoPlayer.Play();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                

                if (videoPlayer.isPlaying)
                {
                    videoPlayer.Pause();
                }
                else
                {
                    videoPlayer.Play();
                }
            }



                if (Input.GetKeyDown(KeyCode.H) || esVideo)
            {

                imgTexture = new Texture2D(1920, 1080);
                RenderTexture renderTexture = videoPlayer.texture as RenderTexture;
               // Debug.Log(renderTexture);
                //Debug.Log(imgTexture);

                
              //  maskMat = new Mat(renderTexture.height, renderTexture.width, CvType.CV_8UC1);


                if (imgTexture.width != renderTexture.width || imgTexture.height != renderTexture.height)
                {
                    imgTexture.Resize(renderTexture.width, renderTexture.height);
                }
                RenderTexture.active = renderTexture;
                imgTexture.ReadPixels(new UnityEngine.Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                imgTexture.Apply();
                RenderTexture.active = null;
                
                //imgTexture = (Texture2D) videoPlayer.texture;
                if (imgTexture == null)
                    Debug.Log("no cargo");
                else
                {


                    imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC3);

                    // gameObject.GetComponent<Renderer>().material.mainTexture = imgTexture;

                    Utils.texture2DToMat(imgTexture, imgMat);
                   // Debug.Log("imgMat.ToString() " + imgMat.ToString());

                    if (gauss)
                    {
                        Imgproc.GaussianBlur(imgMat, imgMat, new Size(tamañoKernel, tamañoKernel), sigma);
                    }


                    Imgproc.cvtColor(imgMat, grayMat, Imgproc.COLOR_RGB2GRAY);

                    //Debug.Log("grayMat.ToString() " + grayMat.ToString());

                    if (canny)
                    {
                        Imgproc.Canny(grayMat, grayMat, uimbral1, uimbral2);
                    }
                    if (maskB)
                    {
                        maskMat2 = new Mat(grayMat.rows(), grayMat.cols(), CvType.CV_8UC1);
                        Imgproc.resize(maskMat, maskMat2, maskMat2.size(), 0, 0, 0);


                        grayMat = grayMat - maskMat2;
                    }

                    /*
                    if (sobel)
                    {
                        Imgproc.Sobel(grayMat, grayMat, ddepth, dx, dy, 3, scale, delta);
                    }
                    */
                    if (imagenAColor)
                    {
                        auxMat = imgMat;
                    }
                    else
                    {
                        auxMat = grayMat;
                    }
                    if (tDeHough)
                    {
                        lines = new Mat();
                        Imgproc.HoughLinesP(grayMat, lines, rho, Mathf.PI / 180, houghVotes, minLineLength, maxLineGap);

                        //                     Debug.Log ("lines.toStirng() " + lines.ToString ());
                                             Debug.Log("lines.dump()" + lines.dump());



                        int[] linesArray = new int[lines.cols() * lines.rows() * lines.channels()];
                        //imprimiendo las lineas

                        lines.get(0, 0, linesArray);


                        for (int i = 0; i < linesArray.Length; i = i + 4)
                        {
                            if ((linesArray[i + 1] - linesArray[i + 3] < -aux || linesArray[i + 1] - linesArray[i + 3] > aux))
                            {
                              //  Debug.Log(linesArray[i + 1] - linesArray[i + 3]);
                                Imgproc.line(auxMat, new Point(linesArray[i + 0], linesArray[i + 1]), new Point(linesArray[i + 2], linesArray[i + 3]), new Scalar(255, 0, 255), 10);
                            }
                        }
                    }


                    texture = new Texture2D(auxMat.cols(), auxMat.rows(), TextureFormat.RGBA32, false);
                    Utils.matToTexture2D(auxMat, texture);

                    gameObject.GetComponent<Renderer>().material.mainTexture = texture;
                }
            }
        }
        
    }

}
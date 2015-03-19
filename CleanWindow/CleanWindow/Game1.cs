using System;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;
using System.Threading;
using Newtonsoft.Json;


namespace CleanWindow
{
   
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const string STRING_MEUN_MUSIC = "menumusic";
        const string STRING_BG_MUSIC_PREFIX = "bgMusics";
        private int ROUNDCNT = 3;
        private int STAGE = 1;
        int PASSPERCENTAGE = 80;
        private int sceneCount = 1;
        private int PLAYTIMEOUT = 6000;

        float readShareTime = 0;

        public Vector2 scaleRate;

        ShareData sharedata;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KinectSensor kinectSensor;
        string connectedStatus = "Not connected";
        Texture2D gameBackground;
        Dictionary<string,Texture2D> gameBackgrounds=new Dictionary<string,Texture2D>();//���ӱ���Dictionary
        //Texture2D gameWindowFrame;
        Texture2D photoFrameTexture;
        Texture2D leftHandSpriteTexture;
        Texture2D rightHandSpriteTexture;

        //Sprite transitionTrain, transitionMapName;
        Texture2D transitionTrainTexture;
        Texture2D smokeTexture;
        int trainKind = 0;
        int smokeKind = 0;

        Texture2D transitionMap, transitionMapLocation;
        //Dictionary<string, Texture2D> transitionMapLocations = new Dictionary<string, Texture2D>();//���� transitionMapLocation dictionary
        //Dictionary<string, Texture2D> transitionMapNameTextures = new Dictionary<string, Texture2D>();//����transitionMapNameTexture dictionary
        Vector2 transitionTrainPosition = new Vector2(1300, 400);
        List<Texture2D> takePhotoTextures = new List<Texture2D>();

        Texture2D playSceneTexture, playSceneNameTexture;

        double playingTimeOut = 0;
        double transitionFlashing = 0;
        Boolean drawFlashing = true;


        //Main Menu
        Sprite startButton, startButtonPressed; //helpButton, helpButtonPressed, settingButton, settingButtonPressed;
        DateTime startPressedStartTime = DateTime.Now;
        Boolean isStartButtonPressed = false;
        DateTime startConfirmPressedStartTime = DateTime.Now;
        Boolean startConfirmButtonPressed = false;

        //DateTime helpPressedStartTime = DateTime.Now;
        //Boolean isHelpButtonPressed = false;
        //DateTime helpConfirmPressedStartTime = DateTime.Now;
        //Boolean helpConfirmButtonPressed = false;

        //DateTime settingPressedStartTime = DateTime.Now;
        //Boolean isSettingButtonPressed = false;
        //DateTime settingConfirmPressedStartTime = DateTime.Now;
        //Boolean settingConfirmButtonPressed = false;

        DateTime transitionStartTime = DateTime.Now;

        protected const int BACKBUFFER_WIDTH = 1280;
        protected const int BACKBUFFER_HEIGHT = 768;

        const int fogX = (int)(100 * (float)BACKBUFFER_WIDTH/1280);
        const int fogY = (int)(60 * (float)BACKBUFFER_HEIGHT /768);

        double FORGIVECOUNT = fogX * fogY * 0.2;
        PlayerManager leftHandManager, rightHandManager;

        Sprite leftHandSprite, rightHandSprite;
        Sprite[,] fogSprite = new Sprite[fogX,fogY];

        Vector2 headPosition = new Vector2(-100, -100);
        Vector2 leftHandPosition = new Vector2(-100,-100);
        Vector2 rightHandPosition = new Vector2(-100,-100);
        Vector2 leftHandOldPosition = new Vector2(-100, -100);
        Vector2 rightHandOldPosition = new Vector2(-100, -100);
        int[,] fog = new int[fogX, fogY];
        DateTime[,] fogCleanRelease = new DateTime[fogX, fogY];
        Vector2[,] fogLocation = new Vector2[fogX, fogY];
        List<Texture2D> fogTextures = new List<Texture2D>();


        //int TimeOutLimit = 10000;
        double timeoutCount = 0;

        List<Texture2D> particleTextures = new List<Texture2D>();

        //this is the pass marks
        List<Texture2D> marks = new List<Texture2D>();

        //Boolean nextStageParticleStart = false;
        SoundEffect soundChim, soundApplus, soundCamera, soundCheering, soundWipe, soundClick;
        SoundEffect takephotoready, takephoto1, takephoto2, takephoto3, takephotosmile, takephotoshutter;

        Boolean takePhotoSouldPlayed1 = false;
        Boolean takePhotoSouldPlayed2 = false;
        Boolean takePhotoSouldPlayed3 = false;
        Boolean takePhotoSouldPlayed4 = false;
        Boolean takePhotoSouldPlayed5 = false;
        Boolean takePhotoSouldPlayed6 = false;

        SoundEffectInstance soundEffectCheeringInstance;
        SoundEffectInstance soundEffectApplusInstance;
                    
        //ParticleEngine[] particleEngines = new ParticleEngine[4];
        //Vector2[] particlesXY = new Vector2[4];
        //int[] particlePeek = new int[4];
        //int[] particleSpeed = new int[4];

        List<Texture2D> magicTextures = new List<Texture2D>();
        MagicEngine magicEngineLeftHand, magicEngineRightHand, magicEngineProgressBar;
       
        enum GameStates {       GO_MAINMENU, MAINMENU,
                                GO_TRANSITION, TRANSITION, GO_PLAY, PLAY, GO_WIN, WIN, GO_LOSE, LOSE,
                                ENDGAME
                            };

        GameStates gameState = GameStates.GO_MAINMENU;
        
        private byte[] colorArray;
        private short[] depthArray;
        private ColorImagePoint[] colorPointArray;
        private byte[] playerPixelArray;
        Texture2D player;
        private ColorImagePoint[] mappedDepthLocations;

        Song bgMusic;
        Dictionary<string, Song> bgMusics = new Dictionary<string, Song>();//���ӱ�������dictionary
        bool bgMusicStart = false;


        Boolean CameraFlashing = false;

        Boolean takePhotoStart = false;
        Boolean takePhotoOK = false;
        RenderTarget2D screenShot;

        private static int skeletonID = -1;

        Texture2D stageTexture;                             //��ǰ�ؿ���ʾͼƬ
        Texture2D stageNumberTexture;
        List<Texture2D> keepPhotos = new List<Texture2D>();           //����ÿ���ؿ�����Ƭ
        List<Texture2D> keepPhotosName = new List<Texture2D>();       //keep stage photo name
        int fileID = 1;
        string filePath;
        //Texture2D[,] senceTexture = new Texture2D[3, 3];    //��Ϸ����ͼƬ
        Texture2D[] senceTexture = new Texture2D[3];    //��Ϸ����ͼƬ
        Vector2 getHeadPosition = Vector2.Zero;
        double headSholderLength = -1;

        string capturesPath;        //��Ƭ���·��
        float headDepth;

        string basePath;
        Form1 formMessage = new Form1();
        private float minAlpha = 0;
        private int TotalStage = 3;
        private int albumShowTime = 5000;
        private int albumNumber = 0;
        double endGameTime = 0;                             //��Ϸ������ʱ��
        private int endGameNumber = 0;
        Texture2D albumTexture;

        //List<string> bgList = new List<string>();
        string bgString = "";
        List<string> countryList = new List<string>();
        string countryString = "";
        Texture2D progressBarTexture;

        bool routeFlag = false;
        List<string> routeList = new List<string>();


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = BACKBUFFER_WIDTH;
            graphics.PreferredBackBufferHeight = BACKBUFFER_HEIGHT;
            graphics.IsFullScreen = Properties.CleanWindow.Default.isFullscreen;
            Content.RootDirectory = "Content";
           
        }

        void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (this.kinectSensor == e.Sensor)
            {
                if (e.Status == KinectStatus.Disconnected ||
                    e.Status == KinectStatus.NotPowered)
                {
                    this.kinectSensor = null;
                    this.DiscoverKinectSensor();
                }
            }
        }

        void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    

                    skeletonFrame.CopySkeletonDataTo(skeletonData);

                    if (skeletonData.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))return;

                    skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletonData);
                    Skeleton playerSkeleton;
                    if (skeletonID == -1)
                    {
                        playerSkeleton = (from s in skeletonData
                                          where s.TrackingState == SkeletonTrackingState.Tracked
                                          select s).FirstOrDefault();
                    }
                    else
                    {
                        playerSkeleton = (from s in skeletonData
                                          where s.TrackingState == SkeletonTrackingState.Tracked && s.TrackingId == skeletonID
                                          select s).FirstOrDefault(); 
                    }

                    if (playerSkeleton != null)
                    {
                        Joint leftHand = playerSkeleton.Joints[JointType.HandLeft];
                        Joint rightHand = playerSkeleton.Joints[JointType.HandRight];
                        Joint leftElbow = playerSkeleton.Joints[JointType.ElbowLeft];
                        Joint rightElbow = playerSkeleton.Joints[JointType.ElbowRight];
                        Joint head = playerSkeleton.Joints[JointType.Head];
                        Joint shoulderCenter = playerSkeleton.Joints[JointType.ShoulderCenter];

                        //String result = geometryDetection(new Vector2(leftElbow.Position.X, leftElbow.Position.Y),
                        //                                  new Vector2(rightElbow.Position.X, rightElbow.Position.Y),
                        //                                  new Vector2(leftHand.Position.X, leftHand.Position.Y),
                        //                                  new Vector2(rightHand.Position.X, rightHand.Position.Y));

                        //if (result == "RESTART")
                        //{
                        //    intializeFog();
                        //}

                        //if (result == "END")
                        //{
                        //    //Exit();
                        //}

                        //if (result == "CLEAR")
                        //{
                        //    for (int i = 0; i < fogX; i++)
                        //    {
                        //        for (int j = 0; j < fogY; j++)
                        //        {
                        //            fog[i, j] = 0;
                        //        }
                        //    }
                        //}

                        skeletonID = playerSkeleton.TrackingId; //if the skeleton gotten from the kinect isn't null then store that id. Yes this is being overwritten constantly. Maybe bad coding? I need to experiment and see what happens when I add a condition not to reset if it has already been set.

                        Vector2 shoulder;
                        getHeadPosition = GetJointPoint(head);
                        shoulder = GetJointPoint(shoulderCenter);
                        headSholderLength = Math.Sqrt(Math.Pow(getHeadPosition.X - shoulder.X, 2) + Math.Pow(getHeadPosition.Y - shoulder.Y, 2));
                        headSholderLength *= 0.5;


                        DepthImagePoint point = this.kinectSensor.MapSkeletonPointToDepth(head.Position, this.kinectSensor.DepthStream.Format);
                        headDepth = point.Depth;

                        headPosition = new Vector2((((head.Position.X) + 0.5f) * (BACKBUFFER_WIDTH)), (((-1 * head.Position.Y) + 0.5f) * (BACKBUFFER_HEIGHT)));
                        leftHandPosition = new Vector2((((leftHand.Position.X) + 0.5f) * 800 + 240)*scaleRate.X, (((-1 * leftHand.Position.Y) + 0.5f) * (BACKBUFFER_HEIGHT)));
                        rightHandPosition = new Vector2((((rightHand.Position.X) + 0.5f) * 800 + 240)*scaleRate.X, (((-1 * rightHand.Position.Y) + 0.5f) * (BACKBUFFER_HEIGHT)));
                        adjustKinectSensor();
                        
                    }
                    else
                    {
                        skeletonID = -1; //if the skeleton is null (say when someone walks off the screen. Then the id used returns null. ) we reset the value in this line
                    }
                }
            }
        }

        private Vector2 GetJointPoint(Joint joint)
        {

            DepthImagePoint point = this.kinectSensor.MapSkeletonPointToDepth(joint.Position, this.kinectSensor.DepthStream.Format);

            return new Vector2(point.X, point.Y);
        }

        String geometryDetection(Vector2 elbowLeft, Vector2 elbowRight, Vector2 handLeft, Vector2 handRight)
        {
            String result = "NA";

            float slope1 = (handLeft.Y - elbowLeft.Y) / (handLeft.X - elbowLeft.X);
            float slope2 = (handRight.Y - elbowRight.Y) / (handRight.X - elbowRight.X);
            float distance = Vector2.Distance(handLeft, handRight);
            if ((slope1 > 0.9 && slope1 < 1.1)
                && (slope2 > -1.1 && slope2 < -0.9)
                && (distance < 0.1))
                result = "RESTART";

            if ((slope1 > -0.9 && slope1 < 0.1)
                && (slope2 > -0.9 && slope2 < 0.1)
                && (distance < 0.1))
                result = "CLEAR";

            if (slope1 > 10 && slope2 > 10 && distance > 0.4)
                result = "END";

            return result;
        }

        private void DiscoverKinectSensor()
        {
            foreach (KinectSensor sensor in KinectSensor.KinectSensors)
            {
                if (sensor.Status == KinectStatus.Connected)
                {
                    // Found one, set our sensor to this
                    kinectSensor = sensor;
                    break;
                }
            }

            if (this.kinectSensor == null)
            {
                connectedStatus = "Found none Kinect Sensors connected to USB";
                return;
            }

            switch (kinectSensor.Status)
            {
                case KinectStatus.Connected:
                    {
                        connectedStatus = "Status: Connected";
                        break;
                    }
                case KinectStatus.Disconnected:
                    {
                        connectedStatus = "Status: Disconnected";
                        break;
                    }
                case KinectStatus.NotPowered:
                    {
                        connectedStatus = "Status: Connect the power";
                        break;
                    }
                default:
                    {
                        connectedStatus = "Status: Error";
                        break;
                    }
            }

            // Init the found and connected device
            if (kinectSensor.Status == KinectStatus.Connected)
            {
                InitializeKinect();
            }
        }
        
        private void adjustKinectSensor()
        {
            if (kinectSensor != null && kinectSensor.IsRunning)
            {

                if (headPosition.Y > (BACKBUFFER_HEIGHT * 2 / 8))
                {
                    try
                    {
                        if (kinectSensor.ElevationAngle > -15)
                            kinectSensor.ElevationAngle -= 3;
                    }
                    catch (InvalidOperationException ex1)
                    {
                    }
                }

                if (headPosition.Y < (BACKBUFFER_HEIGHT * 1 / 8))
                {

                    try
                    {
                        if (kinectSensor.ElevationAngle < 15)
                            kinectSensor.ElevationAngle += 3;
                    }
                    catch (InvalidOperationException ex2)
                    {
                    }
                }
            }
        }

        private bool InitializeKinect()
        {
            
            // Skeleton Stream
            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters()
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            });
            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);

            // Color stream
            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            kinectSensor.DepthStream.Range = (DepthRange)0;

            colorPointArray = new ColorImagePoint[kinectSensor.DepthStream.FramePixelDataLength];
            playerPixelArray = new byte[640 * 480 * 4];

            kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinect_AllFramesReady);



            try
            {
                kinectSensor.Start();
                kinectSensor.ElevationAngle = -10;
            }
            catch
            {
                connectedStatus = "Unable to start the Kinect Sensor";
                return false;
            }
            return true;
        }

        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            if (gameState == GameStates.WIN)
            {
                using (var colorImageFrame = e.OpenColorImageFrame())
                {
                    if (colorImageFrame == null) return;
                    colorArray = new Byte[colorImageFrame.PixelDataLength];
                    colorImageFrame.CopyPixelDataTo(colorArray);
                }

                DepthImageFrame depthImageFrame;
                using (depthImageFrame = e.OpenDepthImageFrame())
                {
                    if (depthImageFrame == null) return;

                    depthArray = new short[depthImageFrame.PixelDataLength];
                    depthImageFrame.CopyPixelDataTo(depthArray);

                    mappedDepthLocations = new ColorImagePoint[depthImageFrame.PixelDataLength];

                    kinectSensor.MapDepthFrameToColorFrame(DepthImageFormat.Resolution640x480Fps30, depthArray, ColorImageFormat.RgbResolution640x480Fps30, mappedDepthLocations);



                    //depthArray = DepthSmoother.CreateSmoothImageFromDepthArray(depthImageFrame);
                    //depthArray = DepthSmoother.CreateAverageDepthArray(depthArray);
                    kinectSensor.MapDepthFrameToColorFrame(DepthImageFormat.Resolution640x480Fps30,
                                                         depthArray,
                                                         ColorImageFormat.RgbResolution640x480Fps30,
                                                         colorPointArray);
                    
                }

                
                reanderPlayer();
            }
           
            
        }


        private void reanderPlayer()
        {
            Color[] playerPixelArray = new Color[kinectSensor.ColorStream.FrameHeight * kinectSensor.ColorStream.FrameWidth];
            int index = 0;
            int depthIndex = 0;
            Microsoft.Xna.Framework.Point reanderP = new Microsoft.Xna.Framework.Point((int)getHeadPosition.X, (int)getHeadPosition.Y);
            for (int y = 0; y < kinectSensor.ColorStream.FrameHeight; y++)
            {
                for (int x = 0; x < kinectSensor.ColorStream.FrameWidth; x++, index += 4)
                {
                    int playerIndex = depthArray[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                    int depth = depthArray[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                    //if ((playerIndex > 0) || ((x > reanderP.X) && (x < (reanderP.X + 60)) && (y > reanderP.Y) && (y < (reanderP.Y + 40))))
                    if ((playerIndex > 0) || (((x > reanderP.X - headSholderLength) && (x < (reanderP.X + headSholderLength)) && (y > reanderP.Y - headSholderLength) && (y < (reanderP.Y + headSholderLength))) && (((depth >= (headDepth - 50)) && (depth <= (headDepth))) || (depth == -1))))
                    //if ((playerIndex > 0) || (((x > reanderP.X) && (x < (reanderP.X + 100)) && (y > reanderP.Y) && (y < (reanderP.Y + 80))) && (depth == -1)))
                    //if(playerIndex > 0)
                    {
                        ColorImagePoint point = mappedDepthLocations[depthIndex];
                        int baseIndex = (point.Y * 640 + point.X) * 4;
                        if ((point.X >= 0 && point.X < 640) && (point.Y >= 0 && point.Y < 480))
                            playerPixelArray[y * kinectSensor.ColorStream.FrameWidth + x] = new Color(colorArray[baseIndex + 2], colorArray[baseIndex + 1], colorArray[baseIndex + 0]);
                    }
                    depthIndex++;
                }
            }
            player = new Texture2D(graphics.GraphicsDevice, kinectSensor.ColorStream.FrameWidth, kinectSensor.ColorStream.FrameHeight);
            player.SetData(playerPixelArray);
        }

        protected override void Initialize()
        {
            KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);
            DiscoverKinectSensor();
            base.Initialize();
        }

        void clearFog()
        {
            for (int i = 0; i < fogX; i++)
            {
                for (int j = 0; j < fogY; j++)
                {
                    fog[i, j] = 0;
                }
            }
        }

        int remainFog()
        {
            int cnt = 0;
           
            for (int i = 0; i < fogX; i++)
            {
                for (int j = 0; j < fogY; j++)
                {
                    if(fog[i, j] >= 1)cnt++;
                }
            }

            return cnt;
        }

        void intializeFog()
        {
            System.Random RandNum = new System.Random();

            int cleanTimes = 1;

            string cleanTimesString = Properties.CleanWindow.Default.cleanTimes;
            List<string> cleanTimesList = cleanTimesString.Split(';').ToList();

            if ((cleanTimesList.Count > 0) && (!String.IsNullOrEmpty(cleanTimesList[0])))
            {
                if (STAGE <= cleanTimesList.Count)
                {
                    cleanTimes = int.Parse(cleanTimesList[STAGE - 1]);
                }
                else
                {
                    cleanTimes = int.Parse(cleanTimesList[cleanTimesList.Count - 1]);
                }
            }
            else
            {
                cleanTimes = STAGE;
            } 
            
            for (int i = 0; i < (int)(fogX); i++)
            {
                for (int j = 0; j < (int)(fogY); j++)
                {
                    int fogRandomTextureNo = RandNum.Next(0, fogTextures.Count);
                    fog[i, j] = cleanTimes;
                    fogCleanRelease[i, j] = DateTime.Now;
                    fogLocation[i, j] = new Vector2(110 + i * 10 + RandNum.Next(10), 70 + j * 10 + RandNum.Next(12));
                    fogSprite[i, j] = new Sprite(fogLocation[i, j], fogTextures[fogRandomTextureNo], fogTextures[fogRandomTextureNo].Bounds, Vector2.Zero);
                }
            }
        }

        void DrawSmoke(int kind, Vector2 position)
        {
            if (kind == 0)
            {
                spriteBatch.Draw(smokeTexture, new Rectangle((int)position.X, (int)position.Y, smokeTexture.Width / 2, smokeTexture.Height / 2), new Rectangle(0, 0, smokeTexture.Width, smokeTexture.Height), Color.White);
            }
            else if (kind == 1)
            {
                spriteBatch.Draw(smokeTexture, new Rectangle((int)position.X + smokeTexture.Width / 3, (int)position.Y - smokeTexture.Height / 2, smokeTexture.Width, smokeTexture.Height), new Rectangle(0, 0, smokeTexture.Width, smokeTexture.Height), Color.White);
            }
            else if (kind == 2)
            {
                spriteBatch.Draw(smokeTexture, new Rectangle((int)position.X + smokeTexture.Width, (int)position.Y - (int)(smokeTexture.Height * 1.0f), (int)(smokeTexture.Width * 1.5f), (int)(smokeTexture.Height * 1.5f)), new Rectangle(0, 0, smokeTexture.Width, smokeTexture.Height), Color.White);
            }
        }

        List<string> GetTextureNameBySearchFile(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            List<string> tempList = new List<string>();            
            foreach (FileInfo fi in dir.GetFiles())
            {
                tempList.Add(fi.Name);           //get textures name by search file
            }
            if (tempList.Count <= 0)
            {
                formMessage.Message_NOTexture();    //show message
                Exit();
            }
            return tempList;
        }

        List<string> GetFileNameBySearchFile(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            List<string> tempList = new List<string>();
            foreach (DirectoryInfo dChild in dir.GetDirectories())
            {
                tempList.Add(dChild.Name);           //get file name by search file
            }
            if (tempList.Count <= 0)
            {
                formMessage.Message_NOFile();    //show message
                Exit();
            }
            return tempList;
        }

        protected override void LoadContent()
        {
            float clientBoundsWidth = (float)GraphicsDevice.Viewport.Width;              //��ȡ�ͻ��˿�
            float clientBoundsHeight = (float)GraphicsDevice.Viewport.Height;            //��ȡ�ͻ��˸�
            Console.WriteLine("����" + clientBoundsWidth + "  �ߣ�" + clientBoundsHeight);
            scaleRate.X = clientBoundsWidth / 1280;
            scaleRate.Y = clientBoundsHeight / 768;

            basePath = Properties.CleanWindow.Default.basePath + "\\";

            string routeString = Properties.CleanWindow.Default.route;
            routeList = routeString.Split(';').ToList();
            if ((routeList.Count > 0) && (!String.IsNullOrEmpty(routeList[0])))     //if set route
            {
                routeFlag = true;
                TotalStage = routeList.Count;
            }
            else
            {
                routeFlag = false;
                routeList = GetFileNameBySearchFile(basePath + Properties.CleanWindow.Default.backgroundPath);

                if (routeList.Count > 0)       //The background folder number is the total stage number
                {
                    TotalStage = routeList.Count;
                }
            }


            //if (Properties.CleanWindow.Default.totalStage > 0)
            //{
            //    TotalStage = Properties.CleanWindow.Default.totalStage;
            //}
            if ((Properties.CleanWindow.Default.minAlpha >= 0) && (Properties.CleanWindow.Default.minAlpha < 1))
            {
                minAlpha = Properties.CleanWindow.Default.minAlpha;
            }

            if (Properties.CleanWindow.Default.albumShowTime_Seconds > 0)
            {
                albumShowTime = Properties.CleanWindow.Default.albumShowTime_Seconds * 1000;
            }

            spriteBatch = new SpriteBatch(GraphicsDevice);

            //add pass marks
            for (int i = 1; i <= 6; i++)
            {
                marks.Add(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + "Textures\\passbar\\mark0"+i+".png")));
            }
            


            particleTextures.Add(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.particleTextures)));
            magicTextures.Add(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.magicTextures)));

            startButton = new Sprite(new Vector2(60, 400), Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.startButton)), new Rectangle(0, 0, 228, 165), Vector2.Zero);
            startButtonPressed = new Sprite(new Vector2(-1000, -100), Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.startButtonPressed)), new Rectangle(0, 0, 285, 206), Vector2.Zero);
            //helpButton = new Sprite(new Vector2(200, 150), Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.helpButton)), new Rectangle(0, 0, 235, 125), Vector2.Zero);
            //helpButtonPressed = new Sprite(new Vector2(-1000, -100), Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.helpButtonPressed)), new Rectangle(0, 0, 294, 156), Vector2.Zero);
            //settingButton = new Sprite(new Vector2(450, 50), Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.settingButton)), new Rectangle(0, 0, 247, 131), Vector2.Zero);
            //settingButtonPressed = new Sprite(new Vector2(-1000, -100), Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.settingButtonPressed)), new Rectangle(0, 0, 309, 165), Vector2.Zero);

            List<string> fogSequenceList = GetTextureNameBySearchFile(basePath + Properties.CleanWindow.Default.fogTextures_BasePath);           
            foreach(string s in fogSequenceList)
            {
                fogTextures.Add(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.fogTextures_BasePath + "\\" + s)));
            }
            intializeFog();

            takePhotoTextures.Add(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.takePhotoTextures_01)));
            takePhotoTextures.Add(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.takePhotoTextures_02)));
            takePhotoTextures.Add(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.takePhotoTextures_03)));
            takePhotoTextures.Add(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.takePhotoTextures_04)));
            takePhotoTextures.Add(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.takePhotoTextures_05)));
            takePhotoTextures.Add(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.takePhotoTextures_06)));

            leftHandSpriteTexture = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.leftHandSpriteTexture));
            rightHandSpriteTexture = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.rightHandSpriteTexture));
           

            //Ԥ���ر���ͼƬ
            gameBackgrounds["mainmenu"] = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.gb_Main));
            gameBackgrounds["game1bg"] = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.gb_01));
            gameBackgrounds["game2bg"] = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.gb_02));
            gameBackgrounds["game3bg"] = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.gb_03));

            leftHandSprite = new Sprite(new Vector2(-100, -100), Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.leftHandSprite)), new Rectangle(0, 0, 80, 134), Vector2.Zero);
            rightHandSprite = new Sprite(new Vector2(-100, -100), Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.rightHandSprite)), new Rectangle(0, 0, 80, 134), Vector2.Zero);
            leftHandManager = new PlayerManager(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.leftHandManager)), 10, new Rectangle(30, 30, 1250, 738));
            rightHandManager = new PlayerManager(Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.rightHandManager)), 10, new Rectangle(30, 30, 1250, 738));

            magicEngineLeftHand = new MagicEngine(magicTextures, new Vector2(300, 300), 0);
            magicEngineRightHand = new MagicEngine(magicTextures, new Vector2(300, 300), 0);
            magicEngineProgressBar = new MagicEngine(magicTextures, Vector2.Zero, 1);
            soundChim = Content.Load<SoundEffect>("Sounds\\chime2");
            soundApplus = Content.Load<SoundEffect>("Sounds\\applause");
            soundCamera = Content.Load<SoundEffect>("Sounds\\camera1");
            soundCheering = Content.Load<SoundEffect>("Sounds\\cheering2");
            soundWipe = Content.Load<SoundEffect>("Sounds\\wipe");
            soundClick = Content.Load<SoundEffect>("Sounds\\Click02");           
            //soundChim = new SoundEffect(File.ReadAllBytes(Properties.CleanWindow.Default.soundChim), 22050, AudioChannels.Mono);
            //soundApplus = new SoundEffect(File.ReadAllBytes(Properties.CleanWindow.Default.soundApplus), 22050, AudioChannels.Mono);
            //soundCamera = new SoundEffect(File.ReadAllBytes(Properties.CleanWindow.Default.soundCamera), 11025, AudioChannels.Mono);
            //soundCheering = new SoundEffect(File.ReadAllBytes(Properties.CleanWindow.Default.soundCheering), 11025, AudioChannels.Mono);
            //soundWipe = new SoundEffect(File.ReadAllBytes(Properties.CleanWindow.Default.soundWipe), 22050, AudioChannels.Mono);
            //soundClick = new SoundEffect(File.ReadAllBytes(Properties.CleanWindow.Default.soundClick), 22050, AudioChannels.Mono);

            takephotoready = Content.Load<SoundEffect>("Sounds\\takephotoready");
            takephoto1 = Content.Load<SoundEffect>("Sounds\\takephoto1");
            takephoto2 = Content.Load<SoundEffect>("Sounds\\takephoto2");
            takephoto3 = Content.Load<SoundEffect>("Sounds\\takephoto3");
            takephotosmile = Content.Load<SoundEffect>("Sounds\\takephotosmile");
            takephotoshutter = Content.Load<SoundEffect>("Sounds\\takephotoshutter");


            //bgMusic = Content.Load<Song>("Music\\menumusic");
            //Ԥ���ر�������
            bgMusics[STRING_MEUN_MUSIC] = Content.Load<Song>("Music\\menumusic");
            bgMusics["trainsition"] = Content.Load<Song>("Music\\transition");
            bgMusics["1"] = Content.Load<Song>("Music\\bgmusic1");
            bgMusics["2"] = Content.Load<Song>("Music\\bgmusic2");
            bgMusics["3"] = Content.Load<Song>("Music\\bgmusic3");

            MediaPlayer.IsRepeating = true;

            //Ԥ����trainsitionͼƬ

            List<string> transitionMapName = GetTextureNameBySearchFile(basePath + Properties.CleanWindow.Default.transitionMap);            
            transitionMap = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.transitionMap + "\\" + transitionMapName[0]));

            List<string> smokeList = GetTextureNameBySearchFile(basePath + Properties.CleanWindow.Default.transitionTrainTexture + "\\smoke");            
            smokeTexture = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.transitionTrainTexture + "\\smoke" + "\\" + smokeList[0]));
           

            screenShot = new RenderTarget2D(graphics.GraphicsDevice, graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                                                         graphics.GraphicsDevice.PresentationParameters.BackBufferHeight);
            
            //��CleanWindow.settings�ļ��У���ȡ·��
            capturesPath = basePath + Properties.CleanWindow.Default.capturesPath;

            stageTexture = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.stageTexture));
            stageNumberTexture = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.stageNumberTexture));
            

            albumTexture = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.albumTexturePath));
            progressBarTexture = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.progressBarTexture));

            sharedata = JsonConvert.DeserializeObject<ShareData>(File.ReadAllText(Directory.GetCurrentDirectory() + "\\share.json"));

            sharedata.Name = "CleanWindow";
            sharedata.IsRunning = "true";
            sharedata.StartTime = DateTime.Now.ToString();
            sharedata.GameNo = "3";
            sharedata.IsSendResults = "false";

            //sharedata = new ShareData
            //{
            //    Name = "CleanWindow",
            //    IsRunning = "true",
            //    Operate = "",
            //    StartTime = DateTime.Now.ToString(),
            //    EndTime = "",
            //    GameNo = "3",
            //    IsSendResults = "false",
            //    Score1 = "",
            //    Score2 = "",
            //    Score3 = ""
            //};
            //File.WriteAllText("C:\\log.txt", Directory.GetCurrentDirectory());
            // serialize JSON to a string and then write string to a file
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\share.json", JsonConvert.SerializeObject(sharedata));
        }

        protected override void UnloadContent()
        {
            kinectSensor.Stop();
            kinectSensor.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {

            KeyboardState newState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) Exit();
            if (newState.IsKeyDown(Keys.Escape))Exit();

            // read file into a string and deserialize JSON to a type
            readShareTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (readShareTime >= 3.0f)
            {
                readShareTime = 0;
                sharedata = JsonConvert.DeserializeObject<ShareData>(File.ReadAllText(Directory.GetCurrentDirectory() + "\\share.json"));
                //MessageBox.Show(sharedata.Operate);
                if (sharedata.Operate == "stop")
                {
                    //MessageBox.Show(sharedata.Operate);
                    sharedata.EndTime = DateTime.Now.ToString();
                    sharedata.IsRunning = "false";
                    sharedata.Operate = "";
                    sharedata.Score1 = "�����"+ STAGE +"�P";
                    sharedata.Score2 = STAGE.ToString();
                    // serialize JSON to a string and then write string to a file
                    File.WriteAllText(Directory.GetCurrentDirectory() + "\\share.json", JsonConvert.SerializeObject(sharedata));
                    this.Exit();
                }
            }
               
            switch (gameState)
            {
                
                #region Goto main menu
                case GameStates.GO_MAINMENU:

                    startConfirmButtonPressed = false;
                    //helpConfirmButtonPressed = false;
                    //settingConfirmButtonPressed = false;
                   //gameBackground = Content.Load<Texture2D>("Textures\\background\\mainmenu");
                    gameBackground = gameBackgrounds["mainmenu"];
                    gameState = GameStates.MAINMENU;
                    
                    //bgMusic = Content.Load<Song>("Music\\menumusic");
                    bgMusic = bgMusics["menumusic"];
                    MediaPlayer.IsRepeating = true;
                    bgMusicStart = false;
                    STAGE = 1;

                    break;
                #endregion

                #region Manin Menu
                case GameStates.MAINMENU:

                    if (!bgMusicStart)
                    {
                        MediaPlayer.Volume = 0.2f;
                        MediaPlayer.Play(bgMusic);
                        bgMusicStart = true;
                    }
                    leftHandSprite.Location = leftHandPosition;
                    rightHandSprite.Location = rightHandPosition;

                    startButton.Update(gameTime);
                    startButtonPressed.Update(gameTime);
                    //helpButton.Update(gameTime);
                    //helpButtonPressed.Update(gameTime);
                    //settingButton.Update(gameTime);
                    //settingButtonPressed.Update(gameTime);

                    //Start button
                    if ((startButton.IsCircleColliding(leftHandSprite.Center, 70) ||
                                startButton.IsCircleColliding(rightHandSprite.Center, 70)))
                    {
                        startButtonPressed.Location = new Vector2(40, 380);
                        if (isStartButtonPressed == false)
                        {
                            soundClick.Play();
                            startPressedStartTime = DateTime.Now;
                            isStartButtonPressed = true;
                        }
                        if (isStartButtonPressed)
                        {
                            TimeSpan elapsedTime = DateTime.Now - startPressedStartTime;
                            if (startConfirmButtonPressed == false && elapsedTime.Seconds > 0.5)
                            {
                                startConfirmPressedStartTime = DateTime.Now;
                                startConfirmButtonPressed = true;
                                soundChim.Play();
                            }
                            
                        }
                        
                    }
                    else
                    {
                        startButtonPressed.Location = new Vector2(-1000, -100);
                        isStartButtonPressed = false;                        
                    }

                    if (startConfirmButtonPressed)
                    {
                        System.Random RandNum = new System.Random();
                        int rx = RandNum.Next(1, 20);
                        int ry = RandNum.Next(1, 20);
                        startButtonPressed.Location = new Vector2(30 + rx, 370 + ry);

                        TimeSpan elapsedTime2 = DateTime.Now - startConfirmPressedStartTime;
                        if (elapsedTime2.Milliseconds > 750)
                        {
                            gameState = GameStates.GO_TRANSITION;
                            transitionStartTime = DateTime.Now;
                        }
                    }


                    //Help button
                    //if ((helpButton.IsCircleColliding(leftHandSprite.Center, 70) ||
                    //                helpButton.IsCircleColliding(rightHandSprite.Center, 70)))
                    //{
                    //    helpButtonPressed.Location = new Vector2(180, 130);
                    //    if (isHelpButtonPressed == false)
                    //    {
                    //        soundClick.Play();
                    //        helpPressedStartTime = DateTime.Now;
                    //        isHelpButtonPressed = true;
                    //    }
                    //    if (isHelpButtonPressed)
                    //    {
                    //        TimeSpan elapsedTime = DateTime.Now - helpPressedStartTime;
                    //        if (helpConfirmButtonPressed == false && elapsedTime.Seconds > 2)
                    //        {
                    //            helpConfirmPressedStartTime = DateTime.Now;
                    //            helpConfirmButtonPressed = true;
                    //            soundChim.Play();
                    //        }

                    //    }
                        
                    //}
                    //else
                    //{
                    //    helpButtonPressed.Location = new Vector2(-1000, -100);
                    //    isHelpButtonPressed = false;    
                    //}

                    //Setting button
                    //if ((settingButton.IsCircleColliding(leftHandSprite.Center, 70) ||
                    //                settingButton.IsCircleColliding(rightHandSprite.Center, 70)))
                    //{
                    //    settingButtonPressed.Location = new Vector2(430, 30);
                    //    if (isSettingButtonPressed == false)
                    //    {
                    //        soundClick.Play();
                    //        settingPressedStartTime = DateTime.Now;
                    //        isSettingButtonPressed = true;
                    //    }
                    //    if (isSettingButtonPressed)
                    //    {
                    //        TimeSpan elapsedTime = DateTime.Now - settingPressedStartTime;
                    //        if (settingConfirmButtonPressed == false && elapsedTime.Seconds > 2)
                    //        {
                    //            settingConfirmPressedStartTime = DateTime.Now;
                    //            settingConfirmButtonPressed = true;
                    //            soundChim.Play();
                    //        }

                    //    }
                    //}
                    //else
                    //{
                    //    settingButtonPressed.Location = new Vector2(-1000, -100);
                    //    isSettingButtonPressed = false; 
                    //}

                    leftHandSprite.Update(gameTime);
                    rightHandSprite.Update(gameTime);

                    //For Testing
                    if (newState.IsKeyDown(Keys.F7))
                        gameState = GameStates.GO_TRANSITION;
                    
                    break;

                #endregion

                #region Goto transistion
                case GameStates.GO_TRANSITION:
                    sceneCount = 1;

                    #region Get PLAYTIMEOUT by Setting file
                    string playTimeOutString = Properties.CleanWindow.Default.playTimeOut;
                    List<string> playTimeOutList = playTimeOutString.Split(';').ToList();

                    if ((playTimeOutList.Count > 0) && (!String.IsNullOrEmpty(playTimeOutList[0])))
                    {
                        if (STAGE <= playTimeOutList.Count)
                        {
                            PLAYTIMEOUT = int.Parse(playTimeOutList[STAGE - 1]) * 1000;
                        }
                        else
                        {
                            PLAYTIMEOUT = int.Parse(playTimeOutList[playTimeOutList.Count - 1]) * 1000;
                        }
                    }
                    else
                    {
                        PLAYTIMEOUT = 60000;     
                    }
	                #endregion

                    #region Get PASSPERCENTAGE by Setting file
                    string passPercentageString = Properties.CleanWindow.Default.PASSPERCENTAGE;
                    List<string> passPercentageList = passPercentageString.Split(';').ToList();
                    if ((passPercentageList.Count > 0) && (!String.IsNullOrEmpty(passPercentageList[0])))
                    {
                        if (STAGE <= passPercentageList.Count)
                        {
                            PASSPERCENTAGE = int.Parse(passPercentageList[STAGE - 1]);
                        }
                        else
                        {
                            PASSPERCENTAGE = int.Parse(passPercentageList[passPercentageList.Count - 1]);
                        }
                    }
                    else
                    {
                        PASSPERCENTAGE = 80;
                    }
                    if ((PASSPERCENTAGE < 0) || (PASSPERCENTAGE > 100))
                    {
                        PASSPERCENTAGE = 80;
                    }
                    FORGIVECOUNT = fogX * fogY * (100 - PASSPERCENTAGE) / 100;
                    #endregion
                    
                    #region Random to get transitionTrain

                    List<string> trainsList = GetTextureNameBySearchFile(basePath + Properties.CleanWindow.Default.transitionTrainTexture);                    
                    string s = trainsList[new Random().Next(0, trainsList.Count)];

                    transitionTrainTexture = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.transitionTrainTexture + "\\" + s));
                    //transitionTrain = new Sprite(new Vector2(1300, 400), transitionTrainTexture, transitionTrainTexture.Bounds , Vector2.Zero);
                    if (s == "train.png")
                    {
                        trainKind = 0;
                    }
                    else if (s == "ship.png")
                    {
                        trainKind = 1;
                    }
                    else //if (s == "plane.png")
                    {
                        trainKind = 2;
                    }
                    transitionTrainPosition = new Vector2(1300, 400);
	                #endregion

                    #region Set the texture position by the using folder name

                    int temp = 0;
                    if (routeFlag == false)
                    {
                        temp = new Random().Next(0, routeList.Count);
                        bgString = routeList[temp];
                        routeList.Remove(routeList[temp]);
                    }
                    else
                    {
                        bgString = routeList[0];
                        routeList.Remove(routeList[0]);
                    }

                    while (countryList.Count > 0)
                    {
                        countryList.Remove(countryList[0]);
                    }
                    countryList = GetFileNameBySearchFile(basePath + Properties.CleanWindow.Default.backgroundPath + "\\" + bgString);  //get all country files name                    


                    List<string> transitionMapName = GetTextureNameBySearchFile(basePath + Properties.CleanWindow.Default.backgroundPath + "\\" + bgString);                    
                    transitionMapLocation = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.backgroundPath +
                        "\\" + bgString + "\\" + transitionMapName[0]));

	                #endregion

                    gameState = GameStates.TRANSITION;
                    transitionStartTime = DateTime.Now;

                    bgMusic = bgMusics["trainsition"];
                    MediaPlayer.IsRepeating = true;
                    bgMusicStart = false;
                    transitionFlashing = 0;
                    break;
                #endregion

                #region Transition
                case GameStates.TRANSITION:
                    if (!bgMusicStart)
                    {
                        MediaPlayer.Volume = 0.2f;
                        MediaPlayer.Play(bgMusic);
                        bgMusicStart = true;
                    }

                    transitionFlashing += gameTime.ElapsedGameTime.Milliseconds;
                    if ((transitionFlashing % 30) == 0)
                    {
                        if (drawFlashing == false) 
                            drawFlashing = true; 
                        else 
                            drawFlashing = false;
                    }
                    if ((transitionFlashing % 50) == 0)
                    {
                        smokeKind++;
                        if (smokeKind > 2)
                        {
                            smokeKind = 0;
                        }
                    }

                    transitionTrainPosition.X -= 5;
                    //transitionTrain.Location = transitionTrainPosition;
                    //if (transitionTrainPosition.X < -(transitionTrain.BoundingBoxRect.Width + 50))
                    if (transitionTrainPosition.X < -1050)
                    {
                        gameState = GameStates.GO_PLAY;
                    }

                    break;
                #endregion

                #region Goto play
                case GameStates.GO_PLAY:
                    
                    temp = new Random().Next(0, countryList.Count);
                    countryString = countryList[temp];
                    countryList.Remove(countryList[temp]);


                    List<string> bgNameList = GetTextureNameBySearchFile(basePath + Properties.CleanWindow.Default.backgroundPath + "\\" + bgString + "\\" + countryString + "\\background");                    
                    temp = new Random().Next(0, bgNameList.Count);
                    playSceneTexture = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.backgroundPath +
                        "\\" + bgString + "\\" + countryString + "\\background\\" + bgNameList[temp]));


                    List<string> countryNameList = GetTextureNameBySearchFile(basePath + Properties.CleanWindow.Default.backgroundPath + "\\" + bgString + "\\" + countryString);                    
                    playSceneNameTexture = Texture2D.FromStream(GraphicsDevice, File.OpenRead(basePath + Properties.CleanWindow.Default.backgroundPath +
                        "\\" + bgString + "\\" + countryString + "\\" + countryNameList[0]));


                    //bgMusic = Content.Load<Song>("Music\\bgmusic"+STAGE);
                    bgMusic = bgMusics[((STAGE - 1) % 3 + 1).ToString()];
                    MediaPlayer.IsRepeating = true;
                    bgMusicStart = false;
                    
                    leftHandPosition = new Vector2(-100,-100);
                    rightHandPosition = new Vector2(-100,-100);
                    leftHandOldPosition = new Vector2(-100, -100);
                    rightHandOldPosition = new Vector2(-100, -100);
                    
                    intializeFog();

                    if (((STAGE - 1) % 3 + 1) == 1) gameBackground = gameBackgrounds["game1bg"];
                    if (((STAGE - 1) % 3 + 1) == 2) gameBackground = gameBackgrounds["game2bg"];
                    if (((STAGE - 1) % 3 + 1) == 3) gameBackground = gameBackgrounds["game3bg"];

                //leftHandSprite = new Sprite(new Vector2(-100, -100), Content.Load<Texture2D>("Textures\\glove\\glove_hold_left"), new Rectangle(0, 0, 112, 103), Vector2.Zero);
                //rightHandSprite = new Sprite(new Vector2(-100, -100), Content.Load<Texture2D>("Textures\\glove\\glove_hold_right"), new Rectangle(0, 0, 112, 103), Vector2.Zero);
                    leftHandSprite = new Sprite(new Vector2(-100, -100),leftHandSpriteTexture, new Rectangle(0, 0, 112, 103), Vector2.Zero);
                    rightHandSprite = new Sprite(new Vector2(-100, -100), rightHandSpriteTexture, new Rectangle(0, 0, 112, 103), Vector2.Zero);
                    gameState = GameStates.PLAY;

                    playingTimeOut = 0;
                        
                    break;
                #endregion

                #region Play
                case GameStates.PLAY:

                    playingTimeOut += gameTime.ElapsedGameTime.Milliseconds;
                    if (playingTimeOut > PLAYTIMEOUT)
                    {
                        gameState = GameStates.GO_WIN;
                    }

                    if (!bgMusicStart)
                    {
                        MediaPlayer.Volume = 1f;
                        MediaPlayer.Play(bgMusic);
                        bgMusicStart = true;
                    }

                    //For Testing
                    if (newState.IsKeyDown(Keys.F5))
                        intializeFog();

                    //For Testing
                    if (newState.IsKeyDown(Keys.F6))
                        clearFog();

                    if (remainFog() < FORGIVECOUNT)
                    {
                        gameState = GameStates.GO_WIN;
                    }

                    for (int i = 0; i < fogX; i++)
                    {
                        for (int j = 0; j < fogY; j++)
                        {
                            if (fog[i, j] > 0)
                            {
                                if ((fogSprite[i, j].IsCircleColliding(leftHandManager.playerSprite.Center, 60) ||
                                fogSprite[i, j].IsCircleColliding(rightHandManager.playerSprite.Center, 60)))
                                {
                                    TimeSpan fogReleaseElapsedTime = DateTime.Now - fogCleanRelease[i,j];
                                    if (fogReleaseElapsedTime.Seconds > 0)
                                    {
                                        fog[i, j] -= 1;
                                        if (fog[i, j] == 0)
                                        {
                                            fogSprite[i, j].alpha = 0;
                                        }
                                        else
                                        {
                                            fogSprite[i, j].alpha = 0.3f * fog[i, j] + minAlpha;
                                        }
                                        fogCleanRelease[i, j] = DateTime.Now;
                                    }
                                    
                                }
                            }
                            fogSprite[i, j].Update(gameTime);
                        }
                    }

                    leftHandSprite.Location = leftHandPosition;
                    rightHandSprite.Location = rightHandPosition;
                    
                    magicEngineLeftHand.EmitterLocation = leftHandManager.playerSprite.Center;
                    magicEngineLeftHand.Update();
                    magicEngineRightHand.EmitterLocation = rightHandManager.playerSprite.Center;
                    magicEngineRightHand.Update();
                    magicEngineProgressBar.EmitterLocation = new Vector2(BACKBUFFER_WIDTH / (TotalStage * ROUNDCNT) * ((STAGE - 1) * ROUNDCNT + sceneCount - 1), 758*scaleRate.Y);
                    magicEngineProgressBar.Update();

                    leftHandManager.playerSprite.Location = leftHandPosition;
                    rightHandManager.playerSprite.Location = rightHandPosition;

                    rightHandManager.playerSprite.Location = new Vector2(rightHandManager.playerSprite.Location.X - 10, rightHandManager.playerSprite.Location.Y - 20);
                    leftHandManager.playerSprite.Location = new Vector2(leftHandManager.playerSprite.Location.X - 20, leftHandManager.playerSprite.Location.Y - 20);

                    leftHandManager.Update(gameTime);
                    rightHandManager.Update(gameTime);
                    leftHandSprite.Update(gameTime);
                    rightHandSprite.Update(gameTime);
                    
                    break;
                #endregion

                #region Goto Win
                case GameStates.GO_WIN:

                        if (MediaPlayer.State == MediaState.Playing) MediaPlayer.Stop();
                        bgMusicStart = false;

                        takePhotoSouldPlayed1 = false;
                        takePhotoSouldPlayed2 = false;
                        takePhotoSouldPlayed3 = false;
                        takePhotoSouldPlayed4 = false;
                        takePhotoSouldPlayed5 = false;
                        takePhotoSouldPlayed6 = false;

                        takePhotoStart = false;
                        takePhotoOK = false;

                        timeoutCount = 0;

                        soundEffectCheeringInstance = soundCheering.CreateInstance();
                        soundEffectApplusInstance = soundApplus.CreateInstance();

                        photoFrameTexture = takePhotoTextures[0];
                        leftHandPosition = new Vector2(-100, -100);
                        rightHandPosition = new Vector2(-100, -100);
                        clearFog();
                        magicEngineLeftHand = new MagicEngine(magicTextures, new Vector2(-100, -100), 0);
                        magicEngineRightHand = new MagicEngine(magicTextures, new Vector2(-100, -100), 0);
                        magicEngineProgressBar = new MagicEngine(magicTextures, new Vector2(-100, -100), 1);
                        gameState = GameStates.WIN;

                        if (keepPhotos.Count == 0)
                        {
                            filePath = capturesPath + "\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + fileID + "\\";
                            while (Directory.Exists(filePath))
                            {
                                fileID++;
                                filePath = capturesPath + "\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + fileID + "\\";
                            }
                            Directory.CreateDirectory(filePath);
                        }
                        break;
                #endregion

                #region Win
                case GameStates.WIN:

                        if (soundEffectApplusInstance.State == SoundState.Stopped)
                        {
                            soundEffectApplusInstance.IsLooped = true;
                            soundEffectApplusInstance.Play();
                        }

                        if (soundEffectCheeringInstance.State == SoundState.Stopped)
                        {
                            soundEffectCheeringInstance.IsLooped = true;
                            soundEffectCheeringInstance.Play();
                        }

                        photoFrameTexture = takePhotoTextures[0];
                        if (!takePhotoSouldPlayed1)
                        {
                            takephotoready.Play();
                            takePhotoSouldPlayed1 = true;
                        }

                        timeoutCount += gameTime.ElapsedGameTime.Milliseconds;
                        int baseWaitTime = 5000;
                        if (timeoutCount > baseWaitTime)
                        {
                            photoFrameTexture = takePhotoTextures[1];
                            if (!takePhotoSouldPlayed2)
                            {
                                takephoto1.Play();
                                takePhotoSouldPlayed2 = true;
                            }
                        }
                        if (timeoutCount > baseWaitTime + 1000)
                        {
                            photoFrameTexture = takePhotoTextures[2];
                            if (!takePhotoSouldPlayed3)
                            {
                                takephoto2.Play();
                                takePhotoSouldPlayed3 = true;
                            }
                        }
                        if (timeoutCount > baseWaitTime + 2000)
                        {
                            photoFrameTexture = takePhotoTextures[3];
                            if (!takePhotoSouldPlayed4)
                            {
                                takephoto3.Play();
                                takePhotoSouldPlayed4 = true;
                            }
                        }
                        if (timeoutCount > baseWaitTime + 3000)
                        {
                            photoFrameTexture = takePhotoTextures[4];
                            if (!takePhotoSouldPlayed5)
                            {
                                takephotosmile.Play();
                                takePhotoSouldPlayed5 = true;
                            }
                        }
                        if (timeoutCount > baseWaitTime + 4000)
                        {
                            CameraFlashing = true;
                            photoFrameTexture = takePhotoTextures[5];
                            if (!takePhotoSouldPlayed6)
                            {
                                takephotoshutter.Play();
                                takePhotoSouldPlayed6 = true;
                            }
                        }
                        if (timeoutCount > baseWaitTime + 4500)
                        {
                            CameraFlashing = false;
                            photoFrameTexture = playSceneTexture;
                            if (takePhotoStart == false && takePhotoOK == false)
                            {
                                takePhotoStart = true;
                            }
                        }
                        if (timeoutCount > baseWaitTime + 8000)
                        {
                            soundEffectApplusInstance.Stop();
                            soundEffectCheeringInstance.Stop();

                            if (sceneCount++ >= ROUNDCNT)
                            {
                                STAGE++;
                                if (STAGE <= TotalStage)
                                    gameState = GameStates.GO_TRANSITION;
                                else
                                {
                                    albumNumber = (keepPhotos.Count - 1) / 9;       //get the album page number
                                    //gameState = GameStates.GO_MAINMENU;
                                    takePhotoStart = false;
                                    takePhotoOK = false;
                                    endGameTime = 0;
                                    gameState = GameStates.ENDGAME;
                                }
                            }
                            else
                            {
                                gameState = GameStates.GO_PLAY;
                            }


                        }

                        break;
                #endregion

                #region End Game
                case GameStates.ENDGAME:

                        endGameTime += gameTime.ElapsedGameTime.Milliseconds;

                        if (endGameTime >= albumShowTime / 2)
                        {
                            if (takePhotoStart == false && takePhotoOK == false)
                            {
                                takePhotoStart = true;
                            }
                        }
                        if (endGameTime >= albumShowTime)
                        {
                            endGameTime = 0;
                            endGameNumber++; 
                            takePhotoStart = false;
                            takePhotoOK = false;
                            if (endGameNumber > albumNumber)
                            {
                                endGameNumber = 0;
                                while (keepPhotos.Count > 0)
                                {
                                    keepPhotos.Remove(keepPhotos[0]);
                                }
                                while (keepPhotosName.Count > 0)
                                {
                                    keepPhotosName.Remove(keepPhotosName[0]);
                                }

                                if (routeFlag == true)
                                {
                                    string routeString = Properties.CleanWindow.Default.route;
                                    routeList = routeString.Split(';').ToList();                                //Read the folder name again
                                }
                                else
                                {
                                    routeList = GetFileNameBySearchFile(basePath + Properties.CleanWindow.Default.backgroundPath);       //Read the folder name again
                                }


                                gameState = GameStates.GO_MAINMENU;

                            }
                        }

                        break;
                #endregion
                default: break;


            }

            base.Update(gameTime);


        }
       
        protected override void Draw(GameTime gameTime)
        {
            if (takePhotoStart && takePhotoOK == false)GraphicsDevice.SetRenderTarget(screenShot);
                
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            spriteBatch.Begin();


            switch (gameState)
            {
                case GameStates.MAINMENU:

                        spriteBatch.Draw(gameBackground, new Rectangle(0, 0, (int)(gameBackground.Width*scaleRate.X), (int)(gameBackground.Height*scaleRate.Y)), Microsoft.Xna.Framework.Color.White);

                        startButton.Draw(spriteBatch);
                        startButtonPressed.Draw(spriteBatch);
                        //helpButton.Draw(spriteBatch);
                        //helpButtonPressed.Draw(spriteBatch);
                        //settingButton.Draw(spriteBatch);
                        //settingButtonPressed.Draw(spriteBatch);

                        leftHandSprite.Draw(spriteBatch);
                        rightHandSprite.Draw(spriteBatch);

                    break;

                case GameStates.TRANSITION:
                    spriteBatch.Draw(transitionMap, new Rectangle(0, 0, (int)(transitionMap.Width * scaleRate.X), (int)(transitionMap.Height * scaleRate.Y)), Microsoft.Xna.Framework.Color.White);
                    if (drawFlashing) spriteBatch.Draw(transitionMapLocation, new Rectangle(0, 0, (int)(transitionMap.Width * scaleRate.X), (int)(transitionMap.Height * scaleRate.Y)), Microsoft.Xna.Framework.Color.White);
                        //transitionMapName.Draw(spriteBatch);

                        //transitionTrain.Draw(spriteBatch);
                        if (trainKind == 0)
                        {
                            spriteBatch.Draw(transitionTrainTexture, new Rectangle((int)transitionTrainPosition.X, (int)transitionTrainPosition.Y, transitionTrainTexture.Width, transitionTrainTexture.Height), new Rectangle(0, 0, transitionTrainTexture.Width, transitionTrainTexture.Height), Color.White);
                            DrawSmoke(smokeKind, transitionTrainPosition + new Vector2(50, -50));
                        }
                        else if (trainKind == 1)
                        {
                            spriteBatch.Draw(transitionTrainTexture, new Rectangle((int)transitionTrainPosition.X, (int)transitionTrainPosition.Y, transitionTrainTexture.Width / 2, transitionTrainTexture.Height / 2), new Rectangle(0, 0, transitionTrainTexture.Width, transitionTrainTexture.Height), Color.White);
                            DrawSmoke(smokeKind, transitionTrainPosition + new Vector2(250, -50));
                        }
                        else //if (trainKind == 2)
                        {
                            spriteBatch.Draw(transitionTrainTexture, new Rectangle((int)transitionTrainPosition.X, (int)transitionTrainPosition.Y, transitionTrainTexture.Width / 2, transitionTrainTexture.Height / 2), new Rectangle(0, 0, transitionTrainTexture.Width, transitionTrainTexture.Height), Color.White);
                        }

                    break;

                case GameStates.PLAY:

                    spriteBatch.Draw(playSceneTexture, new Rectangle(0, 0, (int)(gameBackground.Width * scaleRate.X), (int)(gameBackground.Height * scaleRate.Y)), Microsoft.Xna.Framework.Color.White);
                        
                        for (int i = 0; i < fogX; i++)
                        {
                            for (int j = 0; j < fogY; j++)
                            {
                                if (fog[i, j] > 0)
                                {
                                    fogSprite[i, j].Draw(spriteBatch);
                                }
                            }
                        }

                        spriteBatch.Draw(gameBackground, new Rectangle(0, 0, (int)(gameBackground.Width*scaleRate.X), (int)(gameBackground.Height*scaleRate.Y)), Microsoft.Xna.Framework.Color.White);
                        //spriteBatch.Draw(gameWindowFrame, new Rectangle(0, 0, gameWindowFrame.Width, gameWindowFrame.Height), Color.White);

                        spriteBatch.Draw(stageTexture, new Rectangle(BACKBUFFER_WIDTH / 2 - 150, 0, 200, 80), new Rectangle(0, 0, stageTexture.Width, stageTexture.Height), Color.White);
                        spriteBatch.Draw(stageNumberTexture, new Rectangle(BACKBUFFER_WIDTH / 2 + 30, 0, 60, 80), new Rectangle((STAGE - 1) * (stageNumberTexture.Width / 11), 0, stageNumberTexture.Width / 11, stageNumberTexture.Height), Color.White);
                        spriteBatch.Draw(stageNumberTexture, new Rectangle(BACKBUFFER_WIDTH / 2 + 30 + 60, 0, 60, 80), new Rectangle(10 * (stageNumberTexture.Width / 11), 0, stageNumberTexture.Width / 11, stageNumberTexture.Height), Color.White);
                        spriteBatch.Draw(stageNumberTexture, new Rectangle(BACKBUFFER_WIDTH / 2 + 30 + 2 * 60, 0, 60, 80), new Rectangle((sceneCount - 1) * (stageNumberTexture.Width / 11), 0, stageNumberTexture.Width / 11, stageNumberTexture.Height), Color.White);
                         
                        spriteBatch.Draw(playSceneNameTexture, new Rectangle(0, 0, 240, 160), new Rectangle(0, 0, playSceneNameTexture.Width, playSceneNameTexture.Height/2), Color.White);   //Show the scene name 

                        magicEngineLeftHand.Draw(spriteBatch);
                        magicEngineRightHand.Draw(spriteBatch);
                        leftHandManager.Draw(spriteBatch);
                        rightHandManager.Draw(spriteBatch);
                        
                        leftHandSprite.Draw(spriteBatch);
                        rightHandSprite.Draw(spriteBatch);

                        spriteBatch.Draw(progressBarTexture, new Rectangle(0, (int)(748*scaleRate.Y), BACKBUFFER_WIDTH / (TotalStage * ROUNDCNT) * ((STAGE - 1) * ROUNDCNT + sceneCount - 1), 20), Color.White);
                        magicEngineProgressBar.Draw(spriteBatch);
                        for (int i = 0; i < STAGE; i++)
                        {
                            spriteBatch.Draw(marks[i], new Rectangle((int)((5+i*183)*scaleRate.X), (int)(668*scaleRate.Y), (int)(78*scaleRate.X), (int)(79*scaleRate.Y)), Color.White);
                        }

                        break;

                case GameStates.WIN:

                    if (takePhotoOK)
                    {
                        spriteBatch.Draw(screenShot, new Rectangle(0, 0, (int)(gameBackground.Width*scaleRate.X), (int)(gameBackground.Height*scaleRate.Y)), Microsoft.Xna.Framework.Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw(playSceneTexture, new Rectangle(0, 0, (int)(gameBackground.Width*scaleRate.X), (int)(gameBackground.Height*scaleRate.Y)), Microsoft.Xna.Framework.Color.White);
                        spriteBatch.Draw(photoFrameTexture, new Rectangle(0, 0, (int)(gameBackground.Width*scaleRate.X), (int)(gameBackground.Height*scaleRate.Y)), Microsoft.Xna.Framework.Color.White);
                        if (player != null && CameraFlashing == false)
                        {
                            spriteBatch.Draw(player, new Vector2(198*scaleRate.X, 140*scaleRate.Y), null, Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2(0, 0), 1.35f, SpriteEffects.None, 0f);
                            spriteBatch.Draw(player, new Vector2(200*scaleRate.X, 142*scaleRate.Y), null, Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2(0, 0), 1.35f, SpriteEffects.None, 0f);
                        }
                    }
                    
                    break;

                case GameStates.ENDGAME:

                    spriteBatch.Draw(albumTexture, new Rectangle(0, 0, (int)(gameBackground.Width * scaleRate.X), (int)(gameBackground.Height * scaleRate.Y)), Color.White);
                    for (int i = endGameNumber * 9; (i < (endGameNumber + 1) * 9) && (i < keepPhotos.Count); i++)
                    {
                        int temp = i % 9;
                        spriteBatch.Draw(keepPhotos[i], new Rectangle((int)((407 + (temp / 3) * 285)*scaleRate.X), (int)((78 + (temp % 3) * 228)*scaleRate.Y), (int)(250*scaleRate.X), (int)(150*scaleRate.Y)), new Rectangle(0, 0, keepPhotos[i].Width, keepPhotos[i].Height), Color.White);

                        spriteBatch.Draw(keepPhotosName[i], new Rectangle((int)((60 + 407 + (temp / 3) * 285)*scaleRate.X), (int)((150 + 78 + (temp % 3) * 228)*scaleRate.Y), (int)(120*scaleRate.X), (int)(40*scaleRate.Y)), new Rectangle(0, keepPhotosName[i].Height / 2, keepPhotosName[i].Width, keepPhotosName[i].Height / 2), Color.White);                    
                    }

                    break;

                default: break;
            }
            
                                        
            spriteBatch.End();
            base.Draw(gameTime);

            if (takePhotoStart && takePhotoOK == false)
            {
                GraphicsDevice.SetRenderTarget(null);
                FileStream fs = new FileStream(filePath + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".png", FileMode.OpenOrCreate);
                using (fs)
                //using(FileStream fs = new FileStream("C:\\Games\\CleanWindow\\Captures\\Capture_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".png", FileMode.OpenOrCreate))    
                {
                    screenShot.SaveAsPng(fs, BACKBUFFER_WIDTH, BACKBUFFER_HEIGHT);

                    if (gameState != GameStates.ENDGAME)
                    {
                        //keepPotos[(STAGE - 1) * 3 + sceneCount - 1] = Texture2D.FromStream(GraphicsDevice, fs);
                        keepPhotos.Add(Texture2D.FromStream(GraphicsDevice, fs));
                        keepPhotosName.Add(playSceneNameTexture);
                    }
                }

                takePhotoStart = false;
                takePhotoOK = true;
                player = screenShot;
            }
        }

    }

}

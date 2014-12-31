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



namespace CleanWindow
{
   
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private int ROUNDCNT = 3;
        private int STAGE = 1;
        private int FORGIVECOUNT = 1200;
        private int sceneCount = 1;
        private int PLAYTIMEOUT = 60000;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KinectSensor kinectSensor;
        string connectedStatus = "Not connected";
        Texture2D gameBackground;
        Texture2D gameWindowFrame;
        Texture2D photoFrameTexture;

        Sprite transitionTrain, transitionMapName;
        Texture2D transitionMap, transitionMapLocation;
        Vector2 transitionTrainPosition = new Vector2(1300, 400);
        List<Texture2D> takePhotoTextures = new List<Texture2D>();

        Texture2D playSceneTexture;

        double playingTimeOut = 0;
        double transitionFlashing = 0;
        Boolean drawFlashing = true;


        //Main Menu
        Sprite startButton, startButtonPressed, helpButton, helpButtonPressed, settingButton, settingButtonPressed;
        DateTime startPressedStartTime = DateTime.Now;
        Boolean isStartButtonPressed = false;
        DateTime startConfirmPressedStartTime = DateTime.Now;
        Boolean startConfirmButtonPressed = false;

        DateTime helpPressedStartTime = DateTime.Now;
        Boolean isHelpButtonPressed = false;
        DateTime helpConfirmPressedStartTime = DateTime.Now;
        Boolean helpConfirmButtonPressed = false;

        DateTime settingPressedStartTime = DateTime.Now;
        Boolean isSettingButtonPressed = false;
        DateTime settingConfirmPressedStartTime = DateTime.Now;
        Boolean settingConfirmButtonPressed = false;

        DateTime transitionStartTime = DateTime.Now;

        const int fogX = 100;
        const int fogY = 58;
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

        protected const int BACKBUFFER_WIDTH = 1280;
        protected const int BACKBUFFER_HEIGHT = 768;

        int TimeOutLimit = 10000;
        double timeoutCount = 0;

        List<Texture2D> particleTextures = new List<Texture2D>();
        Boolean nextStageParticleStart = false;
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
                    
        ParticleEngine[] particleEngines = new ParticleEngine[4];
        Vector2[] particlesXY = new Vector2[4];
        int[] particlePeek = new int[4];
        int[] particleSpeed = new int[4];

        List<Texture2D> magicTextures = new List<Texture2D>();
        MagicEngine magicEngineLeftHand, magicEngineRightHand;
       
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
        bool bgMusicStart = false;


        Boolean CameraFlashing = false;

        Boolean takePhotoStart = false;
        Boolean takePhotoOK = false;
        RenderTarget2D screenShot;

        private static int skeletonID = -1;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = BACKBUFFER_WIDTH;
            graphics.PreferredBackBufferHeight = BACKBUFFER_HEIGHT;
            //graphics.IsFullScreen = true;
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

                        headPosition = new Vector2((((head.Position.X) + 0.5f) * (BACKBUFFER_WIDTH)), (((-1 * head.Position.Y) + 0.5f) * (BACKBUFFER_HEIGHT)));
                        leftHandPosition = new Vector2((((leftHand.Position.X) + 0.5f) * 800 + 240), (((-1 * leftHand.Position.Y) + 0.5f) * (BACKBUFFER_HEIGHT)));
                        rightHandPosition = new Vector2((((rightHand.Position.X) + 0.5f) * 800 + 240), (((-1 * rightHand.Position.Y) + 0.5f) * (BACKBUFFER_HEIGHT)));
                        adjustKinectSensor();
                        
                    }
                    else
                    {
                        skeletonID = -1; //if the skeleton is null (say when someone walks off the screen. Then the id used returns null. ) we reset the value in this line
                    }
                }
            }
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
            for (int y = 0; y < kinectSensor.ColorStream.FrameHeight; y++)
            {
                for (int x = 0; x < kinectSensor.ColorStream.FrameWidth; x++, index += 4)
                {
                    int playerIndex = depthArray[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                    if (playerIndex > 0)
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
            
            for (int i = 0; i < fogX; i++)
            {
                for (int j = 0; j < fogY; j++)
                {
                    int fogRandomTextureNo = RandNum.Next(0, 14);
                    fog[i, j] = STAGE;
                    fogCleanRelease[i, j] = DateTime.Now;
                    fogLocation[i, j] = new Vector2(110 + i * 10 + RandNum.Next(10), 70 + j * 10 + RandNum.Next(12));
                    fogSprite[i, j] = new Sprite(fogLocation[i, j], fogTextures[fogRandomTextureNo], fogTextures[fogRandomTextureNo].Bounds, Vector2.Zero);
                }
            }
        }

        protected override void LoadContent()
        {
           
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            particleTextures.Add(Content.Load<Texture2D>("Textures\\particle2"));
            magicTextures.Add(Content.Load<Texture2D>("Textures\\shine-181"));
            
            startButton = new Sprite(new Vector2(60, 400), Content.Load<Texture2D>("Textures\\button\\01-btn1"), new Rectangle(0, 0, 228, 165), Vector2.Zero);
            startButtonPressed = new Sprite(new Vector2(-1000, -100), Content.Load<Texture2D>("Textures\\button\\01-btn1b"), new Rectangle(0, 0, 285, 206), Vector2.Zero);
            helpButton = new Sprite(new Vector2(200, 150), Content.Load<Texture2D>("Textures\\button\\01-btn2"), new Rectangle(0, 0, 235, 125), Vector2.Zero);
            helpButtonPressed = new Sprite(new Vector2(-1000, -100), Content.Load<Texture2D>("Textures\\button\\01-btn2b"), new Rectangle(0, 0, 294, 156), Vector2.Zero);
            settingButton = new Sprite(new Vector2(450, 50), Content.Load<Texture2D>("Textures\\button\\01-btn3"), new Rectangle(0, 0, 247, 131), Vector2.Zero);
            settingButtonPressed = new Sprite(new Vector2(-1000, -100), Content.Load<Texture2D>("Textures\\button\\01-btn3b"), new Rectangle(0, 0, 309, 165), Vector2.Zero);
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-01"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-02"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-03"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-04"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-05"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-06"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-07"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-08"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-09"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-10"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-11"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-12"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-13"));
            fogTextures.Add(Content.Load<Texture2D>("Textures\\dirts\\dirts-14"));
            intializeFog();

            takePhotoTextures.Add(Content.Load<Texture2D>("Textures\\takephoto\\bg07-0"));
            takePhotoTextures.Add(Content.Load<Texture2D>("Textures\\takephoto\\bg07-1"));
            takePhotoTextures.Add(Content.Load<Texture2D>("Textures\\takephoto\\bg07-2"));
            takePhotoTextures.Add(Content.Load<Texture2D>("Textures\\takephoto\\bg07-3"));
            takePhotoTextures.Add(Content.Load<Texture2D>("Textures\\takephoto\\bg07-4"));
            takePhotoTextures.Add(Content.Load<Texture2D>("Textures\\takephoto\\bg07-5"));

            //leftHandSprite = new Sprite(new Vector2(-100, -100), spriteTexture, new Rectangle(0, 0, 100, 123), Vector2.Zero);
            //rightHandSprite = new Sprite(new Vector2(-100, -100), spriteTexture, new Rectangle(101, 0, 100, 123), Vector2.Zero);
            leftHandSprite = new Sprite(new Vector2(-100, -100), Content.Load<Texture2D>("Textures\\glove\\groves-11"), new Rectangle(0, 0, 80, 134), Vector2.Zero);
            rightHandSprite = new Sprite(new Vector2(-100, -100), Content.Load<Texture2D>("Textures\\glove\\groves-21"), new Rectangle(0, 0, 80, 134), Vector2.Zero);
            leftHandManager = new PlayerManager(Content.Load<Texture2D>("Textures\\towel\\towel_up_left"), 10, new Rectangle(30, 30, 1250, 738));
            rightHandManager = new PlayerManager(Content.Load<Texture2D>("Textures\\towel\\towel_up_right"), 10, new Rectangle(30, 30, 1250, 738));
            
            magicEngineLeftHand = new MagicEngine(magicTextures, new Vector2(300, 300));
            magicEngineRightHand = new MagicEngine(magicTextures, new Vector2(300, 300));
            soundChim = Content.Load<SoundEffect>("Sounds\\chime2");
            soundApplus = Content.Load<SoundEffect>("Sounds\\applause");
            soundCamera = Content.Load<SoundEffect>("Sounds\\camera1");
            soundCheering = Content.Load<SoundEffect>("Sounds\\cheering2");
            soundWipe = Content.Load<SoundEffect>("Sounds\\wipe");
            soundClick = Content.Load<SoundEffect>("Sounds\\Click02");

            takephotoready = Content.Load<SoundEffect>("Sounds\\takephotoready");
            takephoto1 = Content.Load<SoundEffect>("Sounds\\takephoto1");
            takephoto2 = Content.Load<SoundEffect>("Sounds\\takephoto2");
            takephoto3 = Content.Load<SoundEffect>("Sounds\\takephoto3");
            takephotosmile = Content.Load<SoundEffect>("Sounds\\takephotosmile");
            takephotoshutter = Content.Load<SoundEffect>("Sounds\\takephotoshutter");

            Texture2D transitionTrainTexture = Content.Load<Texture2D>("Textures\\transition\\train");
            transitionTrain = new Sprite(new Vector2(1300, 400), transitionTrainTexture, transitionTrainTexture.Bounds , Vector2.Zero);
            
            bgMusic = Content.Load<Song>("Music\\menumusic");
            MediaPlayer.IsRepeating = true;

            screenShot = new RenderTarget2D(graphics.GraphicsDevice, graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                                                         graphics.GraphicsDevice.PresentationParameters.BackBufferHeight);

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

            switch (gameState)
            {

                #region Goto main menu
                case GameStates.GO_MAINMENU:

                    startConfirmButtonPressed = false;
                    helpConfirmButtonPressed = false;
                    settingConfirmButtonPressed = false;
                    gameBackground = Content.Load<Texture2D>("Textures\\background\\mainmenu");
                    gameState = GameStates.MAINMENU;
                    
                    bgMusic = Content.Load<Song>("Music\\menumusic");
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
                    helpButton.Update(gameTime);
                    helpButtonPressed.Update(gameTime);
                    settingButton.Update(gameTime);
                    settingButtonPressed.Update(gameTime);

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
                            if (startConfirmButtonPressed == false && elapsedTime.Seconds > 1)
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
                        if (elapsedTime2.Seconds > 0)
                        {
                            gameState = GameStates.GO_TRANSITION;
                            transitionStartTime = DateTime.Now;
                        }
                    }


                    //Help button
                    if ((helpButton.IsCircleColliding(leftHandSprite.Center, 70) ||
                                    helpButton.IsCircleColliding(rightHandSprite.Center, 70)))
                    {
                        helpButtonPressed.Location = new Vector2(180, 130);
                        if (isHelpButtonPressed == false)
                        {
                            soundClick.Play();
                            helpPressedStartTime = DateTime.Now;
                            isHelpButtonPressed = true;
                        }
                        if (isHelpButtonPressed)
                        {
                            TimeSpan elapsedTime = DateTime.Now - helpPressedStartTime;
                            if (helpConfirmButtonPressed == false && elapsedTime.Seconds > 2)
                            {
                                helpConfirmPressedStartTime = DateTime.Now;
                                helpConfirmButtonPressed = true;
                                soundChim.Play();
                            }

                        }
                        
                    }
                    else
                    {
                        helpButtonPressed.Location = new Vector2(-1000, -100);
                        isHelpButtonPressed = false;    
                    }

                    //Setting button
                    if ((settingButton.IsCircleColliding(leftHandSprite.Center, 70) ||
                                    settingButton.IsCircleColliding(rightHandSprite.Center, 70)))
                    {
                        settingButtonPressed.Location = new Vector2(430, 30);
                        if (isSettingButtonPressed == false)
                        {
                            soundClick.Play();
                            settingPressedStartTime = DateTime.Now;
                            isSettingButtonPressed = true;
                        }
                        if (isSettingButtonPressed)
                        {
                            TimeSpan elapsedTime = DateTime.Now - settingPressedStartTime;
                            if (settingConfirmButtonPressed == false && elapsedTime.Seconds > 2)
                            {
                                settingConfirmPressedStartTime = DateTime.Now;
                                settingConfirmButtonPressed = true;
                                soundChim.Play();
                            }

                        }
                    }
                    else
                    {
                        settingButtonPressed.Location = new Vector2(-1000, -100);
                        isSettingButtonPressed = false; 
                    }

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
                    transitionMap = Content.Load<Texture2D>("Textures\\transition\\map");

                    transitionMapLocation = Content.Load<Texture2D>("Textures\\transition\\map" + STAGE);
                    Texture2D transitionMapNameTexture = Content.Load<Texture2D>("Textures\\transition\\map" + STAGE + "a");
                    if(STAGE == 1)transitionMapName = new Sprite(new Vector2(500, 30), transitionMapNameTexture, transitionMapNameTexture.Bounds, Vector2.Zero);
                    if(STAGE == 2)transitionMapName = new Sprite(new Vector2(500, 280), transitionMapNameTexture, transitionMapNameTexture.Bounds, Vector2.Zero);
                    if(STAGE == 3)transitionMapName = new Sprite(new Vector2(520, 320), transitionMapNameTexture, transitionMapNameTexture.Bounds, Vector2.Zero);
                    gameState = GameStates.TRANSITION;
                    transitionStartTime = DateTime.Now;
                    bgMusic = Content.Load<Song>("Music\\transition");
                    MediaPlayer.IsRepeating = true;
                    bgMusicStart = false;
                    transitionTrainPosition = new Vector2(1300, 400);
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
                    transitionTrainPosition.X -= 5;
                    transitionTrain.Location = transitionTrainPosition;
                    if (transitionTrainPosition.X < -(transitionTrain.BoundingBoxRect.Width + 50))
                    {
                        gameState = GameStates.GO_PLAY;
                    }

                    break;
                #endregion

                #region Goto play
                case GameStates.GO_PLAY:

                    using (Stream stream = File.OpenRead("C:\\Games\\CleanWindow\\Photos\\Round" + STAGE + "\\Round" + STAGE + "-" + sceneCount + ".JPG"))
                    {
                        playSceneTexture = Texture2D.FromStream(GraphicsDevice, stream);
                    }  

                    bgMusic = Content.Load<Song>("Music\\bgmusic"+STAGE);
                    MediaPlayer.IsRepeating = true;
                    bgMusicStart = false;
                    
                    leftHandPosition = new Vector2(-100,-100);
                    rightHandPosition = new Vector2(-100,-100);
                    leftHandOldPosition = new Vector2(-100, -100);
                    rightHandOldPosition = new Vector2(-100, -100);
                    
                    intializeFog();

                    if (STAGE == 1) gameBackground = Content.Load<Texture2D>("Textures\\background\\game1bg");
                    if (STAGE == 2) gameBackground = Content.Load<Texture2D>("Textures\\background\\game2bg");
                    if (STAGE == 3) gameBackground = Content.Load<Texture2D>("Textures\\background\\game3bg");

                    leftHandSprite = new Sprite(new Vector2(-100, -100), Content.Load<Texture2D>("Textures\\glove\\glove_hold_left"), new Rectangle(0, 0, 112, 103), Vector2.Zero);
                    rightHandSprite = new Sprite(new Vector2(-100, -100), Content.Load<Texture2D>("Textures\\glove\\glove_hold_right"), new Rectangle(0, 0, 112, 103), Vector2.Zero);
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
                                        fogSprite[i, j].alpha = 0.3f * fog[i, j];
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
                        magicEngineLeftHand = new MagicEngine(magicTextures, new Vector2(-100, -100));
                        magicEngineRightHand = new MagicEngine(magicTextures, new Vector2(-100, -100));
                        gameState = GameStates.WIN;
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
                    if(!takePhotoSouldPlayed1)takephotoready.Play();
                        takePhotoSouldPlayed1 = true;

                    timeoutCount += gameTime.ElapsedGameTime.Milliseconds;
                    int baseWaitTime = 5000;
                    if (timeoutCount > baseWaitTime)
                    {
                        photoFrameTexture = takePhotoTextures[1];
                        if (!takePhotoSouldPlayed2) takephoto1.Play();
                        takePhotoSouldPlayed2 = true;
                    }
                    if (timeoutCount > baseWaitTime + 1000)
                    {
                        photoFrameTexture = takePhotoTextures[2];
                        if (!takePhotoSouldPlayed3) takephoto2.Play();
                        takePhotoSouldPlayed3 = true;
                    }
                    if (timeoutCount > baseWaitTime + 2000)
                    {
                        photoFrameTexture = takePhotoTextures[3];
                        if (!takePhotoSouldPlayed4) takephoto3.Play();
                        takePhotoSouldPlayed4 = true;
                    }
                    if (timeoutCount > baseWaitTime + 3000)
                    {
                        photoFrameTexture = takePhotoTextures[4];
                        if (!takePhotoSouldPlayed5) takephotosmile.Play();
                        takePhotoSouldPlayed5 = true;
                    }
                    if (timeoutCount > baseWaitTime + 4000)
                    {
                        CameraFlashing = true;
                        photoFrameTexture = takePhotoTextures[5];
                        if(!takePhotoSouldPlayed6)takephotoshutter.Play();
                        takePhotoSouldPlayed6 = true;
                        
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
                            if (STAGE <= 3)
                                gameState = GameStates.GO_TRANSITION;
                            else
                                gameState = GameStates.GO_MAINMENU;
                        }
                        else
                        {
                            gameState = GameStates.GO_PLAY;
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

                        spriteBatch.Draw(gameBackground, new Rectangle(0, 0, gameBackground.Width, gameBackground.Height), Microsoft.Xna.Framework.Color.White);

                        startButton.Draw(spriteBatch);
                        startButtonPressed.Draw(spriteBatch);
                        helpButton.Draw(spriteBatch);
                        helpButtonPressed.Draw(spriteBatch);
                        settingButton.Draw(spriteBatch);
                        settingButtonPressed.Draw(spriteBatch);

                        leftHandSprite.Draw(spriteBatch);
                        rightHandSprite.Draw(spriteBatch);

                    break;

                case GameStates.TRANSITION:
                        spriteBatch.Draw(transitionMap, new Rectangle(0, 0, transitionMap.Width, transitionMap.Height), Microsoft.Xna.Framework.Color.White);
                        if (drawFlashing)spriteBatch.Draw(transitionMapLocation, new Rectangle(0, 0, transitionMapLocation.Width, transitionMapLocation.Height), Microsoft.Xna.Framework.Color.White);
                        transitionMapName.Draw(spriteBatch);
                        transitionTrain.Draw(spriteBatch);

                    break;

                case GameStates.PLAY:

                        spriteBatch.Draw(playSceneTexture, new Rectangle(0, 0, gameBackground.Width, gameBackground.Height), Microsoft.Xna.Framework.Color.White);
                        
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

                        spriteBatch.Draw(gameBackground, new Rectangle(0, 0, gameBackground.Width, gameBackground.Height), Microsoft.Xna.Framework.Color.White);
                        //spriteBatch.Draw(gameWindowFrame, new Rectangle(0, 0, gameWindowFrame.Width, gameWindowFrame.Height), Color.White);

                        magicEngineLeftHand.Draw(spriteBatch);
                        magicEngineRightHand.Draw(spriteBatch);
                        leftHandManager.Draw(spriteBatch);
                        rightHandManager.Draw(spriteBatch);
                        
                        leftHandSprite.Draw(spriteBatch);
                        rightHandSprite.Draw(spriteBatch);

                    break;

                case GameStates.WIN:

                    if (takePhotoOK)
                    {
                        spriteBatch.Draw(screenShot, new Rectangle(0, 0, gameBackground.Width, gameBackground.Height), Microsoft.Xna.Framework.Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw(playSceneTexture, new Rectangle(0, 0, gameBackground.Width, gameBackground.Height), Microsoft.Xna.Framework.Color.White);
                        spriteBatch.Draw(photoFrameTexture, new Rectangle(0, 0, gameBackground.Width, gameBackground.Height), Microsoft.Xna.Framework.Color.White);
                        if (player != null && CameraFlashing == false)
                        {
                            spriteBatch.Draw(player, new Vector2(198, 140), null, Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2(0, 0), 1.35f, SpriteEffects.None, 0f);
                        }
                    }
                    
                    break;


                default: break;
            }
            
                                        
            spriteBatch.End();
            base.Draw(gameTime);

            if (takePhotoStart && takePhotoOK == false)
            {
                GraphicsDevice.SetRenderTarget(null);
                using(FileStream fs = new FileStream("C:\\Games\\CleanWindow\\Captures\\Capture_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".png", FileMode.OpenOrCreate))    
                    {        screenShot.SaveAsPng(fs, BACKBUFFER_WIDTH, BACKBUFFER_HEIGHT);
                    }

                takePhotoStart = false;
                takePhotoOK = true;
                player = screenShot;
            }
        }

    }

}

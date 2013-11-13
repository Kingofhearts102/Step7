using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace PrisonStep
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PrisonGame : Microsoft.Xna.Framework.Game
    {

        #region Fields


        private PSLineDraw lineDraw;
        public PSLineDraw LineDraw { get { return lineDraw; } }
        /// <summary>
        /// This graphics device we are drawing on in this assignment
        /// </summary>
        GraphicsDeviceManager graphics;

        /// <summary>
        /// The camera we use
        /// </summary>
        private Camera camera;

        /// <summary>
        /// The player in your game is modeled with this class
        /// </summary>
        private Player player;

        /// <summary>
        /// This is the actual model we are using for the prison
        /// </summary>
        private List<PrisonModel> phibesModels = new List<PrisonModel>();



        #endregion

        #region Properties

        /// <summary>
        /// The game camera
        /// </summary>
        public Camera Camera { get { return camera; } }

        public List<PrisonModel> PhibesModels
        {
            get { return phibesModels; }
        }
        #endregion

        private bool slimed = false;
        public bool Slimed { get { return slimed; } set { slimed = value; } }

        private float slimeLevel = 1.0f;
        public float SlimeLevel { get { return slimeLevel; } }



        /// <summary>
        /// Constructor
        /// </summary>
        public PrisonGame()
        {
            // XNA startup
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Create objects for the parts of the ship
            for(int i=1;  i<=6;  i++)
            {
                phibesModels.Add(new PrisonModel(this, i));
            }

            // Create a player object
            player = new Player(this);

            // Some basic setup for the display window
            this.IsMouseVisible = true;
			this.Window.AllowUserResizing = true;
			this.graphics.PreferredBackBufferWidth = 1024;
			this.graphics.PreferredBackBufferHeight = 768;

            // Basic camera settings
            camera = new Camera(graphics);
            camera.FieldOfView = MathHelper.ToRadians(42);
            camera.Eye = new Vector3(800, 180, 1053);
            camera.Center = new Vector3(275, 90, 1053);

            lineDraw = new PSLineDraw(this, Camera);
            this.Components.Add(lineDraw);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            camera.Initialize();
            player.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            player.LoadContent(Content);

            foreach (PrisonModel model in phibesModels)
            {
                model.LoadContent(Content);
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            lineDraw.Clear();
            lineDraw.Crosshair(new Vector3(0, 100, 0), 20, Color.White);

            lineDraw.Begin();
            lineDraw.Vertex(new Vector3(0, 150, 0), Color.White);
            lineDraw.Vertex(new Vector3(50, 100, 0), Color.Red);
            lineDraw.End();

            //
            // Update game components
            //

            player.Update(gameTime);

            foreach (PrisonModel model in phibesModels)
            {
                model.Update(gameTime);
            }

            camera.Update(gameTime);

            // Amount to change slimeLevel in one second
            float slimeRate = 2.5f;

            if (slimed && slimeLevel >= -1.5)
            {
                slimeLevel -= (float)gameTime.ElapsedGameTime.TotalSeconds * slimeRate;
            }
            else if (!slimed && slimeLevel < 1)
            {
                slimeLevel += (float)gameTime.ElapsedGameTime.TotalSeconds * slimeRate;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            foreach (PrisonModel model in phibesModels)
            {
                model.Draw(graphics, gameTime);
            }

            player.Draw(graphics, gameTime);

            base.Draw(gameTime);
        }
    }
}

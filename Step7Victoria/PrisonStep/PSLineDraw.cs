using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrisonStep
{
    /// <summary>
    /// Simple class to draw lines on the screen.  Adds as a game component.
    /// </summary>
    public class PSLineDraw : DrawableGameComponent
    {
        #region Fields

        /// <summary>
        /// A vertex on a line strip
        /// </summary>
        private struct LineVertex
        {
            public LineVertex(Vector3 p, Color c) { this.P = p; this.Color = c; }
            public Vector3 P;
            public Color Color;
        }

        /// <summary>
        /// This is a list of lists, where each list is a line strip
        /// </summary>
        private List<List<LineVertex>> lines = new List<List<LineVertex>>();

        /// <summary>
        /// This references the line strip we are currently adding
        /// </summary>
        private List<LineVertex> active = null;

        /// <summary>
        /// The default color for lines
        /// </summary>
        private Color defaultColor = Color.White;

        /// <summary>
        /// An arbitrary matrix that can be used to 
        /// transform the entire linedraw system.
        /// </summary>
        private Matrix transform = Matrix.Identity;

        /// <summary>
        /// Camera support
        /// </summary>
        private Camera camera;

        /// <summary>
        /// The custom linedraw effect we use
        /// </summary>
        private Effect effect;

        #endregion

        #region Properties

        /// <summary>
        /// A static variable that can be used to access a
        /// single LineDraw object installed as a DrawableGameComponent.
        /// Not set by this component, so must be set by program using this
        /// feature
        /// </summary>
        public static PSLineDraw LineDrawStatic = null;

        /// <summary>
        /// Default color for line or axis drawing.
        /// </summary>
        public Color DefaultColor { get { return defaultColor; } set { defaultColor = value; } }

        /// <summary>
        /// The camera used by this object.
        /// </summary>
        public Camera Camera { get { return camera; } set { camera = value; } }

        /// <summary>
        /// A transform applied to all lines when rendered.
        /// </summary>
        public Matrix Transform { get { return transform; } set { transform = value; } }

        #endregion

        #region Stored Lines support

        /// <summary>
        /// Delete all stored lines.
        /// </summary>
        public void Clear()
        {
            active = null;
            lines.Clear();
        }


        /// <summary>
        /// Indicate the beginning of a line strip.
        /// </summary>
        public void Begin()
        {
            active = new List<LineVertex>();
            lines.Add(active);
        }

        /// <summary>
        /// Indicate the end of a line strip.
        /// </summary>
        public void End()
        {
            System.Diagnostics.Debug.Assert(active != null);
            active = null;
        }

        /// <summary>
        /// Add a vertex to the currently active line strip.
        /// </summary>
        /// <param name="P">Vertex location</param>
        /// <param name="color">Vertex color</param>
        public void Vertex(Vector3 P, Color color)
        {
            System.Diagnostics.Debug.Assert(active != null);
            active.Add(new LineVertex(P, color));
        }

        /// <summary>
        /// Add a vertex to the currently active line strip.
        /// </summary>
        /// <param name="P">Vertex location</param>
        public void Vertex(Vector3 P)
        {
            System.Diagnostics.Debug.Assert(active != null);
            active.Add(new LineVertex(P, defaultColor));
        }

        #endregion

        #region Composite Objects

        /// <summary>
        /// Draw a coordinate axis.
        /// </summary>
        /// <param name="len"></param>
        /// <param name="color"></param>
        public void Axis(float len, Color color)
        {
            Begin();
            Vertex(new Vector3(0, 0, 0), color);
            Vertex(new Vector3(len, 0, 0), color);
            End();

            Begin();
            Vertex(new Vector3(0, 0, 0), color);
            Vertex(new Vector3(0, len, 0), color);
            End();

            Begin();
            Vertex(new Vector3(0, 0, 0), color);
            Vertex(new Vector3(0, 0, len), color);
            End();
        }

        public void Axis(float len, Color color, Vector3 position)
        {
            Begin();
            Vertex(new Vector3(0, 0, 0) + position, color);
            Vertex(new Vector3(len, 0, 0) + position, color);
            End();

            Begin();
            Vertex(new Vector3(0, 0, 0) + position, color);
            Vertex(new Vector3(0, len, 0) + position, color);
            End();

            Begin();
            Vertex(new Vector3(0, 0, 0) + position, color);
            Vertex(new Vector3(0, 0, len) + position, color);
            End();
        }

        /// <summary>
        /// Draw a crosshair
        /// </summary>
        /// <param name="len"></param>
        /// <param name="color"></param>
        public void Crosshair(Vector3 where, float len, Color color)
        {
            Begin();
            Vertex(where - new Vector3(len, 0, 0), color);
            Vertex(where + new Vector3(len, 0, 0), color);
            End();

            Begin();
            Vertex(where - new Vector3(0, len, 0), color);
            Vertex(where + new Vector3(0, len, 0), color);
            End();

            Begin();
            Vertex(where - new Vector3(0, 0, len), color);
            Vertex(where + new Vector3(0, 0, len), color);
            End();
        }

        #endregion

        #region Construction and Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">XNA game class</param>
        /// <param name="camera">Camera class used by the game.  Indicates 
        /// where the camera is in the 3D space the line is drawn in.</param>
        public PSLineDraw(Game game, Camera camera)
            : base(game)
        {
            this.camera = camera;
            this.DrawOrder = 1000;      // Should be last...
        }


        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            effect = Game.Content.Load<Effect>("LineDraw");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            effect.CurrentTechnique = effect.Techniques["DefaultTechnique"];
            effect.Parameters["World"].SetValue(transform);
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);

            foreach (List<LineVertex> line in lines)
            {
                if (line.Count < 2)
                    continue;           // Too short...

                // Create a vertex buffer and index buffer
                VertexPositionColor[] vertices = new VertexPositionColor[line.Count];
                short[] lineStripIndices = new short[line.Count];
                for (int i = 0; i < line.Count; i++)
                {
                    vertices[i].Position = line[i].P;
                    vertices[i].Color = line[i].Color;
                    lineStripIndices[i] = (short)i;
                }

                // Render the line strips
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip,
                        vertices, 0, line.Count - 1);
                }

            }


            base.Draw(gameTime);
        }

        #endregion

    }
}

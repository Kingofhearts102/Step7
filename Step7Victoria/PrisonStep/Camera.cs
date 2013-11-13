using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;


namespace PrisonStep
{
    /// <summary>
    /// This class implements a simple camera model.
    /// The model has no mouse controls and is assumed to be managed
    /// by some other outside class.
    /// </summary>
    public class Camera
    {
        #region Fields

        /// <summary>
        /// The graphics device manager this camera is for. We need this
        /// to know what the viewport is.
        /// </summary>
        private GraphicsDeviceManager graphics;

        Vector3 eye = new Vector3(1000, 1000, 1000);
        Vector3 center = new Vector3(0, 0, 0);
        Vector3 up = new Vector3(0, 1, 0);
        float fov = MathHelper.ToRadians(35);
        float znear = 10;
        float zfar = 10000;

        Matrix view;
        Matrix projection;

        #endregion

        #region Properties

        /// <summary>
        /// The current camera view matrix
        /// </summary>
        public Matrix View { get { return view; } }

        /// <summary>
        /// The current camera projection matrix
        /// </summary>
        public Matrix Projection { get { return projection; } }

        /// <summary>
        /// Camera field of view in radians
        /// </summary>
        public float FieldOfView { get { return fov; } set { fov = value; ComputeProjection(); } }

        /// <summary>
        /// Camera up direction vector
        /// </summary>
        public Vector3 Up { get { return up; } set { up = value; ComputeView(); } }

        /// <summary>
        /// Camera center point
        /// </summary>
        public Vector3 Center { get { return center; } set { center = value; ComputeView(); } }

        /// <summary>
        /// Camera eye position
        /// </summary>
        public Vector3 Eye { get { return eye; } set { eye = value; ComputeView(); } }

        /// <summary>
        /// The distance to the near clipping plane
        /// </summary>
        public float ZNear { get { return znear; } set { znear = value; ComputeProjection(); } }

        /// <summary>
        /// The distance to the far clipping plane
        /// </summary>
        public float ZFar { get { return zfar; } set { zfar = value; ComputeProjection(); } }

        #endregion

        #region Construction and Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">The graphics device manager we draw to</param>
        public Camera(GraphicsDeviceManager graphics)
        {
            this.graphics = graphics;
        }

        /// <summary>
        /// Initialize the camera
        /// </summary>
        public void Initialize()
        {
            ComputeView();
            ComputeProjection();
        }

        #endregion

        #region Camera Computations

        /// <summary>
        /// Compute the current view matrix
        /// </summary>
        private void ComputeView()
        {
            view = Matrix.CreateLookAt(eye, center, up);
        }

        /// <summary>
        /// Compute the current projection matrix
        /// </summary>
        private void ComputeProjection()
        {
            if (graphics.GraphicsDevice != null)
            {
                projection = Matrix.CreatePerspectiveFieldOfView(fov,
                    graphics.GraphicsDevice.Viewport.AspectRatio, znear, zfar);
            }
        }

        /// <summary>
        /// Update due to advances in game time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            // Currently does not do anything. Provided in case we
            // add camera animation at a future point in time.
        }

        #endregion

    }
}

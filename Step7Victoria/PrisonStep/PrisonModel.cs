using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    /// <summary>
    /// This class implements one section of our prison ship
    /// </summary>
    public class PrisonModel
    {
        #region Constants

        private const float DoorSpeed = 100;        // cm/sec
        private const float DoorMaxY = 200;         // Maximum open in cm

        #endregion

        #region Fields

        /// <summary>
        /// The section (6) of the ship
        /// </summary>
        private int section;

        /// <summary>
        /// The name of the asset (FBX file) for this section
        /// </summary>
        private string asset;

        /// <summary>
        /// The game we are associated with
        /// </summary>
        private PrisonGame game;

        /// <summary>
        /// The XNA model for this part of the ship
        /// </summary>
        private Model model;

        /// <summary>
        /// To make animation possible and easy, we save off the initial (bind) 
        /// transformation for all of the model bones. 
        /// </summary>
        private Matrix[] bindTransforms;

        /// <summary>
        /// The is the transformations for all model bones, potentially after we
        /// have made some change in the tranformation.
        /// </summary>
        private Matrix[] boneTransforms;

        /// <summary>
        /// Lookup table for doors.  Translates a door number
        /// into a door object.
        /// </summary>
        private Dictionary<int, Door> doors = new Dictionary<int, Door>();

        /// <summary>
        /// This is a class that keeps track of the status of 
        /// any doors in the model. 
        /// </summary>
        private class Door
        {
            public int Bone;                // Bone that moves the door
            public bool Opening = false;    // Indicates we are opening the door
            public float Y = 0;             // Current door height
        }

        #endregion

        #region Construction and Loading

        /// <summary>
        /// Constructor. Creates an object for a section.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="section"></param>
        public PrisonModel(PrisonGame game, int section)
        {
            this.game = game;
            this.section = section;
            this.asset = "AntonPhibes" + section.ToString();
        }

        /// <summary>
        /// This function is called to load content into this component
        /// of our game.
        /// </summary>
        /// <param name="content">The content manager to load from.</param>
        public void LoadContent(ContentManager content)
        {
            // Load the second model
            model = content.Load<Model>(asset);

            // Save off all of hte bone information
            int boneCnt = model.Bones.Count;
            bindTransforms = new Matrix[boneCnt];
            boneTransforms = new Matrix[boneCnt];

            model.CopyBoneTransformsTo(bindTransforms);
            model.CopyBoneTransformsTo(boneTransforms);

            // Find all of the doors
            for (int b = 0; b < boneCnt; b++)
            {
                if (model.Bones[b].Name.StartsWith("DoorInner") || model.Bones[b].Name.StartsWith("DoorOuter"))
                {
                    // What is the door number?
                    int dnum = int.Parse(model.Bones[b].Name.Substring(9));

                    // Add to a dictionary that converts door numbers to bone indices
                    doors[dnum] = new Door();
                    doors[dnum].Bone = b;
                }

            }

        }


        #endregion

        #region Doors

        /// <summary>
        /// Indicate that we should open a door
        /// </summary>
        /// <param name="dnum"></param>
        public void SetDoor(int dnum, bool open)
        {
            Door door;
            if (doors.TryGetValue(dnum, out door))
            {
                door.Opening = open;
            }
        }

        public bool DoorIsOpen(int dnum)
        {
            Door door;
            if (doors.TryGetValue(dnum, out door))
            {
                return door.Opening && (door.Y >= DoorMaxY);
            }

            return false;
        }

        #endregion


        #region Update and Draw

        /// <summary>
        /// This function is called to update this component of our game
        /// to the current game time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            double delta = gameTime.ElapsedGameTime.TotalSeconds;

            //
            // Update our doors
            //

            foreach (Door door in doors.Values)
            {
                if (door.Opening && door.Y < DoorMaxY)
                {
                    // We are opening and are not all the way open
                    door.Y += DoorSpeed * (float)delta;
                    if (door.Y > DoorMaxY)
                        door.Y = DoorMaxY;

                    boneTransforms[door.Bone] = Matrix.CreateTranslation(0, door.Y, 0) * bindTransforms[door.Bone];
                    model.CopyBoneTransformsFrom(boneTransforms);

                }
                else if (!door.Opening && door.Y > 0)
                {
                    // We are closing and are not all the way closed
                    door.Y -= DoorSpeed * (float)delta;
                    if (door.Y < 0)
                        door.Y = 0;

                    boneTransforms[door.Bone] = Matrix.CreateTranslation(0, door.Y, 0) * bindTransforms[door.Bone];
                    model.CopyBoneTransformsFrom(boneTransforms);

                }

            }

        }

        /// <summary>
        /// This function is called to draw this game component.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {

            DrawModel(graphics, model, Matrix.Identity);
        }

        private void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world)
        {
            // Apply the bone transforms
            Matrix[] absoTransforms = new Matrix[model.Bones.Count];
            model.CopyBoneTransformsFrom(boneTransforms);
            model.CopyAbsoluteBoneTransformsTo(absoTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(absoTransforms[mesh.ParentBone.Index] * world);
                    effect.Parameters["View"].SetValue(game.Camera.View);
                    effect.Parameters["Projection"].SetValue(game.Camera.Projection);
                    effect.Parameters["Slime"].SetValue(game.SlimeLevel);
                }
                mesh.Draw();
            }
        }

        #endregion

    }
}

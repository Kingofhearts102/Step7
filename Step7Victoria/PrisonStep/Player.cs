using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    /// <summary>
    /// This class describes our player in the game. 
    /// </summary>
    public class Player
    {
        #region Fields

        /// <summary>
        /// Our animated Model
        /// </summary>
        private AnimatedModel victoria;

        /// <summary>
        /// This is a range from the door center that is considered
        /// to be under the door.  This much either side.
        /// </summary>
        private const float DoorUnderRange = 40;

        /// <summary>
        /// Game that uses this player
        /// </summary>
        private PrisonGame game;

        //
        // Player location information.  We keep a x/z location (y stays zero)
        // and an orientation (which way we are looking).
        //

        /// <summary>
        /// Player location in the prison. Only x/z are important. y still stay zero
        /// unless we add some flying or jumping behavior later on.
        /// </summary>
        private Vector3 location = new Vector3(275, 0, 1053); //new Vector3(275, 0, 1053);

        /// <summary>
        /// The player orientation as a simple angle
        /// </summary>
        private float orientation = 1.6f; // 1.6f;



        private enum States { Start, StanceStart, Stance, WalkStart, WalkLoopStart, WalkLoop,LeftTurn, RightTurn  }
        private States state = States.Start;
        
        
        /// <summary>
        /// The player transformation matrix. Places the player where they need to be.
        /// </summary>
        private Matrix transform;

        /// <summary>
        /// The rotation rate in radians per second when player is rotating
        /// </summary>
        private float panRate = 2;

        /// <summary>
        /// The player move rate in centimeters per second
        /// </summary>
        private float moveRate = 500;

        /// <summary>
        /// Id for a door we are opening or 0 if none.
        /// </summary>
        private int openDoor = 0;

        /// <summary>
        /// Keeps track of the last game pad state
        /// </summary>
        GamePadState lastGPS;

        private Dictionary<string, List<Vector2>> regions = new Dictionary<string, List<Vector2>>();



        #endregion


        public Player(PrisonGame game)
        {
            this.game = game;
            victoria = new AnimatedModel(game, "Victoria");
            victoria.AddAssetClip("dance", "Victoria-dance");
            victoria.AddAssetClip("stance", "Victoria-stance");
            victoria.AddAssetClip("walk", "Victoria-walk");
            victoria.AddAssetClip("walkstart", "Victoria-walkstart");
            victoria.AddAssetClip("walkloop", "Victoria-walkloop");
            victoria.AddAssetClip("rightturn", "Victoria-rightturn");
            victoria.AddAssetClip("leftturn", "Victoria-leftturn");
            SetPlayerTransform();
        }

        public void Initialize()
        {
            lastGPS = GamePad.GetState(PlayerIndex.One);
        }

        /// <summary>
        /// Set the value of transform to match the current location
        /// and orientation.
        /// </summary>
        private void SetPlayerTransform()
        {
            transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;
        }


        public void LoadContent(ContentManager content)
        {
            Model model = content.Load<Model>("AntonPhibesCollision");
            victoria.LoadContent(content);
            Matrix[] M = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(M);

            foreach (ModelMesh mesh in model.Meshes)
            {
                // For accumulating the triangles for this mesh
                List<Vector2> triangles = new List<Vector2>();

                // Loop over the mesh parts
                foreach(ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // 
                    // Obtain the vertices for the mesh part
                    //

                    int numVertices = meshPart.VertexBuffer.VertexCount;
                    VertexPositionColorTexture[] verticesRaw = new VertexPositionColorTexture[numVertices];
                    meshPart.VertexBuffer.GetData<VertexPositionColorTexture>(verticesRaw);

                    //
                    // Obtain the indices for the mesh part
                    //

                    int numIndices = meshPart.IndexBuffer.IndexCount;
                    short [] indices = new short[numIndices];
                    meshPart.IndexBuffer.GetData<short>(indices);

                    //
                    // Build the list of triangles
                    //

                    for (int i = 0; i < meshPart.PrimitiveCount * 3; i++)
                    {
                        // The actual index is relative to a supplied start position
                        int index = i + meshPart.StartIndex;

                        // Transform the vertex into world coordinates
                        Vector3 v = Vector3.Transform(verticesRaw[indices[index] + meshPart.VertexOffset].Position, M[mesh.ParentBone.Index]);
                        triangles.Add(new Vector2(v.X, v.Z));
                    }

                }

                regions[mesh.Name] = triangles;
            }

            AnimationPlayer player = victoria.PlayClip("dance");
        }

        private string TestRegion(Vector3 v3)
        {
            // Convert to a 2D Point
            float x = v3.X;
            float y = v3.Z;

            foreach (KeyValuePair<string, List<Vector2>> region in regions)
            {
                // For now we ignore the walls
                if(region.Key.StartsWith("W"))
                    continue;

                for (int i = 0; i < region.Value.Count; i += 3)
                {
                    float x1 = region.Value[i].X;
                    float x2 = region.Value[i + 1].X;
                    float x3 = region.Value[i + 2].X;
                    float y1 = region.Value[i].Y;
                    float y2 = region.Value[i + 1].Y;
                    float y3 = region.Value[i + 2].Y;

                    float d = 1.0f / ((x1 - x3) * (y2 - y3) - (x2 - x3) * (y1 - y3));
                    float l1 = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) * d;
                    if (l1 < 0)
                        continue;

                    float l2 = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) * d;
                    if (l2 < 0)
                        continue;

                    float l3 = 1 - l1 - l2;
                    if (l3 < 0)
                        continue;

                    return region.Key;
                }
            }

            return "";
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float currentOrientation = orientation;
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            orientation += GetDesiredTurnRate(ref keyboardState, ref gamePadState) * (float)delta;
            Console.WriteLine(orientation);
            float speed = 0;
            

            switch (state)
            {
                case States.Start:
                    state = States.StanceStart;
                    delta = 0;
                    break;

                case States.StanceStart:
                    victoria.PlayClip("stance");
                    state = States.Stance;
                    location.Y = 0;
                    break;

                case States.Stance:
                    speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                    if (currentOrientation > orientation)
                    {
                        state = States.RightTurn;
                    }
                    else if(currentOrientation < orientation)
                    {
                        state = States.LeftTurn;
                    }
                    if (speed > 0)
                    {
                        // We need to leave the stance state and start walking
                        victoria.PlayClip("walkstart");
                        victoria.Player.Speed = speed;
                        state = States.WalkStart;
                    }
                    
                    break;
                case States.WalkStart:
                case States.WalkLoop:
                    if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                    {
                        delta = (float)(victoria.Player.Clip.Duration - victoria.Player.Time);

                        // The clip is done after this update
                        state = States.WalkLoopStart;
                    }

                    speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                    if (speed == 0)
                    {
                        delta = 0;
                        state = States.StanceStart;
                    }
                    else
                    {
                        victoria.Player.Speed = speed;
                    }

                    break;
                case States.WalkLoopStart:
                    victoria.PlayClip("walkloop").Speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                    state = States.WalkLoop;
                    break;
                case States.LeftTurn:
                    victoria.PlayClip("leftturn");
                    if (currentOrientation == orientation)
                        state = States.StanceStart;
                    break;
                case States.RightTurn:
                    victoria.PlayClip("rightturn");
                    if (currentOrientation == orientation)
                        state = States.StanceStart;
                    break;
                    
            }

            victoria.Update(gameTime.ElapsedGameTime.TotalSeconds);

            float translation = 0;
            float rotation = 0;


            if (keyboardState.IsKeyDown(Keys.Up))
            {
                translation += moveRate * delta;
            }
            //
            // Part 1:  Compute a new orientation
            //
            orientation += rotation;

            Matrix deltaMatrix = victoria.DeltaMatrix;
            float deltaAngle = (float)Math.Atan2(deltaMatrix.Backward.X, deltaMatrix.Backward.Z);
            float newOrientation = orientation + deltaAngle;

            //
            // Part 2:  Compute a new location
            //
            Vector3 translateVector = new Vector3((float)Math.Sin(orientation), 0, (float)Math.Cos(orientation));
            translateVector *= translation;
            //location =  location + translateVector;


            // We are likely rotated from the angle the model expects to be in
            // Determine that angle.
            Matrix rootMatrix = victoria.RootMatrix;
            float actualAngle = (float)Math.Atan2(rootMatrix.Backward.X, rootMatrix.Backward.Z);
            Vector3 newLocation = location  + translateVector+ Vector3.TransformNormal(victoria.DeltaPosition,
                               Matrix.CreateRotationY(newOrientation - actualAngle));

            //
            // I'm just taking these here.  You'll likely want to add something 
            // for collision detection instead.
            //
            bool collision = false;     // Until we know otherwise

            string region = TestRegion(newLocation);
            Console.WriteLine(region);
            // Slimed support
            if (!game.Slimed && region == "R_Section6")
            {
                game.Slimed = true;
            }
            else if (game.Slimed && region == "R_Section1")
            {
                game.Slimed = false;
            }

            if (region == "")
            {
                // If not in a region, we have stepped out of bounds
                collision = true;
            }
            else if (region.StartsWith("R_Door"))   // Are we in a door region
            {
                // What is the door number for the region we are in?
                int dnum = int.Parse(region.Substring(6));

                // Are we currently facing the door or walking through a 
                // door?

                bool underDoor;
                if (DoorShouldBeOpen(dnum, location, transform.Backward, out underDoor))
                {
                    SetOpenDoor(dnum);
                }
                else
                {
                    SetOpenDoor(0);
                }

                if (underDoor)
                {
                    // is the door actually open right now?
                    bool isOpen = false;
                    foreach (PrisonModel model in game.PhibesModels)
                    {
                        if (model.DoorIsOpen(dnum))
                        {
                            isOpen = true;
                            break;
                        }
                    }

                    if (!isOpen)
                        collision = true;
                }
            }
            else if (openDoor > 0)
            {
                // Indicate none are open
                SetOpenDoor(0);
            }

            if (!collision)
            {
                location = newLocation;
                orientation = newOrientation;
            }
            //location = newLocation;
            //orientation = newOrientation;

            SetPlayerTransform();
            Matrix rotationMatrix = Matrix.CreateRotationY(orientation);

            Vector3 transformedReference = Vector3.Transform(new Vector3(0, 250, -300), rotationMatrix);

            Vector3 cameraPosition = transformedReference + location;

            game.Camera.Eye = cameraPosition; // location + transform.Backward + new Vector3(0, 180, -150);
            game.Camera.Center = game.Camera.Eye + transform.Backward + new Vector3(0, -0.5f, 0);

          
        }

        private float GetDesiredSpeed(ref KeyboardState keyboardState, ref GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.Up))
                return 1;

            float speed = gamePadState.ThumbSticks.Right.Y;

            // I'm not allowing you to walk backwards
            if (speed < 0)
                speed = 0;

            return speed;
        }
        private float GetDesiredTurnRate(ref KeyboardState keyboardState, ref GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                return panRate;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                return -panRate;
            }
           
            return -gamePadState.ThumbSticks.Right.X * panRate;
        }



        /// <summary>
        /// This function is called to draw the player.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
            Matrix transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;

            victoria.Draw(graphics, gameTime, transform);
        }

        /// <summary>
        /// This is the logic that determines if a door should be open.  This is 
        /// based on a position and a direction we are traveling.  
        /// </summary>
        /// <param name="dnum">Door number we are interested in (1-5)</param>
        /// <param name="loc">A location near the door</param>
        /// <param name="dir">Direction we are currently facing as a vector.</param>
        /// <param name="doorVector">A vector pointing throught the door.</param>
        /// <param name="doorCenter">The center of the door.</param>
        /// <param name="under">Return value - indicates we are under the door</param>
        /// <returns>True if we are under the door</returns>
        private bool DoorShouldBeOpen(int dnum, Vector3 loc, Vector3 dir, out bool under)
        {
            Vector3 doorCenter;
            Vector3 doorVector;

            // I need to know information about the doors.  This 
            // is the location and a vector through the door for each door.
            switch (dnum)
            {
                case 1:
                    doorCenter = new Vector3(218, 0, 1023);
                    doorVector = new Vector3(1, 0, 0);
                    break;

                case 2:
                    doorCenter = new Vector3(-11, 0, -769);
                    doorVector = new Vector3(0, 0, 1);
                    break;

                case 3:
                    doorCenter = new Vector3(587, 0, -999);
                    doorVector = new Vector3(1, 0, 0);
                    break;

                case 4:
                    doorCenter = new Vector3(787, 0, -763);
                    doorVector = new Vector3(0, 0, 1);
                    break;

                case 5:
                default:
                    doorCenter = new Vector3(1187, 0, -1218);
                    doorVector = new Vector3(0, 0, 1);
                    break;
            }

            // I want the door vector to indicate the direction we are doing through the
            // door.  This depends on the side of the center we are on.
            Vector3 toDoor = doorCenter - loc;
            if (Vector3.Dot(toDoor, doorVector) < 0)
            {
                doorVector = -doorVector;
            }


            // Determine if we are under the door
            // Determine points after the center where we are 
            // considered to be under the door
            Vector3 doorBefore = doorCenter - doorVector * DoorUnderRange;
            Vector3 doorAfter = doorCenter + doorVector * DoorUnderRange;
            under = false;

            // If we have passed the point before the door, a vector 
            // to our position from that point will be pointing within 
            // 90 degrees of the door vector.  
            if (Vector3.Dot(loc - doorAfter, doorVector) <= 0 &&
                Vector3.Dot(loc - doorBefore, doorVector) >= 0)
            {
                under = true;
                return true;
            }

            // Are we facing the door?
            if (Vector3.Dot(dir, doorVector) >= 0)
            {
                // We are, so the door should be open
                return true;
            }

            return false;
        }


        /// <summary>
        /// Set the current open/opening door
        /// </summary>
        /// <param name="dnum">Door to set open or 0 if none</param>
        private void SetOpenDoor(int dnum)
        {
            // Is this already indicated?
            if (openDoor == dnum)
                return;

            // Is a door other than this already open?
            // If so, make it close
            if (openDoor > 0 && openDoor != dnum)
            {
                foreach (PrisonModel model in game.PhibesModels)
                {
                    model.SetDoor(openDoor, false);
                }
            }

            // Make this the open door and flag it as open
            openDoor = dnum;
            if (openDoor > 0)
            {
                foreach (PrisonModel model in game.PhibesModels)
                {
                    model.SetDoor(openDoor, true);
                }
            }
        }




    }
}

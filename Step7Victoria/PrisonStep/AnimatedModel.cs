using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaAux;

namespace PrisonStep
{
    public class AnimatedModel
    {
        private Matrix[] skinTransforms = null;

        private AnimationPlayer player = null;
        /// <summary>
        /// The number of skinning matrices in SkinnedEffect.fx. This must
        /// match the number in SkinnedEffect.fx.
        /// </summary>
        public const int NumSkinBones = 57;


        private Matrix rootMatrixRaw = Matrix.Identity;
        private Matrix deltaMatrix = Matrix.Identity;

        public Matrix DeltaMatrix { get { return deltaMatrix; } }
        public Vector3 DeltaPosition;
        public Matrix RootMatrix { get { return inverseBindTransforms[skelToBone[0]] * rootMatrixRaw; } }

        /// <summary>
        /// This class describes a single animation clip we load from
        /// an asset.
        /// </summary>
        private class AssetClip
        {
            public AssetClip(string name, string asset)
            {
                Name = name;
                Asset = asset;
                TheClip = null;
            }

            public string Name { get; set; }
            public string Asset { get; set; }
            public AnimationClips.Clip TheClip { get; set; }
        }

        /// <summary>
        /// A dictionary that allows us to look up animation clips
        /// by name. 
        /// </summary>
        private Dictionary<string, AssetClip> assetClips = new Dictionary<string, AssetClip>();

        private List<int> skelToBone = null;
        private Matrix[] inverseBindTransforms = null;

        private Model model; 
        /// <summary>
        /// reference to the game this class uses
        /// </summary>
        private PrisonGame game;
        /// <summary>
        /// name of the asset we are going to load
        /// </summary>
        private string asset;

        private float angle = 0;
        /// <summary>
        /// the bond transforms as loaded from this model
        /// </summary>
        private Matrix[] bindTransforms;
        /// <summary>
        /// The current gone transforms we will use
        /// </summary>
        private Matrix[] bonesTransforms;
        /// <summary>
        /// the computed absolute transforms
        /// </summary>
        private Matrix[] absoTransforms;

        private AnimationClips.Clip clip = null;

        public AnimatedModel(PrisonGame game, string asset)
        {
            this.game = game;
            this.asset = asset;

            skinTransforms = new Matrix[57];
            for (int i = 0; i < skinTransforms.Length; i++)
            {
                skinTransforms[i] = Matrix.Identity;
            }
        }

        public void LoadContent(ContentManager content)
        {
            
            model = content.Load<Model>(asset);
            // allocate the array to the number of bones we have
            int boneCnt = model.Bones.Count;
            bindTransforms = new Matrix[boneCnt];
            bonesTransforms = new Matrix[boneCnt];
            absoTransforms = new Matrix[boneCnt];

            model.CopyBoneTransformsTo(bindTransforms);
            model.CopyBoneTransformsTo(bonesTransforms);
            model.CopyAbsoluteBoneTransformsTo(absoTransforms);

            AnimationClips clips = model.Tag as AnimationClips;
            if (clips != null && clips.SkelToBone.Count > 0)
            {
                skelToBone = clips.SkelToBone;

                inverseBindTransforms = new Matrix[boneCnt];
                skinTransforms = new Matrix[NumSkinBones];

                model.CopyAbsoluteBoneTransformsTo(inverseBindTransforms);

                for (int b = 0; b < inverseBindTransforms.Length; b++)
                    inverseBindTransforms[b] = Matrix.Invert(inverseBindTransforms[b]);

                for (int i = 0; i < skinTransforms.Length; i++)
                    skinTransforms[i] = Matrix.Identity;
            }

            foreach (AssetClip clip in assetClips.Values)
            {
                Model clipmodel = content.Load<Model>(clip.Asset);
                AnimationClips modelclips = clipmodel.Tag as AnimationClips;
                clip.TheClip = modelclips.Clips["Take 001"];
            }
            //PlayClip("Take 001");
        }

                /// <summary>
        /// Add an asset clip to the dictionary.
        /// </summary>
        /// <param name="name">Name we will use for the clip</param>
        /// <param name="asset">The FBX asset to load</param>
        public void AddAssetClip(string name, string asset)
        {
            assetClips[name] = new AssetClip(name, asset);
        }

        

        /// <summary>
        /// Play an animation clip on this model.
        /// </summary>
        /// <param name="name"></param>
        public AnimationPlayer PlayClip(string name)
        {
            if (name != "Take 001")
            {
                player = new AnimationPlayer(this, assetClips[name].TheClip);
                Update(0);
                return player;
            }
            player = null;

            AnimationClips clips = model.Tag as AnimationClips;
            if (clips != null)
            {
                player = new AnimationPlayer(this, clips.Clips[name]);
            }

            return player;
        }

        public void Update(double delta)
        {
            

            if (clip != null)
            {
                // Update the clip
                player.Update(delta);

                for (int b = 0; b < player.BoneCount; b++)
                {
                    AnimationPlayer.Bone bone = player.GetBone(b);
                    if (!bone.Valid)
                        continue;

                    Vector3 scale = new Vector3(bindTransforms[b].Right.Length(),
                        bindTransforms[b].Up.Length(),
                        bindTransforms[b].Backward.Length());

                    bonesTransforms[b] = Matrix.CreateScale(scale) *
                        Matrix.CreateFromQuaternion(bone.Rotation) *
                        Matrix.CreateTranslation(bone.Translation);
                }
                if (skelToBone != null)
                {
                    int rootBone = skelToBone[0];

                    deltaMatrix = Matrix.Invert(rootMatrixRaw) * bonesTransforms[rootBone];
                    DeltaPosition = bonesTransforms[rootBone].Translation - rootMatrixRaw.Translation;

                    rootMatrixRaw = bonesTransforms[rootBone];
                    bonesTransforms[rootBone] = bindTransforms[rootBone];
                }
                model.CopyBoneTransformsFrom(bonesTransforms);
            }
            bonesTransforms[9] = Matrix.CreateRotationX(0.8f) * bindTransforms[9];
           
            model.CopyBoneTransformsFrom(bonesTransforms);
            model.CopyAbsoluteBoneTransformsTo(absoTransforms);
        }

        /// <summary>
        /// This function is called to draw this game component.
        /// </summary>
        /// <param name="graphics">Device to draw the model on.</param>
        /// <param name="gameTime">Current game time.</param>
        /// <param name="transform">Transform that puts the model where we want it.</param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Matrix transform)
        {
            DrawModel(graphics, model, transform);
        }

        private void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world)
        {
            if (skelToBone != null)
            {
                for (int b = 0; b < skelToBone.Count; b++)
                {
                    int n = skelToBone[b];
                    skinTransforms[b] = inverseBindTransforms[n] * absoTransforms[n];
                }
            }

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(absoTransforms[mesh.ParentBone.Index] * world);
                    effect.Parameters["View"].SetValue(game.Camera.View);
                    effect.Parameters["Projection"].SetValue(game.Camera.Projection);

                    effect.Parameters["Bones"].SetValue(skinTransforms);
                }
                mesh.Draw();
            }

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XnaAux;

namespace PrisonStep
{
	/// <summary>
	/// The AnimationPlayer class contains the code to play an animation on
	/// an animated model.
	/// </summary>
	public class AnimationPlayer
	{
		public interface Bone
		{
			bool Valid { get; }
			Quaternion Rotation { get; }
			Vector3 Translation { get; }
		}


		private struct BoneInfo : Bone
		{
			private int currentKeyframe;     // Current keyframe for bone
			private bool valid;

			private Quaternion rotation;
			private Vector3 translation;

			public AnimationClips.Keyframe Keyframe1;
			public double Time1;                    // First time

			public AnimationClips.Keyframe Keyframe2;     // Second keyframe value
			public double Time2;                    // Second time

			public int CurrentKeyframe { get { return currentKeyframe; } set { currentKeyframe = value; } }
			public bool Valid { get { return valid; } set { valid = value; } }
			public Quaternion Rotation { get { return rotation; } set { rotation = value; } }
			public Vector3 Translation { get { return translation; } set { translation = value; } }
		}

		/// <summary>
		/// The model we are playing onto
		/// </summary>
		private AnimatedModel model;

		/// <summary>
		/// The clip we are playing
		/// </summary>
		private AnimationClips.Clip clip = null;

		private bool looping = true;
		private double speed = 1.0f;

        private double time = 0;



		private BoneInfo[] boneInfos;
		private int boneCnt;

		public int BoneCount { get { return boneCnt; } }



		#region Properties


        public double Time
        {
            get { return time; }
            set { time = value; }
        }
		/// <summary>
		/// Indicates if the playback should "loop" or not.
		/// </summary>
		public bool Looping { get { return looping; } set { looping = value; } }

        /// <summary>
        /// Get the clip being played by this player
        /// </summary>
        public AnimationClips.Clip Clip { get { return clip; } }

		/// <summary>
		/// Playback speed
		/// </summary>
		public double Speed { get { return speed; } set { speed = value; } }

		/// <summary>
		/// Get a bone
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public Bone GetBone(int b) { return boneInfos[b]; }
		#endregion

		/// <summary>
		/// Construct an AnimationPlayer to play the clip
		/// </summary>
		/// <param name="model"></param>
		/// <param name="clip"></param>
		public AnimationPlayer(AnimatedModel model, AnimationClips.Clip clip)
		{
			this.model = model;
			this.clip = clip;

			Initialize();
		}

		/// <summary>
		/// Initialize for use
		/// </summary>
		public void Initialize()
		{
			boneCnt = clip.Keyframes.Length;
			boneInfos = new BoneInfo[boneCnt];

			time = 0;
			for (int b = 0; b < boneCnt; b++)
			{
				boneInfos[b].CurrentKeyframe = -1;
				boneInfos[b].Valid = false;
			}
		}


		/// <summary>
		/// Update the clip position
		/// </summary>
		/// <param name="delta">The amount of time that has passed.</param>
		public void Update(double delta)
		{
			time += delta;
            /*if (looping && time >= clip.Duration)
            {
                time -= clip.Duration;
                for (int b = 0; b < boneCnt; b++)
                {
                    boneInfos[b].CurrentKeyframe = -1;
                    boneInfos[b].Valid = false;
                }
            }*/

			for (int b = 0; b < boneInfos.Length; b++)
			{
				List<AnimationClips.Keyframe> keyframes = clip.Keyframes[b];
				if (keyframes.Count == 0)
					continue;

				// The time needs to be greater than or equal to the
				// current keyframe time and less than the next keyframe 
				// time.
				while (boneInfos[b].CurrentKeyframe < 0 ||
					(boneInfos[b].CurrentKeyframe < keyframes.Count - 1 &&
					keyframes[boneInfos[b].CurrentKeyframe + 1].Time <= time))
				{
					// Advance to the next keyframe
					boneInfos[b].CurrentKeyframe++;

					int c = boneInfos[b].CurrentKeyframe;

					boneInfos[b].Keyframe1 = keyframes[c];
					boneInfos[b].Time1 = keyframes[c].Time;
					if (c == keyframes.Count - 1)
					{
						if (looping)
						{
							boneInfos[b].Keyframe2 = keyframes[0];
							boneInfos[b].Time2 = clip.Duration;
						}
						else
						{
							// If not looping, there is no next keyframe.
							boneInfos[b].Keyframe2 = null;
							boneInfos[b].Time2 = clip.Duration;
						}
					}
					else
					{
						boneInfos[b].Keyframe2 = keyframes[c + 1];
						boneInfos[b].Time2 = keyframes[c + 1].Time;
					}

				}

				//
				// Update the bone
				//

				if (boneInfos[b].Keyframe1 != null)
				{
					if (boneInfos[b].Keyframe2 != null)
					{
						AnimationClips.Keyframe keyframe1 = boneInfos[b].Keyframe1;
						AnimationClips.Keyframe keyframe2 = boneInfos[b].Keyframe2;

						float t = (float)((time - boneInfos[b].Time1) / (boneInfos[b].Time2 - boneInfos[b].Time1));

						boneInfos[b].Rotation = Quaternion.Slerp(keyframe1.Rotation, keyframe2.Rotation, t);
						boneInfos[b].Translation = Vector3.Lerp(keyframe1.Translation, keyframe2.Translation, t);
						boneInfos[b].Valid = true;
					}
					else
					{
						AnimationClips.Keyframe keyframe1 = boneInfos[b].Keyframe1;
						boneInfos[b].Rotation = keyframe1.Rotation;
						boneInfos[b].Translation = keyframe1.Translation;
					}

					boneInfos[b].Valid = true;
				}
			}
		}
	}
}

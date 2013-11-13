using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XnaAux
{
    /// <summary>
    /// Class that contains animation clips data.  This class is 
    /// shared between content processing and the runtime.
    /// </summary>
    public class AnimationClips
    {

        /// <summary>
        /// An index array the converts bones in the skeleton
        /// to bones in the model.
        /// </summary>
        public List<int> SkelToBone { get; set; }
        /// <summary>
        /// An Keyframe is a rotation and translation for a moment in time.
        /// </summary>
        public class Keyframe
        {
            public double Time;             // The keyframe time
            public Quaternion Rotation;     // The rotation for the bone
            public Vector3 Translation;     // The translation for the bone
        }

        public interface Bone
        {
            bool Valid { get; }
            Quaternion Rotation { get; }
            Vector3 Translation { get; }
        }

        /// <summary>
        /// An animation clip is a set of keyframes.  
        /// </summary>
        public class Clip
        {
            /// <summary>
            /// Name of the animation clip
            /// </summary>
            public string Name;

            /// <summary>
            /// Duration of the animation clip
            /// </summary>
            public double Duration;


            /// <summary>
            /// The keyframes in the animation. We have an array of bones
            /// each with a list of keyframes.
            /// </summary>
            public List<Keyframe>[] Keyframes;
        }

        /// <summary>
        /// The clips for this set of animation clips.
        /// </summary>
        public Dictionary<string, Clip> Clips = new Dictionary<string,Clip>();

    }
}

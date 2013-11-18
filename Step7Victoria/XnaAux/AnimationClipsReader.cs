using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace XnaAux
{
    public class AnimationClipsReader : ContentTypeReader<AnimationClips>
    {
        protected override AnimationClips Read(ContentReader input, AnimationClips existingInstance)
        {
            AnimationClips clips = new AnimationClips();

            // Determine how many clips there are.
            int clipCnt = input.ReadInt32();
            //clips.SkelToBone = input.ReadObject<List<int>>();
            for (int c = 0; c < clipCnt; c++)
            {
                // Create a clip and load  it up
                AnimationClips.Clip clip = new AnimationClips.Clip();
                clip.Name = input.ReadString();
                clip.Duration = input.ReadDouble();
                
                // Determine how many bones there are.
                int boneCnt = input.ReadInt32();
                
                clip.Keyframes = new List<AnimationClips.Keyframe>[boneCnt];

                for (int i = 0; i < boneCnt; i++)
                {
                    // Determine how many keyframes there are.
                    int cnt = input.ReadInt32();
                    List<AnimationClips.Keyframe> boneKeyframes = new List<AnimationClips.Keyframe>(cnt);
                    clip.Keyframes[i] = boneKeyframes;

                    for (int j = 0; j < cnt; j++)
                    {
                        AnimationClips.Keyframe keyframe = new AnimationClips.Keyframe();
                        keyframe.Time = input.ReadDouble();
                        keyframe.Rotation = input.ReadQuaternion();
                        keyframe.Translation = input.ReadVector3();

                        boneKeyframes.Add(keyframe);

                    }

                }
                
                clips.Clips[clip.Name] = clip;
            }
            clips.SkelToBone = input.ReadObject<List<int>>();
            return clips;
        }
    }
}
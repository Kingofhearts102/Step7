using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline;
using XnaAux;

namespace AnimationPipeline
{
    [ContentTypeWriter]
    public class AnimationClipsWriter : ContentTypeWriter<AnimationClips>
    {
        protected override void Write(ContentWriter output, AnimationClips clips)
        {
            
            output.Write(clips.Clips.Count);
            output.WriteObject(clips.SkelToBone);
            foreach (KeyValuePair<string, AnimationClips.Clip> clipItem in clips.Clips)
            {
                AnimationClips.Clip clip = clipItem.Value;

                output.Write(clip.Name);
                output.Write(clip.Duration);
                output.Write(clip.Keyframes.Length);
                
                foreach (List<AnimationClips.Keyframe> keyframes in clip.Keyframes)
                {
                    output.Write(keyframes.Count);
                    foreach (AnimationClips.Keyframe keyframe in keyframes)
                    {
                        output.Write(keyframe.Time);
                        output.Write(keyframe.Rotation);
                        output.Write(keyframe.Translation);
                    }
                }
                
            }
            

        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(AnimationClipsReader).AssemblyQualifiedName;
        }
    }
}
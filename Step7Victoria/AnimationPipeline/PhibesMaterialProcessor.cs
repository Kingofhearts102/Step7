using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace AnimationPipeline
{
    [ContentProcessor(DisplayName = "Phibes Material Processor")]
    public class PhibesMaterialProcessor : MaterialProcessor
    {
        private int section = 0;

        public int Section { get { return section; } set { section = value; } }

        private double[] lightData =
{   1,      568,      246,    1036,   0.53,   0.53,   0.53,     821,     224, 
  941,  14.2941,       45, 43.9412,    814,    224,   1275,    82.5,       0,  0,
    2,       -5,      169,     428, 0.3964,  0.503, 0.4044,    -5.4,     169,
 1020, 129.4902, 107.5686, 41.8039,   -5.4,    169,   -138, 37.8275,      91, 91,
    3,      113,      217,    -933,    0.5,      0,      0,    -129,     185,
-1085,	     50,        0,       0,    501,    185,  -1087,      48,       0,  0,
    4,      781,      209,    -998,    0.2, 0.1678, 0.1341,    1183,     209,
 -998,	     50,  41.9608, 33.5294,    984,    113,   -932,       0,      80,  0,
    5,      782,      177,    -463,   0.65, 0.5455, 0.4359,     563,     195,
 -197,	     50,        0,       0,   1018,    181,   -188,      80,       0,  0,
    6,     1182,      177,   -1577,   0.65, 0.5455, 0.4359,     971,     181,
-1801,        0,  13.1765,      80,   1406,    181,  -1801,       0, 13.1765,  80};

        private bool skinned = false;

        /// <summary>
        /// Set true if this model should use the SkinnedEffect.fx instead
        /// of the other Phibes model effects.
        /// </summary>
        public bool Skinned { get { return skinned; } set { skinned = value; } }

        public override MaterialContent Process(MaterialContent input, ContentProcessorContext context)
        {
            // Create a new material effect
            EffectMaterialContent customMaterial = new EffectMaterialContent();

            // Access the input as a basic material
            BasicMaterialContent basicMaterial = (BasicMaterialContent)input;

            // If Texture is null, we are not using texture mapping. Otherwise, we are
            if (skinned)
            {
                string effectFile = Path.GetFullPath("SkinnedEffect.fx");
                customMaterial.Effect = new ExternalReference<EffectContent>(effectFile);

                customMaterial.Textures.Add("Texture", basicMaterial.Texture);
                section = 1;
            }
            else if (basicMaterial.Texture == null)
            {
                // I don't know why, but sometimes you get an invalid material.  So,
                // I just let the base processor handle it.
                if (basicMaterial.DiffuseColor == null)
                    return base.Process(input, context);

                string effectFile = Path.GetFullPath("PhibesEffect1.fx");
                customMaterial.Effect = new ExternalReference<EffectContent>(effectFile);
                customMaterial.OpaqueData.Add("DiffuseColor", basicMaterial.DiffuseColor);
            }
            else
            {
                string effectFile = Path.GetFullPath("PhibesEffect2.fx");
                customMaterial.Effect = new ExternalReference<EffectContent>(effectFile);

                customMaterial.Textures.Add("Texture", basicMaterial.Texture);
            }

            customMaterial.OpaqueData.Add("Light1Location", LightInfo(section, 0));
            customMaterial.OpaqueData.Add("Light1Color", LightInfo(section, 1));
            customMaterial.OpaqueData.Add("Light2Location", LightInfo(section, 2));
            customMaterial.OpaqueData.Add("Light2Color", LightInfo(section, 3));
            customMaterial.OpaqueData.Add("Light3Location", LightInfo(section, 4));
            customMaterial.OpaqueData.Add("Light3Color", LightInfo(section, 5));

            // Chain to the base material processor.
            return base.Process(customMaterial, context);
        }

        private Vector3 LightInfo(int section, int item)
        {
            int offset = (section - 1) * 19 + 1 + (item * 3);
            return new Vector3((float)lightData[offset],
                               (float)lightData[offset + 1],
                               (float)lightData[offset + 2]);
        }


    }
}

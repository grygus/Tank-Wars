using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TanksOnAHeightmap.GameBase.Effects
{
    public class TerrainEffect
    {
        // List all the techniques present in this effect
        public enum Techniques
        {
            Terrain = 0
        }

        // Effect file name
        public static string EFFECT_FILENAME = GameAssetsPath.EFFECTS_PATH + "Terrain";

        // Shader effect
       public Effect effect;

        // Only store this to help calculate auxiliar matrices
        Matrix worldMatrix, viewMatrix, projectionMatrix;

        // Effect parameters - Matrices
        EffectParameter worldParam;
        EffectParameter viewInverseParam;
        EffectParameter worldViewProjectionParam;

        // Effect parameters - Material
        EffectParameter diffuseColorParam;
        EffectParameter specularColorParam;
        EffectParameter specularPowerParam;

        // Effect parameters - Lights
        EffectParameter ambientLightColorParam;
        EffectParameter light1PositionParam;
        EffectParameter light1ColorParam;
        EffectParameter light2PositionParam;
        EffectParameter light2ColorParam;

        // Effect parameters - Textures
        EffectParameter diffuseTexture1Param;
        EffectParameter diffuseTexture2Param;
        EffectParameter diffuseTexture3Param;
        EffectParameter diffuseTexture4Param;
        EffectParameter normalMapTextureParam;
        EffectParameter alphaMapTextureParam;

        // Effect parameters - Textures UVs
        EffectParameter textureUV1TileParam;
        EffectParameter textureUV2TileParam;
        EffectParameter textureUV3TileParam;
        EffectParameter textureUV4TileParam;
        EffectParameter textureUVNormalTileParam;

        #region Properties
        public TerrainEffect.Techniques CurrentTechnique
        {
            set
            {
                effect.CurrentTechnique = effect.Techniques[(int)value];
            }
        }

        public EffectPassCollection CurrentTechniquePasses
        {
            get
            {
                return effect.CurrentTechnique.Passes;
            }
        }

        public Matrix World
        {
            get
            {
                return worldMatrix;
            }
            set
            {
                worldMatrix = value;
                worldParam.SetValue(value);
                worldViewProjectionParam.SetValue(worldMatrix * viewMatrix * projectionMatrix);
            }
        }

        public Matrix View
        {
            get
            {
                return viewMatrix;
            }
            set
            {
                viewMatrix = value;
                Matrix viewInverseMatrix = Matrix.Invert(viewMatrix);

                viewInverseParam.SetValue(viewInverseMatrix);
                worldViewProjectionParam.SetValue(worldMatrix * viewMatrix * projectionMatrix);
            }
        }

        public Matrix Projection
        {
            get
            {
                return projectionMatrix;
            }
            set
            {
                projectionMatrix = value;
                worldViewProjectionParam.SetValue(worldMatrix * viewMatrix * projectionMatrix);
            }
        }

        public Vector3 AmbientLightColor
        {
            get
            {
                return ambientLightColorParam.GetValueVector3();
            }
            set
            {
                ambientLightColorParam.SetValue(value);
            }
        }

        public Vector3 DiffuseColor
        {
            get
            {
                return diffuseColorParam.GetValueVector3();
            }
            set
            {
                diffuseColorParam.SetValue(value);
            }
        }

        public Vector3 SpecularColor
        {
            get
            {
                return specularColorParam.GetValueVector3();
            }
            set
            {
                specularColorParam.SetValue(value);
            }
        }

        public float SpecularPower
        {
            get
            {
                return specularPowerParam.GetValueSingle();
            }
            set
            {
                specularPowerParam.SetValue(value);
            }
        }

        public Vector3 Light1Position
        {
            get
            {
                return light1PositionParam.GetValueVector3();
            }
            set
            {
                light1PositionParam.SetValue(value);
            }
        }

        public Vector3 Light1Color
        {
            get
            {
                return light1ColorParam.GetValueVector3();
            }
            set
            {
                light1ColorParam.SetValue(value);
            }
        }

        public Vector3 Light2Position
        {
            get
            {
                return light2PositionParam.GetValueVector3();
            }
            set
            {
                light2PositionParam.SetValue(value);
            }
        }

        public Vector3 Light2Color
        {
            get
            {
                return light2ColorParam.GetValueVector3();
            }
            set
            {
                light2ColorParam.SetValue(value);
            }
        }

        public Texture DiffuseTexture1
        {
            get
            {
                return diffuseTexture1Param.GetValueTexture2D();
            }
            set
            {
                diffuseTexture1Param.SetValue(value);
            }
        }

        public Texture DiffuseTexture2
        {
            get
            {
                return diffuseTexture2Param.GetValueTexture2D();
            }
            set
            {
                diffuseTexture2Param.SetValue(value);
            }
        }

        public Texture DiffuseTexture3
        {
            get
            {
                return diffuseTexture3Param.GetValueTexture2D();
            }
            set
            {
                diffuseTexture3Param.SetValue(value);
            }
        }

        public Texture DiffuseTexture4
        {
            get
            {
                return diffuseTexture4Param.GetValueTexture2D();
            }
            set
            {
                diffuseTexture4Param.SetValue(value);
            }
        }

        public Texture NormalMapTexture
        {
            get
            {
                return normalMapTextureParam.GetValueTexture2D();
            }
            set
            {
                normalMapTextureParam.SetValue(value);
            }
        }

        public Texture AlphaMapTexture
        {
            get
            {
                return alphaMapTextureParam.GetValueTexture2D();
            }
            set
            {
                alphaMapTextureParam.SetValue(value);
            }
        }

        public Vector2 TextureUV1Tile
        {
            get
            {
                return textureUV1TileParam.GetValueVector2();
            }
            set
            {
                textureUV1TileParam.SetValue(value);
            }
        }

        public Vector2 TextureUV2Tile
        {
            get
            {
                return textureUV2TileParam.GetValueVector2();
            }
            set
            {
                textureUV2TileParam.SetValue(value);
            }
        }

        public Vector2 TextureUV3Tile
        {
            get
            {
                return textureUV3TileParam.GetValueVector2();
            }
            set
            {
                textureUV3TileParam.SetValue(value);
            }
        }

        public Vector2 TextureUV4Tile
        {
            get
            {
                return textureUV4TileParam.GetValueVector2();
            }
            set
            {
                textureUV4TileParam.SetValue(value);
            }
        }

        public Vector2 TextureUVBumpTile
        {
            get
            {
                return textureUVNormalTileParam.GetValueVector2();
            }
            set
            {
                textureUVNormalTileParam.SetValue(value);
            }
        }
        #endregion

        public TerrainEffect(Effect effect)
        {
            this.effect = effect;
            GetEffectParameters();
        }

        /// <summary>
        /// Get all effects parameters by name
        /// </summary>
        private void GetEffectParameters()
        {
            // Matrices
            worldParam = effect.Parameters["matW"];
            viewInverseParam = effect.Parameters["matVI"];
            worldViewProjectionParam = effect.Parameters["matWVP"];

            // Material
            diffuseColorParam = effect.Parameters["diffuseColor"];
            specularColorParam = effect.Parameters["specularColor"];
            specularPowerParam = effect.Parameters["specularPower"];

            // Lights
            ambientLightColorParam = effect.Parameters["ambientLightColor"];
            light1PositionParam = effect.Parameters["light1Position"];
            light1ColorParam = effect.Parameters["light1Color"];
            light2PositionParam = effect.Parameters["light2Position"];
            light2ColorParam = effect.Parameters["light2Color"];

            // Textures
            diffuseTexture1Param = effect.Parameters["diffuseTexture1"];
            diffuseTexture2Param = effect.Parameters["diffuseTexture2"];
            diffuseTexture3Param = effect.Parameters["diffuseTexture3"];
            diffuseTexture4Param = effect.Parameters["diffuseTexture4"];
            normalMapTextureParam = effect.Parameters["normalTexture"];
            alphaMapTextureParam = effect.Parameters["alphaTexture"];
            textureUV1TileParam = effect.Parameters["uv1Tile"];
            textureUV2TileParam = effect.Parameters["uv2Tile"];
            textureUV3TileParam = effect.Parameters["uv3Tile"];
            textureUV4TileParam = effect.Parameters["uv4Tile"];
            textureUVNormalTileParam = effect.Parameters["uvNormalTile"];
        }

        /// <summary>
        /// Begin effect
        /// </summary>
        public void Begin()
        {
            effect.CurrentTechnique.Passes[0].Apply();
            
        }

        /// <summary>
        /// End effect
        /// </summary>
        public void End()
        {
            //effect.End();
        }
    }
}

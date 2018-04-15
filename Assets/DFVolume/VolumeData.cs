// DFVolume - Distance field volume generator for Unity
// https://github.com/keijiro/DFVolume

using UnityEngine;

namespace DFVolume
{
    public class VolumeData : ScriptableObject
    {
        #region Exposed attributes

        [SerializeField] Texture3D _texture;
        [SerializeField]public  ComputeBuffer _buffer;
        [SerializeField]public float[] _values;
        [SerializeField]public int dimensions;

        public Texture3D texture {
            get { return _texture; }
        }

        public ComputeBuffer buffer {
            get { return _buffer; }
        }

        public float[] values {
            get { return _values; }
        }

        #endregion

        #if UNITY_EDITOR

        #region Editor functions

        public void Initialize(VolumeSampler sampler)
        {
            var dim = sampler.resolution;

            dimensions = dim;


            Color[] bmp = new Color[dim * dim * dim];

            _buffer = new ComputeBuffer( dim * dim * dim, sizeof(float)*4);


            _values = sampler.GenerateBitmap( out bmp);
            _buffer.SetData(values);

            /*_texture = new Texture3D(dim, dim, dim, TextureFormat.RGBAHalf, true);

            _texture.name = "Distance Field Texture";
            _texture.filterMode = FilterMode.Bilinear;
            _texture.wrapMode = TextureWrapMode.Clamp;
            _texture.SetPixels(bmp);
            _texture.Apply();*/
        }

        #endregion

        #endif
    }
}

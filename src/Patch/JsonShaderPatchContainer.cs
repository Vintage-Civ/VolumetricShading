using Vintagestory.API.Common;
using Vintagestory.Common;

namespace VolumetricShading.Patch
{
    public class JsonShaderPatchContainer : Asset
    {
        public JsonShaderPatchContainer(byte[] bytes, AssetLocation Location, IAssetOrigin origin) : base(bytes, Location, origin)
        {
        }
        
        public double Order { get; set; }
        
        public JsonShaderPatch[] Patches { get; set; }
    }
}
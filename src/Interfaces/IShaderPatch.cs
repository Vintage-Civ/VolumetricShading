using Vintagestory.API.Client;

namespace VolumetricShading.ShaderPatching
{
    public interface IShaderPatch
    {
        bool ShouldPatch(string filename, string code);
        
        string Patch(string filename, string code);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VolumetricShading.ShaderPatching
{
    public class JsonPatchLoader
    {
        public static readonly AssetCategory ShaderPatches = new AssetCategory("shaderpatches", false, EnumAppSide.Client);

        public static readonly AssetCategory ShaderSnippets = new AssetCategory("shadersnippets", false, EnumAppSide.Client);

        private readonly ShaderPatcher _patcher;
        private readonly string _domain;
        private readonly ICoreClientAPI _capi;
        
        public JsonPatchLoader(ShaderPatcher patcher, string domain, ICoreClientAPI capi)
        {
            _patcher = patcher;
            _domain = domain;
            _capi = capi;
        }
        
        public void Load()
        {
            _capi.Assets.Reload(ShaderPatches);
            _capi.Assets.Reload(ShaderSnippets);

            var assets = _capi.Assets.GetMany<JsonShaderPatchContainer>(_capi.World.Logger, "shaderpatches", _domain).Values.ToArray();
            Array.Sort(assets, (a, b) => a.Order.CompareTo(b.Order));
            
            foreach (var asset in assets)
            {
                LoadPatch(asset.Patches);
            }
        }

        public void LoadPatch(JsonShaderPatch[] patches)
        {
            foreach (var patch in patches)
            {
                var content = patch.Content?.Replace("\n", "\r\n  ");

                if (!string.IsNullOrEmpty(patch.Snippet))
                {
                    content = _capi.Assets.Get(new AssetLocation(_domain, "shadersnippets/" + patch.Snippet))
                        .ToText();
                }

                switch (patch.Type)
                {
                    case EnumShaderPatchType.Start:
                        AddStartPatch(patch, content);
                        break;
                        
                    case EnumShaderPatchType.End:
                        AddEndPatch(patch, content);
                        break;

                    case EnumShaderPatchType.Regex:
                        AddRegexPatch(patch, content);
                        break;
                    
                    case EnumShaderPatchType.Token:
                        AddTokenPatch(patch, content);
                        break;
                    
                    default:
                        throw new ArgumentException($"Invalid type {patch.Type}");
                }
            }
        }

        private void AddTokenPatch(JsonShaderPatch patch, string content)
        {
            if (patch.FileName == null)
                _patcher.AddTokenPatch(patch.Tokens, content);
            else
                _patcher.AddTokenPatch(patch.FileName, patch.Tokens, content);
        }

        private void AddRegexPatch(JsonShaderPatch patch, string content)
        {
            var patchObj = patch.FileName == null
                ? new RegexPatch(patch.Regex)
                : new RegexPatch(patch.FileName, patch.Regex);

            patchObj.Multiple = patch.Multiple;
            patchObj.Optional = patch.Optional;
            patchObj.ReplacementString = content;
            _patcher.AddPatch(patchObj);
        }

        private void AddEndPatch(JsonShaderPatch patch, string content)
        {
            if (patch.FileName == null)
                _patcher.AddAtEnd(content);
            else
                _patcher.AddAtEnd(patch.FileName, content);
        }

        private void AddStartPatch(JsonShaderPatch patch, string content)
        {
            if (patch.FileName == null)
                _patcher.AddAtStart(content);
            else
                _patcher.AddAtStart(patch.FileName, content);
        }
    }
}
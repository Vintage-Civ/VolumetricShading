namespace VolumetricShading.Patch
{
    public class JsonShaderPatch
    {
        public EnumShaderPatchType Type { get; set; }

        public string FileName { get; set; }

        public string Content { get; set; }

        public string Snippet { get; set; }

        public string Tokens { get; set; }

        public string Regex { get; set; }

        public bool Multiple { get; set; }

        public bool Optional { get; set; }
    }
}
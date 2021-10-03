using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaterializerLibrary
{
    public record JsonSourcePath
    {
        public JsonSourcePath(string path, bool isArrayElement = false)
            => (Path, IsArray) = (path, isArrayElement);

        public string Path { get; init; }
        public bool IsObject { get; set; }
        public bool IsArray { get; set; }
        public bool IsArrayElement { get; set; }
    }

}

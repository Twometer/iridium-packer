using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IridiumPacker
{
    public class AsarFile
    {
        public string Path;
        public long Offset;
        public long Length;

        public AsarFile(string path, long offset, long length)
        {
            Path = path;
            Offset = offset;
            Length = length;
        }
    }
}

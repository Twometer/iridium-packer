using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IridiumPacker
{
    class Program
    {
        public static void Main(string[] args)
        {
            string srcPath = args.Length < 2 ? Environment.CurrentDirectory : args[0];
            string dstPath = args.Length < 2 ? Path.Combine(Environment.CurrentDirectory, "app.asar") : args[1];
            AsarPacker packer = new AsarPacker(srcPath, dstPath);
            packer.WriteAsarFile();
        }
    }
}

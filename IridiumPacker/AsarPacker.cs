using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IridiumPacker
{
    public class AsarPacker
    {
        private readonly string srcDirectory;
        private readonly string path;
        private readonly List<AsarFile> filesystem = new List<AsarFile>();
        private readonly AsarPackerConfig config;

        private long offset;

        public AsarPacker(string srcDirectory, string path) : this(srcDirectory, path, new AsarPackerConfig()) { }

        public AsarPacker(string srcDirectory, string path, AsarPackerConfig config)
        {
            this.srcDirectory = srcDirectory;
            this.path = path;
            this.config = config;
        }

        public void WriteAsarFile()
        {
            var stream = File.OpenWrite(path);
            var writer = new BinaryWriter(stream);

            WriteMetadata(writer);
            WriteFilesystem(writer);

            writer.Close();
            stream.Close();
        }

        private void WriteFilesystem(BinaryWriter writer)
        {
            foreach (AsarFile file in filesystem)
            {
                FileInfo info = new FileInfo(file.Path);
                if(!CheckFile(info))
                    continue;
                var stream = info.OpenRead();
                byte[] buf = new byte[4096];
                int read = 0;
                while (read < info.Length)
                {
                    int r = stream.Read(buf, 0, buf.Length);
                    read += r;
                    writer.Write(buf, 0, r);
                }
            }
        }

        private void WriteMetadata(BinaryWriter writer)
        {
            var descriptor = BuildFilesystemDescriptor();
            WriteHeader(writer, descriptor.Length);
            writer.Write(descriptor);
        }

        private byte[] BuildFilesystemDescriptor()
        {
            StringBuilder builder = new StringBuilder();
            DirectoryInfo src = new DirectoryInfo(srcDirectory);
            GenerateDescriptor(builder, src, false);
            return Encoding.UTF8.GetBytes(builder.ToString().Trim(',').Replace(",}","}"));
        }

        private void GenerateDescriptor(StringBuilder builder, DirectoryInfo info, bool prependName)
        {
            if (prependName) builder.Append(Quote(info.Name)).Append(":");
            builder.Append("{").Append(Quote("files")).Append(":{");
            foreach (DirectoryInfo subdir in info.EnumerateDirectories())
            {
                if (!CheckDir(subdir))
                    continue;
                GenerateDescriptor(builder, subdir, true);
            }
            var first = true;
            foreach (FileInfo file in info.EnumerateFiles())
            {
                if (!CheckFile(file))
                    continue;
                if (!first)
                    builder.Append(",");
                first = false;
                builder.Append(Quote(file.Name)).Append(":{").Append(Quote("size")).Append(":").Append(file.Length).Append(",").Append(Quote("offset")).Append(":").Append(Quote(offset.ToString())).Append("}");
                filesystem.Add(new AsarFile(file.FullName, offset, file.Length));
                offset += file.Length;
            }
            builder.Append("}},");
        }

        private bool CheckFile(FileInfo info)
        {
            if (info.FullName == path) return false;
            if (config.SkipChangelogFiles && info.Name.ToLower().Contains("changelog")) return false;
            if (config.SkipLicenseFiles && info.Name.ToLower().Contains("license")) return false;
            if (config.SkipReadmeFiles && info.Name.ToLower().Contains("readme")) return false;
            return !config.SkipTypescriptFiles || !info.FullName.EndsWith(".ts");
        }

        private bool CheckDir(DirectoryInfo info)
        {
            if (!config.SkipElectronDirectory) return true;
            return info.Parent.Name != "electron" || info.Name != "dist";
        }

        private string Quote(string s)
        {
            return "\"" + s + "\"";
        }

        private void WriteHeader(BinaryWriter writer, int len)
        {
            writer.Write(new byte[] { 0x04, 0x00, 0x00, 0x00 }); // Magic Number
            writer.Write(GetLittleEndianBytes(len + 8));
            writer.Write(GetLittleEndianBytes(len + 4));
            writer.Write(GetLittleEndianBytes(len));
        }

        private byte[] GetLittleEndianBytes(int i)
        {
            return BitConverter.IsLittleEndian ? BitConverter.GetBytes(i) : ReverseArray(BitConverter.GetBytes(i));
        }

        private byte[] ReverseArray(byte[] b)
        {
            Array.Reverse(b);
            return b;
        }
    }
}

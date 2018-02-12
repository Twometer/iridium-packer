using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IridiumPacker
{
    public class AsarPackerConfig
    {

        public bool SkipLicenseFiles = true;
        public bool SkipReadmeFiles = true;
        public bool SkipChangelogFiles = true;
        public bool SkipTypescriptFiles = true;

        public bool SkipElectronDirectory = true;

    }
}

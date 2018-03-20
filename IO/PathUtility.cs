using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SKCore.IO
{
    public static class PathUtility
    {
        public static void CopyTree(string src, string dst, bool overwrite = false)
        {
            var fileInfo = new FileInfo(dst);
            fileInfo.Directory.Create();

            File.Copy(src, dst, overwrite);
        }
    }
}

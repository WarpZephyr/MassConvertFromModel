using SoulsFormats;
using SoulsFormats.AC4;
using System;

namespace MassConvertFromModel
{
    public static class PathHandler
    {
        public static void EnsureFolderExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string GetExtensionless(string name)
        {
            string extensionlessName = $"{Path.GetFileNameWithoutExtension(name)}";
            while (extensionlessName.Contains('.'))
            {
                extensionlessName = $"{Path.GetFileNameWithoutExtension(extensionlessName)}";
            }
            return extensionlessName;
        }
    }
}

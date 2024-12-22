using Assimp;

namespace MassConvertFromModel
{
    /// <summary>
    /// A cache for supported formats from an assimp context.
    /// </summary>
    internal class ContextFormatCache
    {
        /// <summary>
        /// The export format cache.
        /// </summary>
        private readonly ExportFormatDescription[] ExportCache;

        /// <summary>
        /// Create a new <see cref="ContextFormatCache"/>.
        /// </summary>
        internal ContextFormatCache()
        {
            using var context = new AssimpContext();
            ExportCache = context.GetSupportedExportFormats();
            context.Dispose();
        }

        /// <summary>
        /// Create a new <see cref="ContextFormatCache"/> using the specified context.
        /// </summary>
        /// <param name="context"></param>
        internal ContextFormatCache(AssimpContext context)
        {
            ExportCache = context.GetSupportedExportFormats();
        }

        /// <summary>
        /// Whether or not a format is present in the export cache.
        /// </summary>
        /// <param name="format">The specified format.</param>
        /// <returns>Whether or not a format is present in the export cache.</returns>
        internal bool IsSupportedExportFormat(string format)
        {
            format = format.ToLowerInvariant();
            foreach (var desc in ExportCache)
            {
                if (desc.FormatId == format)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

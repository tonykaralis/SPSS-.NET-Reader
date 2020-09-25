using System.IO;
using System.Threading.Tasks;

namespace Spssly.Compression
{
    public interface IFileZipper
    {
        Task<Stream> ZipToStreamAsync(Stream inputStream);

        Task<Stream> UnZipToStreamAsync(Stream inputStream);
    }
}

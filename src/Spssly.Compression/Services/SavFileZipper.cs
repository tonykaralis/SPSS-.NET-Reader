using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Spssly.Compression.Services
{
    public class SavFileZipper : IFileZipper
    {
        public async Task<Stream> ZipToStreamAsync(Stream inputStream)
        {
            inputStream.Position = 0;

            var outStream = new MemoryStream();

            using (GZipStream compressStream = new GZipStream(outStream, CompressionMode.Compress, true))
            {
                await inputStream.CopyToAsync(compressStream);
            }

            outStream.Position = 0;

            return outStream;
        }

        public async Task<Stream> UnZipToStreamAsync(Stream inputStream)
        {
            inputStream.Position = 0;

            var outStream = new MemoryStream();

            using (GZipStream deCompressStream = new GZipStream(inputStream, CompressionMode.Decompress, true))
            {
                await deCompressStream.CopyToAsync(outStream);
            }

            outStream.Position = 0;

            return outStream;
        }
    }
}

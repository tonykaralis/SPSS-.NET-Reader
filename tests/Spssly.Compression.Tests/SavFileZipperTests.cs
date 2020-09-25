using NUnit.Framework;
using Spssly.Compression.Services;
using System.IO;
using System.Threading.Tasks;

namespace Spssly.Compression.Tests
{
    [TestFixture]
    public class SavFileZipperTests
    {
        private readonly IFileZipper _zipper;
        private readonly string _pathToResource;

        public SavFileZipperTests()
        {
            _zipper = new SavFileZipper();
            _pathToResource = @"TestFiles/spsstestdoc.sav";
        }

        [Test]
        public async Task CompressAsync_SetsStreamPositionToZero()
        {
            using (var inputStream = await OpenFileIntoMemoryStreamAsync(_pathToResource))
            using (var resultStream = await _zipper.ZipToStreamAsync(inputStream))
            {
                Assert.AreEqual(0, resultStream.Position);
            }
        }

        [Test]
        public async Task CompressAsync_CompressesStream()
        {
            using (var inputStream = await OpenFileIntoMemoryStreamAsync(_pathToResource))
            using (var resultStream = await _zipper.ZipToStreamAsync(inputStream))
            {
                Assert.Greater(inputStream.Length, resultStream.Length);
                Assert.Greater(resultStream.Length, 0);
            }
        }

        [Test]
        public async Task DecompressAsync_SetsStreamPositionToZero()
        {
            using (var inputStream = await OpenFileIntoMemoryStreamAsync(_pathToResource))
            using (var compressedStream = await _zipper.ZipToStreamAsync(inputStream))
            using (var decompressedStream = await _zipper.UnZipToStreamAsync(compressedStream))
            {
                Assert.AreEqual(0, decompressedStream.Position);
            }
        }

        [Test]
        public async Task DecompressAsync_DecomporessesBackToOriginal()
        {
            using (var inputStream = await OpenFileIntoMemoryStreamAsync(_pathToResource))
            using (var compressedStream = await _zipper.ZipToStreamAsync(inputStream))
            using (var decompressedStream = await _zipper.UnZipToStreamAsync(compressedStream))
            {
                Assert.AreEqual(inputStream.Length, decompressedStream.Length);
                Assert.AreNotEqual(0, decompressedStream.Length);
            }
        }

        private async Task<MemoryStream> OpenFileIntoMemoryStreamAsync(string fileName)
        {
            var inputStream = new MemoryStream();

            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                await fileStream.CopyToAsync(inputStream);
            }

            inputStream.Position = 0;

            return inputStream;
        }
    }
}

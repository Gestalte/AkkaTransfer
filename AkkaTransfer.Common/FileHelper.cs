using AkkaTransfer.Common;
using System.Diagnostics;

namespace AkkaTransfer
{
    public static class FileHelper
    {
        private const int batchSize = 125;

        public static FilePartMessage[] SplitIntoMessages(string pathToSend, string filename)
        {
            Debug.WriteLine($"{nameof(SplitIntoMessages)} Filename: {filename}",nameof(FileHelper));

            var bytes = File.ReadAllBytes(pathToSend);
            var base64 = Convert.ToBase64String(bytes);

            // if base64.Length / batchSize has a rest, add 1 so that an
            // incomplete batch is still created.
            var batchCount = (base64.Length / batchSize) + ((base64.Length % batchSize) > 0 ? 1 : 0);
            var rest = base64.Length % batchSize; // Size of the last batch that doesn't fill the entire batchSize.
            var hasRest = rest > 0;

            var filePartMessages = new FilePartMessage[batchCount];

            for (int i = 0; i < batchCount; i++)
            {
                var newString = hasRest && i == batchCount - 1
                    ? base64.Substring(i * batchSize, rest)
                    : base64.Substring(i * batchSize, batchSize);

                filePartMessages[i] = new FilePartMessage(newString, i, batchCount, filename);
            }

            return filePartMessages;
        }
    }
}

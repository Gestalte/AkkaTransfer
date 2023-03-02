namespace AkkaTransfer
{
    public  class FileBox
    {
        public FileBox(string boxName)
        {
            BoxPath = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), boxName)).FullName;
        }

        public string BoxPath { get; private set; }

        public List<string> GetFilesInBox()
            => Directory.GetFiles(BoxPath).ToList();

        public static string? FindFilePath(string filename, FileBox box)
        {
            return Directory.GetFiles(box.BoxPath)
                .Where(s => Path.GetFileName(s) == filename)
                .FirstOrDefault()
                ?? throw new ArgumentException("File not found in SendBox", nameof(filename));
        }
    }
}
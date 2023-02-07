namespace AkkaTransfer
{
    public  class FileBox
    {
        public FileBox(string boxName)
        {
            BoxPath = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), boxName)).FullName;
        }

        public string BoxPath { get; set; }

        public List<string> GetFilesInBox()
            => Directory.GetFiles(BoxPath).ToList();

    }
}
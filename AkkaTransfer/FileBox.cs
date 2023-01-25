namespace AkkaTransfer
{
    public abstract class FileBox
    {
        public FileBox()
        {
            MakeBox();
            BoxPath = "";
        }

        public string BoxPath { get; set; }

        public List<string> GetFilesInBox()
            => Directory.GetFiles(BoxPath).ToList();

        public abstract void MakeBox();
    }
}
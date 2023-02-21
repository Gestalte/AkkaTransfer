using Akka.Actor;
using AkkaTransfer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AkkaTransfer.Actors
{
    public class FileRebuilderActor : ReceiveActor
    {
        private readonly IFileHeaderRepository fileHeaderRepository;
        private readonly FileBox receiveBox;

        public FileRebuilderActor(FileBox receiveBox, IFileHeaderRepository fileHeaderRepository)
        {
            this.fileHeaderRepository = fileHeaderRepository;
            this.receiveBox = receiveBox;

            Receive<int>(async id => await WriteFileIfComplete(id));
        }

        public async Task WriteFileIfComplete(int id)
        {
            if (this.fileHeaderRepository.HasEntireFileBeenReceived(id))
            {
                var header = this.fileHeaderRepository.GetFileHeaderById(id);

                var headerPieces = header.FilePieces
                    .AsParallel()
                    .AsOrdered()
                    .OrderBy(s=>s.Position)
                    .Select(s => s.Content)
                    .SelectMany(s => s)
                    .ToArray();

                File.WriteAllBytes(Path.Combine(this.receiveBox.BoxPath, header.FileName), headerPieces);

                this.fileHeaderRepository.DeleteFileHeader(id);
            }
            else
            {
                Thread.Sleep(5000);

                await WriteFileIfComplete(id);
            }
        }
    }
}

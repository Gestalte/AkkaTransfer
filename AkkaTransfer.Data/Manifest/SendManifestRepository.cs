using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer.Data.Manifest
{
    public sealed class SendManifestRepository:IManifestRepository
    {
        private readonly ReceiveDbContext context;

        public SendManifestRepository(ReceiveDbContext context)
        {
            this.context = context;
        }
#nullable enable
        public Common.Manifest LoadNewestManifest()
        {
            SendManifest? dbManifest = this.context.SendManifests
                .AsNoTracking()
                .Include(i=>i.SendManifestFiles)
                .OrderBy(o => o.Timestamp)
                .FirstOrDefault();

            if (dbManifest == null)
            {
                return new Common.Manifest(DateTime.MinValue, new HashSet<Common.ManifestFile>());
            }

            var files = dbManifest.SendManifestFiles
                .Select(s => new Common.ManifestFile(s.Filename, s.FileHash))
                .ToHashSet();

            return new Common.Manifest(dbManifest.Timestamp, files);
        }
#nullable disable

        public void Save(Common.Manifest manifest)
        {
            var manifestToSave = new SendManifest
            {
                Timestamp = manifest.Timesstamp,
                SendManifestFiles = manifest.Files
                    .Select(s => new SendManifestFile
                    {
                        Filename = s.Filename,
                        FileHash = s.FileHash,
                    }).ToList()
            };

            this.context.SendManifests.Add(manifestToSave);
            this.context.SaveChanges();
        }
    }
}

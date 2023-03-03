﻿using Microsoft.EntityFrameworkCore;

namespace AkkaTransfer.Data.Manifest
{
    public sealed class ReceiveManifestRepository : IManifestRepository
    {
        private readonly ReceiveDbContext context;
        private readonly IDbContextFactory dbcontextFactory;

        public ReceiveManifestRepository(IDbContextFactory dbcontextFactory)
        {
            this.dbcontextFactory = dbcontextFactory;
            this.context = this.dbcontextFactory.CreateDbContext();
        }
#nullable enable
        public Common.Manifest LoadNewestManifest()
        {
            ReceiveManifest? dbManifest = this.context.ReceiveManifests
                .AsNoTracking()
                .Include(i=>i.ReceiveManifestFiles)
                .OrderBy(o => o.Timestamp)
                .FirstOrDefault();

            if (dbManifest == null)
            {
                return new Common.Manifest(DateTime.MinValue, new HashSet<Common.ManifestFile>());
            }

            var files = dbManifest.ReceiveManifestFiles
                .Select(s => new Common.ManifestFile(s.Filename, s.FileHash))
                .ToHashSet();

            return new Common.Manifest(dbManifest.Timestamp, files);
        }
#nullable disable

        public void Save(Common.Manifest manifest)
        {
            var manifestToSave = new ReceiveManifest
            {
                Timestamp = manifest.Timesstamp,
                ReceiveManifestFiles = manifest.Files
                    .Select(s => new ReceiveManifestFile
                    {
                        Filename = s.Filename,
                        FileHash = s.FileHash,
                    }).ToList()
            };

            this.context.ReceiveManifests.Add(manifestToSave);
            this.context.SaveChanges();
        }
    }

    public interface IManifestRepository
    {
        Common.Manifest LoadNewestManifest();
        void Save(Common.Manifest manifest);
    }
}

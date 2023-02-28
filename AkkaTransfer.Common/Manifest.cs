using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaTransfer.Common
{
    public sealed record Manifest(DateTime Timesstamp, HashSet<ManifestFile> Files);
    public sealed record ManifestFile(string Filename, string FileHash);
}

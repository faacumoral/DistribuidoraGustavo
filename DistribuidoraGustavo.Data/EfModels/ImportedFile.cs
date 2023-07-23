using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class ImportedFile
{
    public int ImportedFileId { get; set; }

    public string FileName { get; set; } = null!;

    public DateTime DateTime { get; set; }

    public string FileContent { get; set; } = null!;
}

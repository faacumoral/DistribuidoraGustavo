namespace DistribuidoraGustavo.Interfaces.Models;

public class DownloadModel : BaseModel
{
    public byte[] Content { get; set; }
    public string Name { get; set; }
    public string ContentType { get; set; }
}

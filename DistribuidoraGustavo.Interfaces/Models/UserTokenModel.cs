namespace DistribuidoraGustavo.Interfaces.Models
{
    public class UserTokenModel : BaseModel
    {
        public int UserID { get; set; }
        public DateTime ExpireTime { get; set; }
        public string Token { get; set; }
        public string Data { get; set; }
    }
}

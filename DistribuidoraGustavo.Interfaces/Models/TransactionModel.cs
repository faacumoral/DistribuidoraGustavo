namespace DistribuidoraGustavo.Interfaces.Models
{
    public class TransactionModel : BaseModel
    {
        public int TransactionId { get; set; }

        public decimal Amount { get; set; }

        public string Date { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public ClientModel Client { get; set; }
    }
}

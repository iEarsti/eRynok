namespace eRynok.Models
{
    public class BuyerProfile
    {
        public int UserId { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
    }
}

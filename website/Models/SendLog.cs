namespace website.Models
{
    public class SendLog
    {
        public int Id { get; set; }
        public string? sender { get; set; }
        
        public string? receiver { get; set; }
        public string? message { get; set; }
        public string? status { get; set; }
    }
}
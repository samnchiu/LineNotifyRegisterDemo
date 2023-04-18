namespace website.Models
{
    public class LineUser
    {
        public int Id { get; set; }
        public string? sub { get; set; }
        public string? Name { get; set; }
        public string? email { get; set; }
        public string? IdToken  { get; set; }
        public string? AccessToken  { get; set; }
        public bool registed { get; set; }

    }
}
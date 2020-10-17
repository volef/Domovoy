namespace backend.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int CreatorId { get; set; }
        public decimal lat { get; set; }
        public decimal @long { get; set; }

        public string Adress { get; set; }
        public int UserCount { get; set; }
    }
}
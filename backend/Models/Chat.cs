using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Chat
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public decimal lat { get; set; }
        public decimal @long { get; set; }
        [Required]
        public string Adress { get; set; }
        public int UserCount { get; set; }

        protected bool Equals(Chat other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Chat) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
        
        public static bool operator ==(Chat c1,Chat c2)
        {
            return c1.Id == c2.Id;
        }
        
        public static bool operator !=(Chat c1,Chat c2)
        {
            return c1.Id != c2.Id;
        }
    }
}
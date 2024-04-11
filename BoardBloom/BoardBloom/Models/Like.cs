namespace BoardBloom.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int? BloomId { get; set; }
        public string? UserId { get; set; }

        //public virtual ApplicationUser? User { get; set; }

        public virtual Bloom? Bloom { get; set; }
    }
}

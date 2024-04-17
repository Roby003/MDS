using BoardBloom.Models;
using System.ComponentModel.DataAnnotations;

namespace BoardBloom.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul comentariului este obligatoriu")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        // un comentariu apartine unui bloom
        public int? BloomId { get; set; }

        // un comentariu este postat de catre un user
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Bloom? Bloom { get; set; }
    }
}

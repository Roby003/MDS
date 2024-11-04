using System.ComponentModel.DataAnnotations;

namespace BoardBloom.Models
{
    public class Community
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele comunitatii este obligatoriu")]
        [StringLength(50, ErrorMessage = "Numele comunitatiipoate sa aiba maxim 50 de caractere")]
        [MinLength(1, ErrorMessage = "Numele comunitatii trebuie sa aiba cel putin un caracter")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage ="")]
        public string CreatedBy { get; set; } 
        public virtual ApplicationUser CreatedByNavigation { get; set; } = null!;
        public DateTime CreatedDate  { get; set; }

        public virtual ICollection<ApplicationUser>? Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<Bloom> Blooms { get; set; } = new List<Bloom>();


    }
}

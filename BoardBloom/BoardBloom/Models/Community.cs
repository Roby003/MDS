using System.ComponentModel.DataAnnotations;

namespace BoardBloom.Models
{
    public class Community
    {
		[Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele comunitatii este obligatoriu", ErrorMessageResourceName = null)]
        [StringLength(50, ErrorMessage = "Numele comunitatii poate sa aiba maxim 50 de caractere", ErrorMessageResourceName = null)]
        [MinLength(1, ErrorMessage = "Numele comunitatii trebuie sa aiba cel putin un caracter", ErrorMessageResourceName = null)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? CreatedBy { get; set; } = null!;
        public virtual ApplicationUser? CreatedByNavigation { get; set; } = null!;
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<ApplicationUser> Moderators { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<Bloom> Blooms { get; set; } = new List<Bloom>();


    }
}

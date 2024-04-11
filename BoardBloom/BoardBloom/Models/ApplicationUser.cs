using BoardBloom.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardBloom.Models
{
    public class ApplicationUser : IdentityUser
    {
        // un user poate posta mai multe comentarii
        public virtual ICollection<Comment>? Comments { get; set; }

        // un user poate sa creeze mai multe colectii
         public virtual ICollection<Bloom>? Blooms { get; set; }

        public ICollection<Like> Likes { get; set; }

        // atribute suplimentare adaugate pentru user
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public byte[]? ProfilePicture { get; set; }

        // public IEnumerable<SelectListItem>? Categ { get; set; }

        // variabila in care vom retine rolurile existente in baza de date
        // pentru popularea unui dropdown list
        

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

    }
    
}

using BoardBloom.Models;

using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace BoardBloom.Models
{
    public class Bloom
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul Bloomu-ului este obligatoriu")]
        [StringLength(50, ErrorMessage = "Titlul Bloom-ului poate sa aiba maxim 50 de caractere")]
        [MinLength(1, ErrorMessage = "Titlul Bloom-ului trebuie sa aiba cel putin un caracter")]
        public string Title { get; set; }


        [Required(ErrorMessage = "Bloom-ul trebuie sa aiba continut")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        // un Bloom poate avea o colectie de comentarii
        public virtual ICollection<Comment>? Comments { get; set; }

        public int TotalLikes { get; set; }
        public virtual ICollection<Like>? Likes { get; set; }

        public string? Image { get; set; }

        // un Bloom este creat de catre un user
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // relatia many-to-many dintre Bloom si Board
        public virtual ICollection<BloomBoard>? BloomBoards { get; set; }
    }
}

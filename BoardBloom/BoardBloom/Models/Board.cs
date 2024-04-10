using BoardBloom.Models;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace BoardBloom.Models
{
	public class Board
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "Numele colectiei este obligatoriu")]
		public string Name { get; set; }

		[StringLength(110, ErrorMessage = "Note-ul poate sa aiba maxim 110 de caractere")]
		public string? Note { get; set; }

		// o colectie este creata de catre un user
		public string? UserId { get; set; }
		public virtual ApplicationUser? User { get; set; }

		// relatia many-to-many dintre Bloom si Board
		public virtual ICollection<BloomBoard>? BloomBoards{ get; set; }

	}
}
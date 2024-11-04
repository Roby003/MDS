using BoardBloom.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardBloom.Models
{

	public class BloomBoard
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		// cheie primara compusa (Id, BloomId, BoardId)
		public int Id { get; set; }
		public int? BloomId { get; set; }

		public int? BoardId { get; set; }

		public virtual Bloom? Bloom { get; set; }
		public virtual Board? Board { get; set; }

		public DateTime BoardDate { get; set; }
	}
}
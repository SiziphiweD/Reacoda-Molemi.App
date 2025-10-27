using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReacodeApp.Models
{
    public class Favorite : BaseModel
    {
        // Foreign Keys
        public int UserId { get; set; }
        public int ProductId { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}


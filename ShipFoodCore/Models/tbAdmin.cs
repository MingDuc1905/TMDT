using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShipFood.Models;

[Table("tbAdmin")]
public partial class tbAdmin
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int userid { get; set; }

    [Required]
    [MaxLength(50)]
    public string tenadmin { get; set; } = null!;

    // Navigation
    public virtual tbUser tbUser { get; set; } = null!;
}

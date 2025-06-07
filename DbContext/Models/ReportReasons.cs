using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContext.Models;
/*public enum ReportReasons
{
    Violence,
    Nudity,
    IlegalContent,
    OtherReason,
}*/
public class ReportReasons
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ReportReasonId { get; set; }
    [Required]
    [MaxLength(64)]
    public string ReportReason { get; set; }

    [MaxLength(256)]
    public string? Description { get; set; }
}
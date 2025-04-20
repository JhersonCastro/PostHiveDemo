using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContext.Models;


[Table("REPORT")]
public class Report
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ReportId { get; set; }
    
    [Required]
    public int PostId { get; set; }
    public DateTime ReportDate { get; set; }
    
    [Required]
    public int ReportReasonId { get; set; }
    
    [MaxLength(256)]
    public string Description { get; set; }
    
    [ForeignKey("ReportReasonId")]
    public ReportReasons ReportReason { get; set; }
    
    [ForeignKey("PostId")]
    public Post Post { get; set; }
}
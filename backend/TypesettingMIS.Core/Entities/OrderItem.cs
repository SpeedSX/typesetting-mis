using System.ComponentModel.DataAnnotations;

namespace TypesettingMIS.Core.Entities;

public class OrderItem : BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }
    
    public Order? Order { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ItemType { get; set; } = string.Empty; // service, product
    
    [Required]
    public Guid ItemId { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }
    
    [Required]
    public decimal UnitPrice { get; set; }
    
    [Required]
    public decimal TotalPrice { get; set; }
}

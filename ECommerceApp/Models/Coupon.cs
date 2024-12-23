using ECommerceApp.Data;
using Microsoft.SqlServer.Server;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECommerceApp.Models
{
    public class Coupon
    { 
    [Key]
    public int Id { get; set; }
        [UniqueNameAttribute(ErrorMessage = "Coupon Code already exists.")]
        [Required(ErrorMessage = "Coupon Code is required.")]
    [StringLength(50, ErrorMessage = "Coupon Code cannot exceed 50 characters.")]
    public string Code { get; set; }
       
        

        [Required(ErrorMessage = "Discount Percentage is required.")]
    [Range(0, 100, ErrorMessage = "Discount Percentage must be between 0 and 100.")]
    public decimal DiscountPercentage { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid Start Date format.")]
        
        public DateTime? StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End Date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid End Date format.")]
        [DateAndTimeAttribute("StartDate",ErrorMessage = "End Date must be after Start Date.")]
    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; }


    }
}

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NguyenNgocThien_Buoi4.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public int Age { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        // Navigation property
        public virtual ICollection<Order> Orders { get; set; }
    }
}
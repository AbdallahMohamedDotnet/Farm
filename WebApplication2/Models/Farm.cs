using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class Farm
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string OwnerId { get; set; } = string.Empty;
        
        public virtual User Owner { get; set; } = null!;
        public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();
    }
}
using System.ComponentModel.DataAnnotations; 
namespace PetHealthAPI.Models
{   
public class Pet
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Pet Name is mandatory")] 
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Breed { get; set; } = string.Empty;

    [Range(0, 100)] 
    public int HealthScore { get; set; }
}
}
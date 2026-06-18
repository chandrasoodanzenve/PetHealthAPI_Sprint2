using System.ComponentModel.DataAnnotations; 
namespace PetHealthAPI.Models
{   
    /// <summary>
    /// Pet Entity representing a pet in the health tracking system.
    /// </summary>
public class Pet
{
    /// <summary>
    /// Unique identifier for the pet.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the pet. This field is required and has a maximum length of 50 characters.
    /// </summary>
    [Required(ErrorMessage = "Pet Name is mandatory")] 
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Breed of the pet. This field is required.
    /// </summary>
    [Required]
    public string Breed { get; set; } = string.Empty;

    /// <summary>
    /// Health score of the pet, ranging from 0 to 100.
    /// </summary>
    [Range(0, 100)] 
    public int HealthScore { get; set; }
}
}
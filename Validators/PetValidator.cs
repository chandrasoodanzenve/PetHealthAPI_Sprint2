using FluentValidation;
using PetHealthAPI.Models;

namespace PetHealthAPI.Validators
{
    public class PetValidator : AbstractValidator<Pet>
    {
        public PetValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Pet Name is required.")
                .Length(2, 50).WithMessage("Pet Name must be between 2 and 50 characters.");

            RuleFor(p => p.Breed)
                .NotEmpty().WithMessage("Pet Breed is required.");

            RuleFor(p => p.HealthScore)
                .InclusiveBetween(0, 100).WithMessage("Pet Health Score must be between 0 and 100.")
                .Must(BeValidScoreForBulldog).WithMessage("If the pet is a Bulldog, the health score cannot exceed 95 (Custom Rule).");
        }
        private bool BeValidScoreForBulldog(Pet pet, int score)
        {
            if (pet.Breed.ToLower() == "bulldog" && score > 95)
                return false;
            return true;
        }
    }
}
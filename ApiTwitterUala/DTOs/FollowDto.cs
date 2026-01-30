using System.ComponentModel.DataAnnotations;

namespace ApiTwitterUala.DTOs
{
    public sealed class FollowDto : IValidatableObject
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid UserFollowerId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UserId == Guid.Empty)
                yield return new ValidationResult("Campo obligatorio.", [nameof(UserId)]);

            if (UserFollowerId == Guid.Empty)
                yield return new ValidationResult("Campo obligatorio.", [nameof(UserFollowerId)]);

            if (UserId == UserFollowerId)
                yield return new ValidationResult("Un Usuario no puede seguirse a sí mismo.", [nameof(UserFollowerId)]);
        }
    }
}

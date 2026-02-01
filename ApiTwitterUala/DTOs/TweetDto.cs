using System.ComponentModel.DataAnnotations;

namespace ApiTwitterUala.DTOs
{
    public class TweetDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(280)]
        [RegularExpression(@"^[a-zA-Z0-9 &ñÑçÇáéíóúàèìòùâêîôûäëïöüÁÉÍÓÚÀÈÌÒÙÂÊÎÔÛÄËÏÖÜ\.\-_'!?:;,\t\r\n\p{So}\p{Cs}]*$", ErrorMessage = "Caracteres incorrectos.")]
        public string Content { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UserId == Guid.Empty)
                yield return new ValidationResult("Campo obligatorio.", [nameof(UserId)]);

            var lower = (Content ?? string.Empty).ToLowerInvariant();
            var forbidden = new[]
            {
                "javascript:",
                "<script",
                "onerror=",
                "onload=",
                "document.cookie",
                "eval(",
                "alert("
            };

            foreach (var f in forbidden)
            {
                if (lower.Contains(f))
                {
                    yield return new ValidationResult("El Campo contiene caracteres peligrosos.", new[] { nameof(Content) });
                    yield break;
                }
            }
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace ApiTwitterUala.Services.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[A-Za-z0-9 _-]+$", ErrorMessage = "Solo caracteres alfanuméricos y espacios permitidos.")]
        public string UserName { get; set; } = string.Empty;
    }
}
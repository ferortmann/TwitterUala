using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace ApiTwitterUala.Tests.TestHelpers
{
    internal static class ModelValidator
    {
        public static void ValidateAndPopulateModelState(object dto, ControllerBase controller)
        {
            var validationContext = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, validationContext, results, true);

            if (dto is System.ComponentModel.DataAnnotations.IValidatableObject validatable)
            {
                var extra = validatable.Validate(validationContext);
                if (extra != null) results.AddRange(extra);
            }

            foreach (var r in results)
            {
                var member = r.MemberNames?.FirstOrDefault() ?? string.Empty;
                controller.ModelState.AddModelError(member, r.ErrorMessage ?? "Invalid");
            }
        }
    }
}
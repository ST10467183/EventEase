using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var propertyInfo = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (propertyInfo == null)
                return new ValidationResult($"Unknown property: {_comparisonProperty}");

            var startDateValue = (DateTime?)propertyInfo.GetValue(validationContext.ObjectInstance);
            var endDateValue = (DateTime?)value;

            if (startDateValue.HasValue && endDateValue.HasValue && endDateValue <= startDateValue)
            {
                return new ValidationResult(ErrorMessage ?? "End date must be after start date.");
            }

            return ValidationResult.Success!;
        }
    }
}
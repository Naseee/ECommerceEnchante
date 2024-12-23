using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Data
{
    public class DateAndTimeAttribute : ValidationAttribute
    {
        private readonly string _startDatePropertyName;

        public DateAndTimeAttribute(string startDatePropertyName)
        {
            _startDatePropertyName = startDatePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_startDatePropertyName);

            if (property == null)
            {
                return new ValidationResult("Start date property not found.");
            }

            var startDate = (DateTime?)property.GetValue(validationContext.ObjectInstance) ?? DateTime.Today;
            var endDate = value as DateTime?;

            // Validate StartDate
            if (startDate.Date < DateTime.Today)
            {
                return new ValidationResult("Start Date must be today or a future date.");
            }

            // Validate EndDate
            if (endDate.HasValue)
            {
                if (endDate.Value.Date <= startDate.Date)
                {
                    return new ValidationResult("End Date must be after Start Date.");
                }
            }
            else
            {
                return new ValidationResult("End Date is required.");
            }

            return ValidationResult.Success;
        }

    }
}

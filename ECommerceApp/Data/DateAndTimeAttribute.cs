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

            var startDate = (DateTime)property.GetValue(validationContext.ObjectInstance);
            var endDate = (DateTime)value;

            // Check if both dates are in the future
            if (startDate < DateTime.Now)
            {
                return new ValidationResult("Start Date must be a future date.");
            }

            if (endDate < DateTime.Now)
            {
                return new ValidationResult("End Date must be a future date.");
            }

            // Check if the end date is after the start date
            if (endDate <= startDate)
            {
                return new ValidationResult("End Date must be after Start Date and time.");
            }

            return ValidationResult.Success;
        }
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace iread_identity_ms.Web.Util
{

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AfterNowAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime date = (DateTime)value;
            return (date > DateTime.Now);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class BeforeNowAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime date = (DateTime)value;
            return (date < DateTime.Now);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotDefaultValueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime date = (DateTime)value;
            return (date != new DateTime());

        }
    }


    public sealed class AfterAttribute : ValidationAttribute
    {
        public string StartDateProperty { get; set; }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            // Get value of the startDate property
            var property = validationContext.ObjectType.GetProperty(StartDateProperty);
            var valueOfDateStartProperty = property.GetValue(validationContext.ObjectInstance, null);
            DateTime startDate = DateTime.Parse(valueOfDateStartProperty.ToString());

            DateTime endDate = (DateTime)value;
            if (startDate > endDate)
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            return ValidationResult.Success;
        }
    }
}

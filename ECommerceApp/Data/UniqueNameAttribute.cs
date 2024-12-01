using ECommerceApp.Repository.IRepository;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Data
{
    public class UniqueNameAttribute:ValidationAttribute
    {
       

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult(" Name is required.");
            }

            
            var _categoryRepository = validationContext.GetService(typeof(ICategoryRepository)) as ICategoryRepository;
            var _productRepository = validationContext.GetService(typeof(IProductRepository)) as IProductRepository;
            var _couponrepository = validationContext.GetService(typeof(ICouponRepository)) as ICouponRepository;
            var _applicationUserRepository = validationContext.GetService(typeof(IApplicationUserRepository)) as IApplicationUserRepository;
            if (_categoryRepository != null && _categoryRepository.CategoryExists((string)value))
            {
                return new ValidationResult(" Name must be unique.");
            }
            if (_productRepository != null && _productRepository.ProductExists((string)value))
            {
                return new ValidationResult(" Name must be unique.");
            }
            if (_couponrepository != null && _couponrepository.CouponExists((string)value))
            {
                return new ValidationResult(" Code must be unique.");
            }
            if (_applicationUserRepository != null && _applicationUserRepository.ReferralCodeExists((string)value)) 
            {
                return new ValidationResult(" Code must be unique.");
            }

            return ValidationResult.Success;
        }
    }
}

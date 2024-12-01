namespace ECommerceApp.Models.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Offer> Offers { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int? CategoryId { get; set; }
    }
}

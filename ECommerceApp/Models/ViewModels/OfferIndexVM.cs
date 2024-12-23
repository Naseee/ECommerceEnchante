namespace ECommerceApp.Models.ViewModels
{
    public class OfferIndexVM
    {
        public IEnumerable<Offer> Offers { get; set; }
        public string SearchTerm { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}

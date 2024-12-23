using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using ECommerceApp.Repository.IRepository;
using ECommerceApp.ViewModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PayPal.v1.CustomerDisputes;

namespace ECommerceApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OfferController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OfferController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index(string searchText, int page = 1)
        {
            int pageSize = 5;
            var offers = _unitOfWork.Offer.GetAll(
                u => u.IsActive != false,
                includeProperties: "ProductOffers.Product,CategoryOffers.Category"
            );

            if (!string.IsNullOrEmpty(searchText))
            {
                var searchTerm = searchText.Trim().ToLower();
                offers = offers.Where(s => s.OfferName.ToLower().Contains(searchTerm)).ToList();
            }

            var totalOffers = offers.Count();
            var totalPages = (int)Math.Ceiling(totalOffers / (double)pageSize);

        
            var pagedOffers = offers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new OfferIndexVM
            {
                Offers = pagedOffers,
                SearchTerm = searchText, 
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }


        public IActionResult Create()
        {
            var offerVM = new OfferVM
            {
                Categories=_unitOfWork.Category.GetAll(u=>u.IsDeleted!=true,includeProperties:"CategoryOffers"),
                Products=_unitOfWork.Product.GetAll(null)
            };  
            return View(offerVM);
        }

        [HttpPost]
        public IActionResult Create(OfferVM offerVM, List<int> CategoryIds, List<int> ProductIds)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var offer = new Models.Offer
                    {
                        OfferName = offerVM.offer.OfferName,
                        DiscountPercentage = offerVM.offer.DiscountPercentage,
                        StartDate = offerVM.offer.StartDate,
                        EndDate = offerVM.offer.EndDate
                    };

                    _unitOfWork.Offer.Add(offer);
                    _unitOfWork.Save();

                    if (CategoryIds != null && CategoryIds.Any())
                    {
                        foreach (var categoryId in CategoryIds)
                        {
                            var categoryOffer = new CategoryOffer
                            {
                                CategoryId = categoryId,
                                OfferId = offer.OfferId
                            };

                            _unitOfWork.CategoryOffer.Add(categoryOffer);
                        }

                        offer.OfferType = "Category Offer";
                        _unitOfWork.Offer.Update(offer);
                        _unitOfWork.Save();
                    }

                    if (ProductIds != null && ProductIds.Any())
                    {
                        foreach (var productId in ProductIds)
                        {
                            var productOffer = new ProductOffer
                            {
                                ProductId = productId,
                                OfferId = offer.OfferId
                            };

                            _unitOfWork.ProductOffer.Add(productOffer);
                        }

                        offer.OfferType = "Product Offer";
                        _unitOfWork.Offer.Update(offer);
                        _unitOfWork.Save();
                    }

                    TempData["success"] = "Offer added successfully";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Offers_OfferName"))
                    {
                        ModelState.AddModelError("offer.OfferName", "Offer Name must be unique.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "An unexpected error occurred while adding the offer.");
                    }
                }
            }


            offerVM.Categories = _unitOfWork.Category.GetAll(null) ?? new List<Category>();
            offerVM.Products = _unitOfWork.Product.GetAll(null) ?? new List<Product>();
            return View(offerVM);
        }


        public IActionResult Edit(int id)
        {
            var offer = _unitOfWork.Offer.Get(u => u.OfferId == id, includeProperties: "ProductOffers.Product,CategoryOffers.Category");

            if (offer == null)
            {
                return NotFound();
            }

            var offerVM = new OfferVM
            {
                offer = offer,
                Products = _unitOfWork.Product.GetAll(null),
                Categories = _unitOfWork.Category.GetAll(null)
            };

            return View(offerVM);
        }


        [HttpPost]
        public IActionResult Edit(OfferVM offerVM, List<int> CategoryIds, List<int> ProductIds)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var existingOffer = _unitOfWork.Offer.Get(u => u.OfferId == offerVM.offer.OfferId, includeProperties: "ProductOffers,CategoryOffers");

                    if (existingOffer == null)
                    {
                        return NotFound();
                    }

                   
                    existingOffer.OfferName = offerVM.offer.OfferName;
                    existingOffer.DiscountPercentage = offerVM.offer.DiscountPercentage;
                    existingOffer.StartDate = offerVM.offer.StartDate;
                    existingOffer.EndDate = offerVM.offer.EndDate;

                    
                    _unitOfWork.CategoryOffer.RemoveRange(existingOffer.CategoryOffers);
                    if (CategoryIds != null && CategoryIds.Any())
                    {
                        foreach (var categoryId in CategoryIds)
                        {
                            var categoryOffer = new CategoryOffer
                            {
                                CategoryId = categoryId,
                                OfferId = existingOffer.OfferId
                            };
                            _unitOfWork.CategoryOffer.Add(categoryOffer);
                        }
                        existingOffer.OfferType = "Category Offer";
                    }

                    // Update product offers
                    _unitOfWork.ProductOffer.RemoveRange(existingOffer.ProductOffers);
                    if (ProductIds != null && ProductIds.Any())
                    {
                        foreach (var productId in ProductIds)
                        {
                            var productOffer = new ProductOffer
                            {
                                ProductId = productId,
                                OfferId = existingOffer.OfferId
                            };
                            _unitOfWork.ProductOffer.Add(productOffer);
                        }
                        existingOffer.OfferType = "Product Offer";
                    }

                    _unitOfWork.Offer.Update(existingOffer);
                    _unitOfWork.Save();

                    TempData["success"] = "Offer updated successfully";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("IX_Offers_OfferName"))
                    {
                        ModelState.AddModelError("offer.OfferName", "Offer Name must be unique.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "An unexpected error occurred while updating the offer.");
                    }
                }
            }

          
            offerVM.Categories = _unitOfWork.Category.GetAll(null) ?? new List<Category>();
            offerVM.Products = _unitOfWork.Product.GetAll(null) ?? new List<Product>();

            return View(offerVM);
        }

        public IActionResult Delete(int Id)
        {
            var offer = _unitOfWork.Offer.Get(u => u.OfferId == Id, null);
            if (offer == null)
            {
                return NotFound();
            }
            return View(offer);
        }
        [HttpPost]
        public IActionResult Delete(Models.Offer offer)
        {
            if (offer == null)
            {
                return NotFound();

            }
            offer.IsActive = false;
            _unitOfWork.Offer.Update(offer);
            _unitOfWork.Save();
            TempData["success"] = " Offer Deleted Successfully";
            return RedirectToAction("Index");
        }
    }

}

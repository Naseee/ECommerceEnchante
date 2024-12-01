using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using ECommerceApp.Repository.IRepository;
using ECommerceApp.ViewModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IActionResult Index()
        {
            var offers = _unitOfWork.Offer.GetAll(null, includeProperties: "ProductOffers.Product,CategoryOffers.Category");
            return View(offers);
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
            

                TempData["success"] = "offer added successfully";
                return RedirectToAction("Index");
            }
            offerVM.Categories = _unitOfWork.Category.GetAll(null);
            offerVM.Products = _unitOfWork.Product.GetAll(null);
            return View(offerVM);
           
        }

        public IActionResult Edit(int id)
        {
            var offerVM = new OfferVM
            {
                Categories = _unitOfWork.Category.GetAll(u => u.IsDeleted != true, includeProperties: "CategoryOffers"),
                Products = _unitOfWork.Product.GetAll(null),
                offer = _unitOfWork.Offer.Get(u => u.OfferId == id, includeProperties: "CategoryOffers,ProductOffers")
        };
            
            return View(offerVM);
        }

        [HttpPost]
        public IActionResult Edit(OfferVM offerVM, List<int> CategoryIds, List<int> ProductIds)
        {
           
            if (ModelState.IsValid)
            {
                var offer = new Models.Offer
                {
                    OfferName = offerVM.offer.OfferName,
                    DiscountPercentage = offerVM.offer.DiscountPercentage,
                    StartDate = offerVM.offer.StartDate,
                    EndDate = offerVM.offer.EndDate
                   
                };
                _unitOfWork.Offer.Update(offer);
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


                        _unitOfWork.CategoryOffer.Update(categoryOffer);
                    }
                    offer.OfferType = "Category Offer";
                   
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


                        _unitOfWork.ProductOffer.Update(productOffer);
                    }
                    offer.OfferType = "Product Offer";
                   
                }
                _unitOfWork.Offer.Update(offer);
                _unitOfWork.Save();
                TempData["success"] = " offer updated successfully";
                return RedirectToAction("Index");
            }
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

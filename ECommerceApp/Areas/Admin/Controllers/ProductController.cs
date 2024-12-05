using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ECommerceApp.Models;
using ECommerceApp.Repository;
using ECommerceApp.Repository.IRepository;
using System.IO;
using ECommerceApp.Models.ViewModels;
namespace ECommerceApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        [BindProperty]
        public ProductVM productVM { get; set; }

        public ProductController(IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            productVM = new ProductVM
            {
                Product = new Product(),
                ProductImages = new List<ProductImage>(),
                Categories=_unitOfWork.Category.GetAll(null)
            };
            return View(productVM);
        }

        [HttpPost]

        public IActionResult Create(ProductVM productVM, List<IFormFile> files)
        {

           
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = Path.Combine(wwwRootPath, "images", "products", "product-" + productVM.Product.Id);
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }

                        string filePath = Path.Combine(finalPath, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage productImage = new ProductImage
                        {
                            ImageUrl = Path.Combine("/images", "products", "product-" + productVM.Product.Id, fileName),
                            ProductId = productVM.Product.Id
                        };


                        if (productVM.Product.ProductImages == null)
                        {
                            productVM.Product.ProductImages = new List<ProductImage>();
                        }
                        productVM.Product.ProductImages.Add(productImage);
                    }
                }
               
               
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();

                TempData["success"] = "Product added successfully";
                return RedirectToAction("Index");

            }
            return View(productVM);
        }

        public IActionResult Edit(int? Id)
        {
            productVM = new ProductVM()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == Id, includeProperties: "ProductImages"),
                ProductImages = _unitOfWork.ProductImage.GetAll(u => u.ProductId == Id, null),
                Categories=_unitOfWork.Category.GetAll(null)
            };

            if (productVM.Product == null)
            {
                return NotFound();
            }
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Edit(ProductVM productVM, List<IFormFile> files)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, "images", "products", "product-" + productVM.Product.Id.ToString());

                    if (!Directory.Exists(productPath))
                    {
                        Directory.CreateDirectory(productPath);
                    }

                    string filePath = Path.Combine(productPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    ProductImage productImage = new ProductImage
                    {
                        ImageUrl = Path.Combine("/images", "products", "product-" + productVM.Product.Id.ToString(), fileName),
                        ProductId = productVM.Product.Id
                    };

                    if (productVM.Product.ProductImages == null)
                    {
                        productVM.Product.ProductImages = new List<ProductImage>();
                    }
                    productVM.Product.ProductImages.Add(productImage);
                }
            }
           

            _unitOfWork.Product.Update(productVM.Product);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int Id)
        {
            var product = _unitOfWork.Product.Get(u => u.Id == Id, null);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int Id)
        {
            var product = _unitOfWork.Product.Get(u => u.Id == Id, null);
            if (product == null)
            {
                return NotFound();
            }
            product.IsDeleted = true;
            _unitOfWork.Product.Update(product);
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        public IActionResult DeleteImage(int imageId)
        {
            var image = _unitOfWork.ProductImage.Get(u => u.Id == imageId, includeProperties: "Product");
            var ProductId = image.ProductId;
            if (image != null)
            {
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    var imagePath = Path.Combine(wwwRootPath, image.ImageUrl.Trim('\\'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    _unitOfWork.ProductImage.Remove(image);
                    _unitOfWork.Save();
                }
            }

            return RedirectToAction(nameof(Edit), new { id = ProductId });
        }
        [HttpGet]
        public IActionResult GetProducts(string sortorder)
        {
            var products = _unitOfWork.Product.GetAll(u => !u.IsDeleted, includeProperties:"Category");
           
            switch (sortorder)
            {
                case "price":
                    products = products.OrderBy(s => s.Price);
                    break;
                case "name":
                default:
                    products = products.OrderBy(s => s.Name.Trim(' '));
                    break;
            }
            return Json(new { data = products });
        }
    }
}
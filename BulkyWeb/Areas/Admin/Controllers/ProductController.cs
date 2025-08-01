﻿using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace BulkyWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        private readonly IWebHostEnvironment _webHostEnvironment;
        

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {

            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

       

        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();



            return View(objProductList);
        }

        

        public IActionResult Upsert(int? id)    // Update-Insert= Upsert
        {


            ProductVM productVM = new()
            {

                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                   Text = u.Name,
                   Value = u.Id.ToString()

               }),

                Product =new Product()


            };


            if (id==null || id==0)
            {
                //Create

                return View(productVM);
            }
            else 
            {
                // Update

                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
            



            //IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category
            //   .GetAll().Select(u => new SelectListItem
            //   {
            //       Text = u.Name,
            //       Value = u.Id.ToString()

            //   });

            // ViewBag.CategoryList = CategoryList;          (1)  ViewBag

            // ViewData["CategoryList"] = CategoryList;       (2)  ViewData  

            //return View();



        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            #region ASP Validation Summary

            if (productVM.Product.Title == productVM.Product.Author)
            {


                ModelState.AddModelError("Author", "The Author cannot exactly match the Title.");

            }


            #endregion

            if (ModelState.IsValid)
            {

                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath= Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        // Delete the old image

                        var oldImagePath = Path.Combine(wwwRootPath,productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }


                    }


                    using (var fileStream = new FileStream(Path.Combine(productPath,fileName),FileMode.Create))
                    {

                        file.CopyTo(fileStream);

                    }


                    productVM.Product.ImageUrl = @"\images\product\" + fileName;


                }


                if (productVM.Product.Id==0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }


                
                _unitOfWork.Save();

                TempData["success"] = "Product created successfully";

                return RedirectToAction("Index", "Product");
            }
            else
            {

                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()


                });



                return View(productVM);
            }


        }

       


        //public IActionResult Delete(int? id)
        //{

        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }

        //    Product? productFromDb = _unitOfWork.Product.Get(u => u.Id == id);


        //    if (productFromDb == null)
        //    {
        //        return NotFound();
        //    }


        //    return View(productFromDb);

        //}



        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePOST(int? id)
        //{

        //    Product? obj = _unitOfWork.Product.Get(u => u.Id == id);

        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }


        //    _unitOfWork.Product.Remove(obj);
        //    _unitOfWork.Save();

        //    TempData["success"] = "Product deleted successfully";

        //    return RedirectToAction("Index", "Product");


        //}





        #region API CALSS


        [HttpGet]

        public IActionResult GetAll() 
        {

            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return Json(new { data = objProductList});
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            var productToBeDeleted=_unitOfWork.Product.Get(u => u.Id==id);

            if (productToBeDeleted == null) 
            { 
                return Json(new {success=false,message="Error while deleting"});
            }

            var oldmagePath = Path.Combine(_webHostEnvironment.WebRootPath,productToBeDeleted.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldmagePath))
            {
                System.IO.File.Delete(oldmagePath);
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();


            return Json(new { success = true, message = "Delete Successful" });


        }

        #endregion

    }
}

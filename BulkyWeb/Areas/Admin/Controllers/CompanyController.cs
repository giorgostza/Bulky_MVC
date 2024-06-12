using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.VieModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace BulkyWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

      
        

        public CompanyController(IUnitOfWork unitOfWork)
        {

            _unitOfWork = unitOfWork;
          
        }

       

        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();



            return View(objCompanyList);
        }

        

        public IActionResult Upsert(int? id)    // Update-Insert= Upsert
        {


           


            if (id==null || id==0)
            {
                //Create

                return View(new Company());
            }
            else 
            {
                // Update
                Company CompanyObj = _unitOfWork.Company.Get(u => u.Id == id);
                return View(CompanyObj);
            }
            





        }

        [HttpPost]
        public IActionResult Upsert(Company CompanyObj)
        {
            

            if (ModelState.IsValid)
            {


                if (CompanyObj.Id==0)
                {
                    _unitOfWork.Company.Add(CompanyObj);
                }
                else
                {
                    _unitOfWork.Company.Update(CompanyObj);
                }


                
                _unitOfWork.Save();

                TempData["success"] = "Company created successfully";

                return RedirectToAction("Index", "Company");
            }
            else
            {

                return View(CompanyObj);
            }


        }


        #region API CALSS


        [HttpGet]

        public IActionResult GetAll() 
        {

            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();

            return Json(new { data = objCompanyList});
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            var CompanyToBeDeleted=_unitOfWork.Company.Get(u => u.Id==id);

            if (CompanyToBeDeleted == null) 
            { 
                return Json(new {success=false,message="Error while deleting"});
            }

            

            _unitOfWork.Company.Remove(CompanyToBeDeleted);
            _unitOfWork.Save();


            return Json(new { success = true, message = "Delete Successful" });


        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models.DAO;
using Models.EF;
using System.Web.Script.Serialization;
using PagedList;
using PagedList.Mvc;
using System.IO;
using WebsiteNoiThat.Common;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class ProductCateController : HomeController
    {
        DBNoiThat db = new DBNoiThat();

        public ActionResult Index()
        {
            return View();
        }

        [HasCredential(RoleId = "VIEW_CATE")]
        public ActionResult Show()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            return View(db.Categories.ToList());
        }

        [HttpGet]
        [HasCredential(RoleId = "ADD_CATE")]
        public ActionResult Add()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;
            return View();
        }

        [HttpPost]
        [HasCredential(RoleId = "ADD_CATE")]
        public ActionResult Add(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Log the data before saving
                    System.Diagnostics.Debug.WriteLine($"Adding Category: ID={category.CategoryId}, Name={category.Name}, MetaTitle={category.MetaTitle}, ParId={category.ParId}");
                    
                    // For troubleshooting: bypass Entity Framework
                    using (var connection = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBNoiThat"].ConnectionString))
                    {
                        connection.Open();
                        
                        // First, find the max CategoryId to generate a new one
                        int newCategoryId = 1;
                        using (var cmdGetMaxId = new System.Data.SqlClient.SqlCommand("SELECT ISNULL(MAX(CategoryId), 0) + 1 FROM Category", connection))
                        {
                            newCategoryId = (int)cmdGetMaxId.ExecuteScalar();
                        }
                        
                        using (var command = new System.Data.SqlClient.SqlCommand())
                        {
                            command.Connection = connection;
                            command.CommandText = "INSERT INTO Category (CategoryId, Name, MetaTitle, ParId) VALUES (@CategoryId, @Name, @MetaTitle, @ParId)";
                            command.Parameters.AddWithValue("@CategoryId", newCategoryId);
                            command.Parameters.AddWithValue("@Name", category.Name ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@MetaTitle", category.MetaTitle ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@ParId", category.ParId ?? (object)DBNull.Value);
                            
                            int result = command.ExecuteNonQuery();
                            if (result > 0)
                            {
                                TempData["SuccessMessage"] = "Thêm loại sản phẩm thành công!";
                                return RedirectToAction("Show");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Không thể thêm loại sản phẩm. Kiểm tra lại dữ liệu.";
                                return View(category);
                            }
                        }
                    }
                }
                else
                {
                    // Log model state errors for debugging
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage).ToList();
                    TempData["ErrorMessage"] = "Lỗi dữ liệu không hợp lệ: " + string.Join(", ", errors);
                }
                
                return View(category);
            }
            catch (Exception ex)
            {
                // Capture the full exception chain
                string errorMessage = "Lỗi khi thêm loại sản phẩm: " + ex.Message;
                Exception currentEx = ex;
                int depth = 0;
                
                while (currentEx.InnerException != null && depth < 10)
                {
                    currentEx = currentEx.InnerException;
                    errorMessage += "<br/>Chi tiết: " + currentEx.Message;
                    depth++;
                }
                
                // Add stack trace for debugging
                errorMessage += "<br/>Stack trace: " + ex.StackTrace;
                
                TempData["ErrorMessage"] = errorMessage;
                return View(category);
            }
        }

        [HttpGet]
        [HasCredential(RoleId = "EDIT_CATE")]
        public ActionResult Edit(int CategoryId)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            Category a = db.Categories.SingleOrDefault(n => n.CategoryId == CategoryId);
            return View(a);

        }

        [HttpPost]
        [HasCredential(RoleId = "EDIT_CATE")]
        public ActionResult Edit(Category n)
        {
            if (ModelState.IsValid)
            {
                db.Entry(n).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Show");
            }
            else
            {
                return JavaScript("alert('Error');");
            }
        }

        [HttpGet]
        [HasCredential(RoleId = "DELETE_CATE")]
        public ActionResult Delete(int CategoryId)
        {
            try
            {
                var model = db.Categories.Find(CategoryId);
                if (model != null)
                {
                    // Kiểm tra xem có sản phẩm nào thuộc loại này không
                    var products = db.Products.Where(p => p.CateId == CategoryId).ToList();
                    if (products.Count > 0)
                    {
                        TempData["ErrorMessage"] = "Không thể xóa loại sản phẩm này vì đang có sản phẩm thuộc loại này!";
                    }
                    else
                    {
                        db.Categories.Remove(model);
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Xóa loại sản phẩm thành công!";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy loại sản phẩm!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa loại sản phẩm: " + ex.Message;
            }
            
            return RedirectToAction("Show");
        }

        [HttpPost]
        [HasCredential(RoleId = "DELETE_CATE")]
        public ActionResult Delete(FormCollection formCollection)
        {
            string[] ids = formCollection["CategoryId"].Split(new char[] { ',' });

            foreach (string id in ids)
            {
                var model = db.Categories.Find(Convert.ToInt32(id));
                db.Categories.Remove(model);
                db.SaveChanges();
            }
            return RedirectToAction("Show");
        }

    }
}
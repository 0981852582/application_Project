using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QUANLYBANHANG.Models;

namespace WebApplication7.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Khai báo biến và Khởi tạo dữ liệu
        /// </summary>
        /// 
        private readonly AccessContext _context;
        public HomeController(AccessContext context)
        {
            _context = context;
        }
        /// <summary>
        /// giao diện
        /// </summary>
        /// <returns>IActionResult</returns>
        /// 
        public IActionResult Index()
        {
            // kiểm tra quyền khi lên view
            if (AccountController.AccountLogin != null)
            {
                ViewBag.Login = 1;
            }
            else
            {
                ViewBag.Login = 0;
            }
            // trả về view
            return View();
        }
    }
}

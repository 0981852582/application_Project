using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QUANLYBANHANG.Models;
using WebApplication7.Models;
using WebApplication7.Models.EntitySQL;
using System.Threading.Tasks;

namespace WebApplication7.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Khai báo biến và khởi tạo giá trị
        /// </summary>
        public static Account AccountLogin { get; set; }
        private string urlPermission { get; set; }
        private readonly AccessContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        private PermissionController permission { get; set; }
        /// <summary>
        /// Khởi tạo các đối tượng
        /// </summary>
        /// <param name="context"></param>
        /// 
        public AccountController(AccessContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            permission = new PermissionController(context);
            urlPermission = "account";
            this._hostingEnvironment = hostingEnvironment;
        }
        /// <summary>
        /// trang gia diện đăng nhập
        /// </summary>
        /// <returns>IActionResult</returns>
        /// 
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// lấy danh sách tài khoản hiện tại
        /// </summary>
        /// <param name="parameter">JTable</param>
        /// <returns>object</returns>
        /// 
        [HttpPost]
        public object getListAccount([FromBody] JTable parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Access");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện lấy dữ liệu
            dataTable dataTable = new dataTable
            {
                fromRow = (parameter.currentPage - 1) * parameter.numberPage
            };
            try
            {
                var data = _context.Account.
                    Where(x =>
                        (x.Username.Contains(parameter.search) ||
                        x.PermissionCode.Contains(parameter.search) ||
                        x.FullName.Contains(parameter.search) ||
                        x.Address.Contains(parameter.search)
                        )
                    )
                    .OrderUsingSortExpression(parameter.columnOrder == null ? "ID DESC" : parameter.columnOrder)
                    .Skip((parameter.currentPage - 1) * parameter.numberPage)
                    .Take(parameter.numberPage)
                    .Select(x => new
                    {
                        x.ID,
                        x.Username,
                        x.Password,
                        x.Address,
                        x.FullName,
                        x.DateOfBirth,
                        x.PermissionCode
                    }).ToList();
                var dataCount = _context.Account.Where(x =>
                        (x.Username.Contains(parameter.search))
                    ).Count();
                dataTable.totalItem = dataCount;
                dataTable.results = data;

                dataTable.endRow = (dataTable.fromRow + data.Count);
                if (data.Count > 0)
                    dataTable.fromRow++;
                msg.result = dataTable;
                return Json(msg);
            }
            catch (Exception)
            {
                msg.Error = true;
                msg.Title = MessageError.MessageNotPermission;
                return Json(msg);
            }
        }

        /// <summary>
        /// khai báo class chứa parameter đầu vào
        /// </summary>
        /// 
        public class ParameterRequest
        {
            public int ID { get; set; }
        }
        /// <summary>
        /// Lấy item của Account theo ID
        /// </summary>
        /// <param name="parameter">ParameterRequest</param>
        /// <returns>object</returns>
        /// 
        [HttpPost]
        public object getItemAccount([FromBody] ParameterRequest parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Access");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện lấy dữ liệu
            try
            {
                var data = _context.Account.
                    Where(x => x.ID == parameter.ID
                    ).Select(x => new
                    {
                        x.ID,
                        x.Username,
                        x.Password,
                        x.Address,
                        x.FullName,
                        x.DateOfBirth,
                        x.PermissionCode,
                        files = _context.Files.Select(
                            d => new {
                                d.FileName,
                                d.ID,
                                d.Guid,
                                d.Url
                            }).Where(d => d.Guid == x.Guid)
                    })
                    .FirstOrDefault();
                return Json(data);
            }
            catch (Exception)
            {
                return Json(new Account { });
            }
        }
        /// <summary>
        /// thực hiện thêm mới dữ liệu tài khoản
        /// </summary>
        /// <param name="parameter">Account</param>
        /// <returns>object</returns>
        /// 
        public async Task<object> insertAccount(IList<IFormFile> files)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Add");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện thêm mới
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var tempRecrui = Request.Form["data"];
                    Account parameter = JsonConvert.DeserializeObject<Account>(tempRecrui);
                    var checkAccountCode = _context.Account.FirstOrDefault(x => x.Username == parameter.Username);
                    if (checkAccountCode != null)
                    {
                        dbContextTransaction.Rollback();
                        msg.Error = true;
                        msg.Title = "Tài khoản đã tồn tại.";
                        return Json(msg);
                    }
                    _context.Account.Add(parameter);
                    _context.SaveChanges();
                    msg.Title = "Thêm mới tài khoản thành công.";
                    var filesPath = $"{this._hostingEnvironment.WebRootPath}/files";
                    foreach (IFormFile source in files)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(source.FileName) + "_" + DateTime.Now.ToString("dd-MM-yyyy_HH_mm") + Path.GetExtension(source.FileName);

                        // Ensure the file name is correct
                        fileName = fileName.Contains("\\")
                            ? fileName.Trim('"').Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)
                            : fileName.Trim('"');

                        var fullFilePath = Path.Combine(filesPath, fileName);

                        if (source.Length <= 0)
                        {
                            continue;
                        }

                        using (var stream = new FileStream(fullFilePath, FileMode.Create))
                        {
                            await source.CopyToAsync(stream);
                        }
                        Files file = new Files
                        {
                            FileName = fileName,
                            Url = UrlOfFile.urlFile + fileName,
                            Guid = parameter.Guid
                        };
                        _context.Files.Add(file);
                        _context.SaveChanges();
                    }
                    dbContextTransaction.Commit();
                    return Json(msg);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    msg.result = ex;
                    msg.Error = true;
                    msg.Title = "Thêm mới tài khoản thất bại";
                    return Json(msg);
                }
            }
        }
        /// <summary>
        /// thực hiện xóa tài khoản
        /// </summary>
        /// <param name="parameter">Account</param>
        /// <returns>object</returns>
        /// 
        [HttpPost]
        public object deleteAccount([FromBody] Account parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Delete");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện thêm mới
            msg = new Message { Error = false };
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var data = _context.Account.SingleOrDefault(x => x.ID == parameter.ID);
                    if (data != null)
                    {
                        _context.Account.Remove(data);
                        _context.SaveChanges();
                    }
                    msg.Title = "Xóa tài khoản thành công.";
                    dbContextTransaction.Commit();
                    return Json(msg);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    msg.Error = true;
                    msg.result = ex;
                    msg.Title = "Xóa tài khoản thất bại";
                    return Json(msg);
                }
            }
        }
        /// <summary>
        /// thực hiện cập nhật tài khoản
        /// </summary>
        /// <param name="parameter">Account</param>
        /// <returns>object</returns>
        /// 
        [HttpPost]
        public object updateAccount([FromBody] Account parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Edit");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện thêm mới
            msg = new Message { Error = false };
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var entry = _context.Entry(parameter);
                    entry.State = EntityState.Modified;
                    _context.Entry(parameter).Property(x => x.CreatedBy).IsModified = false;
                    _context.Entry(parameter).Property(x => x.ID).IsModified = false;
                    _context.Entry(parameter).Property(x => x.CreatedDate).IsModified = false;
                    _context.Entry(parameter).Property(x => x.Guid).IsModified = false;
                    _context.SaveChanges();
                    msg.Title = "Cập nhật tài khoản thành công.";
                    dbContextTransaction.Commit();
                    return Json(msg);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    msg.Error = true;
                    msg.result = ex;
                    msg.Title = "Câp nhật tài khoản thất bại";
                    return Json(msg);
                }
            }
        }
        /// <summary>
        /// Thực hiện đăng nhập tài khoản
        /// </summary>
        /// <param name="parameter">Account</param>
        /// <returns>object</returns>
        /// 
        [HttpPost]
        public object Login([FromBody] Account parameter)
        {
            Message msg = new Message { Error = false };
            try
            {
                var data = _context.Account.FirstOrDefault(x => x.Username == parameter.Username && x.Password == parameter.Password);
                if (data != null)
                {
                    AccountLogin = data;
                    msg.Title = "Đăng nhập thành công";
                }
                else
                {
                    msg.Error = true;
                    msg.Title = "Đăng nhập không thành công";
                }
            }
            catch (Exception ex)
            {
                msg.Error = true;
                msg.result = ex;
                msg.Title = "Đăng nhập không thành công";
            }
            return Json(msg);
        }
    }
}
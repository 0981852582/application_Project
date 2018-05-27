using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QUANLYBANHANG.Models;
using WebApplication7.Models;
using WebApplication7.Models.EntitySQL;

namespace WebApplication7.Controllers
{
    public class MenuBarController : Controller
    {
        /// <summary>
        /// Khai báo biến và khởi tạo giá trị
        /// </summary>
        private string urlPermission { get; set; }
        private readonly AccessContext _context;
        public PermissionController permission { get; set; }
        /// <summary>
        /// Khởi tạo các đối tượng
        /// </summary>
        /// <param name="context"></param>
        /// 
        public MenuBarController(AccessContext context)
        {
            //MyActionFilterAttribute._context = context;
            _context = context;
            permission = new PermissionController(context);
            urlPermission = "menuBar";
        }
        /// <summary>
        /// Lấy danh sách thanh menu danh mục
        /// </summary>
        /// <param name="parameter">JTable</param>
        /// <returns>object</returns>
        [HttpPost]
        [MyActionFilter(permission = "menuBar", function = "Access")]
        public object getListMenuBar([FromBody] JTable parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Access");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện lấy danh sách dữ liệu
            dataTable dataTable = new dataTable
            {
                fromRow = (parameter.currentPage - 1) * parameter.numberPage
            };
            try
            {
                var data = _context.MenuBar.
                    Where(x =>
                        (x.Title.Contains(parameter.search))
                    )
                    .OrderUsingSortExpression(parameter.columnOrder == null ? "ID DESC" : parameter.columnOrder)
                    .Skip((parameter.currentPage - 1) * parameter.numberPage)
                    .Take(parameter.numberPage)
                    .Select(x => new
                    {
                        x.ID,
                        x.Title,
                        x.Description,
                        x.Status,
                        x.Url,
                        x.urlCode,
                    }).ToList();
                var dataCount = _context.MenuBar.Where(x =>
                        (x.Title.Contains(parameter.search))
                    ).Count();
                dataTable.totalItem = dataCount;
                dataTable.results = data;

                dataTable.endRow = (dataTable.fromRow + data.Count);
                if (data.Count > 0)
                    dataTable.fromRow++;
                msg.result = dataTable;
                return Json(msg);
            }
            catch (Exception ex)
            {
                msg.Error = true;
                msg.result = ex;
                msg.Title = MessageError.MessageNotPermission;
                return Json(msg);
            }
        }
        /// <summary>
        /// Lấy danh sách menu danh mục Lookup
        /// </summary>
        /// <returns>object</returns>
        [HttpPost]
        public object getAllListMenuBar()
        {
            Message msg = new Message { Error = false };
            // kiểm tra quyền trước khi lấy dữ liệu
            if (AccountController.AccountLogin == null)
            {
                msg.Title = "Chưa đăng nhập tài khoản";
                msg.Error = true;
                return Json(msg);
            }
            else if (AccountController.AccountLogin.PermissionCode != HomeController.FullPermission)
            {
                msg.Title = "Tài khoản không có quyền thực hiện chức năng này";
                msg.Error = true;
                return Json(msg);
            }
            try
            {
                var data = _context.MenuBar
                    .OrderByDescending(x => x.ID)
                    .Select(x => new
                    {
                        x.ID,
                        x.Title,
                        x.urlCode
                    }).ToList();
                msg.result = data;
                msg.Title = "Lấy danh sách menu thành công";
                return Json(msg);
            }
            catch (Exception ex)
            {
                msg.Error = true;
                msg.result = ex;
                msg.Title = "Lấy thông tin menu thất bại";
                return Json(msg);
            }
        }
        /// <summary>
        /// khai báo class chứa parameter đầu vào
        /// </summary>
        public class ParameterRequest
        {
            public int ID { get; set; }
        }
        /// <summary>
        /// Lấy Item của MenuBar theo ID
        /// </summary>
        /// <param name="parameter">ParameterRequest</param>
        /// <returns>object</returns>
        public object getItemMenuBar([FromBody] ParameterRequest parameter)
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
                var data = _context.MenuBar.
                    Where(x => x.ID == parameter.ID
                    ).Select(x => new
                    {
                        x.ID,
                        x.Title,
                        x.Description,
                        x.Status,
                        x.Url,
                        x.urlCode,
                    })
                    .FirstOrDefault();
                return Json(data);
            }
            catch (Exception)
            {
                return Json(new MenuBar { });
            }
        }
        /// <summary>
        /// Thực hiện thêm mới MenuBar
        /// </summary>
        /// <param name="parameter">MenuBar</param>
        /// <returns>object</returns>
        public object insertMenuBar([FromBody] MenuBar parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Add");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện thêm mới Menu Bar
            msg = new Message { Error = false };
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var checkMenuBarCode = _context.MenuBar.FirstOrDefault(x => x.urlCode == parameter.urlCode);
                    if (checkMenuBarCode != null)
                    {
                        dbContextTransaction.Rollback();
                        msg.Error = true;
                        msg.Title = "Mã quyền đã tồn tại.";
                        return Json(msg);
                    }
                    parameter.Status = true;
                    _context.MenuBar.Add(parameter);
                    _context.SaveChanges();
                    msg.Title = "Thêm mới quyền thành công.";
                    dbContextTransaction.Commit();
                    return Json(msg);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    msg.result = ex;
                    msg.Error = true;
                    msg.Title = "Thêm mới quyền thất bại";
                    return Json(msg);
                }
            }
        }
        /// <summary>
        /// Thực hiện xóa MenuBar
        /// </summary>
        /// <param name="parameter">MenuBar</param>
        /// <returns>object</returns>
        public object deleteMenuBar([FromBody] MenuBar parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Delete");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện xóa Menu Bar
            msg = new Message { Error = false };
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var data = _context.MenuBar.SingleOrDefault(x => x.ID == parameter.ID);
                    if (data != null)
                    {
                        _context.MenuBar.Remove(data);
                        _context.SaveChanges();
                    }
                    msg.Title = "Xóa quyền thành công.";
                    dbContextTransaction.Commit();
                    return Json(msg);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    msg.Error = true;
                    msg.result = ex;
                    msg.Title = "Xóa quyền thất bại";
                    return Json(msg);
                }
            }
        }
        /// <summary>
        /// Thực hiện cập nhật MenuBar
        /// </summary>
        /// <param name="parameter">MenuBar</param>
        /// <returns>object</returns>
        public object updateMenuBar([FromBody] MenuBar parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Edit");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện cập nhật Menu Bar
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
                    _context.SaveChanges();
                    msg.Title = "Cập nhật quyền thành công.";
                    dbContextTransaction.Commit();
                    return Json(msg);
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    msg.Error = true;
                    msg.result = ex;
                    msg.Title = "Câp nhật quyền thất bại";
                    return Json(msg);
                }
            }
        }
        /// <summary>
        ///  khai báo class chứa parameter đầu vào
        /// </summary>
        public class getViewMenuBarEntity
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public string urlCode { get; set; }
            public string PermissionCode { get; set; }
            public string TypePermission { get; set; }
        }
        /// <summary>
        /// Thực hiện lấy danh MenuBar dùng Lookup
        /// </summary>
        /// <returns>object</returns>
        [HttpPost]
        public object getViewMenuBar()
        {
            Message msg = new Message { Error = false };
            try
            {
                if (AccountController.AccountLogin.PermissionCode != HomeController.FullPermission)
                {
                    var data = _context.MenuOfPage.Select(x => new getViewMenuBarEntity
                    {
                        urlCode = x.urlCode
                    }).Where(x => x.TypePermission == "Access" && x.PermissionCode == AccountController.AccountLogin.PermissionCode).ToList();
                    foreach (var item in data)
                    {
                        var info = _context.MenuBar.FirstOrDefault(x => x.urlCode == item.urlCode);
                        if (info != null)
                        {
                            item.Title = info.Title;
                            item.Url = info.Url;
                        };
                    }
                    msg.result = data;
                    msg.Title = "Lấy menuBar thành công.";
                }
                else if (AccountController.AccountLogin.PermissionCode == HomeController.FullPermission)
                {
                    var data = _context.MenuBar.Select(x => new getViewMenuBarEntity
                    {
                        urlCode = x.urlCode
                    }).ToList();
                    foreach (var item in data)
                    {
                        var info = _context.MenuBar.FirstOrDefault(x => x.urlCode == item.urlCode);
                        if (info != null)
                        {
                            item.Title = info.Title;
                            item.Url = info.Url;
                        };
                    }
                    msg.result = data;
                    msg.Title = "Lấy menuBar thành công.";
                }
                return Json(msg);
            }
            catch (Exception ex)
            {
                msg.result = ex;
                msg.Title = "Lấy menuBar thất bại thành công.";
                return Json(msg);
            }

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYBANHANG.Models;
using WebApplication7.Models;
using WebApplication7.Models.EntitySQL;

namespace WebApplication7.Controllers
{
    public class PermissionController : Controller
    {
        /// <summary>
        /// Khai báo biến và khởi tạo giá trị
        /// </summary>
        private string urlPermission { get; set; }
        private readonly AccessContext _context;
        /// <summary>
        /// Khởi tạo các đối tượng
        /// </summary>
        /// <param name="context"></param>
        /// 
        public PermissionController(AccessContext context)
        {
            _context = context;
            urlPermission = "permission";
        }
        /// <summary>
        /// Lấy danh sách quyền
        /// </summary>
        /// <param name="parameter">JTable</param>
        /// <returns>object</returns>
        public object getListPermission([FromBody] JTable parameter)
        {
            Message msg = checkPermissionMenu(urlPermission, "Access");
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
                var data = _context.Permission.
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
                        x.PermissionCode
                    }).ToList();
                var dataCount = _context.Permission.Where(x =>
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
            catch (Exception)
            {
                msg.Title = MessageError.MessageNotPermission;
                msg.Error = true;
                return Json(msg);
            }
        }
        /// <summary>
        ///  khai báo class chứa parameter đầu vào
        /// </summary>
        public class ParameterRequest
        {
            public int ID { get; set; }
        }
        /// <summary>
        /// Lấy Item của quyền bằng ID
        /// </summary>
        /// <param name="parameter">ParameterRequest</param>
        /// <returns>object</returns>
        [HttpPost]
        public object getItemPermission([FromBody] ParameterRequest parameter)
        {
            Message msg = checkPermissionMenu(urlPermission, "Access");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện lấy Item Quyền
            try
            {
                var data = _context.Permission.
                    Where(x => x.ID == parameter.ID
                    ).Select(x => new
                    {
                        x.ID,
                        x.Title,
                        x.Description,
                        x.Status,
                        x.PermissionCode
                    })
                    .FirstOrDefault();
                return Json(data);
            }
            catch (Exception)
            {
                return Json(new Permission { });
            }
        }
        /// <summary>
        /// Thêm mới quyền
        /// </summary>
        /// <param name="parameter">Permission</param>
        /// <returns>object</returns>
        [HttpPost]
        public object insertPermission([FromBody] Permission parameter)
        {
            Message msg = checkPermissionMenu(urlPermission, "Add");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện thêm mới quyền
            msg = new Message { Error = false };
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var checkPermissionCode = _context.Permission.FirstOrDefault(x => x.PermissionCode == parameter.PermissionCode.Trim());
                    if (checkPermissionCode != null)
                    {
                        dbContextTransaction.Rollback();
                        msg.Error = true;
                        msg.Title = "Mã quyền đã tồn tại.";
                        return Json(msg);
                    }
                    parameter.Status = true;
                    _context.Permission.Add(parameter);
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
        /// Thực hiện xóa quyền
        /// </summary>
        /// <param name="parameter">Permission</param>
        /// <returns>object</returns>
        public object deletePermission([FromBody] Permission parameter)
        {
            Message msg = checkPermissionMenu(urlPermission, "Delete");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện xóa quyền
            msg = new Message { Error = false };
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var data = _context.Permission.SingleOrDefault(x => x.ID == parameter.ID);
                    if (data != null)
                    {
                        _context.Permission.Remove(data);
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
        /// Cập nhật quyền
        /// </summary>
        /// <param name="parameter">Permission</param>
        /// <returns>object</returns>
        [HttpPost]
        public object updatePermission([FromBody] Permission parameter)
        {
            Message msg = checkPermissionMenu(urlPermission, "Edit");
            // kiểm tra quyền trước khi lấy dữ liệu
            if (msg.Error)
            {
                return Json(msg);
            }
            // nếu có quyền thì thực hiện cập nhật quyền
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
                    _context.Entry(parameter).Property(x => x.PermissionCode).IsModified = false;
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
        public class getPermissionMenuBarEntity
        {
            public string TypePermission { get; set; }
            public bool Status { get; set; }
        }
        /// <summary>
        /// Lấy dữ liệu quyền để check ngoài view khi thực hiện hành cộng
        /// </summary>
        /// <param name="urlPage">string</param>
        /// <returns>object</returns>
        [HttpGet]
        public object getPermissionMenuBar(string urlPage)
        {
            Message msg = new Message { Error = false };
            try
            {
                var data = _context.PermissionOfPage.AsNoTracking().Select(x => new getPermissionMenuBarEntity
                {
                    TypePermission = x.TypePermission,
                    Status = false
                }).ToList();
                if (urlPage != null)
                {
                    foreach (var item in data)
                    {
                        if ((_context.MenuOfPage.Count(x => x.TypePermission == item.TypePermission && x.urlCode == urlPage && x.PermissionCode == AccountController.AccountLogin.PermissionCode)) > 0)
                        {
                            item.Status = true;
                        }
                    }
                }
                msg.result = data;
                msg.Title = "Lấy permission thành công.";
                return Json(msg);
            }
            catch (Exception ex)
            {
                msg.result = ex;
                msg.Title = "Lấy permission thất bại thành công.";
                return Json(msg);
            }
        }
        /// <summary>
        ///  khai báo class chứa parameter đầu vào
        /// </summary>
        public class MenuBarConfigEntity
        {
            public string urlPage { get; set; }
            public string PermissionCode { get; set; }
        }
        /// <summary>
        /// Lấy dữ liệu quyền để check ngoài view khi thực hiện hành cộng
        /// </summary>
        /// <param name="urlPage">string</param>
        /// <returns>object</returns>
        [HttpPost]
        public object getPermissionMenuBarConfig([FromBody] MenuBarConfigEntity parameter)
        {
            Message msg = new Message { Error = false };
            try
            {
                var data = _context.PermissionOfPage.AsNoTracking().Select(x => new getPermissionMenuBarEntity
                {
                    TypePermission = x.TypePermission,
                    Status = false
                }).ToList();
                if (parameter.urlPage != null)
                {
                    foreach (var item in data)
                    {
                        if ((_context.MenuOfPage.Count(x => x.TypePermission == item.TypePermission && x.urlCode == parameter.urlPage && x.PermissionCode == parameter.PermissionCode)) > 0)
                        {
                            item.Status = true;
                        }
                    }
                }
                msg.result = data;
                msg.Title = "Lấy permission thành công.";
                return Json(msg);
            }
            catch (Exception ex)
            {
                msg.result = ex;
                msg.Title = "Lấy permission thất bại thành công.";
                return Json(msg);
            }
        }
        /// <summary>
        /// thực hiện kiểm tra khi thực hiện một method nào đó
        /// </summary>
        /// <param name="urlPage">string</param>
        /// <param name="type">string</param>
        /// <returns>Message</returns>
        public Message checkPermissionMenu(string urlPage, string type)
        {
            Message msg = new Message { Error = false };
            try
            {
                if (AccountController.AccountLogin != null)
                {
                    var count = _context.MenuOfPage.Count(x =>
                        x.urlCode == urlPage
                        && x.TypePermission == type
                        && x.PermissionCode == AccountController.AccountLogin.PermissionCode);
                    if (count > 0)
                    {
                        msg.Error = false;
                    }
                    else
                    {
                        msg.Title = "Tài khoản không có quyền thực hiện chức năng này.";
                        msg.Error = true;
                    }
                }
                else
                {
                    msg.Title = "Tài khoản không có quyền thực hiện chức năng này.";
                    msg.Error = true;
                }
            }
            catch (Exception ex)
            {
                msg.Title = "Tài khoản không có quyền thực hiện chức năng này.";
                msg.result = ex;
                msg.Error = true;
            }
            return msg;
        }
        /// <summary>
        ///  khai báo class chứa parameter đầu vào áp dụng quyền
        /// </summary>
        public class ApplyPermissionList
        {
            public string TypePermission { get; set; }
            public bool Status { get; set; }
        }
        /// <summary>
        ///  khai báo class chứa parameter đầu vào áp dụng quyền
        /// </summary>
        public class ApplyPermissionEntity
        {
            public string UrlCode { get; set; }
            public string PermissionCode { get; set; }
            public List<ApplyPermissionList> listPermission { get; set; }
        }
        /// <summary>
        /// Thực hiện áp dụng chức năng cho quyền nào đó
        /// </summary>
        /// <param name="parameter">ApplyPermissionEntity</param>
        /// <returns>object</returns>
        public object ApplyPermission([FromBody] ApplyPermissionEntity parameter)
        {
            Message msg = new Message { Error = false };
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var dataDelete = _context.MenuOfPage.Where(x => x.PermissionCode == parameter.PermissionCode && x.urlCode == parameter.UrlCode).ToList();
                    foreach (var item in dataDelete)
                    {
                        _context.MenuOfPage.Remove(item);
                        _context.SaveChanges();
                    }
                    foreach (var item in parameter.listPermission)
                    {
                        if (item.Status)
                        {
                            MenuOfPage entity = new MenuOfPage
                            {
                                TypePermission = item.TypePermission,
                                PermissionCode = parameter.PermissionCode,
                                urlCode = parameter.UrlCode,
                                CreatedBy = "Trương Quốc Trọng",
                                CreatedDate = DateTime.Now,
                                ModifiedBy = "Trương Quốc Trọng",
                                ModifiedDate = DateTime.Now
                            };
                            _context.MenuOfPage.Add(entity);
                            _context.SaveChanges();
                        }
                    }
                    msg.Title = "Áp dụng quyền thành công";
                    dbContextTransaction.Commit();
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    msg.Error = true;
                    msg.Title = "Áp dụng quyền không thành công.";
                    msg.result = ex;
                }
            }
            return Json(msg);
        }
    }
}
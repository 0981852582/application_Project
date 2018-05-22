using System;
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
        private string urlPermission { get; set; }
        private readonly AccessContext _context;
        public PermissionController(AccessContext context)
        {
            _context = context;
            urlPermission = "permission";
        }
        public object getListPermission([FromBody] JTable parameter)
        {
            // check permisstion
            Message msg = checkPermissionMenu(urlPermission, "Admin", "Access");
            if (msg.Error)
            {
                return Json(msg);
            }
            // query
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
                return Json(dataTable);
            }
            catch (Exception)
            {
                return Json(dataTable);
            }
        }

        public class ParameterRequest
        {
            public int ID { get; set; }
        }
        public object getItemPermission([FromBody] ParameterRequest parameter)
        {
            // check permisstion
            Message msg = checkPermissionMenu(urlPermission, "Admin", "Access");
            if (msg.Error)
            {
                return Json(msg);
            }
            // query
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

        public object insertPermission([FromBody] Permission parameter)
        {
            Message msg = checkPermissionMenu(urlPermission, "Admin", "Add");
            // check permisstion
            if (msg.Error)
            {
                return Json(msg);
            }
            // query
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
        public object deletePermission([FromBody] Permission parameter)
        {
            Message msg = checkPermissionMenu(urlPermission, "Admin", "Delete");
            // check permisstion
            if (msg.Error)
            {
                return Json(msg);
            }
            // query
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
        public object updatePermission([FromBody] Permission parameter)
        {
            Message msg = checkPermissionMenu(urlPermission, "Admin", "Edit");
            // check permisstion
            if (msg.Error)
            {
                return Json(msg);
            }
            // query
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
        public class getPermissionMenuBarEntity
        {
            public string TypePermission { get; set; }
            public bool Status { get; set; }
        }
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
                        if ((_context.MenuOfPage.Count(x => x.TypePermission == item.TypePermission && x.urlCode == urlPage && x.PermissionCode == "admin")) > 0)
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
        public Message checkPermissionMenu(string urlPage, string permission, string type)
        {
            Message msg = new Message { Error = false };
            try
            {
                var count = _context.MenuOfPage.Count(x =>
                        x.PermissionCode == permission
                        && x.urlCode == urlPage
                        && x.TypePermission == type);
                if (count > 0)
                {
                    msg.Error = false;
                }
                else
                {
                    msg.Title = "Lỗi quyền chức năng.";
                    msg.Error = true;
                }

            }
            catch (Exception ex)
            {
                msg.Title = "Lỗi quyền chức năng.";
                msg.result = ex;
                msg.Error = true;
            }
            return msg;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYBANHANG.Models;
using WebApplication7.Models;
using WebApplication7.Models.EntitySQL;

namespace WebApplication7.Controllers
{
    public class MenuBarController : Controller
    {

        private string urlPermission { get; set; }
        private readonly AccessContext _context;
        private PermissionController permission { get; set; }
        public MenuBarController(AccessContext context)
        {
            _context = context;
            permission = new PermissionController(context);
            urlPermission = "menuBar";
        }
        public object getListMenuBar([FromBody] JTable parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Admin", "Access");
            // check permisstion
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
                return Json(dataTable);
            }
            catch (Exception ex)
            {
                return Json(dataTable);
            }
        }

        public class ParameterRequest
        {
            public int ID { get; set; }
        }
        public object getItemMenuBar([FromBody] ParameterRequest parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Admin", "Access");
            // check permisstion
            if (msg.Error)
            {
                return Json(msg);
            }
            // query
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

        public object insertMenuBar([FromBody] MenuBar parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Admin", "Add");
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
        public object deleteMenuBar([FromBody] MenuBar parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Admin", "Delete");
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
        public object updateMenuBar([FromBody] MenuBar parameter)
        {
            Message msg = permission.checkPermissionMenu(urlPermission, "Admin", "Edit");
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
        public class getViewMenuBarEntity
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public string urlCode { get; set; }
            public string PermissionCode { get; set; }
            public string TypePermission { get; set; }
        }
        [HttpPost]
        public object getViewMenuBar()
        {
            Message msg = new Message { Error = false };
            try
            {
                var data = _context.MenuOfPage.Select(x => new getViewMenuBarEntity
                {
                    urlCode = x.urlCode,
                    PermissionCode = x.PermissionCode,
                    TypePermission = x.TypePermission
                }).Where(x => x.TypePermission == "access").ToList();
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
                return Json(msg);
            }
            catch(Exception ex)
            {
                msg.result = ex;
                msg.Title = "Lấy menuBar thất bại thành công.";
                return Json(msg);
            }
            
        }
    }
}
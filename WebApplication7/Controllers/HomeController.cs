﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYBANHANG.Models;
using WebApplication7.Models;
using WebApplication7.Models.EntitySQL;

namespace WebApplication7.Controllers
{
    public class HomeController : Controller
    {
        private readonly AccessContext _context;
        public HomeController(AccessContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.permission = generalPermissionJavascript(getViewMenuBar("menuBar"));
            return View();
        }
        public object getListPermission([FromBody] JTable parameter)
        {
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
            Message msg = new Message { Error = false };
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
            Message msg = new Message { Error = false };
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
            Message msg = new Message { Error = false };
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
        public class get
        {
            public string TypePermission { get; set; }
            public bool Status { get; set; }
        }
        [HttpGet]
        public List<get> getViewMenuBar(string abc)
        {
            var data = _context.PermissionOfPage.AsNoTracking().Select(x => new get
            {
                TypePermission = x.TypePermission,
                Status = false
            }).ToList();
            if (abc != null)
            {
                foreach (var item in data)
                {
                    if ((_context.MenuOfPage.Count(x => x.TypePermission == item.TypePermission && x.urlCode == abc && x.PermissionCode == "admin")) > 0)
                    {
                        item.Status = true;
                    }
                }
            }
            return data;
        }
        [HttpPost]
        public string generalPermissionJavascript(List<get> list)
        {
            var permission = "var permission = {";
            foreach (var item in list)
            {
                permission += item.TypePermission + ":" + item.Status.ToString().ToLower() + ",";
            }
            permission += "};";
            return permission;
        }
        public class get1
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public string urlCode { get; set; }
            public string PermissionCode { get; set; }
            public string TypePermission { get; set; }
        }
        [HttpGet]
        public object getViewMenuBar1()
        {
            var data = _context.MenuOfPage.Select(x => new get1
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
            return Json(data);
        }
        [HttpPost]
        public string generalPermissionJavascript1(List<get> list)
        {
            var permission = "var permission = {";
            foreach (var item in list)
            {
                permission += item.TypePermission + ":" + item.Status.ToString().ToLower() + ",";
            }
            permission += "};";
            return permission;
        }
    }
}

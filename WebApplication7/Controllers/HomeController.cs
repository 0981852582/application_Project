﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
                    msg.result = ex;
                    msg.Title = "Câp nhật quyền thất bại";
                    return Json(msg);
                }
            }
        }
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
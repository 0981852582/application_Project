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
    public class AccountController : Controller
    {

        private string urlPermission { get; set; }
        private readonly AccessContext _context;
        private PermissionController permission { get; set; }
        public AccountController(AccessContext context)
        {
            _context = context;
            permission = new PermissionController(context);
            urlPermission = "account";
        }
        public object getListAccount([FromBody] JTable parameter)
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
        public object getItemAccount([FromBody] ParameterRequest parameter)
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
                        x.PermissionCode
                    })
                    .FirstOrDefault();
                return Json(data);
            }
            catch (Exception)
            {
                return Json(new Account { });
            }
        }

        public object insertAccount([FromBody] Account parameter)
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
        public object deleteAccount([FromBody] Account parameter)
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
        public object updateAccount([FromBody] Account parameter)
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
    }
}
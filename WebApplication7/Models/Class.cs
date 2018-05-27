using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using QUANLYBANHANG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;
using WebApplication7.Controllers;

namespace WebApplication7.Models
{
    public class MyActionFilterAttribute : ActionFilterAttribute
    {
       
        public string permission { get; set; }
        public string function { get; set; }

        public PermissionController Entity_permission { get; set; }
        
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            if (AccountController.AccountLogin != null)
            {
                if (AccountController.AccountLogin.PermissionCode != HomeController.FullPermission)
                {
                    var count = HomeController._context.MenuOfPage.Count(x =>
                    x.urlCode == permission
                    && x.TypePermission == function
                    && x.PermissionCode == AccountController.AccountLogin.PermissionCode);
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Account" }, { "action", "Index" } });
                    if (count > 0)
                    {
                        
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            return;
        }
    }
}

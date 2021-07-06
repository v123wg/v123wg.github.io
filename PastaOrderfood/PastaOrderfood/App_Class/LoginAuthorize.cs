﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PastaOrderfood.Account
{
    /// <summary>
    /// 自定義權限 Filter
    /// </summary>
    public class LoginAuthorize : AuthorizeAttribute
    {
        public string RoleNo { get; set; } = "";
        /// <summary>
        /// 覆寫 Authorize 設定
        /// </summary>

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //除錯模式不檢查權限

            //未限制角色不檢查權限
            if (string.IsNullOrEmpty(RoleNo)) return true;

            //檢查登入者角色是否包含在限制的角色中
            bool bln_authorized = false;
            List<string> lists = RoleNo.Split(',').ToList();
            foreach (string role in lists)
            {
                if (UserAccount.RoleName == role) { bln_authorized = true; break; }
            }
            return bln_authorized;
        }
    }
}
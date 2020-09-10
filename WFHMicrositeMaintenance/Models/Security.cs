using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WFHMicrositeMaintenance.Models
{
    public static class Security
    {
        public static bool IsInGroup(this ClaimsPrincipal User, string GroupName)
        {
            if (User.Identity.IsAuthenticated)
            {
                List<string> roles = new List<string>();
                foreach (var claim in User.Claims)
                {
                    roles.Add(claim.Value);
                }
                return roles.Contains(GroupName);
            }
            return false;
        }

        public static string Name(this ClaimsPrincipal User)
        {
            if (User.Identity.IsAuthenticated)
            {
                return User.Identity.Name;
            }
            return "";
        }
    }
}

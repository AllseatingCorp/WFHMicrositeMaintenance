using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace WFHMicrositeMaintenance.Models
{
    public static class Security
    {
        public static bool IsInGroup(this ClaimsPrincipal User, string GroupName)
        {
            var groups = new List<string>();

            var wi = (WindowsIdentity)User.Identity;
            if (wi.Groups != null)
            {
                foreach (var group in wi.Groups)
                {
                    try
                    {
                        groups.Add(group.Translate(typeof(NTAccount)).ToString());
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                return groups.Contains("ALLSEATING\\" + GroupName);
            }
            return false;
        }

        public static string Name(this ClaimsPrincipal User)
        {
            string name = User.Identity.Name.Substring(11);
            name = name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower();
            return name;
        }
    }
}

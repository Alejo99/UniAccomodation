using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UniAccomodation.Models.ViewModels
{
    public enum CreateRoles
    {
        Student = 0,
        Landlord = 1,
        Admin = 2
    }

    public class CreateModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public CreateRoles Role { get; set; }

        public string IdentityRole
        {
            get
            {
                switch(Role) {
                    case CreateRoles.Landlord:
                        return "Landlord";
                    case CreateRoles.Admin:
                        return "Admin";
                    case CreateRoles.Student:
                    default:
                        return "User";
                }
            }
        }
    }

    public class LoginModel
    {
        [Required]
        [UIHint("email")]
        public string Email { get; set; }
        [Required]
        [UIHint("password")]
        public string Password { get; set; }
    }

    public enum RegisterRoles
    {
        Student = 0,
        Landlord = 1
    }

    public class RegisterModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public RegisterRoles Role { get; set; } = RegisterRoles.Student;

        public string IdentityRole
        {
            get
            {
                switch(Role) {
                    case RegisterRoles.Landlord:
                        return "Landlord";
                    case RegisterRoles.Student:
                    default:
                        return "User";
                }
            }
        }
    }

    public class RoleEditModel
    {
        public IdentityRole Role { get; set; }
        public IEnumerable<ApplicationUser> Members { get; set; }
        public IEnumerable<ApplicationUser> NonMembers { get; set; }
    }

    public class RoleModificationModel
    {
        [Required]
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public string[] IdsToAdd { get; set; }
        public string[] IdsToDelete { get; set; }
    }
}

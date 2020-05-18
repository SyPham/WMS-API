using System;
using System.ComponentModel.DataAnnotations;

namespace Service.Dto
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 1, ErrorMessage = "You must specify a password between 1 and 8 characters")]
        public string Password { get; set; }

        [Required]
        public int RoleID { get; set; }

    }
}
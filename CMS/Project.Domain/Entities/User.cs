using Microsoft.AspNetCore.Identity;
using Project.Domain.Entities.Base;
using Project.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;


namespace Project.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public double Balance { get; set; }
        public DateTime LastLogin { get; set; }
        public int VerificationCode { get; set; } = 0;

        [NotMapped]
        public string Nickname
        {
            get
            {
                return $"{FirstName} {LastName}".Trim();
            }
        }
    }
}
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Project.Domain.Entities
{
    public class User : IdentityUser
    {
        [StringLength(128)]
        public string FirstName { get; set; }
        [StringLength(128)]
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
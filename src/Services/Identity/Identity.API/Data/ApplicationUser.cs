using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Data
{
   
    

    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() {}

        public ApplicationUser(string userName) : base(userName) {
            
        }




        [Required]
        public string CardNumber { get; set; } = "666";
        [Required]
        public string SecurityNumber { get; set; } = "666";
        [Required]
        [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "Expiration should match a valid MM/YY value")]
        public string Expiration { get; set; } = "06/06";
        [Required]
        public string CardHolderName { get; set; } = "666";
        public int CardType { get; set; }
        [Required]
        public string Street { get; set; } = "666";
        [Required]
        public string City { get; set; } = "666";
        [Required]
        public string State { get; set; } = "666";
        [Required]
        public string Country { get; set; } = "666";
        [Required]
        public string ZipCode { get; set; } = "666";
        [Required]
        public string Name { get; set; } = "666";
        [Required]
        public string LastName { get; set; } = "666";
    }
}

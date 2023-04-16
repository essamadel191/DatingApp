using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUser(UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager )
        {
            if(await userManager.Users.AnyAsync()) return;

            var useData =await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(useData);

            var roles = new List<AppRole>{
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"},
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            foreach(var user in users)
            {
//                without identity core
//                using var hmac = new HMACSHA512();
                user.Photos.First().IsApproved = true;
                user.UserName = user.UserName.ToLower();

//                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
//                user.PasswordSalt = hmac.Key;

                await userManager.CreateAsync(user,"Pa$$w0rd");
                await userManager.AddToRoleAsync(user,"Member");
                
            }
            
            var admin = new AppUser
            {
                UserName = "admin"

            };
           

            await userManager.CreateAsync(admin,"Pa$$w0rd");
            await userManager.AddToRolesAsync(admin,new[] {"Admin","Moderator"});

        }
    }
}
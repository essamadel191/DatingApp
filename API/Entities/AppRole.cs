using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppRole : IdentityRole<int>
    {
        //for the join table ...... we will not use the entity framework to handle it
        public ICollection<AppUserRole> UserRoles { get; set; }

    }
}
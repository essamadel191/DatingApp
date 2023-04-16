using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        public PhotoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Photo> GetPhotoById(int id)
        {
            return await _context.Photos
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            return await _context.Photos
                        .Where(p => p.IsApproved == false)
                        .IgnoreQueryFilters()
                        .Select(u => new PhotoForApprovalDto{
                        Id = u.Id,
                        Username = u.AppUser.UserName,
                        Url = u.Url,
                        IsApproved = u.IsApproved
                        }).ToListAsync();
        }

        public void RemovePhoto(Photo photo)
        {
            _context.Photos.Remove(photo);
        }
    }
}

using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Extensions;
using Microsoft.EntityFrameworkCore;
using API.Helpers;

namespace API.Data
{
    public class LikeRepository : ILikesRepository
    {
        public DataContext _context { get; set; }
        public LikeRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId,targetUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {

            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var Likes = _context.Likes.AsQueryable();

            if(likesParams.Predicate == "liked"){
                Likes = Likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = Likes.Select(like => like.TargetUser);
            }
            
            if(likesParams.Predicate == "likedBy"){
                Likes = Likes.Where(like => like.TargetUserId == likesParams.UserId);
                users = Likes.Select(like => like.SourceUser);
            }

            var likedUsers = users.Select(user => new LikeDto {
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Age = user.DateOfBirth.CalculateAge(),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
            City = user.City,
            Id = user.Id
            });


            return await PagedList<LikeDto>.CreateAsync(likedUsers,likesParams.PageNumber,likesParams.PageSize);        
    }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}
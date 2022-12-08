using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.Extensions;
using API.Entities;
using API.DTOs;
using API.Helpers;

namespace API.Controllers
{
    
    public class LikesController: BaseApiController
    {
        private readonly ILikesRepository _likesRepository;
        private readonly IUserRepository _userRepository;

        public LikesController(IUserRepository userRepository,ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;
            
        }


        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _userRepository.GetUserByUsername(username);
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

            if(likedUser == null) return NotFound();

            if(sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await _likesRepository.GetUserLike(sourceUserId,likedUser.Id);

            if(userLike != null) return BadRequest("You already like this user");

            userLike = new UserLike{
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };
            sourceUser.LikedUsers.Add(userLike);

            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");

        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();

            var users = await _likesRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users.CurrentPage,
            users.PageSize,users.TotalCount,users.TotalPages);

            return Ok(users);
        }           
    }
}
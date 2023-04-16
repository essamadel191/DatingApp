using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _UnitOfWork;

        public UsersController(IMapper mapper,IPhotoService photoService,IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
        }  
        
        [HttpGet]
        public async Task <ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userPrams)
        {
            var gender = await _UnitOfWork.UserRepository.GetUserGender(User.GetUsername());
            userPrams.CurrentUsername = User.GetUsername();

            if(string.IsNullOrEmpty(userPrams.Gender)){
                userPrams.Gender = gender == "male" ? "female" : "male";
            }

            var users = await _UnitOfWork.UserRepository.GetMembersAsync(userPrams);

           Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        [Authorize(Roles = "Member")]
        [HttpGet("{username}" , Name ="GetUser")]
        public async Task <ActionResult<MemberDto>> GetUser(string username)
        {

            var CurrentUsername = User.GetUsername();
            return await _UnitOfWork.UserRepository.GetMemberAsync(username,
             isCurrentUser : CurrentUsername == username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _UnitOfWork.UserRepository.GetUserByUsername(User.GetUsername());
            _mapper.Map(memberUpdateDto,user);
            _UnitOfWork.UserRepository.Update(user);
            if (await _UnitOfWork.Complete()) return NoContent();
            return BadRequest("Failed to update user"); 
        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _UnitOfWork.UserRepository.GetUserByUsername(User.GetUsername());
            var result = await _photoService.AddPhotoAsync(file);
            if(result.Error != null) return BadRequest(result.Error.Message);
            //Store The photo
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            //Main Img
            /*
            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }
             */
            user.Photos.Add(photo);

            //Checking all Ok and Map the useful data to dto for the client side
            if(await _UnitOfWork.Complete())
            {
                //return _mapper.Map<PhotoDto>(photo);
                return CreatedAtRoute("GetUser",new {username = user.UserName},_mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _UnitOfWork.UserRepository.GetUserByUsername(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x=> x.Id == photoId);
            if(photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if(currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if(await _UnitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to set main photo!");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
        var user=await _UnitOfWork.UserRepository.GetUserByUsername(User.GetUsername());
        
        
        //var photo= user.Photos.FirstOrDefault(x=>x.Id==photoId);
        var photo = await _UnitOfWork.PhotoRepository.GetPhotoById(photoId);

        if(photo==null) return NotFound();
        if(photo.IsMain) return BadRequest("You can not delete your main photo");
        if(photo.PublicId!=null)
        {
            var result= await _photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error!=null) return BadRequest(result.Error.Message);
 
        }
        user.Photos.Remove(photo);
        if(await _UnitOfWork.Complete()) return Ok();
 
        return BadRequest("Failed to delete");
       }


    }
}
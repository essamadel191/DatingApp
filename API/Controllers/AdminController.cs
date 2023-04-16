using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using API.Services;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PhotoService _photoService;
        public AdminController(UserManager<AppUser> userManager,IUnitOfWork unitOfWork,PhotoService photoService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _photoService = photoService;
        }


        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUserWithRoles(){
           
           var users =  await _userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users); 
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")] // take the user whose roles we wish to edit
        public async Task<ActionResult> EditRoles(string username,[FromQuery]string roles){
            if(string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

            var selectedRoles = roles.Split(",").ToArray();

            var user = await _userManager.FindByNameAsync(username);

            if(user == null) return NotFound();

            var UserRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user,selectedRoles.Except(UserRoles));

            if(!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user,UserRoles.Except(selectedRoles));

            if(!result.Succeeded) return BadRequest();

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration(){
            var photos = _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
            return Ok(photos);

        }
        [Authorize(Policy = "ModeratePhotoRole")]
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId){
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
            if(photo == null) return NotFound("Could not find photo");
            photo.IsApproved = true;

            var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);

            if(!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;
            await _unitOfWork.Complete();
            return Ok();
        }
        
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId){
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
            if(photo.PublicId != null){
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Result == "ok"){
                    _unitOfWork.PhotoRepository.RemovePhoto(photo);
                }
            }
            else{
                _unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
            await _unitOfWork.Complete();
            return Ok();
        }


    }
}
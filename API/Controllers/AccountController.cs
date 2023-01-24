using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
       
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;

        private readonly IMapper _mapper;
        public AccountController(UserManager<AppUser> userManager ,ITokenService tokenService,IMapper mapper)
        {
            _userManager = userManager;
            
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register (RegisterDto registerDto){
            if(await UserExists(registerDto.username)){
                return BadRequest("Username is taken");
            }
            var user = _mapper.Map<AppUser>(registerDto);

//            Without Identity core
//            using var hmac = new HMACSHA512();

            user.UserName = registerDto.username.ToLower();
//            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
//            user.PasswordSalt = hmac.Key;
            
            var result = await _userManager.CreateAsync(user,registerDto.Password);

            if(!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user,"Member");

            if(!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            

            return new UserDto{
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.username);
            
            if(user == null) return Unauthorized("Invalid username");

            var result = await _userManager.CheckPasswordAsync(user,loginDto.Password);

            if(!result) return Unauthorized("Invalid password");


//            using var hmac = new HMACSHA512(user.PasswordSalt);
//            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
//
//            for(int i=0;i<computeHash.Length;i++){
//                if(computeHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
//            }
            
            return new UserDto{
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl =user.Photos.FirstOrDefault(x=> x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
            

        }

        private async Task<bool> UserExists (string username){
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }




    }
}
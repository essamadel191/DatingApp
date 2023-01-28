using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
         public static IServiceCollection AddApplicationServices(this IServiceCollection services,
         IConfiguration config){
            
            services.AddDbContext<DataContext>(option => {
                option.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<ITokenService,TokenService>();
            services.AddScoped<IPhotoService,PhotoService>();
            services.AddScoped<LogUserActivity>();
            services.AddScoped<ILikesRepository,LikeRepository>();
            services.AddScoped<IUserRepository,UserRepository>();
            services.AddScoped<IMessageRepository,MessageRepository>();
            services.AddSignalR();
            
            return services;
        }
    }
}
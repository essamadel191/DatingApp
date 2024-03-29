using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class UserDto
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("photoUrl")]
        public string PhotoUrl { get; set; }

        public string KnownAs { get; set; }

        public string Gender { get; set; }
        
    }
}
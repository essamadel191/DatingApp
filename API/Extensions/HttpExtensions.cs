using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using API.Helpers;

namespace API.Extensions
{
        public static class HttpExtensions
        {
            public static void AddPaginationHeader(this HttpResponse response,int currentPage,
            int itemsPerPage,int totalItems,int totalPages)
            {
                var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

            var options = new JsonSerializerOptions { // This changes the header response json data to camel case. We pass these options to our serialize method below
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            };

            response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader, options)); // This adds the header to the response. We have to serialize this because when we add this our response headers take a key and string value. So the ""Pagination" will be our key and our JsonSerializer.Serialize(paginationHeader) is our value. 

            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
            }
        }
    
}
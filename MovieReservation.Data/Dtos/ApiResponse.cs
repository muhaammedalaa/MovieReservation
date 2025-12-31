using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Dtos
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }

        // Success response with data
        public ApiResponse(T data, string message = "Success")
        {
            Success = true;
            Data = data;
            Message = message;
        }

        // Error response
        public ApiResponse(string message = "An error occurred")
        {
            Success = false;
            Message = message;
        }

        // Error response with validation errors
        public ApiResponse(string message, Dictionary<string, string[]> errors)
        {
            Success = false;
            Message = message;
            Errors = errors;
        }
    }

    
    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse(string message = "An error occurred") : base(message) { }
    }
}

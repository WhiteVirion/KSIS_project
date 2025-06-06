using Microsoft.AspNetCore.Http;

namespace test_server.Data
{
    public class TrackUploadRequest
    {
        public IFormFile file { get; set; }
        public string? title { get; set; }
        public string? artist { get; set; }
    }
} 
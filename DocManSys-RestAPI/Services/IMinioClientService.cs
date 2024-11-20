using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.ObjectModelRemoting;

namespace DocManSys_RestAPI.Services
{
    public interface IMinioClientService
    {
        public Task UploadFile(IFormFile file);
        public Task<ActionResult> DownloadFile(string fileName);
        public Task<ActionResult> DeleteFile(string fileName);
    }
}

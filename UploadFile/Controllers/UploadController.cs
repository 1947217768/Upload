using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Polly;
using UploadFile.Models;

namespace UploadFile.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UploadController : ControllerBase
    {

        [EnableCors("AllowSpecificOrigin")]
        [HttpPost]
        public async Task<IActionResult> RuleUploadFile([FromQuery] SliceFileInfo file)
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload");

                //获取文件上传边界
                var files = Request.Form.Files;
                var buffer = new byte[file.Size];
                var fileName = file.Name;
                path = path + "//" + fileName + "//";
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                string filepath = path + "//" + file.Name + "^" + file.Number;
                using (var stream = new FileStream(filepath, FileMode.Append))
                {
                    await files[0].CopyToAsync(stream);
                }
                var filesList = Directory.GetFiles(Path.GetDirectoryName(path));

                //当顺序号等于分片总数量 合并文件
                if ((file.Number + 1) == file.Count || filesList.Length == file.Count)
                {

                    await MergeFile(file);
                }
                return this.Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        /// <summary>
        /// 合并文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task MergeFile(SliceFileInfo file)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload");
            var fileName = file.Name;
            path = path + "//" + fileName + "//";
            string baseFileName = path + fileName.Split("~")[0].ToString();
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            var filesList = Directory.GetFiles(Path.GetDirectoryName(path));
            if (filesList.Length != file.Count)
            {
                return;
            }
            List<FileSort> lstFile = new List<FileSort>();
            foreach (var item in filesList)
            {
                lstFile.Add(new FileSort()
                {
                    Name = item,
                    NumBer = Convert.ToInt32(item.Substring(item.IndexOf('^') + 1))
                });
            }
            lstFile = lstFile.OrderBy(x => x.NumBer).ToList();
            using (var fileStream = new FileStream(baseFileName, FileMode.Create))
            {
                //foreach (var fileSort in filesList)
                //{
                //    using (FileStream fileChunk = new FileStream(fileSort, FileMode.Open))
                //    {
                //        await fileChunk.CopyToAsync(fileStream);
                //    }

                //}
                await Policy.Handle<IOException>()
                        .RetryForeverAsync()
                        .ExecuteAsync(async () =>
                        {
                            foreach (var fileSort in lstFile)
                            {
                                using (FileStream fileChunk = new FileStream(fileSort.Name, FileMode.Open))
                                {
                                    await fileChunk.CopyToAsync(fileStream);
                                }

                            }
                        });


            }
            //删除分片文件
            foreach (var dirfile in filesList)
            {
                System.IO.File.Delete(dirfile);
            }
        }

        //public async Task<IActionResult> RuleUploadFile([FromForm] List<IFormFile> files)
        //{
        //    long size = files.Sum(f => f.Length);
        //    var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Upload");
        //    if (!System.IO.Directory.Exists(path))
        //    {
        //        System.IO.Directory.CreateDirectory(path);
        //    }
        //    string filepath = "";
        //    foreach (var file in files)
        //    {
        //        if (file.Length > 0)
        //        {
        //            filepath = path + "\\" + file.FileName;
        //            using (var stream = new FileStream(filepath, FileMode.Create))
        //            {
        //                await file.CopyToAsync(stream);
        //            }
        //        }
        //    }
        //    return Ok(new { count = files.Count, size });
        //}


    }
}
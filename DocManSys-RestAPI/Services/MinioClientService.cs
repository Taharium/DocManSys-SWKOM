﻿using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;

namespace DocManSys_RestAPI.Services {
    public class MinioClientService : IMinioClientService {
        private readonly IMinioClient _minioClient;

        public string? Bucket { get; private set; }

        public MinioClientService(IConfiguration configuration) {
            _minioClient = new MinioClient()
                .WithEndpoint(configuration["MinIOCredentials:MinIOEndpoint"])
                .WithCredentials(configuration["MinIOCredentials:MinIOAccessKey"],
                    configuration["MinIOCredentials:MinIOSecretKey"])
                .WithSSL(false)
                .Build();
            Bucket = configuration["MinIOCredentials:MinIOBucketName"];
        }

        // Constructor for testing purposes
        public MinioClientService(IMinioClient minioClient) {
            _minioClient = minioClient;
            Bucket = "test-uploads";
        }


        public async Task UploadFile(IFormFile file) {
            await EnsureBucketExists();

            var fileName = Path.GetFileName(file.FileName);
            await using var fileStream = file.OpenReadStream();

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(Bucket)
                .WithObject(fileName)
                .WithStreamData(fileStream)
                .WithObjectSize(file.Length));
        }

        public async Task<ActionResult> DownloadFile(string fileName) {
            var memoryStream = new MemoryStream();

            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(Bucket)
                .WithObject(fileName)
                .WithCallbackStream(stream => { stream.CopyTo(memoryStream); }));

            memoryStream.Position = 0;
            return new FileStreamResult(memoryStream, "application/octet-stream") {
                FileDownloadName = fileName
            };
        }

        public async Task<ActionResult> DeleteFile(string fileName) {
            try {
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(Bucket)
                    .WithObject(fileName));

                return new OkObjectResult(new { message = $"Datei '{fileName}' erfolgreich gelöscht." });
            }
            catch (Exception ex) {
                return new ObjectResult( new { message = $"Fehler beim Löschen der Datei: {ex.Message}"});
            }
        }

        public async Task EnsureBucketExists() {
            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(Bucket));
            if (!found) {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(Bucket));
            }
        }
    }
}
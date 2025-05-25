using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using Core.Utils;
using FileStoringService.Application.Dtos;
using FileStoringService.Application.Services;
using FileStoringService.Controllers;
using FileStoringService.Infrastructure.Persistence;
using FileStoringService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

public class HashHelperTests
{
    [Fact(DisplayName = "ComputeHash выдаёт ожидаемый SHA256")]
    public void ComputeHash_ОжидаемыйРезультатДляИзвестныхДанных()
    {
        var data = Encoding.UTF8.GetBytes("abc");
        var hash = HashHelper.ComputeHash(data);
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
    }
}

public class FileStorageHelperTests
{
    [Fact(DisplayName = "GetStoragePath соединяет пути корректно")]
    public void GetStoragePath_СоединяетКорректно()
    {
        var path = FileStorageHelper.GetStoragePath("root", "file.txt");
        Assert.Equal(Path.Combine("root", "file.txt"), path);
    }

    [Fact(DisplayName = "GetStoragePath бросает при пустом корне")]
    public void GetStoragePath_ПустойКорень_Исключение()
    {
        Assert.Throws<ArgumentException>(() => FileStorageHelper.GetStoragePath("", "f"));
    }

    [Fact(DisplayName = "EnsureDirectory создаёт каталог если его нет")]
    public void EnsureDirectory_СоздаётКаталог()
    {
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "sub", "file.txt");
        if (Directory.Exists(Path.GetDirectoryName(temp))) Directory.Delete(Path.GetDirectoryName(temp), true);
        FileStorageHelper.EnsureDirectory(temp);
        Assert.True(Directory.Exists(Path.GetDirectoryName(temp)));
        Directory.Delete(Path.GetDirectoryName(temp), true);
    }
}

public class FileStorageServiceTests
{
    private IConfiguration CreateConfig(string root)
        => new ConfigurationBuilder()
           .AddInMemoryCollection(new Dictionary<string, string> { ["StoragePath"] = root })
           .Build();

    private class StubRepo : IFileRepository
    {
        public List<FileMetadata> All = new();
        public Task<FileMetadata?> GetByHashAsync(string hash)
            => Task.FromResult(All.FirstOrDefault(f => f.Hash == hash));
        public Task<FileMetadata?> GetByIdAsync(Guid id)
            => Task.FromResult(All.FirstOrDefault(f => f.Id == id));
        public Task AddAsync(FileMetadata file)
        {
            All.Add(file);
            return Task.CompletedTask;
        }
    }

    [Fact(DisplayName = "SaveFileAsync сохраняет новый файл и возвращает existed = false")]
    public async Task SaveFileAsync_NewFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var repo = new StubRepo();
        var service = new FileStorageService(repo, CreateConfig(tempDir));
        var content = "test";
        var file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes(content)), 0, content.Length, "f", "a.txt");
        var result = await service.SaveFileAsync(file);
        Assert.False(result.AlreadyExists);
        Assert.Single(repo.All);
        var saved = repo.All[0];
        Assert.Equal(result.Id, saved.Id);
        Assert.True(File.Exists(saved.Location));
        Directory.Delete(tempDir, true);
    }

    [Fact(DisplayName = "GetFileAsync возвращает поток и имя файла")]
    public async Task GetFileAsync_Success()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var id = Guid.NewGuid();
        var fname = "b.txt";
        var path = Path.Combine(tempDir, fname);
        var data = "123";
        await File.WriteAllTextAsync(path, data);
        var meta = new FileMetadata { Id = id, Name = fname, Hash = "h", Location = path };
        var repo = new StubRepo();
        repo.All.Add(meta);
        var service = new FileStorageService(repo, CreateConfig(tempDir));
        var (stream, contentType, fileName) = await service.GetFileAsync(id);
        using var sr = new StreamReader(stream);
        var read = await sr.ReadToEndAsync();
        Assert.Equal("application/octet-stream", contentType);
        Assert.Equal(fname, fileName);
        Assert.Equal(data, read);
        Directory.Delete(tempDir, true);
    }

    [Fact(DisplayName = "GetFileAsync если нет записи бросает KeyNotFoundException")]
    public async Task GetFileAsync_NotFound()
    {
        var repo = new StubRepo();
        var service = new FileStorageService(repo, CreateConfig(Path.GetTempPath()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetFileAsync(Guid.NewGuid()));
    }
}

public class FilesControllerTests
{
    [Fact(DisplayName = "Upload возвращает BadRequest при null")]
    public async Task Upload_NullFile()
    {
        var ctrl = new FilesController(new StubService());
        var res = await ctrl.Upload(null);
        Assert.IsType<BadRequestObjectResult>(res);
    }

    [Fact(DisplayName = "Upload возвращает OkObjectResult при успехе")]
    public async Task Upload_Success()
    {
        var dto = new UploadResponseDto { Id = Guid.NewGuid(), AlreadyExists = true };
        var svc = new StubService { SaveResult = dto };
        var ctrl = new FilesController(svc);
        var file = new FormFile(new MemoryStream(), 0, 0, "f", "n.txt");
        var res = await ctrl.Upload(file) as OkObjectResult;
        dynamic body = res.Value;
        Assert.Equal(dto.Id, (Guid)body.Id);
        Assert.True((bool)body.AlreadyExists);
    }

    [Fact(DisplayName = "GetFile возвращает NotFound при отсутствии")]
    public async Task GetFile_NotFound()
    {
        var svc = new StubService { ThrowNotFound = true };
        var ctrl = new FilesController(svc);
        var res = await ctrl.GetFile(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(res);
    }

    private class StubService : IFileStorageService
    {
        public UploadResponseDto SaveResult;
        public (Stream, string, string) GetResult;
        public bool ThrowNotFound;

        public Task<UploadResponseDto> SaveFileAsync(IFormFile file)
        {
            if (file == null) throw new ArgumentNullException();
            return Task.FromResult(SaveResult);
        }

        public Task<(Stream Content, string ContentType, string FileName)> GetFileAsync(Guid id)
        {
            if (ThrowNotFound) throw new KeyNotFoundException();
            return Task.FromResult(GetResult);
        }
    }
}

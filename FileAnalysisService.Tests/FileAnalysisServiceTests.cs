namespace FileAnalysisService.Tests;

using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using FileAnalysisService.Infrastructure.Adapters;
using FileAnalysisService.Application.Dtos;
using FileAnalysisService.Application.Services;
using FileAnalysisService.Controllers;
using FileAnalysisService.Infrastructure.Adapters;
using Moq;
using Moq.Protected;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

class HandlerCaptor : HttpMessageHandler
{
    public HttpRequestMessage? Request { get; private set; }
    readonly HttpResponseMessage _response;
    public HandlerCaptor(HttpResponseMessage response) => _response = response;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        Request = request;
        return Task.FromResult(_response);
    }
}

public class FileStorerHttpAdapterTests
{
    [Fact(DisplayName = "GetFileStreamAsync запрашивает правильный URL и возвращает поток")]
    public async Task GetFileStreamAsync_КорректныйЗапросИВозврат()
    {
        var expected = new MemoryStream(new byte[] { 1, 2, 3 });
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(expected)
        };
        var handler = new HandlerCaptor(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var adapter = new FileStorerHttpAdapter(client);
        var id = Guid.NewGuid();
        var stream = await adapter.GetFileStreamAsync(id);
        Assert.Equal($"/files/storage/files/{id}", handler.Request?.RequestUri?.PathAndQuery);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        Assert.Equal(new byte[] { 1, 2, 3 }, ms.ToArray());
    }

    [Fact(DisplayName = "GetFileStreamAsync бросает при ошибочном статусе")]
    public async Task GetFileStreamAsync_ОшибкаСтатуса_Исключение()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = new HandlerCaptor(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var adapter = new FileStorerHttpAdapter(client);
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            adapter.GetFileStreamAsync(Guid.NewGuid()));
    }
}

public class WordCloudHttpAdapterTests
{
    [Fact(DisplayName = "GenerateCloudAsync бросает при неуспешном статусе")]
    public async Task GenerateCloudAsync_НеуспешныйСтатус_Исключение()
    {
        HttpResponseMessage response = new(HttpStatusCode.BadRequest);
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var client = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://test/") };
        var adapter = new WordCloudHttpAdapter(client);
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            adapter.GenerateCloudAsync(new Dictionary<string, int>()));
    }
}

public class AnalysisControllerTests
{
    [Fact(DisplayName = "Analyze возвращает OK с DTO")]
    public async Task Analyze_Успех_OkObject()
    {
        var dto = new AnalysisResultDto
        {
            FileId = Guid.NewGuid(),
            ParagraphCount = 2,
            WordCount = 3,
            CharCount = 4,
            CloudImagePath = "path.png",
            AnalyzedAt = DateTime.UtcNow
        };
        var serviceMock = new MockService { AnalyzeResult = dto };
        var ctrl = new AnalysisController(serviceMock);
        var ok = await ctrl.Analyze(dto.FileId) as OkObjectResult;
        dynamic body = ok.Value;
        Assert.Equal(dto.FileId, (Guid)body.FileId);
        Assert.Equal(2, (int)body.ParagraphCount);
    }

    [Fact(DisplayName = "GetResult возвращает NotFound если нет результата")]
    public async Task GetResult_NoData_NotFound()
    {
        var serviceMock = new MockService(); 
        var ctrl = new AnalysisController(serviceMock);
        var res = await ctrl.GetResult(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(res);
    }

    [Fact(DisplayName = "GetResult возвращает OK с DTO")]
    public async Task GetResult_Успех_OkObject()
    {
        var dto = new AnalysisResultDto
        {
            FileId = Guid.NewGuid(),
            ParagraphCount = 1,
            WordCount = 1,
            CharCount = 1,
            CloudImagePath = "c.png",
            AnalyzedAt = DateTime.UtcNow
        };
        var serviceMock = new MockService { GetResult = dto };
        var ctrl = new AnalysisController(serviceMock);
        var ok = await ctrl.GetResult(dto.FileId) as OkObjectResult;
        dynamic body = ok.Value;
        Assert.Equal("c.png", (string)body.CloudImagePath);
    }

    [Fact(DisplayName = "GetCloud отдаёт файл если существует")]
    public void GetCloud_FileExists_FileResult()
    {
        var tempDir = Path.Combine(Directory.GetCurrentDirectory(), "storage");
        Directory.CreateDirectory(tempDir);
        var loc = "sub/test.png";
        var full = Path.Combine(tempDir, loc);
        Directory.CreateDirectory(Path.GetDirectoryName(full));
        File.WriteAllBytes(full, new byte[] { 9, 8 });
        var ctrl = new AnalysisController(new MockService());
        var res = ctrl.GetCloud(loc) as FileContentResult;
        Assert.Equal("image/png", res.ContentType);
        Assert.Equal(new byte[] { 9, 8 }, res.FileContents);
        Directory.Delete(tempDir, true);
    }

    [Fact(DisplayName = "GetCloud возвращает NotFound если нет файла")]
    public void GetCloud_NoFile_NotFound()
    {
        var ctrl = new AnalysisController(new MockService());
        var res = ctrl.GetCloud("no/such.png");
        Assert.IsType<NotFoundResult>(res);
    }

    class MockService : IFileAnalyseService
    {
        public AnalysisResultDto? AnalyzeResult;
        public AnalysisResultDto? GetResult;
        public Task<AnalysisResultDto> AnalyzeAsync(Guid id) =>
            Task.FromResult(AnalyzeResult!);
        public Task<AnalysisResultDto?> GetResultAsync(Guid id) =>
            Task.FromResult(GetResult);
    }
}

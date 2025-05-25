using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiGateway.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

class StubHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;
    public StubHttpClientFactory(HttpClient client) => _client = client;
    public HttpClient CreateClient(string name) => _client;
}

class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;
    public FakeHttpMessageHandler(HttpResponseMessage response) => _response = response;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_response);
}

public class FileStorageProxyControllerTests
{
    [Fact(DisplayName = "При успешной загрузке возвращается JSON и корректный ContentType")]
    public async Task Upload_ReturnsContentAndContentType_WhenSuccess()
    {
        var expected = "{\"fileId\":\"123\"}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expected, Encoding.UTF8, "application/json")
        };
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var controller = new FileStorageProxyController(new StubHttpClientFactory(client));
        var httpCtx = new DefaultHttpContext();
        httpCtx.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("data"));
        httpCtx.Request.ContentType = "text/plain";
        controller.ControllerContext = new ControllerContext { HttpContext = httpCtx };

        var result = await controller.Upload();

        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal(expected, content.Content);
        Assert.Equal("application/json", content.ContentType);
    }

    [Fact(DisplayName = "При запросе файла возвращается FileStreamResult и корректный ContentType")]
    public async Task GetFile_ReturnsFileStreamResult_WhenSuccess()
    {
        var data = new byte[] {1,2,3};
        var ms = new MemoryStream(data);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(ms)
        };
        response.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/octet-stream");
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var controller = new FileStorageProxyController(new StubHttpClientFactory(client));

        var action = await controller.GetFile("xyz");
        var fileResult = Assert.IsType<FileStreamResult>(action);
        Assert.Equal("application/octet-stream", fileResult.ContentType);
        Assert.NotNull(fileResult.FileStream);
    }

    [Fact(DisplayName = "При ошибке получения файла возвращается код ошибки")]
    public async Task GetFile_ReturnsStatusCode_WhenNotFound()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var controller = new FileStorageProxyController(new StubHttpClientFactory(client));

        var result = await controller.GetFile("xyz");

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal((int)HttpStatusCode.NotFound, status.StatusCode);
    }
}

public class FileAnalysisProxyControllerTests
{
    [Fact(DisplayName = "При анализе возвращается JSON и корректный ContentType")]
    public async Task Analyze_ReturnsJson_WhenSuccess()
    {
        var json = "{\"words\":5}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var controller = new FileAnalysisProxyController(new StubHttpClientFactory(client));

        var result = await controller.Analyze("abc");
        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal(json, content.Content);
        Assert.Equal("application/json", content.ContentType);
    }

    [Fact(DisplayName = "При ошибке анализа возвращается код ошибки")]
    public async Task Analyze_ReturnsStatusCode_WhenError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadGateway);
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var controller = new FileAnalysisProxyController(new StubHttpClientFactory(client));

        var result = await controller.Analyze("abc");
        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal((int)HttpStatusCode.BadGateway, status.StatusCode);
    }

    [Fact(DisplayName = "При запросе результата возвращается JSON и application/json")]
    public async Task GetResult_ReturnsJson_WhenSuccess()
    {
        var json = "{\"count\":10}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var controller = new FileAnalysisProxyController(new StubHttpClientFactory(client));

        var result = await controller.GetResult("id");
        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal(json, content.Content);
        Assert.Equal("application/json", content.ContentType);
    }

    [Fact(DisplayName = "При ошибке запроса результата возвращается код ошибки")]
    public async Task GetResult_ReturnsStatusCode_WhenError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var controller = new FileAnalysisProxyController(new StubHttpClientFactory(client));

        var result = await controller.GetResult("id");
        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal((int)HttpStatusCode.NotFound, status.StatusCode);
    }

    [Fact(DisplayName = "При получении облака возвращается FileStreamResult и image/png")]
    public async Task GetCloud_ReturnsImage_WhenSuccess()
    {
        var data = new byte[] {0xA,0xB};
        var ms = new MemoryStream(data);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(ms)
        };
        response.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/png");
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var controller = new FileAnalysisProxyController(new StubHttpClientFactory(client));

        var action = await controller.GetCloud("loc/path");
        var fileResult = Assert.IsType<FileStreamResult>(action);
        Assert.Equal("image/png", fileResult.ContentType);
    }

    [Fact(DisplayName = "При ошибке получения облака возвращается код ошибки")]
    public async Task GetCloud_ReturnsStatusCode_WhenError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var handler = new FakeHttpMessageHandler(response);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var controller = new FileAnalysisProxyController(new StubHttpClientFactory(client));

        var result = await controller.GetCloud("loc/path");
        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, status.StatusCode);
    }
}

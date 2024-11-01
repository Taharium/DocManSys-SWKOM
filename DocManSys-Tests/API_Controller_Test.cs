/*
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DocManSys_DAL.Entities;
using DocManSys_RestAPI.Controllers;
using DocManSys_RestAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using FakeItEasy;
using Microsoft.AspNetCore.Http;

// Custom HttpMessageHandler for mocking HttpClient responses
public class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public TestHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}

[TestFixture]
public class DocumentControllerTests
{
    private IHttpClientFactory _clientFactory;
    private ILogger<DocumentController> _logger;
    private IMapper _mapper;

    [SetUp]
    public void SetUp()
    {
        // Create mocks for dependencies
        _clientFactory = A.Fake<IHttpClientFactory>();
        _logger = A.Fake<ILogger<DocumentController>>();
        _mapper = A.Fake<IMapper>();
        
        // Initialize the controller with mocked dependencies
        //_controller = new DocumentController(_clientFactory, _logger, _mapper);
    }

    [Test]
    public async Task GetDocuments_WithSearchTerm_ReturnsOkResultWithDocuments()
    {
        using DocumentController controller = new DocumentController(_clientFactory, _logger, _mapper);

        // Arrange
        var searchTerm = "test";
        var mockResponse = new List<DocumentEntity>
        {
            new DocumentEntity { Title = "hello", Author = "hello", Id = 1, Image = "hello"}
        };

        // Create a mocked HttpResponseMessage for a successful response
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(mockResponse)
        };
        
        // Create a HttpClient with the test handler and set BaseAddress
        var httpClient = new HttpClient(new TestHttpMessageHandler(httpResponse))
        {
            BaseAddress = new Uri("http://localhost/") // Set a valid base address
        };
        A.CallTo(() => _clientFactory.CreateClient("DocManSys-DAL")).Returns(httpClient);

        // Map the mock response to Document
        var expectedDocuments = new List<Document>
        {
            new Document { Title = "hello", Author = "hello", Id = 1, Image = "hello" }
        };
        A.CallTo(() => _mapper.Map<IEnumerable<Document>>(mockResponse)).Returns(expectedDocuments);

        // Act
        var result = await controller.GetDocuments(searchTerm) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    [Test]
    public async Task GetDocuments_WithoutSearchTerm_ReturnsOkResultWithDocuments()
    {
        using DocumentController controller = new DocumentController(_clientFactory, _logger, _mapper);

        // Arrange
        var mockResponse = new List<DocumentEntity>
        {
            new () { Title = "hello", Author = "hello", Id = 1, Image = "hello" }
        };

        // Create a mocked HttpResponseMessage for a successful response
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(mockResponse)
        };

        // Create a HttpClient with the test handler and set BaseAddress
        var httpClient = new HttpClient(new TestHttpMessageHandler(httpResponse))
        {
            BaseAddress = new Uri("http://localhost/") // Set a valid base address
        };
        A.CallTo(() => _clientFactory.CreateClient("DocManSys-DAL")).Returns(httpClient);

        // Map the mock response to Document
        var expectedDocuments = new List<Document>
        {
            new Document { Title = "hello", Author = "hello", Id = 1, Image = "hello" }
        };
        A.CallTo(() => _mapper.Map<IEnumerable<Document>>(mockResponse)).Returns(expectedDocuments);

        // Act
        var result = await controller.GetDocuments() as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    [Test]
    public async Task GetDocuments_WhenResponseFails_ReturnsErrorStatusCode()
    {
        using DocumentController controller = new DocumentController(_clientFactory, _logger, _mapper);

        // Arrange
        var searchTerm = "test";

        // Create a mocked HttpResponseMessage for an error response
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };

        // Create a HttpClient with the test handler and set BaseAddress
        var httpClient = new HttpClient(new TestHttpMessageHandler(httpResponse))
        {
            BaseAddress = new Uri("http://localhost/") // Set a valid base address
        };
        A.CallTo(() => _clientFactory.CreateClient("DocManSys-DAL")).Returns(httpClient);

        // Act
        var result = await controller.GetDocuments(searchTerm) as ObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        Assert.That(result.Value, Is.EqualTo("Error retrieving Documents from DAL"));
    }
}
*/

//using System.Net;
//using System.Net.Http.Json;
//using AutoMapper;
//using DocManSys_DAL.Entities;
//using DocManSys_RestAPI.Controllers;
//using DocManSys_RestAPI.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using FakeItEasy;
//using DocManSys_RestAPI.Services;

//public class MockHttpMessageHandler : HttpMessageHandler
//{
//    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

//    public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
//    {
//        _handler = handler;
//    }

//    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//    {
//        return Task.FromResult(_handler(request));
//    }
//}


//namespace DocManSys_Tests
//{
//    [TestFixture]
//    public class API_DocumentControllerTests
//    {
//        private IHttpClientFactory _mockClientFactory;
//        private ILogger<DocumentController> _mockLogger;
//        private IMapper _mockMapper;
//        private IMessageQueueService _mockMessageQueueService;
//        private IMinioClientService _mockMinioClientService;
//        private ElasticsearchService _mockElasticsearchService;
//        private DocumentController _controller;

//        [SetUp]
//        public void SetUp()
//        {
//            _mockClientFactory = A.Fake<IHttpClientFactory>();
//            _mockLogger = A.Fake<ILogger<DocumentController>>();
//            _mockMapper = A.Fake<IMapper>();
//            _mockMessageQueueService = A.Fake<IMessageQueueService>();
//            _mockMinioClientService = A.Fake<IMinioClientService>();
//            _mockElasticsearchService = A.Fake<ElasticsearchService>();

//            _controller = new DocumentController(
//                _mockClientFactory,
//                _mockLogger,
//                _mockMapper,
//                _mockMessageQueueService,
//                _mockMinioClientService,
//                _mockElasticsearchService
//            );
//        }

//        [Test]
//        public async Task GetDocuments_ShouldReturnOkWithDocuments_WhenResponseIsSuccessful()
//        {
//            // Arrange
//            var documentEntities = new List<DocumentEntity> { new DocumentEntity { Id = 1, Author = "John", Title = "Hello", OcrText = "Carefull" } };
//            var mockHttpMessageHandler = new MockHttpMessageHandler(request => new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.OK,
//                Content = JsonContent.Create(documentEntities)
//            });

//            var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri("http://localhost:8082") };
//            A.CallTo(() => _mockClientFactory.CreateClient("DocManSys-DAL")).Returns(httpClient);

//            var mappedDocuments = new List<Document> { new Document { Id = 1, Author = "John", Title = "Hello", OcrText = "Carefull" } };
//            A.CallTo(() => _mockMapper.Map<IEnumerable<Document>>(documentEntities)).Returns(mappedDocuments);

//            // Act
//            var result = await _controller.GetDocuments() as OkObjectResult;

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.That(result.StatusCode, Is.EqualTo(200));

//            // Assert that the contents match
//            var resultDocuments = result.Value as IEnumerable<Document>;
//            Assert.IsNotNull(resultDocuments);
//            Assert.That(resultDocuments, Is.EquivalentTo(mappedDocuments));
//        }

//    }
//}




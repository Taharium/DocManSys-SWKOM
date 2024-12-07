using DocManSys_DAL.Entities;
using DocManSys_RestAPI.Models;

namespace DocManSys_RestAPI.Services;

using AutoMapper;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class RabbitMqListenerService : IHostedService {
    private IConnection? _connection;
    private IModel? _channel;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RabbitMqListenerService> _logger;

    public Task StartAsync(CancellationToken cancellationToken) {
        ConnectToRabbitMq();
        StartListening();
        return Task.CompletedTask;
    }

    public RabbitMqListenerService(IHttpClientFactory httpClientFactory, ILogger<RabbitMqListenerService> logger) {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private void ConnectToRabbitMq() {
        int retries = 10;
        while (retries > 0) {
            try {
                var factory = new ConnectionFactory()
                    { HostName = "rabbitmq", UserName = "user", Password = "password" };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: "ocr_result_queue", durable: false, exclusive: false, autoDelete: false,
                    arguments: null);
                _logger.LogInformation("Successfully connected with RabbitMQ and queue created.");
                break; // Wenn die Verbindung klappt, verlässt es die Schleife
            }
            catch (Exception ex) {
                _logger.LogError(
                    $"Error while connecting with RabbitMQ: {ex.Message}. Retry in 5 seconds...");
                Thread.Sleep(5000);
                retries--;
            }
        }

        if (_connection == null || !_connection.IsOpen) {
            _logger.LogCritical("Could not connect to RabbitMQ, all tries failed");
            throw new Exception("Could not connect to RabbitMQ, all tries failed");
        }
    }

    private void StartListening() {
        try {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) => {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var parts = message.Split('|');
                _logger.LogInformation($@"[Listener] Message received: {message}");
                if (parts.Length == 2) {
                    var id = parts[0];
                    var extractedText = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
                    if (string.IsNullOrEmpty(extractedText)) {
                        _logger.LogWarning($@"Error: empty OCR-Text for document {id}. Message ignored.");
                        return;
                    }

                    var client = _httpClientFactory.CreateClient("DocManSys-DAL");
                    var response = await client.GetAsync($"/api/DAL/document/{id}");
                    if (response.IsSuccessStatusCode) {
                        var document = await response.Content.ReadFromJsonAsync<DocumentEntity>();
                        if (document != null) {
                            _logger.LogInformation($@"[Listener] Document {id} successfully retrived.");
                            _logger.LogInformation($@"[Listener] OCR Text for Document {id}: {extractedText}");
                            _logger.LogInformation($@"[Listener] Document before Update: {document}");
                            document.OcrText = extractedText;
                            var updateResponse = await client.PutAsJsonAsync($"/api/DAL/document/{id}", document);
                            if (!updateResponse.IsSuccessStatusCode) {
                                _logger.LogError($@"Error while updating Document mit ID {id}");
                            }
                            else {
                                _logger.LogInformation($@"OCR Text for Document {id} successfully updated.");
                            }
                        }
                        else {
                            _logger.LogError($@"[Listener] Document {id} not found.");
                        }
                    }
                    else {
                        _logger.LogError($@"Error at retrieving Document with ID {id}: {response.StatusCode}");
                    }
                }
                else {
                    _logger.LogError(@"Error: invalid message received.");
                }
            };
            _channel.BasicConsume(queue: "ocr_result_queue", autoAck: true, consumer: consumer);
        }
        catch (Exception ex) {
            _logger.LogError($@"Error while starting listener for OCR-Results: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        _channel?.Close();
        _connection?.Close();
        return Task.CompletedTask;
    }
}
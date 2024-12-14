using DocManSys_DAL.Entities;
using DocManSys_RestAPI.Models;
using Elastic.Clients.Elasticsearch;

namespace DocManSys_RestAPI.Services;

public class ElasticsearchService : IElasticsearchService {
    private readonly ElasticsearchClient _elasticClient;
    private readonly ILogger<ElasticsearchService> _logger;

    public ElasticsearchService(ElasticsearchClient elasticClient, ILogger<ElasticsearchService> logger) {
        _elasticClient = elasticClient;
        _logger = logger;
    }

    public async Task<bool> IndexDocumentEntityAsync(DocumentEntity document) {
        try {
            var indexResponse = await _elasticClient.IndexAsync(document,
                i => i.Index("documents").Id(document.Id));
            if (!indexResponse.IsValidResponse) {
                _logger.LogError($"Error indexing document {document.Id}: {indexResponse.DebugInformation}");
                return false;
            }

            _logger.LogInformation($"Document {document.Id} indexed successfully.");
            return true;
        }
        catch (Exception ex) {
            _logger.LogError($"Error while indexing document {document.Id}: {ex.Message}");
            return false;
        }
    }

    public async Task<IndexResponse> IndexDocumentAsync(DocumentEntity document) {
        var indexResponse = await _elasticClient.IndexAsync(document,
            i => i.Index("documents").Id(document.Id));
        if (!indexResponse.IsValidResponse) {
            _logger.LogError($"Error indexing document {document.Id}: {indexResponse.DebugInformation}");
            return indexResponse;
        }

        _logger.LogInformation($"Document {document.Id} indexed successfully.");
        return indexResponse;
    }

    public async Task<DeleteResponse> DeleteDocumentAsync(int documentId) {
        var deleteResponse = await _elasticClient.DeleteAsync<Document>(documentId, i => i.Index("documents"));
        if (deleteResponse.Result == Result.NotFound) {
            _logger.LogError($"Document with ID {documentId} does not exist.");
            return deleteResponse;
        }

        if (!deleteResponse.IsValidResponse) {
            _logger.LogError($"Error deleting document with ID {documentId}: {deleteResponse.DebugInformation}");
            return deleteResponse;
        }

        _logger.LogInformation($"Document with ID{documentId} deleted successfully.");
        return deleteResponse;
    }

    public async Task<SearchResponse<Document>> SearchDocumentsFuzzyAsync(string searchTerm) {
        var response = await _elasticClient.SearchAsync<Document>(s => s
            .Index("documents")
            .Query(q => q.Fuzzy(m => m
                .Field(p => p.Author)
                .Field(p => p.Title)
                .Field(p => p.OcrText)
                .Value(searchTerm)
                .Fuzziness(new Fuzziness(2))
            )));
        return response;
    }

    public async Task<SearchResponse<Document>> SearchDocumentsQueryAsync(string searchTerm) {
        var response = await _elasticClient.SearchAsync<Document>(s => s
            .Index("documents")
            .Query(q => q.QueryString(qs => qs.Query($"*{searchTerm}*"))));
        return response;
    }
}
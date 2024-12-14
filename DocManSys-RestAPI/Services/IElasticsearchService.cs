using DocManSys_DAL.Entities;
using DocManSys_RestAPI.Models;
using Elastic.Clients.Elasticsearch;

namespace DocManSys_RestAPI.Services;

public interface IElasticsearchService {
    public Task<bool> IndexDocumentEntityAsync(DocumentEntity document);
    public Task<IndexResponse> IndexDocumentAsync(DocumentEntity document);
    public Task<DeleteResponse> DeleteDocumentAsync(int documentId);
    public Task<SearchResponse<Document>> SearchDocumentsFuzzyAsync(string searchTerm);
    public Task<SearchResponse<Document>> SearchDocumentsQueryAsync(string searchTerm);

}
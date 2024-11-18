namespace DocManSys_RestAPI.Services;

public interface IMessageQueueService { 
    void SendToQueue(string message);
}
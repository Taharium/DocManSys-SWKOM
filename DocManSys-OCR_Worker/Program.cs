namespace DocManSys_OCR_Worker;

public class Program {
    static void Main(string[] args) {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        var worker = new OcrWorker(configuration);
        worker.Start();
        Console.WriteLine("OCR Worker is running. Press Ctrl+C to exit.");
        while (true) {
            Thread.Sleep(1000);
        }
    }
}
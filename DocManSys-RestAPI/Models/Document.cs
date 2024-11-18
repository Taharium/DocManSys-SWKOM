namespace DocManSys_RestAPI.Models {
    public class Document {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Image { get; set; } = "";
        public string OcrText { get; set; } = "";


        //For the future
        //public string FileName { get; set; } = "";
        //public string FileExtension { get; set; } = "";

    }
}

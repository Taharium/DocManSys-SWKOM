﻿namespace DocManSys_RestAPI.Models {
    public class Document {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FileExtension { get; set; } = "";

    }
}
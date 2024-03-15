namespace Base.DTOs
{
    public class MessageDTO
    {
        public string type { get; set; }
        public string id { get; set; }
        public string text { get; set; }
        
        //For Sticker
        public string stickerId { get; set; }
        public string packageId { get; set; }
        public string stickerResourceType { get; set; }

        //For Image
        public string originalContentUrl { get; set; }
        public string previewImageUrl { get; set; }
        public string ImageBase64 { get; set; }

        //For File
        public string fileName { get; set; }
    }
}
using System.Collections.Generic;

namespace Base.DTOs
{
    public class SFRecordResponse
    {
        //public int totalSize { get; set; }
        public List<records> records;
        public string nextRecordsUrl;
    }

    public class records
    {
        public string id { get; set; }
        public string name { get; set; }
        public string Status__c { get; set; }
        public string Line_UserID__c { get; set; }
        public string ContentDocumentId {  get; set; }
    }
}
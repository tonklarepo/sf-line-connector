using System;

namespace Base.DTOs
{
    public class CustomerDTO
    {
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalPoints { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string SalesType { get; set; }
        public string PrivilegeType { get; set; }
    }
}
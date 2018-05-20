using System;

namespace WebApplication7.Models
{
    public class JTable
    {
        public string search { get; set; }
        public string columnOrder { get; set; }
        public int currentPage { get; set; }
        public int numberPage { get; set; }
        public int totalItem { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CHOJ.Models
{
    public class Group
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int QuestionCount { get; set; }
        public int Order { get; set; }
        public int Type { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}

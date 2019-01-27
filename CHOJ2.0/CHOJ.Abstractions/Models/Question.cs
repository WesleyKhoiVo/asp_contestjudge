using System;

namespace CHOJ.Models
{
    public class Question
    {
        public string Id { get; set; }
        public string  Title { get; set; }
        public string Body { get; set; }
        public int TimeLimit { get; set; }
        public int MemoryLimit{get;set;}
        public int AcceptedCount { get; set; }
        public int SubmitCount { get; set; }
        public string  UserId { get; set; }
        public DateTime AddTime { get; set; }
        public string  GroupId { get; set; }
        public bool IsTrue { get; set; }
        public string  Test { get; set; }
    }
}
using System;

namespace CHOJ.Models
{
    public class Answer
    {
        public string Id { get; set; }

        public int UseMemory { get; set; }
        public int UseTime { get; set; }
        public int Type { get; set; }
        public string Complier { get; set; }
        public DateTime AddTime { get; set; }
        public int Status { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string QuestionTitle { get; set; }
        public string QuestionId { get; set; }

        public string  Code { get; set; }
    }
}
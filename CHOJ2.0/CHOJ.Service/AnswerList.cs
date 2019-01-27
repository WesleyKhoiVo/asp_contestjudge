using System.Collections.Generic;
using CHOJ.Models;

namespace CHOJ
{
    static public class AnswerList
    {
        public  static Dictionary<string, Answer> Answers { get; set; }

        static AnswerList()
        {
            Answers = new Dictionary<string, Answer>();
        }
    }
}
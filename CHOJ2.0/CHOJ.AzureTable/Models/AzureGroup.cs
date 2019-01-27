using System.Data.Services.Common;
using CHOJ.Models;

namespace CHOJ.AzureTable.Models
{
    [DataServiceKey("Id","Kind")]
    public class AzureGroup : Group
    {
        public string Kind { get; set; }
        public AzureGroup()
        {
            Kind = "Group";
        }
    }
}
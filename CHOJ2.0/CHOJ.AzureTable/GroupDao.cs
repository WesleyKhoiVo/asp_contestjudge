using System;
using System.Collections.Generic;
using CHOJ.Abstractions;
using System.Linq;
using CHOJ.AzureTable.Models;
using CHOJ.Models;


namespace CHOJ.AzureTable
{
    public class GroupDao:BaseDao ,IGroupDao
    {
        public IEnumerable<Group> GroupList()
        {
            
            return DbContext.CreateQuery<AzureGroup>("Group").Select(c=>c).Cast<Group>().ToList();
        }

        public Group GetGroup(string id)
        {
            return DbContext.CreateQuery<Group>("Group").SingleOrDefault(c => c.Id == id);
        }

        public void Add(Group group)
        {
            
            group.BeginTime = DateTime.Now;
            group.EndTime = DateTime.Now;
            group.Id = Guid.NewGuid().ToString();
            DbContext.AddObject("Group", group);
            DbContext.SaveChanges();
        }

        public void Delete(string id)
        {
            DbContext.DeleteObject(GetGroup(id));
            DbContext.SaveChanges();
        }
    }
}
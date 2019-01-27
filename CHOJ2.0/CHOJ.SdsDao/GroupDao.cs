using System;
using System.Collections.Generic;
using CHOJ.Abstractions;
using CHOJ.Models;
using Microsoft.Samples.Cloud.Data;
using System.Linq;
namespace CHOJ.SdsDao
{
    public class GroupDao:BaseDao ,IGroupDao
    {
        public IEnumerable<Group> GroupList()
        {
            SsdsContainer c1 = DbContext.OpenContainer("Group");
            return c1.Query<Group>(c => true).Select(c => c.Entity);
        }

        public Group GetGroup(string id)
        {
            SsdsContainer c1 = DbContext.OpenContainer("Group");
            return c1.Single<Group>(c => c.Entity.Id == id).Entity;
        }

        public void Add(Group group)
        {
            SsdsContainer c1 = DbContext.OpenContainer("Group");
            group.BeginTime = DateTime.Now;
            group.EndTime = DateTime.Now;
            group.Id = Guid.NewGuid().ToString();
            c1.Insert(group, group.Id);
        }

        public void Delete(string id)
        {
            SsdsContainer c1 = DbContext.OpenContainer("Group");
            c1.Delete(id);
        }
    }
}
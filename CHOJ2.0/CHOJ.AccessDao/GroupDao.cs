using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CHOJ.Abstractions;

using System.Data;
using CHOJ.Models;

namespace CHOJ.AccessDao
{
    public class GroupDao:BaseSqlMapDao,IGroupDao {
        public IEnumerable<Group> GroupList() {
            var rows = GroupTable().AsEnumerable().Where(c => c.Type == 0).OrderBy(c => c.Order).ToList();
            return rows;
        }
        public Group GetGroup(Guid id) {
            var rets = GroupTable().AsEnumerable().FirstOrDefault(c => c.Id == id);
            if (rets != null) return rets;
            return null;
        }

        public void Add(Group group)
        {
            throw new NotImplementedException();
        }

        IList<Group> GroupTable()
        {
            return ExecuteQueryForList<Group>("AllGroup", null);
        }
    }
}
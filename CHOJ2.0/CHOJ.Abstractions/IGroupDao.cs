using System;
using System.Collections.Generic;
using CHOJ.Models;

namespace CHOJ.Abstractions
{
    public interface IGroupDao
    {
        IEnumerable<Group> GroupList();
        Group GetGroup(string  id);
        void Add(Group group);
        void Delete(string id);
    }
}
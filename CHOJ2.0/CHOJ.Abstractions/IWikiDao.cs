using System;
using System.Collections.Generic;
using CHOJ.Models;

namespace CHOJ.Abstractions
{
    public interface IWikiDao
    {
        void Add(Wiki wiki);
        void Update(Wiki wiki);
        void Delete(string id);
        Wiki Get(string title);
        Wiki GetById(string id);
        IEnumerable<Wiki> List();
    }
}
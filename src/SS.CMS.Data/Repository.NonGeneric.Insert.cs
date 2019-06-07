﻿using System.Threading.Tasks;
using SS.CMS.Data.Utils;

namespace SS.CMS.Data
{
    public partial class Repository
    {
        public virtual int Insert<T>(T dataInfo) where T : Entity
        {
            return RepositoryUtils.InsertObject(DbContext, TableName, TableColumns, dataInfo);
        }

        public virtual async Task<int> InsertAsync<T>(T dataInfo) where T : Entity
        {
            return await RepositoryUtils.InsertObjectAsync(DbContext, TableName, TableColumns, dataInfo);
        }
    }
}
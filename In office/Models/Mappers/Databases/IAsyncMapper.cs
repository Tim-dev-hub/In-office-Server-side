using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using In_office.Models.Types;

namespace In_office.Models.Data.Mappers
{
    public interface IAsyncMapper<T> where T : BaseServerType
    {
        public abstract Task<object> GetAsync(long id);
        public abstract Task SaveAsync(T user);
        public abstract Task ChangeAsync(T original, T alternative);
        public abstract Task DeleteAsync(T deletable);
    }
}

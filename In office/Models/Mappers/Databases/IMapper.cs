using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using In_office.Models.Types;

namespace In_office.Models.Data.Mappers
{
    public interface IMapper
    {
        public abstract User Get(long id);
        public abstract void Save(User user);
        public abstract void Change(User original, User alternative);
        public abstract void Delete(User deletable);
    }
}

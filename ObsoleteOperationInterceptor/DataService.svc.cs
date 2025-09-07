using ObsoleteOperationInterceptor.DTO;
using System;

namespace ObsoleteOperationInterceptor
{
    public class DataService : IDataService
    {
        //[Obsolete("2025-09-05", true)]
        public CustomObject GetObsolete(string Id)
        {
            return new CustomObject { Id = Id, Name = "Obsolete Object" };
        }

        public CustomObject GetActive(string Id)
        {
            return new CustomObject { Id = Id, Name = "Active Object" };
        }
    }
}

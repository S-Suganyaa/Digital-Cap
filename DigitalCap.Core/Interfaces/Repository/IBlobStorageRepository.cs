using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IBlobStorageRepository
    {
        Task DeleteFile(string id);
    }
}

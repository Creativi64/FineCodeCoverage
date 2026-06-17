using System;
using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.Ms
{
    internal interface IVsRunSettingsWriter
    {
        Task<bool> RemoveRunSettingsFilePathAsync(Guid projectGuid);
    }
}

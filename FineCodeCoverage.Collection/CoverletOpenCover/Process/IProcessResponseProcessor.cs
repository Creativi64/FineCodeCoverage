using System;
using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Process
{
    internal interface IProcessResponseProcessor
    {
        Task<bool> ProcessAsync(ExecuteResponse executeResponse, Func<int, bool> exitCodeSuccessPredicate, bool throwError, string title, Action successCallback = null);
    }
}

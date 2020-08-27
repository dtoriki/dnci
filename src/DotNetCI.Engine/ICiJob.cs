using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCI.Engine
{
    public interface ICiJob
    {
        string Name { get; }
        string? Description { get; }

        Task ExecuteJobAsync(); 
    }
}

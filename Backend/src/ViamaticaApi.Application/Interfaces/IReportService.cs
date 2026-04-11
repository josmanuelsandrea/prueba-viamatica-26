using System;
using System.Collections.Generic;
using System.Text;

namespace ViamaticaApi.Application.Interfaces
{
    public interface IReportService
    {
        Task<string> GetClientesConContratosVigentesAsync();
    }
}

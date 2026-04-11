using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetClientesConContratosVigentesAsync()
        {
            var activeContracts = await _context.Clients
                .Include(c => c.Contracts)
                .ThenInclude(c => c.Service)
                .Where(c => c.Contracts.Any(ct => ct.StatuscontractStatusid == "VIG"))
                .ToListAsync();

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='es'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>Reporte de Clientes con Contratos Vigentes</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine("h1 { color: #333; }");
            sb.AppendLine("h2 { color: #555; margin-top: 30px; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }");
            sb.AppendLine("th { background-color: #4CAF50; color: white; padding: 8px; text-align: left; }");
            sb.AppendLine("td { border: 1px solid #ddd; padding: 8px; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
            sb.AppendLine(".cliente-info { background-color: #f9f9f9; padding: 10px; border-left: 4px solid #4CAF50; margin-bottom: 10px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1>Reporte de Clientes con Contratos Vigentes</h1>");
            sb.AppendLine($"<p>Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}</p>");

            foreach (var cliente in activeContracts)
            {
                sb.AppendLine("<div class='cliente-info'>");
                sb.AppendLine($"<h2>{cliente.Name} {cliente.Lastname}</h2>");
                sb.AppendLine($"<p><strong>Identificación:</strong> {cliente.Identification}</p>");
                sb.AppendLine($"<p><strong>Email:</strong> {cliente.Email}</p>");
                sb.AppendLine($"<p><strong>Teléfono:</strong> {cliente.Phonenumber}</p>");
                sb.AppendLine("</div>");

                sb.AppendLine("<table>");
                sb.AppendLine("<thead>");
                sb.AppendLine("<tr><th>ID Contrato</th><th>Servicio</th><th>Descripción</th><th>Precio</th><th>Fecha Inicio</th><th>Fecha Fin</th><th>Estado</th></tr>");
                sb.AppendLine("</thead>");
                sb.AppendLine("<tbody>");

                foreach (var contrato in cliente.Contracts.Where(ct => ct.StatuscontractStatusid == "VIG"))
                {
                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td>{contrato.Contractid}</td>");
                    sb.AppendLine($"<td>{contrato.Service.Servicename}</td>");
                    sb.AppendLine($"<td>{contrato.Service.Servicedescription}</td>");
                    sb.AppendLine($"<td>${contrato.Service.Price:F2}</td>");
                    sb.AppendLine($"<td>{contrato.Startdate:dd/MM/yyyy}</td>");
                    sb.AppendLine($"<td>{(contrato.Enddate.HasValue ? contrato.Enddate.Value.ToString("dd/MM/yyyy") : "Indefinido")}</td>");
                    sb.AppendLine($"<td>{contrato.StatuscontractStatusid}</td>");
                    sb.AppendLine("</tr>");
                }

                sb.AppendLine("</tbody>");
                sb.AppendLine("</table>");
            }

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}

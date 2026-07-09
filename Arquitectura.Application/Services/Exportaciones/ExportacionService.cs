using System.Data;
using System.Text;
using System.Text.Json;
using Arquitectura.Application.DTOs.Exportaciones;
using Arquitectura.Application.Interfaces.Exportaciones;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arquitectura.Application.Services.Exportaciones;

public class ExportacionService : IExportacionService
{
    private readonly ArquitecturaDbContext _context;

    public ExportacionService(ArquitecturaDbContext context)
    {
        _context = context;
    }

    public async Task<ExportacionArchivoDto> ExportarUsuariosAsync(string formato)
    {
        return await ExportarConsultaAsync(
            "usuarios",
            """
            SELECT Id, Nombre, Apellidos, Email, Telefono, Puesto, Salario, Estado, FechaContratacion
            FROM Usuario
            WHERE ISNULL(Estado, '') <> 'Baja'
            ORDER BY Id
            """,
            formato);
    }

    public async Task<ExportacionArchivoDto> ExportarProyectosAsync(string formato)
    {
        return await ExportarConsultaAsync(
            "proyectos",
            """
            SELECT *
            FROM Proyectos
            ORDER BY Id
            """,
            formato);
    }

    public async Task<ExportacionArchivoDto> ExportarTareasAsync(string formato)
    {
        return await ExportarConsultaAsync(
            "tareas",
            """
            SELECT *
            FROM Tareas
            ORDER BY Id
            """,
            formato);
    }

    public async Task<ExportacionArchivoDto> ExportarTransaccionesAsync(string formato)
    {
        return await ExportarConsultaAsync(
            "transacciones",
            """
            SELECT Id, Tipo, CategoriaId, Monto, Descripcion, Fecha, UsuarioId, FechaRegistro, Activo
            FROM Transacciones
            WHERE Activo = 1
            ORDER BY Id DESC
            """,
            formato);
    }

    public async Task<ExportacionArchivoDto> ExportarNominaAsync(string formato)
    {
        return await ExportarConsultaAsync(
            "nomina",
            """
            SELECT *
            FROM Nomina
            ORDER BY Id DESC
            """,
            formato);
    }

    private async Task<ExportacionArchivoDto> ExportarConsultaAsync(
        string nombreBase,
        string sql,
        string formato)
    {
        formato = NormalizarFormato(formato);

        var datos = await EjecutarConsultaAsync(sql);

        return formato switch
        {
            "json" => CrearJson(nombreBase, datos),
            "txt" => CrearTxt(nombreBase, datos),
            _ => CrearCsv(nombreBase, datos)
        };
    }

    private async Task<List<Dictionary<string, object?>>> EjecutarConsultaAsync(string sql)
    {
        var resultado = new List<Dictionary<string, object?>>();

        var connection = _context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var fila = new Dictionary<string, object?>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var nombreColumna = reader.GetName(i);
                var valor = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);

                fila[nombreColumna] = valor;
            }

            resultado.Add(fila);
        }

        return resultado;
    }

    private static string NormalizarFormato(string formato)
    {
        var formatoNormalizado = string.IsNullOrWhiteSpace(formato)
            ? "csv"
            : formato.Trim().ToLower();

        if (formatoNormalizado != "csv" &&
            formatoNormalizado != "json" &&
            formatoNormalizado != "txt")
        {
            throw new Exception("Formato no válido. Use csv, json o txt.");
        }

        return formatoNormalizado;
    }

    private static ExportacionArchivoDto CrearJson(
        string nombreBase,
        List<Dictionary<string, object?>> datos)
    {
        var json = JsonSerializer.Serialize(datos, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return CrearArchivo(
            $"{nombreBase}_{DateTime.Now:yyyyMMdd_HHmmss}.json",
            "application/json",
            json);
    }

    private static ExportacionArchivoDto CrearTxt(
        string nombreBase,
        List<Dictionary<string, object?>> datos)
    {
        var txt = new StringBuilder();

        txt.AppendLine($"EXPORTACIÓN: {nombreBase.ToUpper()}");
        txt.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        txt.AppendLine("========================================");
        txt.AppendLine();

        if (!datos.Any())
        {
            txt.AppendLine("No hay datos para exportar.");
        }

        foreach (var fila in datos)
        {
            foreach (var columna in fila)
            {
                txt.AppendLine($"{columna.Key}: {columna.Value}");
            }

            txt.AppendLine("----------------------------------------");
        }

        return CrearArchivo(
            $"{nombreBase}_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
            "text/plain",
            txt.ToString());
    }

    private static ExportacionArchivoDto CrearCsv(
        string nombreBase,
        List<Dictionary<string, object?>> datos)
    {
        var csv = new StringBuilder();

        if (!datos.Any())
        {
            csv.AppendLine("Sin datos");
        }
        else
        {
            var columnas = datos.First().Keys.ToList();

            csv.AppendLine(string.Join(";", columnas.Select(CampoCsv)));

            foreach (var fila in datos)
            {
                var valores = columnas.Select(c =>
                    CampoCsv(fila[c]?.ToString()));

                csv.AppendLine(string.Join(";", valores));
            }
        }

        return CrearArchivo(
            $"{nombreBase}_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            "text/csv",
            csv.ToString());
    }

    private static ExportacionArchivoDto CrearArchivo(
        string nombreArchivo,
        string contentType,
        string contenido)
    {
        return new ExportacionArchivoDto
        {
            NombreArchivo = nombreArchivo,
            ContentType = contentType,
            Contenido = Encoding.UTF8.GetBytes(contenido)
        };
    }

    private static string CampoCsv(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return "";

        var limpio = valor.Replace("\"", "\"\"");

        return $"\"{limpio}\"";
    }
}
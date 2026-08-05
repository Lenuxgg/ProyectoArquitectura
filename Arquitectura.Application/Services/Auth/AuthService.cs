using Arquitectura.Application.DTOs.Auth;
using Arquitectura.Application.Interfaces.Auth;
using Arquitectura.Domain.Entities;
using Arquitectura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Arquitectura.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ArquitecturaDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        ArquitecturaDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    private static string HashPassword(string password)
    {
        var passwordLimpio = (password ?? string.Empty).Trim();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(passwordLimpio));
        return Convert.ToHexString(bytes);
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
        var password = (dto.Password ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var usuario = await _context.Usuario
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Roles)
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == email &&
                u.Estado != "Baja");

        if (usuario == null)
            return null;

        var hashEsperado = HashPassword(password);
        var hashActual = (usuario.PasswordHash ?? string.Empty).Trim();

        if (!string.Equals(hashActual, hashEsperado, StringComparison.OrdinalIgnoreCase))
            return null;

        var rol = usuario.UserRoles
            .Select(r => r.Roles.Nombre)
            .FirstOrDefault() ?? "Empleado";

        var token = GenerarToken(usuario, rol);

        return new LoginResponseDto
        {
            Token = token,
            UsuarioId = usuario.Id,
            Nombre = $"{usuario.Nombre} {usuario.Apellidos}",
            Email = usuario.Email,
            Rol = rol,
            EsAdministrador = string.Equals(rol, "Administrador", StringComparison.OrdinalIgnoreCase) || usuario.Admin
        };
    }

    private string GenerarToken(Usuario usuario, string rol)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, rol)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(4),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

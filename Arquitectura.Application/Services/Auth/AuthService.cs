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
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var usuario = await _context.Usuario
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Roles)
            .FirstOrDefaultAsync(u =>
                u.Email == dto.Email &&
                u.Estado != "Baja");

        if (usuario == null)
            return null;

        if (usuario.PasswordHash != HashPassword(dto.Password))
            return null;

        var rol = usuario.UserRoles
            .Select(r => r.Roles.Nombre)
            .FirstOrDefault() ?? "Empleado";

        var token = GenerarToken(usuario, rol);

        return new LoginResponseDto
        {
            Token = token,
            Nombre = $"{usuario.Nombre} {usuario.Apellidos}",
            Rol = rol
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
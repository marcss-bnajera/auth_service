using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Exceptions;
using AuthService.Application.Extensions;
using AuthService.Application.Validators;
using AuthService.Domain.Constants;
using AuthService.Domain.Entitis;
using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AuthService.Application.DTOs.Email;

namespace AuthService.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService,
    ICloudinaryService cloudinaryService,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        if (await userRepository.ExistsByEmailAsync(registerDto.Email))
            throw new BusinessException("EMAIL_ALREADY_EXISTS", "Email already exists");

        if (await userRepository.ExistsByUsernameAsync(registerDto.Username))
            throw new BusinessException("USERNAME_ALREADY_EXISTS", "Username already exists");

        string profilePicturePath;
        if (registerDto.ProfilePicture != null && registerDto.ProfilePicture.Length > 0)
        {
            using var ms = new MemoryStream();
            await registerDto.ProfilePicture.CopyToAsync(ms);

            var fileData = new InternalFileData
            {
                Data = ms.ToArray(),
                FileName = registerDto.ProfilePicture.FileName,
                ContentType = registerDto.ProfilePicture.ContentType,
                Size = registerDto.ProfilePicture.Length
            };

            profilePicturePath = await cloudinaryService.UploadImageAsync(fileData, FileValidator.GenerateSecureFileName(fileData.FileName));
        }
        else
        {
            profilePicturePath = cloudinaryService.GetDefaultAvatarUrl();
        }

        var emailVerificationToken = TokenGenerator.GenerateEmailVerificationToken();
        var userId = UuidGenerator.GenerateUserId();

        var defaultRole = await roleRepository.GetByNameAsync(RoleConstants.USER_ROLE);
        
        if (defaultRole == null)
        {
            logger.LogError("Critical Error: Role {RoleName} not found in database.", RoleConstants.USER_ROLE);
            throw new BusinessException("SYSTEM_CONFIGURATION_ERROR", "El rol predeterminado no está configurado.");
        }

        var user = new User
        {
            Id = userId,
            Name = registerDto.Name,
            SurName = registerDto.Surname,
            UserName = registerDto.Username,
            Email = registerDto.Email.ToLowerInvariant(),
            Password = passwordHashService.HashPassword(registerDto.Password),
            Status = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Profile = new UserProfile
            {
                Id = UuidGenerator.GenerateUserId(),
                UserId = userId,
                ProfilePicture = profilePicturePath,
                Phone = registerDto.Phone
            },
            UserEmail = new UserEmail
            {
                Id = UuidGenerator.GenerateUserId(),
                UserId = userId,
                EmailVerified = false,
                EmailVerificationToken = emailVerificationToken,
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            },
            UserRoles = new List<UserRole> 
            { 
                new UserRole 
                { 
                    Id = UuidGenerator.GenerateUserId(),
                    UserId = userId, 
                    RoleId = defaultRole.Id 
                } 
            }
        };

        var createdUser = await userRepository.CreateAsync(user);
        
        try 
        {
            await emailService.SendEmailAsync(
                createdUser.Email, 
                "Verificación de cuenta", 
                $"Tu código de verificación es: {emailVerificationToken}"
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not send verification email to {Email}", createdUser.Email);
        }

        return new RegisterResponseDto { Success = true, User = MapToUserResponseDto(createdUser) };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = loginDto.EmailOrUsername.Contains('@') 
            ? await userRepository.GetByEmailAsync(loginDto.EmailOrUsername.ToLowerInvariant())
            : await userRepository.GetByUsernameAsync(loginDto.EmailOrUsername);

        if (user == null || !passwordHashService.VerifyPassword(loginDto.Password, user.Password))
            throw new BusinessException("INVALID_CREDENTIALS", "Credenciales inválidas");

        var token = jwtTokenService.GenerateToken(user);
        return new AuthResponseDto { Success = true, Token = token, UserDetails = MapToUserDetailsDto(user) };
    }

    private UserResponseDto MapToUserResponseDto(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Surname = user.SurName,
        Username = user.UserName,
        Email = user.Email,
        ProfilePicture = user.Profile != null ? cloudinaryService.GetFullImageUrl(user.Profile.ProfilePicture) : "", 
        Phone = user.Profile?.Phone ?? "",
        Role = user.UserRoles?.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE,
        Status = user.Status,
        IsEmailVerified = user.UserEmail?.EmailVerified ?? false,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };

    private UserDetailsDto MapToUserDetailsDto(User user) => new()
    {
        Id = user.Id,
        Username = user.UserName,
        Role = user.UserRoles?.FirstOrDefault()?.Role?.Name ?? RoleConstants.USER_ROLE
    };

    public async Task<EmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
    {
        var user = await userRepository.GetByEmailVerificationTokenAsync(verifyEmailDto.Token);
        if (user == null || user.UserEmail == null) throw new BusinessException("INVALID_TOKEN", "Token inválido");

        user.UserEmail.EmailVerified = true;
        user.Status = true;
        await userRepository.UpdateAsync(user);
        return new EmailResponseDto { Success = true, Message = "Email verificado" };
    }

    public async Task<EmailResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto)
    {
        var user = await userRepository.GetByEmailAsync(resendDto.Email);
        if (user == null) return new EmailResponseDto { Success = false, Message = "Usuario no encontrado" };

        await emailService.SendEmailAsync(
            user.Email, 
            "Verificación de cuenta", 
            $"Tu código de verificación es: {user.UserEmail?.EmailVerificationToken}"
        );

        return new EmailResponseDto { Success = true, Message = "Email enviado" };
    }

    public async Task<EmailResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await userRepository.GetByEmailAsync(forgotPasswordDto.Email);
        if (user == null) return new EmailResponseDto { Success = true, Message = "Si el correo existe, se enviará un enlace" };

        var clientUrl = configuration["AppSettings:ClientUrl"] ?? "http://localhost:3000";
        
        await emailService.SendEmailAsync(
            user.Email, 
            "Recuperación de contraseña", 
            $"Para restablecer tu contraseña, haz clic aquí: {clientUrl}/reset-password"
        );

        return new EmailResponseDto { Success = true, Message = "Enlace enviado" };
    }

    public async Task<EmailResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        await Task.CompletedTask;
        return new EmailResponseDto { Success = true, Message = "Contraseña actualizada" };
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(string userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return user == null ? null : MapToUserResponseDto(user);
    }

    // CLASE PRIVADA PARA MAPEAR IFileData SIN CAMBIAR EL SERVICIO DE CLOUDINARY
    private class InternalFileData : IFileData
    {
        public byte[] Data { get; init; } = [];
        public string ContentType { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public long Size { get; init; }
    }
}
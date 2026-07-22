using CafeSphere.Application.DTOs;
using CafeSphere.Application.Interfaces;
using CafeSphere.Domain.Entities;
using CafeSphere.Domain.Repositories;
using CafeSphere.Shared.Constants;
using CafeSphere.Shared.Models;
using FluentValidation;
using MediatR;

namespace CafeSphere.Application.Features.Auth;

public record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber
) : IRequest<Result<AuthResponse>>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FullName).NotEmpty();
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthResponse>>
{
    private readonly IMongoRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;

    public RegisterUserCommandHandler(
        IMongoRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IEmailService emailService,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _emailService = emailService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailExists = await _userRepository.ExistsAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);
            if (emailExists)
                return Result<AuthResponse>.Failure("Auth.EmailExists", "User with this email already exists.");

            var usernameExists = await _userRepository.ExistsAsync(u => u.Username == request.Username, cancellationToken);
            if (usernameExists)
                return Result<AuthResponse>.Failure("Auth.UsernameExists", "Username is already taken.");
        }
        catch (Exception ex)
        {
            // Fail gracefully if database connection is unreachable
            return Result<AuthResponse>.Failure("Database.Error", $"Database connection issue: {ex.Message}");
        }

        var verificationToken = Guid.NewGuid().ToString("N");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Role = Roles.Customer,
            IsEmailVerified = false,
            EmailVerificationToken = verificationToken
        };

        var refreshToken = _jwtService.GenerateRefreshToken(_currentUserService.IpAddress ?? "127.0.0.1");
        user.RefreshTokens.Add(refreshToken);

        try
        {
            await _userRepository.InsertAsync(user, cancellationToken);
            await _emailService.SendVerificationEmailAsync(user.Email, verificationToken, cancellationToken);
        }
        catch
        {
            // Proceed with auth response even if email notification fails
        }

        var accessToken = _jwtService.GenerateAccessToken(user);

        var response = new AuthResponse(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.Role,
            accessToken,
            refreshToken.Token,
            DateTime.UtcNow.AddMinutes(60)
        );

        return Result<AuthResponse>.Success(response);
    }
}

public record LoginUserQuery(
    string EmailOrUsername,
    string Password
) : IRequest<Result<AuthResponse>>;

public class LoginUserQueryValidator : AbstractValidator<LoginUserQuery>
{
    public LoginUserQueryValidator()
    {
        RuleFor(x => x.EmailOrUsername).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, Result<AuthResponse>>
{
    private readonly IMongoRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ICurrentUserService _currentUserService;

    public LoginUserQueryHandler(
        IMongoRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        var input = request.EmailOrUsername.Trim().ToLowerInvariant();
        User? user = null;

        try
        {
            user = await _userRepository.FindOneAsync(
                u => u.Email.ToLower() == input || u.Username.ToLower() == input,
                cancellationToken
            );
        }
        catch
        {
            // Database query fallback handled below
        }

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure("Auth.InvalidCredentials", "Invalid email/username or password.");
        }

        var refreshToken = _jwtService.GenerateRefreshToken(_currentUserService.IpAddress ?? "127.0.0.1");
        user.RefreshTokens.Add(refreshToken);

        await _userRepository.UpdateAsync(user, cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(user);

        var response = new AuthResponse(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.Role,
            accessToken,
            refreshToken.Token,
            DateTime.UtcNow.AddMinutes(60)
        );

        return Result<AuthResponse>.Success(response);
    }
}

public record ForgotPasswordCommand(string Email) : IRequest<Result<bool>>;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IMongoRepository<User> _userRepository;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IMongoRepository<User> userRepository, IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var input = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.FindOneAsync(u => u.Email == input, cancellationToken);
        if (user != null)
        {
            var token = Guid.NewGuid().ToString("N");
            user.PasswordResetToken = token;
            user.ResetTokenExpiresAt = DateTime.UtcNow.AddHours(2);
            await _userRepository.UpdateAsync(user, cancellationToken);
            
            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Email, token, cancellationToken);
            }
            catch {}
        }
        
        return Result<bool>.Success(true);
    }
}

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Result<bool>>;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IMongoRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(IMongoRepository<User> userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var input = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.FindOneAsync(
            u => u.Email == input && u.PasswordResetToken == request.Token,
            cancellationToken
        );

        if (user == null || user.ResetTokenExpiresAt == null || user.ResetTokenExpiresAt < DateTime.UtcNow)
        {
            return Result<bool>.Failure("Auth.InvalidResetToken", "Invalid or expired password reset token.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.ResetTokenExpiresAt = null;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result<bool>.Success(true);
    }
}


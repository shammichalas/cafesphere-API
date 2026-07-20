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
        var emailExists = await _userRepository.ExistsAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);
        if (emailExists)
            return Result<AuthResponse>.Failure("Auth.EmailExists", "User with this email already exists.");

        var usernameExists = await _userRepository.ExistsAsync(u => u.Username == request.Username, cancellationToken);
        if (usernameExists)
            return Result<AuthResponse>.Failure("Auth.UsernameExists", "Username is already taken.");

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

        await _userRepository.InsertAsync(user, cancellationToken);

        await _emailService.SendVerificationEmailAsync(user.Email, verificationToken, cancellationToken);

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
        var input = request.EmailOrUsername.ToLowerInvariant();
        var user = await _userRepository.FindOneAsync(
            u => u.Email == input || u.Username.ToLower() == input,
            cancellationToken
        );

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

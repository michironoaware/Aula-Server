using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Aula.Server.Domain.AccessControl;
using Aula.Server.Domain.Users;
using Aula.Server.Shared.Persistence;
using Aula.Server.Shared.Snowflakes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aula.Server.Shared.Identity;

internal sealed class UserManager
{
	private const String UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private const String LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";
	private const String Digits = "0123456789";

	private static readonly ConcurrentDictionary<Snowflake, PendingEmailConfirmation> s_pendingEmailConfirmations = [ ];
	private static readonly ConcurrentDictionary<Snowflake, PendingPasswordReset> s_pendingPasswordResets = [ ];
	private static readonly TimeSpan s_pendingEmailConfirmationsLifeTime = TimeSpan.FromMinutes(15);
	private static readonly TimeSpan s_pendingPasswordResetsLifeTime = TimeSpan.FromMinutes(15);
	private readonly List<User> _cachedUsers = [ ];
	private readonly AppDbContext _dbContext;
	private readonly IPasswordHasher<User> _passwordHasher;
	private Role? _cachedGlobalRole;

	public UserManager(
		AppDbContext dbContext,
		IPasswordHasher<User> passwordHasher,
		IOptions<IdentityOptions> identityOptions)
	{
		_dbContext = dbContext;
		_passwordHasher = passwordHasher;
		Options = identityOptions.Value;
	}

	internal IdentityOptions Options { get; }

	internal static Int32 ClearExpiredEmailConfirmations()
	{
		var confirmationsCleared = 0;
		var now = DateTime.UtcNow;
		foreach (var pendingEmailConfirmation in s_pendingEmailConfirmations)
		{
			if (now - pendingEmailConfirmation.Value.CreationDate > s_pendingEmailConfirmationsLifeTime)
			{
				_ = s_pendingEmailConfirmations.TryRemove(pendingEmailConfirmation.Key, out _);
				confirmationsCleared++;
			}
		}

		return confirmationsCleared;
	}

	internal static Int32 ClearExpiredPasswordResets()
	{
		var resetsCleared = 0;
		var now = DateTime.UtcNow;
		foreach (var pendingPasswordReset in s_pendingPasswordResets)
		{
			if (now - pendingPasswordReset.Value.CreationDate > s_pendingPasswordResetsLifeTime)
			{
				_ = s_pendingEmailConfirmations.TryRemove(pendingPasswordReset.Key, out _);
				resetsCleared++;
			}
		}

		return resetsCleared;
	}

	[SuppressMessage("Performance", "CA1822:Mark members as static",
		Justification = "Should be used through Dependency Injection")]
	internal Snowflake? GetUserId(ClaimsPrincipal user)
	{
		var idClaimValue = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
		if (idClaimValue is null ||
		    !Snowflake.TryParse(idClaimValue, out var id))
			return null;

		return id;
	}

	internal async ValueTask<User?> GetUserAsync(ClaimsPrincipal user)
	{
		var id = GetUserId(user);
		if (id is null)
			return null;

		return await FindByIdAsync((Snowflake)id);
	}

	internal async ValueTask<User?> FindByIdAsync(Snowflake userId)
	{
		var user = _cachedUsers.FirstOrDefault(u => u.Id == userId);
		if (user is not null)
			return user;

		user = await _dbContext.Users
			.AsNoTracking()
			.Where(u => u.Id == userId)
			.Include(u => u.RoleAssignments)
			.FirstOrDefaultAsync();

		if (user is not null)
			_cachedUsers.Add(user);

		return user;
	}

	internal async ValueTask<StandardUser?> FindByEmailAsync(String email)
	{
		var user = _cachedUsers
			.OfType<StandardUser>()
			.FirstOrDefault(u => u.Email == email);
		if (user is not null)
			return user;

		user = await _dbContext.Users
			.AsNoTracking()
			.OfType<StandardUser>()
			.Where(u => u.Email == email)
			.FirstOrDefaultAsync();

		if (user is not null)
			_cachedUsers.Add(user);

		return user;
	}

	internal async ValueTask<StandardUser?> FindByUserNameAsync(String userName)
	{
		var user = _cachedUsers
			.OfType<StandardUser>()
			.FirstOrDefault(u => u.UserName == userName);
		if (user is not null)
			return user;

		user = await _dbContext.Users
			.AsNoTracking()
			.OfType<StandardUser>()
			.Where(u => u.UserName == userName)
			.FirstOrDefaultAsync();

		if (user is not null)
			_cachedUsers.Add(user);

		return user;
	}

	internal async Task<RegisterUserResult> RegisterAsync(StandardUser user)
	{
		if (await _dbContext.Users
			    .OfType<StandardUser>()
			    .AnyAsync(u => u.Email == user.Email))
			return RegisterUserResult.EmailInUse;

		if (await _dbContext.Users
			    .OfType<StandardUser>()
			    .AnyAsync(u => u.UserName == user.UserName))
			return RegisterUserResult.UserNameInUse;

		if (user.UserName.Any(c => !StandardUser.UserNameValidChars.Contains(c)))
			return RegisterUserResult.InvalidUserNameChar;

		_ = _dbContext.Users.Add(user);
		_ = await _dbContext.SaveChangesAsync();
		return RegisterUserResult.Success;
	}

	[SuppressMessage("Performance", "CA1822:Mark members as static",
		Justification = "Should be used through Dependency Injection")]
	internal String GenerateEmailConfirmationToken(StandardUser user)
	{
		var emailConfirmation = new PendingEmailConfirmation();
		_ = s_pendingEmailConfirmations.AddOrUpdate(
			user.Id,
			static (_, e) => e,
			static (_, _, e) => e,
			emailConfirmation);
		return emailConfirmation.Token;
	}

	internal async ValueTask<Boolean> ConfirmEmailAsync(StandardUser user, String token)
	{
		if (!s_pendingEmailConfirmations.TryGetValue(user.Id, out var emailConfirmation) ||
		    emailConfirmation.Token != token ||
		    DateTime.UtcNow - emailConfirmation.CreationDate > s_pendingEmailConfirmationsLifeTime)
			return false;

		_ = s_pendingEmailConfirmations.TryRemove(user.Id, out _);

		_ = _dbContext.Attach(user);
		user.EmailConfirmed = true;
		_ = await _dbContext.SaveChangesWithConcurrencyByPassAsync();

		return true;
	}

	internal Boolean CheckPassword(StandardUser user, String password)
	{
		if (user.PasswordHash is null)
			throw new InvalidOperationException("The user has no password.");

		var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
		return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
	}

	[SuppressMessage("Performance", "CA1822:Mark members as static",
		Justification = "Should be used through Dependency Injection")]
	internal String GeneratePasswordResetToken(StandardUser user)
	{
		var passwordReset = new PendingPasswordReset();
		_ = s_pendingPasswordResets.AddOrUpdate(
			user.Id,
			static (_, p) => p,
			static (_, _, p) => p,
			passwordReset);
		return passwordReset.Token;
	}

	internal async ValueTask<ResetPasswordResult> ResetPasswordAsync(
		StandardUser user,
		String newPassword,
		String token)
	{
		if (!s_pendingPasswordResets.TryGetValue(user.Id, out var passwordReset) ||
		    passwordReset.Token != token ||
		    DateTime.UtcNow - passwordReset.CreationDate > s_pendingPasswordResetsLifeTime)
			return ResetPasswordResult.InvalidToken;

		if (Options.Password.RequireUppercase &&
		    newPassword.Any(c => !UppercaseCharacters.Contains(c)))
			return ResetPasswordResult.MissingUppercaseChar;

		if (Options.Password.RequireLowercase &&
		    newPassword.Any(c => !LowercaseCharacters.Contains(c)))
			return ResetPasswordResult.MissingUppercaseChar;

		if (Options.Password.RequireDigit &&
		    newPassword.Any(c => !Digits.Contains(c)))
			return ResetPasswordResult.MissingDigit;

		if (newPassword.Length < Options.Password.RequiredLength ||
		    newPassword.Length > StandardUser.PasswordMaxLength)
			return ResetPasswordResult.InvalidLength;

		if (Options.Password.RequireNonAlphanumeric &&
		    newPassword.All(c =>
			    LowercaseCharacters.Contains(c) || UppercaseCharacters.Contains(c) || Digits.Contains(c)))
			return ResetPasswordResult.MissingNonAlphanumericChar;

		var passwordCharacters = new HashSet<Char>();
		if (Options.Password.RequiredUniqueChars > 0)
		{
			foreach (var character in newPassword)
				_ = passwordCharacters.Add(character);
		}

		if (passwordCharacters.Count < Options.Password.RequiredUniqueChars)
			return ResetPasswordResult.NotEnoughUniqueChars;

		if (!s_pendingPasswordResets.TryRemove(user.Id, out _))
		{
			// Already removed by concurrent reset password operation.
			return ResetPasswordResult.UnknownProblem;
		}

		_ = _dbContext.Attach(user);
		user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
		_ = await _dbContext.SaveChangesWithConcurrencyByPassAsync();
		return ResetPasswordResult.Success;
	}

	internal async Task AccessFailedAsync(StandardUser user)
	{
		_ = _dbContext.Attach(user);
		user.AccessFailedCount++;
		if (user.AccessFailedCount >= Options.Lockout.MaximumFailedAccessAttempts)
			user.LockoutEndTime = DateTime.UtcNow.AddMinutes(Options.Lockout.LockoutMinutes);

		_ = await _dbContext.SaveChangesWithConcurrencyByPassAsync();
	}

	internal async Task DeleteAsync(StandardUser user)
	{
		if (user.IsDeleted)
			return;

		_ = _dbContext.Attach(user);
		var deleteId = Guid.CreateVersion7().ToString("N");
		user.UserName = $"deleted_user_{deleteId}";
		user.Email = "x@example.com";
		user.PasswordHash = null;
		user.SecurityStamp = null;
		user.IsDeleted = true;
		user.DisplayName = $"Deleted user {deleteId}";
		user.Description = String.Empty;
		user.CurrentRoomId = null;
		user.ConcurrencyStamp = Guid.NewGuid().ToString("N");
		_ = await _dbContext.SaveChangesAsync();
	}

	internal async ValueTask<Permissions> GetPermissionsAsync(IEnumerable<RoleAssignment> roleAssignments)
	{
		var userPermissions = roleAssignments.Select(ra => ra.Role.Permissions).Aggregate((a, b) => a | b);
		var globalRolePermissions = (await GetGlobalRole()).Permissions;
		return userPermissions & globalRolePermissions;
	}

	internal async ValueTask<Permissions> GetPermissionsAsync(User user) =>
		await GetPermissionsAsync(user.RoleAssignments);

	internal async ValueTask<Boolean> HasPermissionAsync(User user, Permissions permissionFlag)
		=> (await GetPermissionsAsync(user)).HasFlag(permissionFlag);

	internal async ValueTask<Boolean> HasPermissionAsync(
		IEnumerable<RoleAssignment> roleAssignments,
		Permissions permissionFlag)
		=> (await GetPermissionsAsync(roleAssignments)).HasFlag(permissionFlag);

	private async ValueTask<Role> GetGlobalRole()
	{
		return _cachedGlobalRole ??= await _dbContext.Roles
				.Where(r => r.IsGlobal)
				.FirstOrDefaultAsync()
			?? throw new InvalidOperationException("No global role exists.");
	}

	private sealed class PendingEmailConfirmation
	{
		internal String Token { get; } = Guid.CreateVersion7().ToString("N");

		internal DateTime CreationDate { get; } = DateTime.UtcNow;
	}

	private sealed class PendingPasswordReset
	{
		internal String Token { get; } = Guid.CreateVersion7().ToString("N");

		internal DateTime CreationDate { get; } = DateTime.UtcNow;
	}
}

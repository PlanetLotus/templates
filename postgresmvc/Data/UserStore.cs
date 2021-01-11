namespace postgresmvc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;

    using Dapper;

    // TODO: This is missing argument exception handling, like null checks, before db is called
    // See here for best example (I found this late):
    // https://github.com/aspnet/samples/blob/master/samples/aspnet/Identity/AspNet.Identity.MySQL/UserStore.cs
    public sealed class UserStore :
        IUserLoginStore<ApplicationUser>,
        // https://stackoverflow.com/questions/21645323/what-is-the-claims-in-asp-net-identity
        // https://stackoverflow.com/questions/29593214/claims-without-roles
        IUserClaimStore<ApplicationUser>,
        IUserRoleStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserSecurityStampStore<ApplicationUser>,
        IUserEmailStore<ApplicationUser>,
        IUserPhoneNumberStore<ApplicationUser>,
        IUserTwoFactorStore<ApplicationUser>,
        IUserLockoutStore<ApplicationUser>
    {
        private readonly DbConnection _connection;

        public UserStore(DbConnection connection)
        {
            _connection = connection;
        }

        public void Dispose() { /*nothing to dispose*/ }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            user.Id = await _connection.ExecuteScalarAsync<long>(@"

                insert into ""user""
                (
                    user_name, normalized_user_name, email, normalized_email, email_confirmed,
                    password_hash, security_stamp, concurrency_stamp, phone_number, phone_number_confirmed,
                    two_factor_enabled, lockout_end, lockout_enabled, access_failed_count
                )
                values
                (
                    @user_name, @normalized_user_name, @email, @normalized_email, @email_confirmed,
                    @password_hash, @security_stamp, @concurrency_stamp, @phone_number, @phone_number_confirmed,
                    @two_factor_enabled, @lockout_end, @lockout_enabled, @access_failed_count
                )
                returning id;

            ", GetUserParameters(user));

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            // TODO: This probably needs to delete referenced rows first, such as claims
            cancellationToken.ThrowIfCancellationRequested();
            await _connection.ExecuteAsync(@"

                delete from ""user""
                where
                    id = @id

            ", new { id = user.Id });
            return IdentityResult.Success;
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
            await FindAsync(cancellationToken, userId: Convert.ToInt64(userId));

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
            await FindAsync(cancellationToken, name: normalizedUserName);

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            // TODO: Shouldn't this call update?
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            // TODO: Shouldn't this call update?
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _connection.ExecuteAsync(@"

                update ""user""
                set
                    user_name = @user_name,
                    normalized_user_name = @normalized_user_name,
                    email = @email,
                    normalized_email = @normalized_email,
                    email_confirmed = @email_confirmed,
                    password_hash = @password_hash,
                    security_stamp = @security_stamp,
                    concurrency_stamp = @concurrency_stamp,
                    phone_number = @phone_number,
                    phone_number_confirmed = @phone_number_confirmed,
                    two_factor_enabled = @two_factor_enabled,
                    lockout_end = @lockout_end,
                    lockout_enabled = @lockout_enabled,
                    access_failed_count = @access_failed_count
                where
                    id = @id

            ", GetUserParameters(user));
            return IdentityResult.Success;
        }

        public async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
            => await FindAsync(cancellationToken, email: normalizedEmail);

        public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Email);

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.EmailConfirmed);

        public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.NormalizedEmail);

        public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.PhoneNumber);

        public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.PhoneNumberConfirmed);

        public Task SetPhoneNumberAsync(ApplicationUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            // TODO: Shouldn't this call update?
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            // TODO: Shouldn't this call update?
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.TwoFactorEnabled);

        public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
        {
            // TODO: Shouldn't this call update?
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.PasswordHash);

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.PasswordHash != null);

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            // TODO: Shouldn't this call update?
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var param = new
            {
                id = user.Id,
                role = roleName.ToUpper()
            };

            await _connection.ExecuteAsync(@"

                insert into user_role ( user_id, role_id )

                select
                    @id as user_id, id as role_id

                from
                    ""role""

                where
                    normalized_name = @role

            ", param);
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return (await _connection.QueryAsync<string>(@"

                select
                    r.name
                from
                    ""role"" r

                    inner join user_role ur
                    on r.id = ur.role_id and ur.user_id = @id

            ", new { id = user.Id })).ToList();
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var param = new
            {
                role = roleName.ToUpper()
            };

            return (await _connection.QueryAsync<ApplicationUser>($@"

                select
                    {GetUserSelectRecord()}

                from
                    ""user"" u

                    inner join user_role ur
                    on u.id = ur.user_id

                    inner join ""role"" r
                    on ur.role_id = r.id and r.normalized_name = @role

            ", param)).ToList();
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var param = new
            {
                id = user.Id,
                role = roleName.ToUpper()
            };

            // TODO: Does this work?
            return await _connection.ExecuteScalarAsync<bool>(@"

                select
                    1
                from
                    ""role"" r

                    inner join user_role ur
                    on r.id = ur.role_id and ur.user_id = @id

                where
                    r.normalized_name = @role

            ", param);
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var param = new
            {
                id = user.Id,
                role = roleName.ToUpper()
            };

            await _connection.ExecuteAsync(@"

                delete from user_role ur
                using ""role"" r
                where
                    ur.role_id = r.id and
                    r.normalized_name = @role and
                    ur.user_id = @id

            ", param);
        }

        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string Sql = @"SELECT * FROM user_claim WHERE user_id = @UserId";

            var dbClaims = await _connection.QueryAsync<DbClaim>(Sql, new { UserId = user.Id });

            return dbClaims.Select(db => new Claim(db.ClaimType, db.ClaimValue)).ToList();
        }

        public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string Sql =
@"
INSERT INTO user_claim (user_id, claim_type, claim_value)
VALUES (@UserId, @ClaimType, @ClaimValue);
";

            // TODO: Batch this
            foreach (var claim in claims)
            {
                var param = new
                {
                    UserId = user.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                };

                await _connection.ExecuteAsync(Sql, param);
            }
        }

        public async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Modelling off of this
            // https://github.com/dotnet/aspnetcore/blob/839cf8925278018903f53f22d580d15b0a59ca0f/src/Identity/EntityFrameworkCore/src/UserStore.cs#L484
            const string Sql =
@"
UPDATE user_claim
SET
    claim_type = @NewClaimType,
    claim_value = @NewClaimValue
WHERE
    user_id = @UserId AND
    claim_type = @OldClaimType AND
    claim_value = @OldClaimValue;
";

            var param = new
            {
                UserId = user.Id,
                NewClaimType = newClaim.Type,
                NewClaimValue = newClaim.Value,
                OldClaimType = claim.Type,
                OldClaimValue = claim.Value
            };

            await _connection.ExecuteAsync(Sql, param);
        }

        public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string Sql =
@"
DELETE FROM user_claim
WHERE
    user_id = @UserId AND
    claim_type = @ClaimType AND
    claim_value = @ClaimValue;
";

            // TODO: Batch this
            foreach (var claim in claims)
            {
                var param = new
                {
                    UserId = user.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                };

                await _connection.ExecuteAsync(Sql, param);
            }
        }

        public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string Sql =
@"
SELECT u.*
FROM ""user"" u
JOIN user_claim uc ON u.id = uc.user_id
WHERE
    uc.claim_type = @ClaimType AND
    uc.claim_value = @ClaimValue
";

            var param = new
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            };

            return (await _connection.QueryAsync<ApplicationUser>(Sql, param)).ToList();
        }

        public async Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            const string Sql =
@"
INSERT INTO user_login (login_provider, provider_key, provider_display_name, user_id)
VALUES (@LoginProvider, @ProviderKey, @ProviderDisplayName, @UserId);
";

            var param = new
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                ProviderDisplayName = login.ProviderDisplayName,
                UserId = user.Id
            };

            await _connection.ExecuteAsync(Sql, param);
        }

        public async Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            const string Sql =
@"
DELETE FROM user_login
WHERE
    login_provider = @LoginProvider AND
    provider_key = @ProviderKey AND
    user_id = @UserId;
";

            var param = new
            {
                LoginProvider = loginProvider,
                ProviderKey = providerKey,
                UserId = user.Id
            };

            await _connection.ExecuteAsync(Sql, param);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            const string Sql = @"SELECT * FROM user_login WHERE user_id = @UserId";

            return (await _connection.QueryAsync<UserLoginInfo>(Sql, new { UserId = user.Id })).ToList();
        }

        public async Task<ApplicationUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string Sql =
@"
SELECT u.*
FROM ""user"" u
JOIN user_login ul ON u.id = ul.user_id
WHERE
    ul.login_provider = @LoginProvider AND
    ul.provider_key = @ProviderKey;
";

            var param = new
            {
                LoginProvider = loginProvider,
                ProviderKey = providerKey
            };

            return await _connection.QuerySingleOrDefaultAsync<ApplicationUser>(Sql, param);
        }

        public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;

            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        private async Task<ApplicationUser> FindAsync(CancellationToken cancellationToken, long? userId = null, string name = null, string email = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var param = new
            {
                id = userId,
                name = name,
                email = email
            };

            return await _connection.QueryFirstOrDefaultAsync<ApplicationUser>($@"

                select
                    {GetUserSelectRecord()}

                from
                    ""user"" u

                where
                    ( @id is null or u.id = @id ) and
                    ( @name is null or u.normalized_user_name = @name ) and
                    ( @email is null or u.normalized_email = @email )

            ", param);
        }

        private static string GetUserSelectRecord() => @$"
                u.id as {nameof(IdentityRole.Id)},
                u.user_name as {nameof(ApplicationUser.UserName)},
                u.normalized_user_name as {nameof(ApplicationUser.NormalizedUserName)},
                u.email as {nameof(ApplicationUser.Email)},
                u.normalized_email as {nameof(ApplicationUser.NormalizedEmail)},
                u.email_confirmed as {nameof(ApplicationUser.EmailConfirmed)},
                u.password_hash as {nameof(ApplicationUser.PasswordHash)},
                u.security_stamp as {nameof(ApplicationUser.SecurityStamp)},
                u.concurrency_stamp as {nameof(ApplicationUser.ConcurrencyStamp)},
                u.phone_number as {nameof(ApplicationUser.PhoneNumber)},
                u.phone_number_confirmed as {nameof(ApplicationUser.PhoneNumberConfirmed)},
                u.two_factor_enabled as {nameof(ApplicationUser.TwoFactorEnabled)},
                u.lockout_end as {nameof(ApplicationUser.LockoutEnd)},
                u.lockout_enabled as {nameof(ApplicationUser.LockoutEnabled)},
                u.access_failed_count as {nameof(ApplicationUser.AccessFailedCount)}
         ";

        private static object GetUserParameters(ApplicationUser user)
        {
            return new
            {
                id = user.Id,
                user_name = user.UserName,
                normalized_user_name = user.NormalizedUserName ?? user.UserName?.ToUpper(),
                email = user.Email,
                normalized_email = user.NormalizedEmail ?? user.Email?.ToUpper(),
                email_confirmed = user.EmailConfirmed,
                password_hash = user.PasswordHash,
                security_stamp = user.SecurityStamp,
                concurrency_stamp = user.ConcurrencyStamp,
                phone_number = user.PhoneNumber,
                phone_number_confirmed = user.PhoneNumberConfirmed,
                two_factor_enabled = user.TwoFactorEnabled,
                lockout_end = user.LockoutEnd,
                lockout_enabled = user.LockoutEnabled,
                access_failed_count = user.AccessFailedCount
            };
        }

        private sealed class DbClaim
        {
            public int Id { get; set; }
            public long UserId { get; set; }
            public string ClaimType { get; set; }
            public string ClaimValue { get; set; }
        }

        public IQueryable<ApplicationUser> Users { get; }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public async Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
        {
            const string Sql =
@"
UPDATE ""user""
SET lockout_enabled = @LockoutEnabled
WHERE id = @UserId;
";

            var param = new
            {
                UserId = user.Id,
                LockoutEnabled = enabled
            };

            await _connection.ExecuteAsync(Sql, param);
        }
    }
}
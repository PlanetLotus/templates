using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Dapper;

namespace postgresmvc.Data
{
    public sealed class RoleStore : IRoleStore<IdentityRole<int>>
    {
        private readonly DbConnection _connection;

        public RoleStore(DbConnection connection)
        {
            _connection = connection;
        }

        public void Dispose() { /*nothing to dispose*/ }

        public async Task<IdentityResult> CreateAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var param = new
            {
                name = role.Name,
                normalized_name = role.NormalizedName ?? role.Name?.ToUpper(),
                concurrency_stamp = role.ConcurrencyStamp
            };

            role.Id = await _connection.ExecuteScalarAsync<int>(@"

                insert into ""role""
                (
                    name, normalized_name, concurrency_stamp
                )
                values
                (
                    @name, @normalized_name, @concurrency_stamp
                )
                returning id;

            ", param);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _connection.ExecuteAsync(@"

                delete from ""role""
                where
                    id = @id

            ", new { id = role.Id });
            return IdentityResult.Success;
        }

        public async Task<IdentityRole<int>> FindByIdAsync(string roleId, CancellationToken cancellationToken) =>
            await FindAsync(cancellationToken, roleId: Convert.ToInt64(roleId));

        public async Task<IdentityRole<int>> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) =>
            await FindAsync(cancellationToken, name: normalizedRoleName);

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole<int> role, CancellationToken cancellationToken) => Task.FromResult(role.NormalizedName);

        public Task<string> GetRoleIdAsync(IdentityRole<int> role, CancellationToken cancellationToken) => Task.FromResult(role.Id.ToString());

        public Task<string> GetRoleNameAsync(IdentityRole<int> role, CancellationToken cancellationToken) => Task.FromResult(role.Name);

        public Task SetNormalizedRoleNameAsync(IdentityRole<int> role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // TODO: Shouldn't this call update?
            role.NormalizedName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetRoleNameAsync(IdentityRole<int> role, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // TODO: Shouldn't this call update?
            role.Name = roleName;
            return Task.FromResult(0);
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var param = new
            {
                id = role.Id,
                name = role.Name,
                normalized_name = role.NormalizedName ?? role.Name?.ToUpper(),
                concurrency_stamp = role.ConcurrencyStamp
            };

            await _connection.ExecuteAsync(@"

                update ""role""
                set
                    name = @name,
                    normalized_name = @normalized_name,
                    concurrency_stamp = @concurrency_stamp
                where
                    id = @id

            ", param);

            return IdentityResult.Success;
        }

        private async Task<IdentityRole<int>> FindAsync(CancellationToken cancellationToken, long? roleId = null, string name = null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var param = new
            {
                id = roleId,
                name = name
            };

            return await _connection.QueryFirstOrDefaultAsync<IdentityRole<int>>($@"

                select
                    id as {nameof(IdentityRole.Id)},
                    name as {nameof(IdentityRole.Name)},
                    normalized_name as {nameof(IdentityRole.NormalizedName)},
                    concurrency_stamp as {nameof(IdentityRole.ConcurrencyStamp)}

                from
                    ""role""

                where
                    ( @id is null or id = @id ) and ( @name is null or normalized_name = @name )


            ", param);
        }
    }
}
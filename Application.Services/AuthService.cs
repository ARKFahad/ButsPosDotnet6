using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Core.Common;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using Application.Repositories;
using Application.Core.PermissionHelpers;
using Application.Core.Constants;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IBaseRepository<User> _userRepository;
        private readonly IBaseRepository<Role> _roleRepository;
        private readonly IBaseRepository<Company> _companyRepository;

        private readonly IBaseRepository<RoleClaim> _roleClaimRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        public AuthService(IConfiguration configuration, IHttpContextAccessor contextAccessor,
            IBaseRepository<User> userRepository, IBaseRepository<Role> roleRepository,
            IBaseRepository<RoleClaim> roleClaimRepository,
            IBaseRepository<Company> companyRepository
           )
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _roleClaimRepository = roleClaimRepository;
            _configuration = configuration;
            _contextAccessor = contextAccessor;
            _companyRepository = companyRepository;
        }
        public async Task<Result<string>> Login(string username, string password)
        {
            User user = await _userRepository.GetAll().FirstOrDefaultAsync(x =>!x.Deleted && x.Username.ToLower().Equals(username.ToLower()));
            if (user == null) return await Result<string>.FailAsync("Object Not Found Exception", "User Not Found");
            else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) return await Result<string>.FailAsync("", "Wrong Password");
            else return await Result<string>.SuccessAsync(CreateToken(user), "Login Successful");
          
        }

        public async Task<Result<List<RoleClaimModel>>> GetAllPermissionsAsync(string roleId)
        {
          
            var allPermissions = new List<RoleClaimModel>();
            allPermissions.GetAllPermissions();
            var role = await _roleRepository.FindAsync(roleId);
            if (role != null)
            {
                
                var allRoleClaims =  _roleClaimRepository.GetAll();
                var roleClaims = _roleClaimRepository.GetAll().Where(x=>x.RoleId==role.Id);
                if (roleClaims!=null)
                {
                    var allClaimValues = allPermissions.Select(a => a.Value).ToList();
                    var roleClaimValues = roleClaims.Select(a => a.Value).ToList();
                    var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();
                    foreach (var permission in allPermissions)
                    {
                        permission.Id = allRoleClaims?.SingleOrDefault(x => x.RoleId == roleId && x.Type == permission.Type && x.Value == permission.Value)?.Id ?? "";
                        permission.RoleId = roleId;
                        if (authorizedClaims.Any(a => a == permission.Value))
                        {
                            permission.Selected = true;
                            var roleClaim = roleClaims.SingleOrDefault(a => a.Type == permission.Type && a.Value == permission.Value);
                            if (roleClaim.Description != null)
                            {
                                permission.Description = roleClaim.Description;
                            }

                            if (roleClaim?.Group != null)
                            {
                                permission.Group = roleClaim.Group;
                            }
                        }
                    }
                }
                else
                {
                    return await Result<List<RoleClaimModel>>.FailAsync("","No Role Claims for this role");
                }
            }
            return await Result<List<RoleClaimModel>>.SuccessAsync(allPermissions,"Permission Found");
        }


        public async Task<Result<string>> UpdatePermissionsAsync(PermissionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RoleId))
                {
                    return await Result<string>.FailAsync("", "Role is required.");
                }
                var role = await _roleRepository.FindAsync(request.RoleId);
                if (role.Name == RoleConstants.SuperAdmin)
                {
                    return await Result<string>.FailAsync("", "Not allowed to modify Permissions for this Role.");
                }

                var selectedClaims = request.RoleClaims.Where(a => a.Selected).ToList();
                if (role.Name == RoleConstants.SuperAdmin)
                {
                    if (selectedClaims.All(x => x.Value != Application.Core.Constants.Permissions.Roles.View) ||
                        selectedClaims.All(x => x.Value != Application.Core.Constants.Permissions.RoleClaims.View) ||
                        selectedClaims.All(x => x.Value != Application.Core.Constants.Permissions.RoleClaims.Edit))
                    {
                        return await Result<string>.FailAsync("", string.Format(
                            "Not allowed to deselect {0} or {1} or {2} for this Role.",
                            Application.Core.Constants.Permissions.Roles.View,
                            Application.Core.Constants.Permissions.RoleClaims.View,
                            Application.Core.Constants.Permissions.RoleClaims.Edit));
                    }
                }

                IList<RoleClaim> claims = _roleClaimRepository.GetAll().Where(x => x.RoleId == role.Id).ToList();
                if (claims.Any())
                {
                    foreach (var claim in claims)
                    {
                        await _roleClaimRepository.DeleteAsync(claim.Id);
                    }
                }
               

                foreach (var claim in selectedClaims)
                {
                    var roleClaim = new RoleClaim
                    {
                        Id=Guid.NewGuid().ToString(),
                        RoleId=claim.RoleId,
                        Type=claim.Type,
                        Value=claim.Value,
                        Group=claim.Group,
                        Description=claim.Description,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow,
                        CreatedBy = _contextAccessor.HttpContext.User?.GetUserId(),
                        UpdatedBy = _contextAccessor.HttpContext.User?.GetUserId()
                    };
                    await _roleClaimRepository.AddAsync(roleClaim);
                }

                return await Result<string>.SuccessAsync("","Permissions Updated.");
            }
            catch (Exception ex)
            {
                return await Result<string>.FailAsync(ex.ToString(),ex.Message);
            }
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _userRepository.GetAll().AnyAsync(x => x.Username.ToLower() == username.ToLower()))
            {
                return true;
            }
            return false;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        private  string CreateToken(User user)
        {
            var permissionClaims = new List<Claim>();
            var roleClaims = _roleClaimRepository.GetAll().Where(x => x.RoleId == user.RoleId);
            var company =  _companyRepository.GetAllAsNoTracking().FirstOrDefault();
            foreach (var roleClaim in roleClaims)
            {
                permissionClaims.Add(new Claim(roleClaim.Type, roleClaim.Value));
            }

            IEnumerable<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("branchId", user.BranchId??""),
                new Claim("company",company.Name),
                new Claim("logo",company.LogoPath),
                new Claim(ClaimTypes.Role, _roleRepository.GetAll().FirstOrDefault(x=>x.Id==user.RoleId)?.Name)
            }.Union(permissionClaims);

            SymmetricSecurityKey key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value)
            );

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddYears(1),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
       
    }
}
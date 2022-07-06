
using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.PermissionHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
  public  interface IAuthService
    {
        Task<Result<string>> Login(string username, string password);
        Task<Result<List<RoleClaimModel>>> GetAllPermissionsAsync(string roleId);
        Task<Result<string>> UpdatePermissionsAsync(PermissionRequest request);
        Task<bool> UserExists(string username);
    }
}

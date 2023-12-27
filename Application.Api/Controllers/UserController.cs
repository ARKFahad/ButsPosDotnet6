using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.PermissionHelpers;
using Application.RequestModel;
using Application.Services;
using Application.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PbApp.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IBaseQueryService<User, UserRequestModel, UserViewModel> _userQueryService;
        private readonly IBaseCommandService<User> _userCommandService;
        private readonly GHP_POS_DBContext _context;
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _contextAccessor;
        public UserController(IBaseQueryService<User, UserRequestModel, UserViewModel> userQueryService,
            IBaseCommandService<User> userCommandService,
            IHttpContextAccessor contextAccessor, 
            IAuthService authService,
            GHP_POS_DBContext context)
        {
            _authService = authService;
            _userCommandService = userCommandService;
            _userQueryService = userQueryService;
            _contextAccessor = contextAccessor;
            _context = context;
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Users.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<UserViewModel>, int>>> Search(UserRequestModel request)
        {
            try
            {
                Tuple<List<UserViewModel>, int> content = await _userQueryService.SearchAsync(request);

                foreach (var item in content.Item1)
                {
                    if (!string.IsNullOrEmpty(item.BranchId))
                    {
                        item.BranchName = _context.Branches.Find(item.BranchId)?.Name;
                    }
                    else
                    {
                        item.BranchName = "N/A";
                    }
                   
                }
                return await Result<Tuple<List<UserViewModel>, int>>.SuccessAsync(content, "Users Found");
            }
            catch (Exception exception)
            {
                return await Result<Tuple<List<UserViewModel>, int>>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Users.View)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<Result<UserViewModel>> Detail(string id)
        {
            try
            {
                UserViewModel content = await _userQueryService.GetDetailAsync(id);
                return await Result<UserViewModel>.SuccessAsync(content, "User Found");
            }
            catch (Exception exception)
            {
                return await Result<UserViewModel>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        // [Authorize(Policy = Permissions.Users.View)]
        //// [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [Route("reference-no")]
        // [HttpGet]
        // public async Task<IActionResult> GetReferenceNo()
        // {
        //     string referenceNo = await _service.GetReferanceIdAsync();
        //     return Ok(new { ReferenceNo = referenceNo });
        // }

      //  [Authorize(Policy = Permissions.Users.Create)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public virtual async Task<Result> Register(RegisterRequestModel model)
        {
            if (await _authService.UserExists(model.Username)) return await Result<string>.FailAsync("", "User already exists.");
            CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);
            
            try
            {
                User user = new User();
                user.Id = Guid.NewGuid().ToString();
                user.Username = model.Username;
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.RoleId = model.RoleId;
                user.BranchId = model.BranchId;
                user.CreatedOn = DateTime.UtcNow;
                user.UpdatedOn = DateTime.UtcNow;
                user.CreatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                user.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _userCommandService.AddAsync(user);
                return await Result<User>.SuccessAsync(user, "User Added Successfully");
            }
            catch (Exception exception)
            {
                return await Result<User>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [Authorize(Policy = Permissions.Users.Edit)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("/api/user/update")]
        public virtual async Task<Result> Put([FromBody] UserUpdateRequestModel model)
        {
            try
            {
                User user =await _userCommandService.FindAsync(model.Id);
                if (user == null) return await Result<User>.FailAsync("","User Not Found");
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);
                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }
                if (model.RoleId != RoleIdConstants.SalesManRoleId)
                    user.BranchId = "";
                else
                    user.BranchId = model.BranchId;
                user.RoleId = model.RoleId;
                user.UpdatedOn = DateTime.Now;
                user.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                await _userCommandService.UpdateAsync(user);
                return await Result<User>.SuccessAsync(user, "User Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<User>.FailAsync(exception.ToString(), exception.Message);
            }

        }
        [Authorize(Policy = Permissions.Users.Delete)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(string id)
        {
            try
            {
                User user = await _userCommandService.FindAsync(id);
                if (user != null)
                {
                    user.Deleted = true;
                    user.UpdatedOn = DateTime.UtcNow;
                    user.UpdatedBy = _contextAccessor.HttpContext?.User?.GetUserId();
                    await _userCommandService.UpdateAsync(user);
                }
               
                return await Result<string>.SuccessAsync(id, "User Deleted Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
      
    }
}

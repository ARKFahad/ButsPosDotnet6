using Application.Core.Entities;
using Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IAccountService
    {
        decimal GetAccountBalance(string accountId, string type, string branchId = null);
    }
}

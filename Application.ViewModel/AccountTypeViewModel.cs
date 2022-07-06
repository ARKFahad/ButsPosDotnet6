using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class AccountTypeViewModel
    {
        public AccountTypeViewModel(AccountType accountType)
        {
            Id = accountType.Id;
            Name = accountType.Name;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        
    }
}

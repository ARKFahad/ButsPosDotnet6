using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
  public  class AccountHeadViewModel
    {
        public AccountHeadViewModel(AccountHead accountHead)
        {
            Id = accountHead.Id;
            Name = accountHead.Name;
            Code = accountHead.Code;
            AccountTypeId = accountHead.AccountTypeId;
            AccountTypeName = accountHead.AccountType?.Name;
            Level = accountHead.Level;
            ParentId = accountHead.ParentId;
            ParentName = accountHead.Parent?.Name;
            IsControlAccount = accountHead.IsControlAccount;
            
        }
        public string Id { get; set; }
        public string Code { get; set; }
        public string? AccountTypeName { get; set; }
        public string AccountTypeId { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public string? ParentName { get; set; }
        public bool IsControlAccount { get; set; }

    }
}

using Application.Core.Entities;
using Application.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class PayReceiveViewModel
    {
        public PayReceiveViewModel(PayReceive payReceive)
        {
            Id = payReceive.Id;
            Vnumber = payReceive.Vnumber;
            VoucherType =payReceive.VoucherType;
            VoucherDate = payReceive.VoucherDate;
            BranchName = payReceive?.Branch?.Name;
            PaymentMethodName = payReceive?.PaymentMethod?.Name;
            CheckNo = payReceive.CheckNo;
            AccountName = payReceive?.Account?.Name;
            Amount = payReceive.Amount;
            Description = payReceive.Description;
            CreatedOn = payReceive.CreatedOn;
        }

        public string Id { get; set; }
        public string Vnumber { get; set; }
        public int VoucherType { get; set; }
        public DateTime VoucherDate { get; set; }
        public string BranchName { get; set; }
        public string PaymentMethodName { get; set; }
        public string CheckNo { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

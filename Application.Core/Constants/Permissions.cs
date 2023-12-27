

using System.ComponentModel;

namespace Application.Core.Constants
{
    public static class Permissions
    {
        [DisplayName("MasterMenus")]
        [Description("MasterMenus Permissions")]
        public static class MasterMenus
        {
            public const string Configuration = "Permissions.MasterMenus.Configuration";
            public const string Transaction = "Permissions.MasterMenus.Transaction";
            public const string Reports = "Permissions.MasterMenus.Reports";
            public const string Accounts = "Permissions.MasterMenus.Accounts";
        }
        [DisplayName("Dashboard")]
        [Description("Dashboard Permissions")]
        public static class Dashboard
        {
            public const string View = "Permissions.Dashboard.View";
            
        }
        [DisplayName("Users")]
        [Description("Users Permissions")]
        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";
        }

       

        [DisplayName("Roles")]
        [Description("Roles Permissions")]
        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Create = "Permissions.Roles.Create";
            public const string Edit = "Permissions.Roles.Edit";
            public const string Delete = "Permissions.Roles.Delete";
        }

     

        [DisplayName("Role Claims")]
        [Description("Role Claims Permissions")]
        public static class RoleClaims
        {
            public const string View = "Permissions.RoleClaims.View";
            public const string Create = "Permissions.RoleClaims.Create";
            public const string Edit = "Permissions.RoleClaims.Edit";
            public const string Delete = "Permissions.RoleClaims.Delete";
        }

       

     

        [DisplayName("Categories")]
        [Description("Categories Permissions")]
        public static class Categories
        {
            public const string View = "Permissions.Categories.View";
            public const string ViewAll = "Permissions.Categories.ViewAll";
            public const string Add = "Permissions.Categories.Add";
            public const string Update = "Permissions.Categories.Update";
            public const string Remove = "Permissions.Categories.Remove";
        }
       
        [DisplayName("Branches")]
        [Description("Branches Permissions")]
        public static class Branches
        {
            public const string View = "Permissions.Branches.View";
            public const string ViewAll = "Permissions.Branches.ViewAll";
            public const string Add = "Permissions.Branches.Add";
            public const string Update = "Permissions.Branches.Update";
            public const string Remove = "Permissions.Branches.Remove";
        }


        [DisplayName("Products")]
        [Description("Products Permissions")]
        public static class Products
        {
            public const string View = "Permissions.Products.View";
            public const string ViewAll = "Permissions.Products.ViewAll";
            public const string Add = "Permissions.Products.Add";
            public const string Update = "Permissions.Products.Update";
            public const string Remove = "Permissions.Products.Remove";
        }

        [DisplayName("Suppliers")]
        [Description("Suppliers Permissions")]
        public static class Suppliers
        {
            public const string View = "Permissions.Suppliers.View";
            public const string ViewAll = "Permissions.Suppliers.ViewAll";
            public const string Add = "Permissions.Suppliers.Add";
            public const string Update = "Permissions.Suppliers.Update";
            public const string Remove = "Permissions.Suppliers.Remove";
        }

        [DisplayName("Customers")]
        [Description("Customers Permissions")]
        public static class Customers
        {
            public const string View = "Permissions.Customers.View";
            public const string ViewAll = "Permissions.Customers.ViewAll";
            public const string Add = "Permissions.Customers.Add";
            public const string Update = "Permissions.Customers.Update";
            public const string Remove = "Permissions.Customers.Remove";
        }
        [DisplayName("GenericNames")]
        [Description("GenericNames Permissions")]
        public static class GenericNames
        {
            public const string View = "Permissions.GenericNames.View";
            public const string ViewAll = "Permissions.GenericNames.ViewAll";
            public const string Add = "Permissions.GenericNames.Add";
            public const string Update = "Permissions.GenericNames.Update";
            public const string Remove = "Permissions.GenericNames.Remove";
        }

        [DisplayName("Countries")]
        [Description("Countries Permissions")]
        public static class Countries
        {
            public const string View = "Permissions.Countries.View";
            public const string ViewAll = "Permissions.Countries.ViewAll";
            public const string Add = "Permissions.Countries.Add";
            public const string Update = "Permissions.Countries.Update";
            public const string Remove = "Permissions.Countries.Remove";
        }
        [DisplayName("MeasurementUnits")]
        [Description("MeasurementUnits Permissions")]
        public static class MeasurementUnits
        {
            public const string View = "Permissions.MeasurementUnits.View";
            public const string ViewAll = "Permissions.MeasurementUnits.ViewAll";
            public const string Add = "Permissions.MeasurementUnits.Add";
            public const string Update = "Permissions.MeasurementUnits.Update";
            public const string Remove = "Permissions.MeasurementUnits.Remove";
        }

        [DisplayName("Purchases")]
        [Description("Purchases Permissions")]
        public static class Purchases
        {
            public const string View = "Permissions.Purchases.View";
            public const string Add = "Permissions.Purchases.Add";
            public const string Update = "Permissions.Purchases.Update";
            public const string Remove = "Permissions.Purchases.Remove";
            public const string ItemWisePurchase = "Permissions.Purchases.ItemWisePurchase";
        }
        [DisplayName("PurchaseItems")]
        [Description("PurchaseItems Permissions")]
        public static class PurchaseItems
        {
            public const string View = "Permissions.PurchaseItems.View";
            public const string Print = "Permissions.PurchaseItems.Print";
          
        }
        [DisplayName("SaleItems")]
        [Description("SaleItems Permissions")]
        public static class SaleItems
        {
            public const string View = "Permissions.SaleItems.View";
            public const string Print = "Permissions.SaleItems.Print";

        }

        [DisplayName("CustomerPayments")]
        [Description("CustomerPayments Permissions")]
        public static class CustomerPayments
        {
            public const string View = "Permissions.CustomerPayments.View";
            public const string Add = "Permissions.CustomerPayments.Add";
            public const string Update = "Permissions.CustomerPayments.Update";
            public const string Remove = "Permissions.CustomerPayments.Remove";
        }

        [DisplayName("SupplierPayments")]
        [Description("SupplierPayments Permissions")]
        public static class SupplierPayments
        {
            public const string View = "Permissions.SupplierPayments.View";
            public const string Add = "Permissions.SupplierPayments.Add";
            public const string Update = "Permissions.SupplierPayments.Update";
            public const string Remove = "Permissions.SupplierPayments.Remove";
        }

        [DisplayName("PaymentMethods")]
        [Description("PaymentMethods Permissions")]
        public static class PaymentMethods
        {
            public const string View = "Permissions.PaymentMethods.View";
            public const string Add = "Permissions.PaymentMethods.Add";
            public const string Update = "Permissions.PaymentMethods.Update";
            public const string Remove = "Permissions.PaymentMethods.Remove";
        }
        [DisplayName("ExpenseTypes")]
        [Description("ExpenseTypes Permissions")]
        public static class ExpenseTypes
        {
            public const string View = "Permissions.ExpenseTypes.View";
            public const string Add = "Permissions.ExpenseTypes.Add";
            public const string Update = "Permissions.ExpenseTypes.Update";
            public const string Remove = "Permissions.ExpenseTypes.Remove";
        }
        [DisplayName("Expenses")]
        [Description("Expenses Permissions")]
        public static class Expenses
        {
            public const string View = "Permissions.Expenses.View";
            public const string Add = "Permissions.Expenses.Add";
            public const string Update = "Permissions.Expenses.Update";
            public const string Remove = "Permissions.Expenses.Remove";
        }

      

        [DisplayName("Sales")]
        [Description("Sales Permissions")]
        public static class Sales
        {
            public const string View = "Permissions.Sales.View";
            public const string Add = "Permissions.Sales.Add";
            public const string Update = "Permissions.Sales.Update";
            public const string Remove = "Permissions.Sales.Remove";
            public const string ItemWiseSaleProfitLoss = "Permissions.Sales.ItemWiseSaleProfitLoss";
        }
        [DisplayName("SaleReturns")]
        [Description("SaleReturns Permissions")]
        public static class SaleReturns
        {
            public const string View = "Permissions.SaleReturns.View";
            public const string Add = "Permissions.SaleReturns.Add";
        }
        [DisplayName("PurchaseReturns")]
        [Description("PurchaseReturns Permissions")]
        public static class PurchaseReturns
        {
            public const string View = "Permissions.PurchaseReturns.View";
            public const string Add = "Permissions.PurchaseReturns.Add";
        }

        [DisplayName("Transfers")]
        [Description("Transfers Permissions")]
        public static class Transfers
        {
            public const string View = "Permissions.Transfers.View";
            public const string Add = "Permissions.Transfers.Add";
            public const string Update = "Permissions.Transfers.Update";
            public const string Remove = "Permissions.Transfers.Remove";
        }

        [DisplayName("DamageLosts")]
        [Description("DamageLosts Permissions")]
        public static class DamageLosts
        {
            public const string View = "Permissions.DamageLosts.View";
            public const string Add = "Permissions.DamageLosts.Add";
            public const string Update = "Permissions.DamageLosts.Update";
            public const string Remove = "Permissions.DamageLosts.Remove";
        }
        [DisplayName("StockAdjustments")]
        [Description("StockAdjustments Permissions")]
        public static class StockAdjustments
        {
            public const string View = "Permissions.StockAdjustments.View";
            public const string Add = "Permissions.StockAdjustments.Add";
        }

        [DisplayName("Stocks")]
        [Description("Stocks Permissions")]
        public static class Stocks
        {
            public const string View = "Permissions.Stocks.View";
        }
        [DisplayName("Reports")]
        [Description("Reports Permissions")]
        public static class Reports
        {
            public const string PL = "Permissions.Reports.PL";
            public const string SupplierLedger = "Permissions.Reports.SupplierLedger";
            public const string CustomerLedger = "Permissions.Reports.CustomerLedger";
            public const string Balance = "Permissions.Reports.Balance";

        }
        //accounts
        [DisplayName("AccountHeads")]
        [Description("AccountHeads Permissions")]
        public static class AccountHeads
        {
            public const string View = "Permissions.AccountHeads.View";
            public const string Add = "Permissions.AccountHeads.Add";
            public const string Update = "Permissions.AccountHeads.Update";
            public const string Remove = "Permissions.AccountHeads.Remove";
        }

        [DisplayName("PayReceives")]
        [Description("PayReceives Permissions")]
        public static class PayReceives
        {
            public const string View = "Permissions.PayReceives.View";
            public const string Add = "Permissions.PayReceives.Add";
            public const string Update = "Permissions.PayReceives.Update";
            public const string Remove = "Permissions.PayReceives.Remove";
        }

        [DisplayName("JournalEntries")]
        [Description("JournalEntries Permissions")]
        public static class JournalEntries
        {
            public const string View = "Permissions.JournalEntries.View";
            public const string Add = "Permissions.JournalEntries.Add";
            public const string Update = "Permissions.JournalEntries.Update";
            public const string Remove = "Permissions.JournalEntries.Remove";
        }
        [DisplayName("AccountingReports")]
        [Description("AccountingReports Permissions")]
        public static class AccountingReports
        {
            public const string ChartOfAccounts = "Permissions.AccountingReports.ChartOfAccounts";
            public const string SubsidiaryLedger = "Permissions.AccountingReports.SubsidiaryLedger";
            public const string TrialBalance_BalanceSheet_IncomeStatement = "Permissions.AccountingReports.TrialBalance_BalanceSheet_IncomeStatement";
            

        }



    }
}
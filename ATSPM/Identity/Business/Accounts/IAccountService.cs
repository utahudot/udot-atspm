﻿
namespace Identity.Business.Accounts
{
    public interface IAccountService
    {
        Task<AccountResult> CreateUser(ApplicationUser user, string password);
        Task<AccountResult> Login(string email, string password, bool rememberMe);
    }
}
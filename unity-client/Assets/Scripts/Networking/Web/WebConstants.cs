using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Web
{
    public class WebConstants
    {
        public const string INIT = "/app/init";
        public const string ACCOUNT_VERIFY = "/account/verify";
        public const string ACCOUNT_REGISTER = "/account/register";
        public const string CHAR_DELETE = "/char/delete";
        public const string CHAR_FAME = "/char/fame";
        public const string CHAR_LIST = "/char/list";
        public const string GUILD_LIST_MEMBERS = "/guild/listMembers";

        public const string Email = "email";
        public const string Password = "password";
        public const string NewUsername = "newUsername";
        public const string NewEmail = "newEmail";
        public const string NewPassword = "newPassword";
        public const string CharId = "charId";
        public const string AccountId = "accountId";
    }
}

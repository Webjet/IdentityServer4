using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;

namespace AdminPortal
{
    public class MyPasswordChecker: IBackOfficeUserPasswordChecker
    {
        public Task<BackOfficeUserPasswordCheckerResult> CheckPasswordAsync(BackOfficeIdentityUser user, string password)
        {
            var result = (password == "test")
                ? Task.FromResult(BackOfficeUserPasswordCheckerResult.ValidCredentials)
                : Task.FromResult(BackOfficeUserPasswordCheckerResult.InvalidCredentials);

            return result;

        }

    }
}
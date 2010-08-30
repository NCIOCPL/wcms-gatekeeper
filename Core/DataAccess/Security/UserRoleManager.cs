using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;

namespace GateKeeper.DataAccess.Security
{
    class UserRoleManager : RoleProvider, IDisposable
    {
        #region Public Properties

        public override string ApplicationName
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion

        #region Public Methods

        #region IDisposable Members

        public void Dispose()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void CreateRole(string roleName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string[] GetAllRoles()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string[] GetRolesForUser(string username)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool RoleExists(string roleName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}

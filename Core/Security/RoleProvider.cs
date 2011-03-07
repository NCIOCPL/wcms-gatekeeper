using  System;
using  System.Web.Security;
using  System.Web;
using  System.Web.Configuration;
using  System.Security.Principal;
using  System.Security.Permissions;
using  System.Globalization;
using  System.Runtime.Serialization;
using  System.Collections;
using  System.Collections.Specialized;
using  System.Data;
using  System.Data.SqlClient;
using  System.Data.SqlTypes;
using  System.Text;
using  System.Configuration.Provider;
using  System.Configuration;
using  System.Web.DataAccess;
using  System.Web.Hosting;
using  System.Web.Util;


namespace GateKeeper.Security 
{
    public class SqlRoleProvider : RoleProvider
    {
        private string  _AppName;
        private string  _sqlConnectionString;
        private int     _CommandTimeout;

        // Public properties
        private int CommandTimeout
        {
            get{ return _CommandTimeout; }
        }


        public override  void Initialize(string name, NameValueCollection config){
            // Remove CAS from sample: HttpRuntime.CheckAspNetHostingPermission (AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);
            if (config == null)
               throw new ArgumentNullException("config");

            if (String.IsNullOrEmpty(name))
                name = "SqlRoleProvider";
            if (string.IsNullOrEmpty(config["description"])) {
                config.Remove("description");
                config.Add("description", SR.GetString(SR.RoleSqlProvider_description));
            }
            base.Initialize(name, config);

            _CommandTimeout = SecUtility.GetIntValue( config, "commandTimeout", 30, true, 0 );

            string temp = config["connectionStringName"];
            if (temp == null || temp.Length < 1)
                throw new ProviderException(SR.GetString(SR.Connection_name_not_specified));
            _sqlConnectionString = SqlConnectionHelper.GetConnectionString(temp, true, true);
            if (_sqlConnectionString == null || _sqlConnectionString.Length < 1) {
                throw new ProviderException(SR.GetString(SR.Connection_string_not_found, temp));
            }

            _AppName = config["applicationName"];
            if (string.IsNullOrEmpty(_AppName))
                _AppName = SecUtility.GetDefaultAppName();

            if( _AppName.Length > 256 )
            {
                throw new ProviderException(SR.GetString(SR.Provider_application_name_too_long));
            }

            config.Remove("connectionStringName");
            config.Remove("applicationName");
            config.Remove("commandTimeout");
            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ProviderException(SR.GetString(SR.Provider_unrecognized_attribute, attribUnrecognized));
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            SecUtility.CheckParameter(ref username, true, false, true, 256, "username");
            if (username.Length < 1)
                return false;

            try {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);

                    SqlCommand    cmd     = new SqlCommand("dbo.usp_UsersInRoles_IsUserInRole", holder.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, roleName));
                    cmd.ExecuteNonQuery();
                    int iStatus = GetReturnValue(cmd);

                    switch(iStatus)
                    {
                    case 0:
                        return false;
                    case 1:
                        return true;
                    case 2:
                        return false;
                        // throw new ProviderException(SR.GetString(SR.Provider_user_not_found));
                    case 3:
                        return false; // throw new ProviderException(SR.GetString(SR.Provider_role_not_found, roleName));
                    }
                    throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
  
        public override  string [] GetRolesForUser(string username)
        {
            SecUtility.CheckParameter(ref username, true, false, true, 256, "username");
            if (username.Length < 1)
                return new string[0];
            try {
                SqlConnectionHolder holder = null;

                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);

                    SqlCommand      cmd     = new SqlCommand("dbo.usp_UsersInRoles_GetRolesForUser", holder.Connection);
                    SqlParameter    p       = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    SqlDataReader   reader  = null;
                    StringCollection       sc      = new StringCollection();

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    try {
                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                            sc.Add(reader.GetString(0));
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                    if (sc.Count > 0)
                    {
                        String [] strReturn = new String[sc.Count];
                        sc.CopyTo(strReturn, 0);
                        return strReturn;
                    }

                    switch(GetReturnValue(cmd))
                    {
                    case 0:
                        return new string[0];
                    case 1:
                        return new string[0];
                        //throw new ProviderException(SR.GetString(SR.Provider_user_not_found));
                    default:
                        throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
                    }
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override  void CreateRole(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            try {
                SqlConnectionHolder holder = null;

                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    SqlCommand cmd = new SqlCommand("dbo.usp_Roles_CreateRole", holder.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);

                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, roleName));
                    cmd.ExecuteNonQuery();

                    int returnValue = GetReturnValue(cmd);

                    switch (returnValue) {
                    case 0 :
                        return;

                    case 1 :
                        throw new ProviderException(SR.GetString(SR.Provider_role_already_exists, roleName));

                    default :
                        throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
                    }
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
  
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            try {
                SqlConnectionHolder holder = null;

                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);

                    SqlCommand    cmd     = new SqlCommand("dbo.usp_Roles_DeleteRole", holder.Connection);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, HttpUtility.HtmlDecode(roleName)));
                    cmd.Parameters.Add(CreateInputParam("@DeleteOnlyIfRoleIsEmpty", SqlDbType.Bit, throwOnPopulatedRole ? 1 : 0));
                    cmd.ExecuteNonQuery();
                    int returnValue = GetReturnValue(cmd);

                    if( returnValue == 2 )
                    {
                        throw new ProviderException(SR.GetString(SR.Role_is_not_empty));
                    }

                    return ( returnValue == 0 );
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override  bool RoleExists(string roleName)
        {
            SecUtility.CheckParameter( ref roleName, true, true, true, 256, "roleName" );

            try {
                SqlConnectionHolder holder = null;

                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);

                    SqlCommand    cmd     = new SqlCommand("dbo.usp_Roles_RoleExists", holder.Connection);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, roleName));
                    cmd.ExecuteNonQuery();
                    int returnValue = GetReturnValue(cmd);

                    switch(returnValue)
                    {
                    case 0:
                        return false;
                    case 1:
                        return true;
                    }
                    throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 256, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 256, "usernames");

            try {
                SqlConnectionHolder holder = null;
                try
                {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    //Only pass in one parameter 
                    AddUsersToRolesCore(holder.Connection, usernames[0], roleNames[0]);
                    
                } 
                finally {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            } catch {
                throw;
            }
        }

        private void  AddUsersToRolesCore(SqlConnection conn, string usernames, string roleNames)
        {
            SqlCommand      cmd     = new SqlCommand("dbo.usp_UsersInRoles_AddUserToRole", conn);
            SqlDataReader   reader  = null;
            SqlParameter    p       = new SqlParameter("@ReturnValue", SqlDbType.Int);
            string          s1      = String.Empty, s2 = String.Empty;

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = CommandTimeout;

            p.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(p);
            cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, roleNames));
            cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, usernames));
            try {
                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read()) {
                    if (reader.FieldCount > 0)
                        s1 = reader.GetString(0);
                    if (reader.FieldCount > 1)
                        s2 = reader.GetString(1);
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            switch(GetReturnValue(cmd))
            {
            case 0:
                return;
            case 1:
                throw new ProviderException(SR.GetString(SR.Provider_this_user_not_found, s1));
            case 2:
                throw new ProviderException(SR.GetString(SR.Provider_role_not_found, s1));
            case 3:
                throw new ProviderException(SR.GetString(SR.Provider_this_user_already_in_role, s1, s2));
            }
            throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 256, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 256, "usernames");

             try {
                SqlConnectionHolder holder = null;
                try
                {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                   
                    RemoveUsersFromRolesCore(holder.Connection, usernames[0], roleNames[0]);
                } finally {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            } catch {
                throw;
            }
        }

        private void RemoveUsersFromRolesCore(SqlConnection conn, string usernames, string roleNames)
        {
            SqlCommand      cmd     = new SqlCommand("dbo.usp_UsersInRoles_RemoveUserFromRole", conn);
            SqlDataReader   reader  = null;
            SqlParameter    p       = new SqlParameter("@ReturnValue", SqlDbType.Int);
            string          s1      = String.Empty, s2 = String.Empty;

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = CommandTimeout;

            p.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(p);
            cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, usernames));
            cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, roleNames));
            try {
                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read()) {
                    if (reader.FieldCount > 0)
                        s1 = reader.GetString(0);
                    if (reader.FieldCount > 1)
                        s2 = reader.GetString(1);
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            switch (GetReturnValue(cmd))
            {
                case 0:
                    return;
                case 1:
                    throw new ProviderException(SR.GetString(SR.Provider_this_user_not_found, s1));
                case 2:
                    throw new ProviderException(SR.GetString(SR.Provider_role_not_found, s2));
                case 3:
                    throw new ProviderException(SR.GetString(SR.Provider_this_user_already_not_in_role, s1, s2));
            }
            throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
        }
        
        public override  string [] GetAllRoles(){
            try {
                SqlConnectionHolder holder = null;

                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);

                    SqlCommand      cmd     = new SqlCommand("dbo.usp_Roles_GetAllRoles", holder.Connection);
                    StringCollection       sc      = new StringCollection();
                    SqlParameter    p       = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    SqlDataReader   reader  = null;

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);

                    try {
                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                            sc.Add(reader.GetString(0));
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }

                    String [] strReturn = new String [sc.Count];
                    sc.CopyTo(strReturn, 0);
                    return strReturn;
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        
        public override  string [] GetUsersInRole(string roleName)
        {
            throw new Exception("Not implemented") ;            
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new Exception("Not implemented") ;
        }
        
        public override  string ApplicationName
        {
            get { return _AppName; }
            set {
                _AppName = value;

                if ( _AppName.Length > 256 )
                {
                    throw new ProviderException( SR.GetString( SR.Provider_application_name_too_long ) );
                }
            }
        }

        private SqlParameter CreateInputParam(string paramName, SqlDbType dbType, object objValue){
            SqlParameter param = new SqlParameter(paramName, dbType);
            if (objValue == null)
                objValue = String.Empty;
            param.Value = objValue;
            return param;
        }

        private int GetReturnValue(SqlCommand cmd) {
            foreach(SqlParameter param in cmd.Parameters){
                if (param.Direction == ParameterDirection.ReturnValue && param.Value != null && param.Value is int)
                    return (int) param.Value;
            }
            return -1;
        }
    }
}




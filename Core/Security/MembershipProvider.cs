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
using  System.Security.Cryptography;
using  System.Text;
using  System.Text.RegularExpressions;
using  System.Configuration.Provider;
using  System.Configuration;
using  System.Web.DataAccess;
using  System.Web.Management;
using  System.Web.Util;

namespace GateKeeper.Security 
{
    /// <summary>
    /// This code is from Microsoft Sample code, which can be downloaded from
    /// http://download.microsoft.com/download/a/b/3/ab3c284b-dc9a-473d-b7e3-33bacfcc8e98/ProviderToolkitSamples.msi
    /// The original sample has profileprovider and other stuff that Gatekeeper doesn't need.
    /// So the revised version will only contain membership and role provider with related helper class.
    /// The tables and Sprocs in the database will be updated to reflect Gatekeeper's simple requirement.
    /// The namespace is changed to Gatekeeper 
    /// MembershipProvider/RoleProvider will be updated according to our own requirement.
    /// </summary>
    public class SqlMembershipProvider : MembershipProvider
    {
        ////////////////////////////////////////////////////////////
        // Public properties

        public override bool    EnablePasswordRetrieval   { get { return _EnablePasswordRetrieval; } }

        public override bool    EnablePasswordReset       { get { return _EnablePasswordReset; } }

        public override bool    RequiresQuestionAndAnswer   { get { return _RequiresQuestionAndAnswer; } }

        public override bool    RequiresUniqueEmail         { get { return _RequiresUniqueEmail; } }

        public override MembershipPasswordFormat PasswordFormat { get { return _PasswordFormat; }}

        public override int MaxInvalidPasswordAttempts { get { return 0; } }

        public override int PasswordAttemptWindow { get { return 0; } }

        public override int MinRequiredPasswordLength
        {
            get { return _MinRequiredPasswordLength; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _MinRequiredNonalphanumericCharacters; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _PasswordStrengthRegularExpression; }
        }

        public override string ApplicationName
        {
            get { return _AppName; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                if (value.Length > 256)
                    throw new ProviderException( SR.GetString( SR.Provider_application_name_too_long ) );
                _AppName = value;
            }
        }

        private string    _sqlConnectionString;
        private bool      _EnablePasswordRetrieval;
        private bool      _EnablePasswordReset;
        private bool      _RequiresQuestionAndAnswer;
        private string    _AppName;
        private bool      _RequiresUniqueEmail;
        private int       _CommandTimeout;
        private int       _MinRequiredPasswordLength;
        private int       _MinRequiredNonalphanumericCharacters;
        private string    _PasswordStrengthRegularExpression;
        private MembershipPasswordFormat _PasswordFormat;

        //private int       _SchemaVersionCheck;
        //private int       _MaxInvalidPasswordAttempts;
        //private int       _PasswordAttemptWindow;

        private const int      PASSWORD_SIZE  = 14;

        public override void Initialize(string name, NameValueCollection config)
        {
            // Remove CAS from sample: HttpRuntime.CheckAspNetHostingPermission (AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);
            if (config == null)
                throw new ArgumentNullException("config");
            if (String.IsNullOrEmpty(name))
                name = "SqlMembershipProvider";
            if (string.IsNullOrEmpty(config["description"])) {
                config.Remove("description");
                config.Add("description", SR.GetString(SR.MembershipSqlProvider_description));
            }
            base.Initialize(name, config);

            _EnablePasswordRetrieval    = SecUtility.GetBooleanValue(config, "enablePasswordRetrieval", false);
            _EnablePasswordReset        = SecUtility.GetBooleanValue(config, "enablePasswordReset", true);
            _RequiresQuestionAndAnswer  = SecUtility.GetBooleanValue(config, "requiresQuestionAndAnswer", true);
            _RequiresUniqueEmail        = SecUtility.GetBooleanValue(config, "requiresUniqueEmail", true);
        
            _MinRequiredPasswordLength  = SecUtility.GetIntValue( config, "minRequiredPasswordLength", 7, false, 128 );
            _MinRequiredNonalphanumericCharacters = SecUtility.GetIntValue( config,  "minRequiredNonalphanumericCharacters", 0, true, 128 );

            _PasswordStrengthRegularExpression = config["passwordStrengthRegularExpression"];
            if( _PasswordStrengthRegularExpression != null )
            {
                _PasswordStrengthRegularExpression = _PasswordStrengthRegularExpression.Trim();
                if( _PasswordStrengthRegularExpression.Length != 0 )
                {
                    try
                    {
                        Regex regex = new Regex( _PasswordStrengthRegularExpression );
                    }
                    catch( ArgumentException e )
                    {
                        throw new ProviderException( e.Message, e );
                    }
                }
            }
            else
            {
                _PasswordStrengthRegularExpression = string.Empty;
            }
            if (_MinRequiredNonalphanumericCharacters > _MinRequiredPasswordLength)
                throw new HttpException(SR.GetString(SR.MinRequiredNonalphanumericCharacters_can_not_be_more_than_MinRequiredPasswordLength));

            _CommandTimeout = SecUtility.GetIntValue( config, "commandTimeout", 30, true, 0 );
            _AppName = config["applicationName"];
            if (string.IsNullOrEmpty(_AppName))
                _AppName = SecUtility.GetDefaultAppName();

            if( _AppName.Length > 256 )
            {
                throw new ProviderException(SR.GetString(SR.Provider_application_name_too_long));
            }

             _PasswordFormat = MembershipPasswordFormat.Clear;
          
            string temp = config["connectionStringName"];
            if (temp == null || temp.Length < 1)
                throw new ProviderException(SR.GetString(SR.Connection_name_not_specified));
            _sqlConnectionString = SqlConnectionHelper.GetConnectionString(temp, true, true);
            if (_sqlConnectionString == null || _sqlConnectionString.Length < 1) {
                throw new ProviderException(SR.GetString(SR.Connection_string_not_found, temp));
            }

            config.Remove("connectionStringName");
            config.Remove("enablePasswordRetrieval");
            config.Remove("enablePasswordReset");
            config.Remove("requiresQuestionAndAnswer");
            config.Remove("applicationName");
            config.Remove("requiresUniqueEmail");
            config.Remove("commandTimeout");
            config.Remove("name");
            config.Remove("minRequiredPasswordLength");
            config.Remove("minRequiredNonalphanumericCharacters");
            config.Remove("passwordStrengthRegularExpression");
            if (config.Count > 0) {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ProviderException(SR.GetString(SR.Provider_unrecognized_attribute, attribUnrecognized));
            }
        }

        private int CommandTimeout
        {
            get{ return _CommandTimeout; }
        }

        public override MembershipUser CreateUser( string username,
                                                   string password,
                                                   string email,
                                                   string passwordQuestion,
                                                   string passwordAnswer,
                                                   bool   isApproved,
                                                   object providerUserKey,
                                                   out    MembershipCreateStatus status )
        {
            if( !SecUtility.ValidateParameter(ref password, true, true, false, 128))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if ( password.Length > 128 )
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if( passwordAnswer != null )
            {
                passwordAnswer = passwordAnswer.Trim();
            }

            if (!string.IsNullOrEmpty(passwordAnswer)) {
                if( passwordAnswer.Length > 128 )
                {
                    status = MembershipCreateStatus.InvalidAnswer;
                    return null;
                }
            }

            if (!SecUtility.ValidateParameter(ref passwordAnswer, RequiresQuestionAndAnswer, true, false, 128))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }

            if( !SecUtility.ValidateParameter( ref username,true, true, true, 256))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }

            if( !SecUtility.ValidateParameter( ref email,
                                               RequiresUniqueEmail,
                                               RequiresUniqueEmail,
                                               false,
                                               256 ) )
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            if( !SecUtility.ValidateParameter( ref passwordQuestion, RequiresQuestionAndAnswer, true, false, 256))
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }

            if( providerUserKey != null )
            {
                if( !( providerUserKey is Guid ) )
                {
                    status = MembershipCreateStatus.InvalidProviderUserKey;
                    return null;
                }
            }

            if( password.Length < MinRequiredPasswordLength )
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            int count = 0;

            for( int i = 0; i < password.Length; i++ )
            {
                if( !char.IsLetterOrDigit( password, i ) )
                {
                    count++;
                }
            }

            if( count < MinRequiredNonAlphanumericCharacters )
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if( PasswordStrengthRegularExpression.Length > 0 )
            {
                if( !Regex.IsMatch( password, PasswordStrengthRegularExpression ) )
                {
                    status = MembershipCreateStatus.InvalidPassword;
                    return null;
                }
            }

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs( username, password, true );
            OnValidatingPassword( e );

            if( e.Cancel )
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            try
            {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection( _sqlConnectionString, true );
                    
                    DateTime    dt  = RoundToSeconds(DateTime.UtcNow);
                    SqlCommand  cmd = new SqlCommand("dbo.usp_Membership_CreateUser", holder.Connection);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                   
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    cmd.Parameters.Add(CreateInputParam("@Password", SqlDbType.NVarChar, password));
                    cmd.Parameters.Add(CreateInputParam("@Email", SqlDbType.NVarChar, email));
                    cmd.Parameters.Add(CreateInputParam("@PasswordQuestion", SqlDbType.NVarChar, passwordQuestion));
                    cmd.Parameters.Add(CreateInputParam("@PasswordAnswer", SqlDbType.NVarChar, passwordAnswer));
                    cmd.Parameters.Add(CreateInputParam("@UniqueEmail", SqlDbType.Int, RequiresUniqueEmail ? 1 : 0));
                    SqlParameter p = CreateInputParam("@UserID", SqlDbType.UniqueIdentifier, providerUserKey);
                    p.Direction= ParameterDirection.InputOutput;
                    cmd.Parameters.Add( p );

                    p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);

                    cmd.ExecuteNonQuery();
                    int iStatus = ((p.Value!=null) ? ((int) p.Value) : -1);
                    if (iStatus < 0 || iStatus > (int) MembershipCreateStatus.ProviderError)
                        iStatus = (int) MembershipCreateStatus.ProviderError;
                    status = (MembershipCreateStatus) iStatus;
                    if (iStatus != 0) // !success
                        return null;

                    providerUserKey = new Guid( cmd.Parameters[ "@UserId" ].Value.ToString() );
                    dt = dt.ToLocalTime();
                    return new MembershipUser( this.Name,
                                               username,
                                               providerUserKey,
                                               email,
                                               passwordQuestion,
                                               null,
                                               true,
                                               false,
                                               dt,
                                               dt,
                                               dt,
                                               dt,
                                               new DateTime( 1754, 1, 1 ) );
                }
                finally
                {
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

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            SecUtility.CheckParameter( ref username, true, true, true, 256, "username" );
            SecUtility.CheckParameter( ref password, true, true, false, 128, "password" );

            if (!CheckPassword(username, password, false))
                return false;
            SecUtility.CheckParameter(ref newPasswordQuestion, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 256, "newPasswordQuestion");
         
            if( newPasswordAnswer != null )
            {
                newPasswordAnswer = newPasswordAnswer.Trim();
            }

            SecUtility.CheckParameter(ref newPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "newPasswordAnswer");

            try {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection( _sqlConnectionString, true );

                    SqlCommand    cmd     = new SqlCommand("dbo.usp_Membership_ChangePasswordQuestionAndAnswer", holder.Connection);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    cmd.Parameters.Add(CreateInputParam("@NewPasswordQuestion", SqlDbType.NVarChar, newPasswordQuestion));
                    cmd.Parameters.Add(CreateInputParam("@NewPasswordAnswer", SqlDbType.NVarChar, newPasswordAnswer));

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);

                    cmd.ExecuteNonQuery();
                    int status = ( ( p.Value != null ) ? ( ( int )p.Value ) : -1 );
                    if( status != 0 )
                    {
                        throw new ProviderException( GetExceptionText( status ) );
                    }

                    return ( status == 0 );
                }
                finally
                {
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

        public override string GetPassword(string username, string passwordAnswer)
        {
            if ( !EnablePasswordRetrieval )
            {
                throw new NotSupportedException( SR.GetString( SR.Membership_PasswordRetrieval_not_supported ) );
            }

            SecUtility.CheckParameter( ref username, true, true, true, 256, "username" );

            SecUtility.CheckParameter(ref passwordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "passwordAnswer");

            string errText;
            int passwordFormat = 0;
            int status = 0;

            string pass = GetPasswordFromDB(username, passwordAnswer, RequiresQuestionAndAnswer, out passwordFormat, out status);

            if ( pass == null )
            {
                errText = GetExceptionText( status );
                if ( IsStatusDueToBadPassword( status ) )
                {
                    throw new MembershipPasswordException( errText );
                }
                else
                {
                    throw new ProviderException( errText );
                }
            }

            return  pass;
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            SecUtility.CheckParameter( ref username, true, true, true, 256, "username" );
            SecUtility.CheckParameter( ref oldPassword, true, true, false, 128, "oldPassword" );
            SecUtility.CheckParameter( ref newPassword, true, true, false, 128, "newPassword" );

            int status;

            if (!CheckPassword( username, oldPassword, false))
            {
               return false;
            }

            if( newPassword.Length < MinRequiredPasswordLength )
            {
                throw new ArgumentException(SR.GetString(
                              SR.Password_too_short,
                              "newPassword",
                              MinRequiredPasswordLength.ToString(CultureInfo.InvariantCulture)));
            }

            int count = 0;

            for( int i = 0; i < newPassword.Length; i++ )
            {
                if( !char.IsLetterOrDigit( newPassword, i ) )
                {
                    count++;
                }
            }

            if( count < MinRequiredNonAlphanumericCharacters )
            {
                throw new ArgumentException(SR.GetString(
                              SR.Password_need_more_non_alpha_numeric_chars,
                              "newPassword",
                              MinRequiredNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture)));
            }

            if( PasswordStrengthRegularExpression.Length > 0 )
            {
                if( !Regex.IsMatch( newPassword, PasswordStrengthRegularExpression ) )
                {
                    throw new ArgumentException(SR.GetString(SR.Password_does_not_match_regular_expression,
                                                             "newPassword"));
                }
            }

            if ( newPassword.Length > 128 )
            {
                throw new ArgumentException(SR.GetString(SR.Membership_password_too_long), "newPassword");
            }

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs( username, newPassword, false );
            OnValidatingPassword( e );

            if( e.Cancel )
            {
                if( e.FailureInformation != null )
                {
                    throw e.FailureInformation;
                }
                else
                {
                    throw new ArgumentException( SR.GetString( SR.Membership_Custom_Password_Validation_Failure ), "newPassword");
                }
            }


            try {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection( _sqlConnectionString, true );

                    SqlCommand  cmd = new SqlCommand( "dbo.usp_Membership_SetPassword", holder.Connection );

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    cmd.Parameters.Add(CreateInputParam("@NewPassword", SqlDbType.NVarChar, newPassword));

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);

                    cmd.ExecuteNonQuery();

                    status =  ( ( p.Value != null ) ? ( ( int )p.Value ) : -1 );

                    if ( status != 0 )
                    {
                        string errText = GetExceptionText( status );

                        if ( IsStatusDueToBadPassword( status ) )
                        {
                            throw new MembershipPasswordException( errText );
                        }
                        else
                        {
                            throw new ProviderException( errText );
                        }
                    }

                    return true;
                }
                finally
                {
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

        public override string ResetPassword( string username, string passwordAnswer )
        {
            if ( !EnablePasswordReset )
            {
                throw new NotSupportedException( SR.GetString( SR.Not_configured_to_support_password_resets ) );
            }

            SecUtility.CheckParameter( ref username, true, true, true, 256, "username" );

            SecUtility.CheckParameter(ref passwordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "passwordAnswer");
            string newPassword  = GeneratePassword();

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs( username, newPassword, false );
            OnValidatingPassword( e );

            if( e.Cancel )
            {
                if( e.FailureInformation != null )
                {
                    throw e.FailureInformation;
                }
                else
                {
                    throw new ProviderException( SR.GetString( SR.Membership_Custom_Password_Validation_Failure ) );
                }
            }

            int status;
            try
            {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection( _sqlConnectionString, true );

                    SqlCommand    cmd     = new SqlCommand("dbo.usp_Membership_ResetPassword", holder.Connection);
                    string        errText;

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                   
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    cmd.Parameters.Add(CreateInputParam("@NewPassword", SqlDbType.NVarChar, newPassword));
                   
                    if (RequiresQuestionAndAnswer) {
                        cmd.Parameters.Add(CreateInputParam("@PasswordAnswer", SqlDbType.NVarChar, passwordAnswer));
                    }

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);

                    cmd.ExecuteNonQuery();

                    status = ( ( p.Value != null ) ? ( ( int )p.Value ) : -1 );

                    if ( status != 0 )
                    {
                        errText = GetExceptionText( status );

                        if ( IsStatusDueToBadPassword( status ) )
                        {
                            throw new MembershipPasswordException( errText );
                        }
                        else
                        {
                            throw new ProviderException( errText );
                        }
                    }

                    return newPassword;
                }
                finally
                {
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


        public override void UpdateUser(MembershipUser user)
        {
            if( user == null )
            {
                throw new ArgumentNullException( "user" );
            }

            string temp = user.UserName;
            SecUtility.CheckParameter( ref temp, true, true, true, 256, "UserName" );
            temp = user.Email;
            SecUtility.CheckParameter( ref temp,
                                       RequiresUniqueEmail,
                                       RequiresUniqueEmail,
                                       false,
                                       256,
                                       "Email");
            user.Email = temp;
            try {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection( _sqlConnectionString, true );

                    SqlCommand    cmd     = new SqlCommand("dbo.usp_Membership_UpdateUser", holder.Connection);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, user.UserName));
                    cmd.Parameters.Add(CreateInputParam("@Email", SqlDbType.NVarChar, user.Email));
                    cmd.Parameters.Add(CreateInputParam("@LastLoginDate", SqlDbType.DateTime, user.LastLoginDate.ToUniversalTime()));
                    cmd.Parameters.Add(CreateInputParam("@UniqueEmail", SqlDbType.Int, RequiresUniqueEmail ? 1 : 0));
                   

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    int status = ((p.Value!=null) ? ((int) p.Value) : -1);
                    if (status != 0)
                        throw new ProviderException(GetExceptionText(status));
                    return;
                }
                finally
                {
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
        
        public override bool ValidateUser(string username, string password)
        {
            if (    SecUtility.ValidateParameter(ref username, true, true, true, 256) &&
                    SecUtility.ValidateParameter(ref password, true, true, false, 128) &&
                    CheckPassword(username, password, true))
            {
                return true;
            } else {
                return false;
            }
        }
        
        public override MembershipUser GetUser( object providerUserKey, bool userIsOnline )
        {
            if( providerUserKey == null )
            {
                throw new ArgumentNullException( "providerUserKey" );
            }

            if ( !( providerUserKey is Guid ) )
            {
                throw new ArgumentException( SR.GetString( SR.Membership_InvalidProviderUserKey ), "providerUserKey" );
            }

            SqlDataReader       reader = null;

            try {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection( _sqlConnectionString, true );

                    SqlCommand    cmd     = new SqlCommand( "dbo.usp_Membership_GetUserByUserId", holder.Connection );

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateInputParam("@UserId", SqlDbType.UniqueIdentifier, providerUserKey ) );
                    
                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);

                    reader = cmd.ExecuteReader();
                    if ( reader.Read() )
                    {
                        string email = GetNullableString(reader, 0);
                        string passwordQuestion = GetNullableString( reader, 1 );
                        DateTime dtCreate = reader.GetDateTime(2).ToLocalTime();
                        DateTime dtLastLogin = reader.GetDateTime(3).ToLocalTime();
                        string userName = GetNullableString(reader, 4);
                       
                        ////////////////////////////////////////////////////////////
                        // Step 4 : Return the result
                        return new MembershipUser( this.Name,
                                                   userName,
                                                   providerUserKey,
                                                   email,
                                                   passwordQuestion,
                                                   string.Empty,
                                                   true,
                                                   false,
                                                   dtCreate,
                                                   dtLastLogin,
                                                   DateTime.Now,
                                                   DateTime.Now,
                                                   DateTime.Now );
                    }

                    return null;
                }
                finally
                {
                    if ( reader != null )
                    {
                        reader.Close();
                        reader = null;
                    }

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

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            SecUtility.CheckParameter(
                            ref username,
                            true,
                            false,
                            true,
                            256,
                            "username" );

            SqlDataReader        reader = null;

            try {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection( _sqlConnectionString, true );

                    SqlCommand    cmd     = new SqlCommand("dbo.usp_Membership_GetUserByName", holder.Connection);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                   
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                   
                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);

                    reader = cmd.ExecuteReader();
                    if ( reader.Read() )
                    {
                        string email = GetNullableString(reader, 0);
                        string passwordQuestion = GetNullableString( reader, 1 );
                        DateTime dtCreate = reader.GetDateTime(2).ToLocalTime();
                        DateTime dtLastLogin = reader.GetDateTime(3).ToLocalTime();
                        Guid userId = reader.GetGuid( 4 );
                      
                        ////////////////////////////////////////////////////////////
                        // Step 4 : Return the result
                        return new MembershipUser( this.Name,
                                                   username,
                                                   userId,
                                                   email,
                                                   passwordQuestion,
                                                   string.Empty,
                                                   true,
                                                   false,
                                                   dtCreate,
                                                   dtLastLogin,
                                                   DateTime.Now,
                                                   DateTime.Now,
                                                   DateTime.Now );
                    }

                    return null;

                }
                finally
                {
                    if ( reader != null )
                    {
                        reader.Close();
                        reader = null;
                    }

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

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            SecUtility.CheckParameter( ref username, true, true, true, 256, "username" );

            try {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);

                    SqlCommand cmd = new SqlCommand("dbo.usp_Membership_DeleteUser", holder.Connection);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                   
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);

                    cmd.ExecuteNonQuery();
                    int status = ((p.Value!=null) ? ((int) p.Value) : -1);
                    
                    return ( status > 0 );
                }
                finally
                {
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


        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            if ( pageIndex < 0 )
                throw new ArgumentException(SR.GetString(SR.PageIndex_bad), "pageIndex");
            if ( pageSize < 1 )
                throw new ArgumentException(SR.GetString(SR.PageSize_bad), "pageSize");

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if ( upperBound > Int32.MaxValue )
                throw new ArgumentException(SR.GetString(SR.PageIndex_PageSize_bad), "pageIndex and pageSize");

            MembershipUserCollection users   = new MembershipUserCollection();
            totalRecords = 0;
            try {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);

                    SqlCommand               cmd     = new SqlCommand("dbo.usp_Membership_GetAllUsers", holder.Connection);
                    SqlDataReader            reader  = null;
                    SqlParameter             p       = new SqlParameter("@ReturnValue", SqlDbType.Int);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                   
                    cmd.Parameters.Add(CreateInputParam("@PageIndex", SqlDbType.Int, pageIndex));
                    cmd.Parameters.Add(CreateInputParam("@PageSize", SqlDbType.Int, pageSize));
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    try {
                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read()) {
                            string username, email, passwordQuestion;
                            DateTime dtCreate, dtLastLogin;
                            Guid userId;

                            username = GetNullableString( reader, 0 );
                            email = GetNullableString(reader, 1);
                            passwordQuestion = GetNullableString( reader, 2 );
                            dtCreate = reader.GetDateTime(3).ToLocalTime();
                            dtLastLogin = reader.GetDateTime(4).ToLocalTime();
                            userId = reader.GetGuid( 5 );
                         
                            users.Add( new MembershipUser( this.Name,
                                                           username,
                                                           userId,
                                                           email,
                                                           passwordQuestion,
                                                           string.Empty,
                                                           true,
                                                           false,
                                                           dtCreate,
                                                           dtLastLogin,
                                                           DateTime.Now,
                                                           DateTime.Now,
                                                           DateTime.Now ) );
                        }
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                        if (p.Value != null && p.Value is int)
                            totalRecords = (int)p.Value;
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
            } catch {
                throw;
            }
            return users;
        }

        public virtual string GeneratePassword()
        {
            return Membership.GeneratePassword(
                      MinRequiredPasswordLength < PASSWORD_SIZE ? PASSWORD_SIZE : MinRequiredPasswordLength,
                      MinRequiredNonAlphanumericCharacters );
        }

        public override bool UnlockUser( string username )
        {
            throw new Exception("Not implemented") ;
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new Exception("Not implemented") ;
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new Exception("Not implemented") ;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new Exception("Not implemented") ;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new Exception("Not implemented") ;
        }  

        #region private method
        private bool CheckPassword( string username, string password, bool updateLastLoginActivityDate)
        {
            SqlConnectionHolder holder = null;
            int                 pwdFormat;
            int                 status;
            bool                isPasswordCorrect = true;

            //Check password if true, otherwise. update info
            string passwordFromDB = GetPasswordFromDB(username, string.Empty, false, out pwdFormat, out status);
            if (status != 0)
                return false;
            
            if (passwordFromDB != password)
                isPasswordCorrect= false;

            if (updateLastLoginActivityDate)
            {
                try
                {
                    try
                    {
                        holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);

                        SqlCommand cmd = new SqlCommand( "dbo.usp_Membership_UpdateUserInfo", holder.Connection );
                        DateTime   dtNow = DateTime.UtcNow;
                        cmd.CommandTimeout = CommandTimeout;
                        cmd.CommandType = CommandType.StoredProcedure;
                       
                        cmd.Parameters.Add( CreateInputParam( "@UserName", SqlDbType.NVarChar, username ) );
                      
                        SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                        p.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(p);

                        cmd.ExecuteNonQuery();

                        status = ( ( p.Value != null ) ? ( ( int )p.Value ) : -1 );
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

            return isPasswordCorrect;
        }

        private string GetPasswordFromDB( string       username,
                                          string       passwordAnswer,
                                          bool         requiresQuestionAndAnswer,
                                          out int      passwordFormat,
                                          out int      status )
        {
            try {
                SqlConnectionHolder holder = null;
                SqlDataReader       reader = null;
                SqlParameter        p       = null;

                try {
                    holder = SqlConnectionHelper.GetConnection( _sqlConnectionString, true );
                   
                    SqlCommand cmd = new SqlCommand( "dbo.usp_Membership_GetPassword", holder.Connection );

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                   
                    cmd.Parameters.Add( CreateInputParam( "@UserName", SqlDbType.NVarChar, username ) );
                   
                    if ( requiresQuestionAndAnswer )
                    {
                        cmd.Parameters.Add( CreateInputParam( "@PasswordAnswer", SqlDbType.NVarChar, passwordAnswer ) );
                    }

                    p = new SqlParameter( "@ReturnValue", SqlDbType.Int );
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);

                    reader = cmd.ExecuteReader( CommandBehavior.SingleRow );

                    string password = null;

                    status = -1;

                    if ( reader.Read() )
                    {
                        password = reader.GetString( 0 );
                    }
                    else
                    {
                        password = null;
                    }
                    passwordFormat = 0;

                    return password;
                }
                finally
                {
                    if( reader != null )
                    {
                        reader.Close();
                        reader = null;

                        status = ( ( p.Value != null ) ? ( ( int )p.Value ) : -1 );
                    }

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

 
        private SqlParameter CreateInputParam( string paramName,
                                               SqlDbType dbType,
                                               object objValue )
        {

            SqlParameter param = new SqlParameter( paramName, dbType );

            if( objValue == null )
            {
                param.IsNullable = true;
                param.Value = DBNull.Value;
            }
            else
            {
                param.Value = objValue;
            }

            return param;
        }

        private string GetNullableString(SqlDataReader reader, int col)
        {
            if( reader.IsDBNull( col ) == false )
            {
                return reader.GetString( col );
            }

            return null;
        }

        private string GetExceptionText(int status) {
            string key;
            switch (status)
            {
            case 0:
                return String.Empty;
            case 1:
                key = SR.Membership_UserNotFound;
                break;
            case 2:
                key = SR.Membership_WrongPassword;
                break;
            case 3:
                key = SR.Membership_WrongAnswer;
                break;
            case 4:
                key = SR.Membership_InvalidPassword;
                break;
            case 5:
                key = SR.Membership_InvalidQuestion;
                break;
            case 6:
                key = SR.Membership_InvalidAnswer;
                break;
            case 7:
                key = SR.Membership_InvalidEmail;
                break;
            case 99:
                key = SR.Membership_AccountLockOut;
                break;
            default:
                key = SR.Provider_Error;
                break;
            }
            return SR.GetString(key);
        }

        private bool IsStatusDueToBadPassword( int status )
        {
            return ( status >= 2 && status <= 6 || status == 99 );
        }

        private DateTime RoundToSeconds(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
        }
        #endregion 
    }
  
}

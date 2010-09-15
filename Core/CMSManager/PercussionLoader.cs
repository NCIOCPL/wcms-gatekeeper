using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GKManagers.CMS.PercussionWebSvc;
namespace GKManagers.CMS
{
    public class PercussionLoader
    {
        /**
 * The Rhythmyx session, initialized by login()}. 
 */
        string m_rxSession;

        /**
         * The security service instance; used to perform operations defined in
         * the security services. It is initialized by login().
         */
        securitySOAP m_secService;

        /**
         * The content service instance; used to perform operations defined in
         * the content services. It is initialized by login().
         */
        contentSOAP m_contService;

        /**
         * The system service instance; used to perform operations defined in
         * the system service. It is initialized by login().
         */
        systemSOAP m_sysService;


        /**
         * The loader properties, read from the file 'Loader.xml'.
         */
        private Dictionary<string, string> m_props;

        /**
 * The Content Item data to be uploaded; read from the file 'DataFile.xml' 
 */
        private List<Dictionary<string, string>> m_itemData;


        /**
         * The property name of the protocol of the server connection.
         */
        public static String PROTOCOL = "Protocol";

        /**
         * The property name of the host of the server connection.
         */
        public static String HOST = "Host";

        /**
         * The property name of the port of the server connection.
         */
        public static String PORT = "Port";

        /**
         * The property name of the name of the login user.
         */
        public static String USER_NAME = "Username";

        /**
         * The property name of the password of the login user.
         */
        public static String PASSWORD = "Password";

        /**
         * The property name of the name of the login Community.
         */
        public static String COMMUNITY = "Community";

        /**
         * The property name of the name of the Content Type of the Content Items to
         * be uploaded. 
         */
        public static String CONTENT_TYPE = "ContentType";

        /**
         * The property name of the target Folder path in Rhythmyx.
         */

        public static String TARGET_FOLDER = "TargetFolder";
        /**
         * The property name of the name of the data file.
         */
        public static String DATA_FILE = "DataFile";

        public PercussionLoader()
        {            
            // Load the connection information
            Dictionary<string, string> props = new Dictionary<string, string>();

            props.Add(PercussionLoader.PROTOCOL, "http");
            props.Add(PercussionLoader.HOST, "156.40.134.66");
            props.Add(PercussionLoader.PORT, "9922");
            
            // Load the login credentail
            props.Add(PercussionLoader.USER_NAME, "prasadbk");
            props.Add(PercussionLoader.PASSWORD, "password");
            props.Add(PercussionLoader.COMMUNITY, "CancerGov");
            m_props = props;
        }

       public void Login()
        {
            PSWSUtils.SetConnectionInfo(m_props[PROTOCOL], m_props[HOST],
                Int16.Parse(m_props[PORT]));

            m_secService = PSWSUtils.GetSecurityService();
            m_rxSession = PSWSUtils.Login(m_secService, m_props[USER_NAME],
                  m_props[PASSWORD], m_props[COMMUNITY], null);

            m_contService = PSWSUtils.GetContentService(m_secService.CookieContainer,
                m_secService.PSAuthenticationHeaderValue);
            m_sysService = PSWSUtils.GetSystemService(m_secService.CookieContainer,
                m_secService.PSAuthenticationHeaderValue);
        }


       public void Logout()
       {
           PSWSUtils.Logout(m_secService, m_rxSession);
       }
    }
}

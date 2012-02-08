using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using NCI.WCM.CMSManager.CMS;
using NCI.WCM.CMSManager.PercussionWebSvc;

namespace GateKeeper.UnitTest.CMSManager
{
    [TestFixture]
    public class PSFolder
    {
        /// <summary>
        /// Test for finding the path in the current site when the
        /// item lives in more than one.
        /// </summary>
        /// <param name="sitePath">The site path.</param>
        /// <param name="expectedPath">The expected path.</param>
        [TestCase("//sites/site1", "/path1")]
        [TestCase("//sites/site2", "/path2")]
        [TestCase("//sites/site3", "/path3")]
        public void GetPathInSiteCurrent(string sitePath, string expectedPath)
        {
            CMSController controller = new CMSController();
            controller.SiteRootPath = sitePath;

            PSItem item = new PSItem();
            item.Folders = new PSItemFolders[3];
            item.Folders[0] = new PSItemFolders();
            item.Folders[1] = new PSItemFolders();
            item.Folders[2] = new PSItemFolders();
            item.Folders[0].path = "//sites/site1/path1";
            item.Folders[1].path = "//sites/site2/path2";
            item.Folders[2].path = "//sites/site3/path3";

            string path = controller.GetPathInSite(item);
            Assert.AreEqual(path, expectedPath);
        }

        /// <summary>
        /// Verify null return when a content item doesn't reside in the current site.
        /// </summary>
        /// <param name="sitePath">The site path.</param>
        /// <param name="expectedPath">The expected path.</param>
        [TestCase("/path1")]
        [TestCase("/path2")]
        [TestCase("/path3")]
        public void GetPathInSiteUnknownSite(string expectedPath)
        {
            CMSController controller = new CMSController();
            controller.SiteRootPath = "//sites/othersite";

            PSItem item = new PSItem();
            item.Folders = new PSItemFolders[3];
            item.Folders[0] = new PSItemFolders();
            item.Folders[1] = new PSItemFolders();
            item.Folders[2] = new PSItemFolders();
            item.Folders[0].path = "//sites/site1/path1";
            item.Folders[1].path = "//sites/site2/path2";
            item.Folders[2].path = "//sites/site3/path3";

            string path = controller.GetPathInSite(item);
            Assert.IsNull(path);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GKManagers.CMSManager;

namespace GKManagers.CMSManager.CMS
{
    public class CMSController
    {
        // TODO: The PercussionLoader class needs to move to the CMS namespace.
        // TODO: PercussionLoader is not a good name. This is the interface to Percussion.
        //       (It does much more than just load content items.)
        //       PercussionController would be confusing vs. CMSController.
        //       Renaming to simply Percussion is probably the best way to go for now.

        #region Fields

        // This is the one and only instance of the interface to Percussion.  It gets created
        // when the CMSController is created. Login occurs when the CMSController constructor
        // is run.  This instance is used by all CMSController methods which need to communicate
        // with the Percussion system.
        private readonly PercussionLoader percCMS = new PercussionLoader();

        #endregion

        public CMSController()
        {
            // Percussion system login and any other needed intitialization goes here.
        }


        /// A few methods which are definitely needed:
        ///     CreateContentItem (based on a string containing the Content Type).
        ///     LoadContentItem (Does this need to be a content ID? Can it be a path?)
        ///     ContentItemExists (Boolean -- true if an item exists, false otherwise).
        ///     CreatePath (Based on a string containing the path. Is a site name needed?)
        ///     
    }
}

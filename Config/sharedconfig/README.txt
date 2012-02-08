The connectionStrings.config file contains the database connection strings used by
all GateKeeper applications.  The individual applications reference the file via
a configSource attribute on the connectionStrings section of their respective
configuration files.

The sharedConfig folder is referenced in the individual application and test harness
folders via the Subversion svn:externals property.  Changes to shared files in one
project are automatically propogated to all others via the edit, commit, update cycle.

For more information on

configSource - See the .Net documentation:
ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/dv_aspnetgenref/html/46b74975-6e2b-4ec8-ac4a-7b00ab02cad3.htm

svn:externals - See the Subversion documentation:
http://svnbook.red-bean.com/en/1.6/svn.advanced.externals.html
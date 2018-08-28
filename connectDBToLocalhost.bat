for %%a in (admin cdrpreviewws procmgr websvc) do copy Config\sharedconfig\connectionStrings-LOCALHOST.config app\%%a\sharedconfig\connectionStrings.config 
for %%a in (UnitTest PromotionTester) do copy Config\sharedconfig\connectionStrings-LOCALHOST.config "Test Harnesses\%%a\sharedconfig\connectionStrings.config" 

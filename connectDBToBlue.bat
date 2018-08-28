for %%a in (admin cdrpreviewws procmgr websvc) do copy Config\sharedconfig\connectionStrings-BLUE.config app\%%a\sharedconfig\connectionStrings.config 
for %%a in (UnitTest PromotionTester) do copy Config\sharedconfig\connectionStrings-BLUE.config "Test Harnesses\%%a\sharedconfig\connectionStrings.config" 

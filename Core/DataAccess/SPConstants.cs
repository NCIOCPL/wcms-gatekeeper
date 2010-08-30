using System;

/// <summary>
/// Store store procedure names for all document types
/// </summary>
namespace GateKeeper.DataAccess.StoreProcedures
{
    public class SPCommon
    {
        # region Fields
            private static string _spSaveDocumentData = "dbo.usp_saveDocumentData";
            private static string _spPushDocument = "dbo.usp_PushDocument";
            private static string _spClearDocument = "dbo.usp_ClearDocument";
            private static string _spGetDocumentIds = "dbo.usp_GetDocumentInfo";
            private static string _spGetNCIViewDependencies = "dbo.usp_GetNCIViewDependencies";
            private static string _spGetNCIViewID = "dbo.usp_GetNCIViewByObjectId";
            private static string _spSummaryPushCheck = "dbo.usp_SummaryPushCheck";
            private static string _spGetDocumentStatus = "dbo.usp_GetDocumentStatus";
        # endregion

        # region Public properties
            public static string SP_SAVE_DOCUMENT_DATA
            { get {return _spSaveDocumentData; } }

            public static string SP_PUSH_DOCUMENT
            { get { return _spPushDocument; } }

            public static string SP_CLEAR_DOCUMENT
            { get { return _spClearDocument; } }

            public static string SP_GET_DOCUMENT_IDS
            { get { return _spGetDocumentIds; } }

            public static string SP_GET_NCI_VIEW_DEPENDENCY
            { get { return _spGetNCIViewDependencies; } }

            public static string SP_GET_NCI_VIEW_ID
            { get { return _spGetNCIViewID; } }

            public static string SP_SUMMARY_PUSH_CHECK
            { get { return _spSummaryPushCheck; } }

            public static string SP_GET_DOCUMENT_STATUS
            { get { return _spGetDocumentStatus; } }

        #endregion
    }

    public class SPDocumentXPath
    {
        public static string _spSaveDocumentXPath = "dbo.usp_GetXMLXPath";

        # region Public properties
        public static string SP_GET_DOCUMENT_XPATH
        { get { return _spSaveDocumentXPath; } }
        #endregion
    }

    public class SPGlossaryTerm
    {
        #region Fields
            private static string _spClearGlossaryData = "dbo.usp_clearExtractedGlossaryData";
            private static string _spSaveGlossaryTerm = "dbo.usp_saveGlossaryTerm";
            private static string _spSaveGTDefinition = "dbo.usp_saveGlossaryTermDefinition";
            private static string _spSaveGTDefinitionAudi = "dbo.usp_saveGlossaryTermDefinitionAudience";
            private static string _spSaveGTDefinitionDic = "dbo.usp_saveGlossaryTermDefinitionDictionary";
            private static string _spSaveGTDocumentData = "dbo.usp_saveDocumentData";
            private static string _spPushGTDocumentData = "dbo.usp_PushExtractedGlossaryData";
        #endregion

        #region Public properties
            public static string SP_CLEAR_GLOSSARY_DATA
            { get { return _spClearGlossaryData; } }

            public static string SP_SAVE_GLOSSARY_TERM
            { get { return _spSaveGlossaryTerm; } }

            public static string SP_SAVE_GT_DEFINITION
            { get { return _spSaveGTDefinition; } }

            public static string SP_SAVE_GT_DEFINITION_AUDI
            { get { return _spSaveGTDefinitionAudi; } }

            public static string SP_SAVE_GT_DEFINITION_DIC
            { get { return _spSaveGTDefinitionDic; } }

            public static string SP_SAVE_GT_DOCUMENT_DATA
            { get { return _spSaveGTDocumentData; } }

            public static string SP_PUSH_GT_DOCUMENT_DATA
            { get { return _spPushGTDocumentData; } }
        #endregion
    }

    public class SPTerminology
    {
        #region Fields
            private static string _spClearTerminologyData = "dbo.usp_clearExtractedTerminologyData";
            private static string _spSaveCDRMenus = "dbo.usp_saveCDRMenus";
            private static string _spSaveTerminology = "dbo.usp_saveTerminology";
            private static string _spSaveTermDefinition = "dbo.usp_saveTermDefinition";
            private static string _spSaveTermSemanticType = "dbo.usp_saveTermSemanticType";
            private static string _spSaveTermOtherName = "dbo.usp_saveTermOtherName";
            private static string _spPushTermdata = "dbo.usp_PushExtractedTerminologyData";
        #endregion

        #region Public properties
        public static string SP_CLEAR_TERMINOLOGY_DATA
        { get { return _spClearTerminologyData; } }

        public static string SP_SAVE_CDR_MENUS
        { get { return _spSaveCDRMenus; } }

        public static string SP_SAVE_TERMINOLOGY
        { get { return _spSaveTerminology; } }

        public static string SP_SAVE_TERM_DEFINITION
        { get { return _spSaveTermDefinition; } }

        public static string SP_SAVE_SEMANTIC_TYPE
        { get { return _spSaveTermSemanticType; } }

        public static string SP_SAVE_TERM_OTHER_NAME
        { get { return _spSaveTermOtherName; } }

        public static string SP_PUSH_TERM_DATA
        { get { return _spPushTermdata; } }
        #endregion
    }

    public class SPDrugInfo
    {
        #region Fields
        private static string _spClearDrugInfoSummaryData = "dbo.usp_ClearExtractedDrugInfoSummaryData";
        private static string _spSaveDrugInfoSummary = "dbo.usp_SaveDrugInfoSummary";
        private static string _spPushDrugInfoSummary = "dbo.usp_PushExtractedDrugInfoSummaryData";
        private static string _spPushDrugInfoToPreview = "dbo.PushDrugInfoSummaryToPreview";
        private static string _spPushDrugInfoToLive = "dbo.PushDrugInfoToProduction";
        private static string _spDeleteDrugInfoInPreview = "dbo.DeleteDrugInfoSummaryPreview";
        private static string _spDeleteDrugInfoInLive = "dbo.DeleteDrugInfoSummaryLive";
        private static string _spGetDrugInfo = "dbo.usp_GetDrugInfoSummary ";
        #endregion

        #region Public properties
        public static string SP_CLEAR_DRUG_DATA
        { get { return _spClearDrugInfoSummaryData; } }

        public static string SP_SAVE_DRUG_INFO
        { get { return _spSaveDrugInfoSummary; } }

        public static string SP_PUSH_DRUG_INFO
        { get { return _spPushDrugInfoSummary; } }

        public static string SP_PUSH_DRUG_INFO_TO_PREVIEW
        { get { return _spPushDrugInfoToPreview; } }

        public static string SP_PUSH_DRUG_INFO_TO_LIVE
        { get { return _spPushDrugInfoToLive; } }

        public static string SP_DELETE_DRUG_INFO_IN_PREVIEW
        { get { return _spDeleteDrugInfoInPreview; } }

        public static string SP_DELETE_DRUG_INFO_IN_LIVE
        { get { return _spDeleteDrugInfoInLive; } }

        public static string SP_GET_DRUG_INFO
        { get { return _spGetDrugInfo; } }

        #endregion
    }

    public class SPGenProf
    {
        #region Fields
        private static string _spClearGenProfData = "dbo.usp_ClearExtractedGeneticProfessionalData";
        private static string _spSaveGenProf = "dbo.usp_SaveGenProf";
        private static string _spSaveGenProfLocation = "dbo.usp_SaveGenProfPracticeLocation";
        private static string _spSaveGenProfSyndrome = "dbo.usp_SaveGenProfFamilyCancerSyndrome";
        private static string _spSaveGenProfTypeOfCancer = "dbo.usp_SaveGenProfTypeOfCancer";
        private static string _spPushGenProf = "dbo.usp_PushExtractedGeneticsProfessionalData";
        #endregion

        #region Public properties
        public static string SP_CLEAR_GENPROF_DATA
        { get { return _spClearGenProfData; } }

        public static string SP_SAVE_GENPROF
        { get { return _spSaveGenProf; } }

        public static string SP_SAVE_GENPROF_LOCATION
        { get { return _spSaveGenProfLocation; } }

        public static string SP_SAVE_GENPROF_SYNDROME
        { get { return _spSaveGenProfSyndrome; } }

        public static string SP_SAVE_GENPROF_TYPEOFCANCER
        { get { return _spSaveGenProfTypeOfCancer; } }

        public static string SP_PUSH_GENPROF
        { get { return _spPushGenProf; } }
       
        #endregion
    }

    public class SPSummary
    {
        #region Fields
            private static string _spClearSummaryData = "dbo.usp_clearExtractedSummaryData";
            private static string _spSaveSummary = "dbo.usp_saveSummary";
            private static string _spSaveSummarySection = "dbo.usp_saveSummarySection";
            private static string _spSaveSummaryRelations = "dbo.usp_saveSummaryRelations";
            private static string _spGetDocumentGuid = "dbo.usp_getDocumentGUID";
            private static string _spGetSummaryInfo = "dbo.usp_GetSummaryInfo";
            private static string _spPushExtractedSummaryData = "dbo.usp_PushExtractedSummaryData";
            private static string _spUpdateNCIViewAndViewObject = "dbo.usp_UpdateNCIViewandViewObjectForSummary";
            private static string _spUpdateNCIViewStatus = "dbo.usp_UpdateNCIViewStatus";
            private static string _spPushNCIViewToProduction = "dbo.usp_PushNCIViewToProduction";
            private static string _spDeleteViewObject = "dbo.usp_deleteViewObjectForSummary";
        #endregion

        #region  Public properties
            public static string SP_CLEAR_SUMMARY_DATA
            { get { return _spClearSummaryData; } }

            public static string SP_SAVE_SUMMARY
            { get { return _spSaveSummary; } }

            public static string SP_SAVE_SUMMARY_SECTION
            { get { return _spSaveSummarySection; } }

            public static string SP_SAVE_SUMMARY_RELATIONS
            { get { return _spSaveSummaryRelations; } }

            public static string SP_GET_DOCUMENT_GUID
            { get { return _spGetDocumentGuid; } }

            public static string SP_Get_Summary_Info
            { get { return _spGetSummaryInfo; } }

            public static string SP_Push_Extracted_Summary_Data
            { get { return _spPushExtractedSummaryData; } }

            public static string SP_Update_NCI_View_And_ViewObject
            { get { return _spUpdateNCIViewAndViewObject; } }

            public static string SP_Update_NCI_View_Status
            { get { return _spUpdateNCIViewStatus; } }

            public static string SP_Push_NCI_View_To_Production
            { get { return _spPushNCIViewToProduction; } }

            public static string SP_Delete_View_Object
            { get { return _spDeleteViewObject; } }
        #endregion
    }

    public class SPProtocol
    {
        #region Fields
            private static string _spClearExtractedProtocolData = "dbo.usp_ClearExtractedProtocolData";
            private static string _spSaveProtocol = "dbo.usp_SaveProtocol";
            private static string _spSaveProtocolOldID = "dbo.usp_SaveProtocolOLDID";
            private static string _spSaveProtocolDrug = "dbo.usp_SaveProtocolDrug";
            private static string _spSaveProtocolTypeofCancer = "dbo.usp_SaveProtocolTypeOfCancer";
            private static string _spSaveProtocolSpecialCategory = "dbo.usp_SaveProtocolSpecialCategory";
            private static string _spSaveprotocolSection = "dbo.usp_SaveProtocolSection";
            private static string _spSaveProtocolAlternateID = "dbo.usp_SaveProtocolAlternateID";
            private static string _spSaveProtocolSponsors = "dbo.usp_SaveProtocolSponsors";
            private static string _spSaveprotocolStudyCategory = "dbo.usp_SaveProtocolStudyCategory";
            private static string _spSaveProtocolModality = "dbo.usp_SaveProtocolModality";
            private static string _spSaveProtocolPhase = "dbo.usp_SaveProtocolPhase";
            private static string _spSaveProtocolDetail = "dbo.usp_SaveProtocolDetail";
            private static string _spSaveProtocolSecondaryPrettyUrlID = "dbo.usp_SaveProtocolSecondaryPrettyUrlID";
            private static string _spSaveProtocolTrialSite = "dbo.usp_SaveProtocolTrialSite";
            private static string _spSaveProtocolLeadOrg = "dbo.usp_SaveProtocolLeadOrg";
            private static string _spSaveProtocolContactInfoHTML = "dbo.usp_SaveProtocolContactInfoHTML";
            private static string _spSaveProtocolContactInfoHTMLMap = "dbo.usp_SaveProtocolContactInfoHTMLMap";
            private static string _spPushProtocolData = "dbo.usp_PushExtractedProtocolData";
            private static string _spPushMapTables2Preivew = "dbo.usp_PushMapTables2Preview";
            private static string _spPushMapTables2Live = "dbo.usp_PushMapTables2Live";
            private static string _spGetDateFirstPublished = "dbo.usp_GetProtocolDateFirstPublished";


        #endregion

        #region Public properties
            public static string SP_CLEAR_EXTRACTED_PROTOCOL_DATA
            { get { return _spClearExtractedProtocolData; } }

            public static string SP_SAVE_PROTOCOL
            { get { return _spSaveProtocol; } }

            public static string SP_SAVE_PROTOCOL_OLD_ID
            { get { return _spSaveProtocolOldID; } }

            public static string SP_SAVE_PROTOCOL_DRUG
            { get { return _spSaveProtocolDrug; } }

            public static string SP_SAVE_PROTOCOL_TYPE_OF_CANCER
            { get { return _spSaveProtocolTypeofCancer; } }

            public static string SP_SAVE_PROTOCOL_SPECIAL_CATEGORY
            { get { return _spSaveProtocolSpecialCategory; } }

            public static string SP_SAVE_PROTOCOL_SECTION
            { get { return _spSaveprotocolSection; } }

            public static string SP_SAVE_PROTOCOL_ALTERNATE_ID
            { get { return _spSaveProtocolAlternateID; } }

            public static string SP_SAVE_PROTOCOL_SPONSORS
            { get { return _spSaveProtocolSponsors; } }

            public static string SP_SAVE_PROTOCOL_STUDY_CATEGORY
            { get { return _spSaveprotocolStudyCategory; } }

            public static string SP_SAVE_PROTOCOL_MODALITY
            { get { return _spSaveProtocolModality; } }

            public static string SP_SAVE_PROTOCOL_PHASE
            { get { return _spSaveProtocolPhase; } }

            public static string SP_SAVE_PROTOCOL_DETAIL
            { get { return _spSaveProtocolDetail; } }

            public static string SP_SAVE_PROTOCOL_SECONDARY_PRETTY_URL_ID
            { get { return _spSaveProtocolSecondaryPrettyUrlID; } }

            public static string SP_SAVE_PROTOCOL_TRIAL_SITE
            { get { return _spSaveProtocolTrialSite; } }

            public static string SP_SAVE_PROTOCOL_LEAD_ORG
            { get { return _spSaveProtocolLeadOrg; } }

            public static string SP_SAVE_PROTOCOL_CONTACT_INFO_HTML
            { get { return _spSaveProtocolContactInfoHTML; } }

            public static string SP_SAVE_PROTOCOL_CONTACT_INFO_HTML_MAP
            { get { return _spSaveProtocolContactInfoHTMLMap; } }

            public static string SP_PUSH_PROTOCOL_DATA
            { get { return _spPushProtocolData; } }

            public static string SP_PUSH_MAP_TABLE_PREVIEW
            { get { return _spPushMapTables2Preivew; } }
        
             public static string SP_PUSH_MAP_TABLE_LIVE
             { get { return _spPushMapTables2Live; } }

             public static string SP_GET_DATE_FIRST_PUBLISHED
             { get { return _spGetDateFirstPublished; } }

        #endregion
    }

    public class SPPoliticalSubUnit
    {
        # region Fields
        private static string _spClearPoliticalSubUnitData = "dbo.usp_ClearExtractedPoliticalSubUnitData";
        private static string _spSavePoliticalSubUnit = "dbo.usp_SavePoliticalSubUnit";
        private static string _spPushExtractedPoliticalSubUnitData = "dbo.usp_PushExtractedPoliticalSubUnitData";
        # endregion

        # region Public properties
        public static string SP_CLEAR_POLITICALSUBUNIT_DATA
        { get { return _spClearPoliticalSubUnitData; } }

        public static string SP_SAVE_POLITICALSUBUNIT_DATA
        { get { return _spSavePoliticalSubUnit; } }

        public static string SP_PUSH_EXTRACTED_POLITICALSUBUNIT_DATA
        { get { return _spPushExtractedPoliticalSubUnitData; } }
         #endregion
    }

    public class SPOrganization
    {
        # region Fields
        private static string _spClearOrganizationData = "dbo.usp_ClearExtractedOrganizationData";
        private static string _spSaveOrganizationName = "dbo.usp_SaveOrganizationName";
        private static string _spPushExtractedOrganizationData = "dbo.usp_PushExtractedOrganizationData";
        # endregion

        # region Public properties
        public static string SP_CLEAR_ORGANIZATION_DATA
        { get { return _spClearOrganizationData; } }

        public static string SP_SAVE_ORGANIZATION_DATA
        { get { return _spSaveOrganizationName; } }

        public static string SP_PUSH_EXTRACTED_ORGANIZATION_DATA
        { get { return _spPushExtractedOrganizationData; } }
        #endregion
    }

}

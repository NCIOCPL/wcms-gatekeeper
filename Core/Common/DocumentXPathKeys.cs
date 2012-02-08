using System;
using System.Collections.Generic;
using System.Text;

// <summary>
/// Look up place for document XPath
/// </summary>
namespace GateKeeper.Common.XPathKeys
{
    public class CommonXPath
    {
        #region Fields
        private static string _cdrid = "CDRID";
        private static string _lastModifiedDate = "LastModifiedDate";
        private static string _firstPublishedDate = "FirstPublishedDate";

        #endregion

        #region public properties
        public static string CDRID
        { get { return _cdrid; } }

        public static string LastModifiedDate
        { get { return _lastModifiedDate; } }

        public static string FirstPublishedDate
        { get { return _firstPublishedDate; } }
        #endregion
    }

    public class SummaryXPath
    {
        #region Fields
        private static string _url = "SummaryURL";
        private static string _mobileUrl = "MobileURL";
        private static string _patientVersion = "SummaryPatientVersionOf";
        private static string _title = "SummaryTitle";
        private static string _type = "SummaryType";
        private static string _description = "SummaryDescription";
        private static string _language = "SummaryLanguage";
        private static string _audience = "SummaryAudience";
        private static string _translation = "SummaryTranslationOf";
        private static string _sRef = "SummaryRef";
        private static string _link = "SummaryLink";
        private static string _sectTitle = "SummarySectionTitle";
        private static string _sectTable = "SummarySectionTable";
        private static string _topSection = "SummaryTopSection";
        private static string _subSection = "SummarySubSection";
        private static string _reference = "SummaryReference";
        private static string _basePrettyURL = "SummaryBasePrettyURL";
        private static string _baseMobileURL = "SummaryBaseMobileURL";
        private static string _replacement = "SummaryReplacement";
        private static string _relationID = "SummaryRelationRef";
        private static string _prettyURLHref = "SummaryPrettyURLHref";
        private static string _prettyURLRef = "SummaryPrettyURLRef";
        private static string _section = "SummarySection";
        private static string _mediaLink = "SummaryMediaLink";
        private static string _referenceID = "SummaryReferenceID";
        private static string _referenceII = "SummaryReferenceII";
        private static string _mediaLinkCaption = "SummaryMediaLinkCaption";
        private static string _alterTitle = "SummaryAlterTitle";
        private static string _titleType = "SummaryTitleType";
        #endregion

        #region public properties
        public static string URL
        { get { return _url; } }

        public static string MobileURL
        { get { return _mobileUrl; } }

        public static string PatientVersion
        { get { return _patientVersion; } }

        public static string Title
        { get { return _title; } }

        public static string Type
        { get { return _type; } }

        public static string Descript
        { get { return _description; } }

        public static string Lang
        { get { return _language; } }

        public static string Audience
        { get { return _audience; } }

        public static string Translation
        { get { return _translation; } }

        public static string Ref
        { get { return _sRef; } }

        public static string Link
        { get { return _link; } }

        public static string SectTitle
        { get { return _sectTitle; } }

        public static string SectTable
        { get { return _sectTable; } }

        public static string TopSection
        { get { return _topSection; } }

        public static string SubSection
        { get { return _subSection; } }

        public static string Reference
        { get { return _reference; } }

        public static string BasePrettyURL
        { get { return _basePrettyURL; } }

        public static string BaseMobileURL
        { get { return _baseMobileURL; } }

        public static string Replacement
        { get { return _replacement; } }

        public static string RelationID
        { get { return _relationID; } }

        public static string PrettyURLHref
        { get { return _prettyURLHref; } }

        public static string PrettyURLRef
        { get { return _prettyURLRef; } }

        public static string Section
        { get { return _section; } }

        public static string MediaLink
        { get { return _mediaLink; } }

        public static string ReferenceID
        { get { return _referenceID; } }

        public static string ReferenceII
        { get { return _referenceII; } }

        public static string MediaLinkCaption
        { get { return _mediaLinkCaption; } }

        public static string AlterTitle
        { get { return _alterTitle; } }

        public static string TitleType
        { get { return _titleType; } }

        #endregion
    }

    public class GlossaryTermXPath
    {
        #region Fields
        private static string _name = "GlossaryTermName";
        private static string _pronunciation = "GlossaryTermPron";
        private static string _definition = "GlossaryTermDef";
        private static string _spanishName = "GlossaryTermSpanishName";
        private static string _spanishDef = "GlossaryTermSpanishDef";
        private static string _defText = "GlossaryTermDefText";
        private static string _dictionary = "GlossaryTermDictionary";
        private static string _audience = "GlossaryTermAudience";
        private static string _mediaLink = "GlossaryTermMediaLink";
        private static string _mediaID = "GlossaryTermMediaID";
        private static string _mediaThumb = "GlossaryTermMediaThumb";
        private static string _mediaRef = "GlossaryTermMediaRef";
        private static string _mediaAlt = "GlossaryTermMediaAlt";
        private static string _mediaInline = "GlossaryTermMediaInline";
        private static string _mediaMidWidth = "GlossaryTermMediaMinWidth";
        private static string _mediaSize = "GlossaryTermMediaSize";
        private static string _mediaLanguage = "GlossaryTermMediaLanguage";
        private static string _mediaCaption = "GlossaryTermMediaCaption";
        private static string _mediaType = "GlossaryTermMediaType";

        #endregion

        #region Public Properties
        public static string Name
        { get { return _name; } }

        public static string Pronunciation
        { get { return _pronunciation; } }

        public static string Definition
        { get { return _definition; } }

        public static string SpanishName
        { get { return _spanishName; } }

        public static string SpanishDef
        { get { return _spanishDef; } }

        public static string DefText
        { get { return _defText; } }

        public static string Dictionary
        { get { return _dictionary; } }

        public static string Audience
        { get { return _audience; } }

        public static string MediaLink
        { get { return _mediaLink; } }

        public static string MediaID
        { get { return _mediaID; } }

        public static string MediaThumb
        { get { return _mediaThumb; } }

        public static string MediaRef
        { get { return _mediaRef; } }

        public static string MediaAlt
        { get { return _mediaAlt; } }

        public static string MediaInline
        { get { return _mediaInline; } }

        public static string MediaMidWidth
        { get { return _mediaMidWidth; } }

        public static string MediaSize
        { get { return _mediaSize; } }

        public static string MediaLanguage
        { get { return _mediaLanguage; } }

        public static string MediaCaption
        { get { return _mediaCaption; } }

        public static string MediaType
        { get { return _mediaType; } }

        #endregion
    }

    public class TerminologyXPath
    {
        #region Fields
        private static string _preferenceName = "TerminologyPreferenceName";
        private static string _definitionType = "TerminologyDefinitionType";
        private static string _definitionText = "TerminologyDefinitionText";
        private static string _typeName = "TerminologyTypeName";
        private static string _otherName = "TerminologyOtherName";
        private static string _semanticType = "TerminologySemanticType";
        private static string _menuItem = "TerminologyMenuItem";
        private static string _otherTermName = "TerminologyOtherTermName";
        private static string _otherNameType = "TerminologyOtherNameType";
        private static string _semanticTypeRef = "TerminologySemanticTypeRef";
        private static string _menuSortOrder = "TerminologyMenuSortOrder";
        private static string _menuDisplayName = "TerminologyMenuDisplayName";
        private static string _menuParent = "TerminologyMenuParent";
        private static string _menuRef = "TerminologyMenuRef";


        #endregion

        #region Public Properties
        public static string PerferenceName
        { get { return _preferenceName; } }

        public static string DefinitionType
        { get { return _definitionType; } }

        public static string DefinitionText
        { get { return _definitionText; } }

        public static string TypeName
        { get { return _typeName; } }

        public static string OtherName
        { get { return _otherName; } }

        public static string SemanticType
        { get { return _semanticType; } }

        public static string MenuItem
        { get { return _menuItem; } }

        public static string OtherTermName
        { get { return _otherTermName; } }

        public static string OtherNameType
        { get { return _otherNameType; } }

        public static string SemanticTypeRef
        { get { return _semanticTypeRef; } }

        public static string MenuSortOrder
        { get { return _menuSortOrder; } }

        public static string MenuDisplayName
        { get { return _menuDisplayName; } }

        public static string MenuParent
        { get { return _menuParent; } }

        public static string MenuRef
        { get { return _menuRef; } }


        #endregion
    }

    public class DrugInfoXPath
    {
        #region Fields
        private static string _title = "DrugInfoTitle";
        private static string _description = "DrugInfoDesc";
        private static string _URL = "DrugInfoURL";
        private static string _termLink = "DrugInfoTermLink";
        private static string _termLinkRef = "DrugInfoTermLinkRef";
        #endregion

        #region Public Properties
        public static string Title
        { get { return _title; } }

        public static string Description
        { get { return _description; } }

        public static string URL
        { get { return _URL; } }

        public static string TermLink
        { get { return _termLink; } }

        public static string TermLinkRef
        { get { return _termLinkRef; } }
        #endregion
    }

    public class GenProfXPath
    {
        #region Fields
        private static string _id = "GenProfID";
        private static string _firstName = "GenProfFirstName";
        private static string _lastName = "GenProfLastName";
        private static string _shortName = "GenProfShortName";
        private static string _suffix = "GenProfSuffix";
        private static string _degree = "GenProfDegree";
        private static string _specialty = "GenProfSpecialty";
        private static string _location = "GenProfLocation";
        private static string _state = "GenProfState";
        private static string _zip = "GenProfZip";
        private static string _country = "GenProfCountry";
        private static string _city = "GenProfCity";
        private static string _familySyndrome = "GenProfFamilySyndrome";
        private static string _syndromeName = "GenProfSyndromeName";
        private static string _cancerType = "GenProfCancerType";
        private static string _cancerTypeName = "GenProfCancerTypeName";
        private static string _cancerSite = "GenProfCancerSite";
        private static string _cdrID = "GenProfCDRID";
        #endregion

        #region Public Properties
        public static string ID
        { get { return _id; } }

        public static string FirstName
        { get { return _firstName; } }

        public static string LastName
        { get { return _lastName; } }

        public static string ShortName
        { get { return _shortName; } }

        public static string Suffix
        { get { return _suffix; } }

        public static string Degree
        { get { return _degree; } }

        public static string Specialty
        { get { return _specialty; } }

        public static string Location
        { get { return _location; } }

        public static string State
        { get { return _state; } }

        public static string Zip
        { get { return _zip; } }

        public static string Country
        { get { return _country; } }

        public static string City
        { get { return _city; } }

        public static string FamilySyndrome
        { get { return _familySyndrome; } }

        public static string SyndromeName
        { get { return _syndromeName; } }

        public static string CancerType
        { get { return _cancerType; } }

        public static string CancerTypeName
        { get { return _cancerTypeName; } }

        public static string CancerSite
        { get { return _cancerSite; } }

        public static string CDRID
        { get { return _cdrID; } }


        #endregion
    }


    public class ProtocolXPath
    {
        #region Fields
        // Below are XPathes shared by both Protocol and CTGovProtocol
        private static string _status = "ProtocolStatus";
        private static string _eligibility = "ProtocolEligibility";
        private static string _sponsor = "ProtocolSponsor";
        private static string _primaryID = "ProtocolPrimaryID";
        private static string _alternateID = "ProtocolAlternateID";
        private static string _IDString = "ProtocolIDString";
        private static string _IDType = "ProtocolIDType";
        private static string _cancerType1 = "ProtocolCancerTypeI";
        private static string _cancerType2 = "ProtocolCancerTypeII";
        private static string _cancerType3 = "ProtocolCancerTypeIII";
        private static string _cancerType4 = "ProtocolCancerTypeIV";
        private static string _modalityType1 = "ProtocolModalityTypeI";
        private static string _modalityType2 = "ProtocolModalityTypeII";
        private static string _modalityType3 = "ProtocolModalityTypeIII";
        private static string _durg = "ProtocolDrug";
        private static string _site = "ProtocolSite";
        private static string _siteName = "ProtocolSiteName";
        private static string _sitePerson = "ProtocolSitePerson";
        private static string _legacyPDQID = "ProtocolLegacyPDQID";
        private static string _studyCategory = "ProtocolStudyCategory";
        private static string _phase = "ProtocolPhase";
        private static string _specialCategory = "ProtocolSpecialCategory";
        private static string _title = "ProtocolTitle";
        private static string _pdqTitle = "ProtocolTitlePDQ";
        private static string _leadOrg = "ProtocolLeadOrg";
        private static string _leadOrgName = "ProtocolLeadOrgName";
        private static string _leadOrgRole = "ProtocolLeadOrgRole";
        private static string _person = "ProtocolPerson";
        private static string _givenName = "ProtocolPersonGivenName";
        private static string _surName = "ProtocolPersonSurName";
        private static string _profSuffix = "ProtocolPersonProfSuffix";
        private static string _role = "ProtocolPersonRole";
        private static string _city = "ProtocolPersonCity";
        private static string _state = "ProtocolPersonState";
        private static string _stateID = "ProtocolPersonStateID";
        private static string _country = "ProtocolPersonCountry";
        private static string _zipCode = "ProtocolPersonZipCode";
        private static string _ageRange = "ProtocolAgeRange";
        private static string _lowAge = "ProtocolLowAge";
        private static string _highAge = "ProtocolHighAge";
        private static string _sitePersonRef = "ProtocolSitePersonRef";
        private static string _leadOrgRef = "ProtocolLeadOrgRef";
        private static string _siteRef = "ProtocolSiteRef";
        private static string _titleAudience = "ProtocolTitleAudience";
        private static string _drugRef = "ProtocolDrugRef";
        private static string _modalityRef = "ProtocolModalityRef";
        private static string _cancerTypeRef = "ProtocolCancerTypeRef";
        private static string _stateRef = "ProtocolStateRef";
        private static string _primaryUrlID = "ProtocolPrimaryUrlID";
        private static string _secondaryUrlID = "ProtocolSecondaryUrlID";


        // Following XPathes are used by CTGovProtocol
        private static string _ctBriefTitle = "ProtocolBriefTitle";
        private static string _ctLastModifiedDate = "ProtocolLastModifiedDate";
        private static string _ctOfficialTitle = "ProtocolOfficialTitle";
        private static string _ctStudyID = "ProtocolOrgStudyID";
        private static string _ctSecondaryID = "ProtocolSecondaryID";
        private static string _ctNCIID = "ProtocolNCIID";
        private static string _ctGivenName = "ProtocolGivenName";
        private static string _ctSurName = "ProtocolSurName";
        private static string _ctSuffix = "ProtocolSuffix";
        private static string _ctRole = "ProtocolRole";
        private static string _ctPhone = "ProtocolPhone";
        private static string _ctPhoneExt = "ProtocolPhoneExt";
        private static string _ctLeadSponsor = "ProtocolLeadSponsor";
        private static string _ctOverallContact = "ProtocolOverallContact";
        private static string _ctOverallBackup = "ProtocolOverallBackup";
        private static string _ctOfficial = "ProtocolOfficial";
        private static string _ctFacilityName = "ProtocolFacilityName";
        private static string _ctState = "ProtocolState";
        private static string _ctCity = "ProtocolCity";
        private static string _ctCountry = "ProtocolCountry";
        private static string _ctZip = "ProtocolZip";
        private static string _ctFacility = "ProtocolFacility";
        private static string _ctContact = "ProtocolCTGovContact";
        private static string _ctContactBackup = "ProtocolCTGovContactBackup";
        private static string _ctInvestigator = "ProtocolInvestigator";
        private static string _ctSponsor = "ProtocolCTGovSponsor";
        private static string _ctLocation = "ProtocolLocation";
        private static string _ctPDQSponsor = "ProtocolPDQSponsor";
        private static string _ctStatus = "ProtocolCTGovStatus";
        #endregion

        #region Public Properties
        // Below are all the variables for xpath 
        public static string Status
        { get { return _status; } }

        public static string Eligibility
        { get { return _eligibility; } }

        public static string Sponsor
        { get { return _sponsor; } }

        public static string PrimaryID
        { get { return _primaryID; } }

        public static string AlternateID
        { get { return _alternateID; } }

        public static string IDString
        { get { return _IDString; } }

        public static string IDType
        { get { return _IDType; } }

        public static string CancerType1
        { get { return _cancerType1; } }

        public static string CancerType2
        { get { return _cancerType2; } }

        public static string CancerType3
        { get { return _cancerType3; } }

        public static string CancerType4
        { get { return _cancerType4; } }

        public static string ModalityType1
        { get { return _modalityType1; } }

        public static string ModalityType2
        { get { return _modalityType2; } }

        public static string ModalityType3
        { get { return _modalityType3; } }

        public static string Durg
        { get { return _durg; } }

        public static string Site
        { get { return _site; } }

        public static string SiteName
        { get { return _siteName; } }

        public static string SitePerson
        { get { return _sitePerson; } }

        public static string LegacyPDQID
        { get { return _legacyPDQID; } }

        public static string StudyCategory
        { get { return _studyCategory; } }

        public static string Phase
        { get { return _phase; } }

        public static string SpecialCategory
        { get { return _specialCategory; } }

        public static string Title
        { get { return _title; } }

        public static string PDQTitle
        { get { return _pdqTitle; } }

        public static string LeadOrg
        { get { return _leadOrg; } }

        public static string LeadOrgName
        { get { return _leadOrgName; } }

        public static string LeadOrgRole
        { get { return _leadOrgRole; } }

        public static string Person
        { get { return _person; } }

        public static string GivenName
        { get { return _givenName; } }

        public static string SurName
        { get { return _surName; } }

        public static string ProfSuffix
        { get { return _profSuffix; } }

        public static string Role
        { get { return _role; } }

        public static string City
        { get { return _city; } }

        public static string State
        { get { return _state; } }

        public static string StateID
        { get { return _stateID; } }

        public static string Country
        { get { return _country; } }

        public static string ZipCode
        { get { return _zipCode; } }

        public static string AgeRange
        { get { return _ageRange; } }

        public static string LowAge
        { get { return _lowAge; } }

        public static string HighAge
        { get { return _highAge; } }

        public static string SitePersonRef
        { get { return _sitePersonRef; } }

        public static string LeadOrgRef
        { get { return _leadOrgRef; } }

        public static string SiteRef
        { get { return _siteRef; } }

        public static string TitleAudience
        { get { return _titleAudience; } }

        public static string DrugRef
        { get { return _drugRef; } }

        public static string ModalityRef
        { get { return _modalityRef; } }

        public static string CancerTypeRef
        { get { return _cancerTypeRef; } }

        public static string StateRef
        { get { return _stateRef; } }

        public static string PrimaryUrlID
        { get { return _primaryUrlID; } }

        public static string SecondaryUrlID
        { get { return _secondaryUrlID; } }

        public static string CTBriefTitle
        { get { return _ctBriefTitle; } }

        public static string CTLastModifiedDate
        { get { return _ctLastModifiedDate; } }

        public static string CTOfficialTitle
        { get { return _ctOfficialTitle; } }

        public static string CTOrgStudyID
        { get { return _ctStudyID; } }

        public static string CTSecondaryID
        { get { return _ctSecondaryID; } }

        public static string CTNCIID
        { get { return _ctNCIID; } }

        public static string CTGivenName
        { get { return _ctGivenName; } }

        public static string CTSurName
        { get { return _ctSurName; } }

        public static string CTSuffix
        { get { return _ctSuffix; } }

        public static string CTRole
        { get { return _ctRole; } }

        public static string CTPhone
        { get { return _ctPhone; } }

        public static string CTPhoneExt
        { get { return _ctPhoneExt; } }

        public static string CTLeadSponsor
        { get { return _ctLeadSponsor; } }

        public static string CTOverallContact
        { get { return _ctOverallContact; } }

        public static string CTOverallBackup
        { get { return _ctOverallBackup; } }

        public static string CTOfficial
        { get { return _ctOfficial; } }

        public static string CTFacilityName
        { get { return _ctFacilityName; } }

        public static string CTState
        { get { return _ctState; } }

        public static string CTCity
        { get { return _ctCity; } }

        public static string CTCountry
        { get { return _ctCountry; } }

        public static string CTZip
        { get { return _ctZip; } }

        public static string CTFacility
        { get { return _ctFacility; } }

        public static string CTGovContact
        { get { return _ctContact; } }

        public static string CTGovContactBackup
        { get { return _ctContactBackup; } }

        public static string CTInvestigator
        { get { return _ctInvestigator; } }

        public static string CTSponsor
        { get { return _ctSponsor; } }

        public static string CTLocation
        { get { return _ctLocation; } }

        public static string CTPDQSponsor
        { get { return _ctPDQSponsor; } }

        public static string CTGovStatus
        { get { return _ctStatus; } }

        #endregion
    }

    public class PoliticalSubUnitXPath
    {
        #region Fields
        private static string _shortName = "PoliticalSubUnitShortName";
        private static string _fullName = "PoliticalSubUnitFullName";
        private static string _countryName = "PoliticalSubUnitCountryName";
        private static string _countryRef = "PoliticalSubUnitCountryRef";
        #endregion

        #region Public Properties
        public static string ShortName
        { get { return _shortName; } }

        public static string FullName
        { get { return _fullName; } }

        public static string CountryName
        { get { return _countryName; } }

        public static string CountryRef
        { get { return _countryRef; } }

        #endregion
    }

    public class OrganizationXPath
    {
        #region Fields
        private static string _shortName = "OrganizationShortName";
        private static string _officialName = "OrganizationOfficialName";
        private static string _alterName = "OrganizationAlterName";
        #endregion

        #region Public Properties
        public static string ShortName
        { get { return _shortName; } }

        public static string OfficialName
        { get { return _officialName; } }

        public static string AlterName
        { get { return _alterName; } }

        #endregion
    }

    public class MediaXPath
    {
        #region Fields
        private static string _type = "MediaType";
        private static string _size = "MediaSize";
        private static string _encodingType = "MediaEncodingType";
        #endregion

        #region Public Properties
        public static string Type
        { get { return _type; } }

        public static string Size
        { get { return _size; } }

        public static string Encoding
        { get { return _encodingType; } }

        #endregion
    }

    public class RelatedInformationXPath
    {
        public static string RelatedExternalRef = "RelatedInformationExternalRef";
        public static string RelatedSummaryRef = "RelatedInformationSummaryRef";
        public static string RelatedDrugSummaryRef = "RelatedInformationDrugSummaryRef";
        public static string XRef = "RelatedInformationXRef";
        public static string HRef = "RelatedInformationHRef";
        public static string UseWith = "RelatedInformationUseWith";
    }
}

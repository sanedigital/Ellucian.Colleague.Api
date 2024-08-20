// Copyright 2023 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Api
{
    /// <summary>
    /// Maintains the accept header constants. Some of them have a version placeholder that is expected to be replaced
    /// </summary>
    public class RouteConstants
    {
        /// <summary>
        /// "application/vnd.hedtech.integration.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationMediaTypeFormat = "application/vnd.hedtech.integration.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.maximum.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationMaximumMediaTypeFormat = "application/vnd.hedtech.integration.maximum.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.sections-maximum.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationSectionsMaximumMediaTypeFormat = "application/vnd.hedtech.integration.sections-maximum.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.vendors-maximum.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationVendorsMaximumMediaTypeFormat = "application/vnd.hedtech.integration.vendors-maximum.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.minimum.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationMinimumMediaTypeFormat = "application/vnd.hedtech.integration.minimum.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.room-availability.v{0}+json";
        /// </summary>
        public const string HedtechIntegrationRoomAvailabilityQapiMediaTypeFormat = "application/vnd.hedtech.integration.room-availability.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.room-minimum.v{0}+json";
        /// </summary>
        public const string HedtechIntegrationRoomMinimumQapiMediaTypeFormat = "application/vnd.hedtech.integration.room-minimum.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.afa_transactions.v{0}+json";
        /// </summary>
        public const string HedtechIntegrationAfaTransactionsQapiMediaTypeFormat = "application/vnd.hedtech.integration.afa_transactions.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.student-transcript-grades-options.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationStudentTranscriptGradesOptionsFormat = "application/vnd.hedtech.integration.student-transcript-grades-options.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.student-unverified-grades-submissions.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationStudentUnverifiedGradesSubmissionsFormat = "application/vnd.hedtech.integration.student-unverified-grades-submissions.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.student-academic-programs-submissions.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationStudentAcademicProgramSubmissionsFormat = "application/vnd.hedtech.integration.student-academic-programs-submissions.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.student-academic-programs-replacements.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationStudentAcademicProgramReplacements = "application/vnd.hedtech.integration.student-academic-programs-replacements.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.student-transcript-grades-adjustments.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationStudentTranscriptGradesAdjustmentsFormat = "application/vnd.hedtech.integration.student-transcript-grades-adjustments.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.section-registrations-grade-options.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationSectionRegistrationGradeOptionsFormat = "application/vnd.hedtech.integration.section-registrations-grade-options.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.admission-applications-submissions.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationAdmissionApplicationsSubmissionsFormat = "application/vnd.hedtech.integration.admission-applications-submissions.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.prospect-opportunities-submissions.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationProspectOpportunitiesSubmissionsFormat = "application/vnd.hedtech.integration.prospect-opportunities-submissions.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.person-matching-requests-initiations-prospects.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationPersonMatchingRequestsInitiationsProspectsFormat = "application/vnd.hedtech.integration.person-matching-requests-initiations-prospects.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.compound-configuration-settings-options.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationCompoundConfigurationSettingsOptionsFormat = "application/vnd.hedtech.integration.compound-configuration-settings-options.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.configuration-settings-options.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationConfigurationSettingsOptionsFormat = "application/vnd.hedtech.integration.configuration-settings-options.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.default-settings-options.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationDefaultSettingsOptionsFormat = "application/vnd.hedtech.integration.default-settings-options.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.default-settings-advanced-search-options.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationDefaultSettingsAdvancedSearchOptionsFormat = "application/vnd.hedtech.integration.default-settings-advanced-search-options.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.mapping-settings-options.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationMappingSettingsOptionsFormat = "application/vnd.hedtech.integration.mapping-settings-options.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.collection-configuration-settings-options.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationCollectionConfigurationSettingsOptionsFormat = "application/vnd.hedtech.integration.collection-configuration-settings-options.v{0}+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.bulk-requests.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationBulkRequestMediaTypeFormat = "application/vnd.hedtech.integration.bulk-requests.v{0}+json";
        /// <summary>
        /// "application/vnd.oai.openapi.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationOpenApiMetdataTypeFormat = "application/vnd.oai.openapi.v{0}+json";
        /// <summary>
        /// "application/vnd.oai.openapi.publish.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationOpenApiMetdataPublishTypeFormat = "application/vnd.oai.openapi.publish.v{0}+json";
        /// <summary>
        /// "application/vnd.oai.openapi+json"
        /// </summary>
        public const string HedtechIntegrationDefaultOpenApiMetdataTypeFormat = "application/vnd.oai.openapi+json";
        /// <summary>
        /// "application/vnd.hedtech.integration.person-filters-persons.v{0}+json"
        /// </summary>
        public const string HedtechIntegrationPersonFiltersPersonsFormat = "application/vnd.hedtech.integration.person-filters-persons.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian.v{0}+pdf"
        /// </summary>
        public const string EllucianPDFMediaTypeFormat = "application/vnd.ellucian.v{0}+pdf";
        /// <summary>
        /// "application/vnd.ellucian-pilot.v{0}+json"
        /// </summary>
        public const string EllucianJsonPilotMediaTypeFormat = "application/vnd.ellucian-pilot.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-ilp.v{0}+json"
        /// </summary>
        public const string EllucianJsonIlpMediaTypeFormat = "application/vnd.ellucian-ilp.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-human-resource-demographics.v{0}+json"
        /// </summary>
        public const string EllucianHumanResourceDemographicsTypeFormat = "application/vnd.ellucian-human-resource-demographics.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-step-up-authentication.v{0}+json"
        /// </summary>
        public const string EllucianStepUpAuthenticationFormat = "application/vnd.ellucian-step-up-authentication.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-configuration.v{0}+json"
        /// </summary>
        public const string EllucianConfigurationFormat = "application/vnd.ellucian-configuration.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-proxy-user.v{0}+json"
        /// </summary>
        public const string EllucianProxyUserFormat = "application/vnd.ellucian-proxy-user.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-with-invalid-keys.v{0}+json"
        /// </summary>
        public const string EllucianInvalidKeysFormat = "application/vnd.ellucian-with-invalid-keys.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-person-search-exact-match.v{0}+json"
        /// </summary>
        public const string EllucianPersonSearchExactMatchFormat = "application/vnd.ellucian-person-search-exact-match.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-retention-alert-case-note.v{0}+json"
        /// </summary>
        public const string EllucianRetentionAlertCaseNoteFormat = "application/vnd.ellucian-retention-alert-case-note.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-retention-alert-case-followup.v{0}+json"
        /// </summary>
        public const string EllucianRetentionAlertCaseFollowUpFormat = "application/vnd.ellucian-retention-alert-case-followup.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-retention-alert-case-comm-code.v{0}+json"
        /// </summary>
        public const string EllucianRetentionAlertCaseCommCodeFormat = "application/vnd.ellucian-retention-alert-case-comm-code.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-retention-alert-case-type.v{0}+json"
        /// </summary>
        public const string EllucianRetentionAlertCaseTypeFormat = "application/vnd.ellucian-retention-alert-case-type.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-retention-alert-case-priority.v{0}+json"
        /// </summary>
        public const string EllucianRetentionAlertCasePriorityFormat = "application/vnd.ellucian-retention-alert-case-priority.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-retention-alert-case-close.v{0}+json"
        /// </summary>
        public const string EllucianRetentionAlertCaseCloseFormat = "application/vnd.ellucian-retention-alert-case-close.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-retention-alert-case-set-reminder.v{0}+json"
        /// </summary>
        public const string EllucianRetentionAlertCaseSetReminderFormat = "application/vnd.ellucian-retention-alert-case-set-reminder.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-retention-alert-manage-reminders.v{0}+json"
        /// </summary>
        public const string EllucianRetentionAlertCaseManageRemindersFormat = "application/vnd.ellucian-retention-alert-manage-reminders.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-retention-alert-case-send-mail.v{0}+json"
        /// </summary>
        public const string EllucianRetentionAlertCaseSendMailFormat = "application/vnd.ellucian-retention-alert-case-send-mail.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-retention-alert-case-reassign.v{0}+json"
        /// </summary>
        public const string EllucianRetentionAlertCaseReassignFormat = "application/vnd.ellucian-retention-alert-case-reassign.v{0}+json";
        /// <summary>
        /// "application/vnd.ellucian-instant-enrollment.v{0}+json"
        /// </summary>
        public const string EllucianInstantEnrollmentFormat = "application/vnd.ellucian-instant-enrollment.v{0}+json";

    }
}

// Copyright 2014-2023 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ellucian.Colleague.Api.Models;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Utility
{
    /// <summary>
    /// Static helper class for the Integration API
    /// </summary>
    public static class IntegrationApiUtility
    {
        private static Dictionary<string, IntegrationApiErrorMessage> _errorMessages { get; set; }

        static IntegrationApiUtility()
        {
            //_errorMessages = new Dictionary<string, IntegrationApiErrorMessage>();
            _errorMessages = BuildErrorTable();
        }

        /// <summary>
        /// Default error code
        /// </summary>
        private static string DefaultErrorCode = "Global.Internal.Error";

        /// <summary>
        /// Default Error message for not supported messages
        /// </summary>
        public static string DefaultNotSupportedApiErrorMessage
        {
            get { return defaultNotSupportedApiErrorMessage; }
            set { defaultNotSupportedApiErrorMessage = value; }
        }
        private static string defaultNotSupportedApiErrorMessage = "The method is not supported by the resource.";

        /// <summary>
        /// Default error message
        /// </summary>
        public static IntegrationApiErrorMessage DefaultMessage
        {
            get
            {
                // The first message in the table is the default
                return _errorMessages[DefaultErrorCode];
            }
        }

        /// <summary>
        /// Default API error
        /// </summary>
        public static IntegrationApiError DefaultApiError
        {
            get
            {
                return new IntegrationApiError(DefaultErrorCode, DefaultMessage.Description, "", DefaultMessage.ReturnCode);
            }
        }


        /// <summary>
        /// Default API error
        /// </summary>
        public static IntegrationApiError DefaultNotSupportedApiError
        {
            get
            {
                return new IntegrationApiError(DefaultErrorCode, DefaultMessage.Description, DefaultNotSupportedApiErrorMessage, HttpStatusCode.MethodNotAllowed);
            }
        }


        /// <summary>
        /// Static helper method to get the default API error with a user-provided message
        /// </summary>
        /// <param name="message">Message from caller</param>
        /// <returns>An IntegrationApiError</returns>
        public static IntegrationApiError GetDefaultApiError(string message)
        {
            var error = DefaultApiError;
            error.Message = string.IsNullOrEmpty(message) ? null : message.Replace("\r\n", "  ");

            return error;
        }

        /// <summary>
        /// Static helper method to convert a repository exception into an integration API exception
        /// </summary>
        /// <param name="ex">The repository exception</param>
        /// <returns>An integration API exception</returns>
        public static IntegrationApiException ConvertToIntegrationApiException(RepositoryException ex)
        {
            var iae = new IntegrationApiException();
            iae.AddErrors(ex.Errors.ToList().ConvertAll(x => ConvertToIntegrationApiError(x)));

            //Depending on how the RepositoryException is built, the parent object may have an associated error message.
            //make sure we are not generating an IntegrationApiException for the default message.  
            if ((!string.IsNullOrEmpty(ex.Message)) && !ex.Message.Equals("Repository exception", StringComparison.OrdinalIgnoreCase)
                && (!ex.Message.Equals(new RepositoryException().Message, StringComparison.OrdinalIgnoreCase))
                && (!iae.Errors.Any(e => !string.IsNullOrEmpty(e.Message) 
                     && e.Message.Equals(ex.Message, StringComparison.OrdinalIgnoreCase))))
            {
                iae.AddError(GetDefaultApiError(ex.Message));
            }
            return iae;
        }

        /// <summary>
        /// Static helper method to convert a repository error into an integration API error
        /// </summary>
        /// <param name="error">A repository error</param>
        /// <returns>An integration API error</returns>
        public static IntegrationApiError ConvertToIntegrationApiError(RepositoryError error)
        {
            var errorMessage = GetMessage(error.Code);
            return new IntegrationApiError(
                error.Code,
                errorMessage.Description,
                error.Message,
                errorMessage.ReturnCode,
                string.IsNullOrEmpty(error.Id) ? null : error.Id,
                string.IsNullOrEmpty(error.SourceId) ? null : error.SourceId);
        }

        /// <summary>
        /// Static helper method to convert a permissions exception into an integration API exception
        /// </summary>
        /// <param name="ex">The permissions exception</param>
        /// <returns>An integration API exception</returns>
        public static IntegrationApiException ConvertToIntegrationApiException(PermissionsException ex)
        {
            return new IntegrationApiException("Permissions error",
                    IntegrationApiUtility.PopulateError(new IntegrationApiError("Global.Client.UnauthorizedOperation", message:ex.Message)));
        }

        /// <summary>
        /// Static helper method to convert an argument exception into an integration API exception
        /// </summary>
        /// <param name="ex">The argument exception</param>
        /// <returns>An integration API exception</returns>
        public static IntegrationApiException ConvertToIntegrationApiException(ArgumentException ex)
        {
            return new IntegrationApiException("Argument exception", IntegrationApiUtility.GetDefaultApiError(ex.Message));
        }

        /// <summary>
        /// Static helper method to take an integration API exception and format it for output
        /// </summary>
        /// <param name="ex">The repository exception</param>
        /// <returns>An integration API exception</returns>
        public static IntegrationApiException ConvertToIntegrationApiException(IntegrationApiException ex)
        {
            return PopulateExceptionErrors(ex);
        }

        /// <summary>
        /// Static helper method to convert a generic exception into an integration API exception
        /// </summary>
        /// <param name="ex">The generic exception</param>
        /// <returns>An integration API exception</returns>
        public static IntegrationApiException ConvertToIntegrationApiException(Exception ex)
        {
            return new IntegrationApiException("Other Exception", IntegrationApiUtility.GetDefaultApiError(ex.Message));
        }

        /// <summary>
        /// Static helper method to take an integration API exception, populate all the messages in it,
        /// and return another IntegrationApiException
        /// </summary>
        /// <param name="ex">The source exception</param>
        /// <returns>A new exception with populated errors</returns>
        public static IntegrationApiException PopulateExceptionErrors(IntegrationApiException ex)
        {
            var iae = new IntegrationApiException(ex.Message, ex.InnerException);
            foreach (var error in ex.Errors)
            {
                // Do not include any errors where
                // 1.  The message simply states "repository exception"
                // 2.  The message is empty/null
                // 3.  The exact message is duplicated.
                if ((!string.IsNullOrEmpty(error.Message)) 
                    && !error.Message.Equals("Repository exception", StringComparison.OrdinalIgnoreCase)
                    && (!error.Message.Equals(new RepositoryException().Message, StringComparison.OrdinalIgnoreCase))
                    && (!iae.Errors.Any(e => !string.IsNullOrEmpty(e.Message)
                    &&      e.Message.Equals(error.Message, StringComparison.OrdinalIgnoreCase)
                    &&      !string.IsNullOrEmpty(e.Id)
                    &&      e.Id.Equals(error.Id, StringComparison.OrdinalIgnoreCase)
                    &&      !string.IsNullOrEmpty(e.Guid)
                    &&      e.Guid.Equals(error.Guid, StringComparison.InvariantCultureIgnoreCase))))
                {
                    iae.AddError(PopulateError(error));
                }              
            }

            return iae;
        }

        /// <summary>
        /// Static helper method to populate the description for an API error
        /// </summary>
        /// <param name="error">Current API error</param>
        /// <returns>Fully populated API error</returns>
        public static IntegrationApiError PopulateError(IntegrationApiError error)
        {
            var apiError = GetMessage(error.Code, error.StatusCode);
            return new IntegrationApiError(error.Code,                     
                string.IsNullOrEmpty(error.Description) ? apiError.Description : error.Description,
                string.IsNullOrEmpty(error.Message) ? null : error.Message.Replace("\r\n","  "),
                apiError.ReturnCode,
                string.IsNullOrEmpty(error.Guid) ? null : error.Guid,
                string.IsNullOrEmpty(error.Id) ? null : error.Id);
        }

        private static IntegrationApiErrorMessage GetMessage(string code, HttpStatusCode? httpStatusCodeOveride = null)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }

            IntegrationApiErrorMessage message = null;
            if (_errorMessages.TryGetValue(code, out message))
            {
                if (httpStatusCodeOveride != null)
                {
                    message.ReturnCode = (HttpStatusCode)httpStatusCodeOveride;
                }
                return message;
            }

            return new IntegrationApiErrorMessage(code, "Unknown error code.", HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// List of standard error messages for the Integration API
        /// </summary>
        public static List<IntegrationApiErrorMessage> ApiErrorMessages
        {
            get { return apiErrorMessages; }
            set { apiErrorMessages = value; }
        }
        private static List<IntegrationApiErrorMessage> apiErrorMessages = new List<IntegrationApiErrorMessage>() {
            new IntegrationApiErrorMessage( "Authentication.Required                                               ", "Authentication failed or wasn't provided.                                                                                                                                          ", 401 ),
            new IntegrationApiErrorMessage( "Access.Denied                                                         ", "Client not authorized to perform this action.                                                                                                                                      ", 403 ),
            new IntegrationApiErrorMessage( "Global.Internal.Error                                                 ", "Unspecified Error on the system which prevented execution.                                                                                                                         ", 400 ),
            new IntegrationApiErrorMessage( "Global.SchemaValidation.Error                                         ", "Errors parsing input JSON.                                                                                                                                                         ", 400 ),
            new IntegrationApiErrorMessage( "Global.Client.UnauthorizedOperation                                   ", "Client not authorized to perform this action.                                                                                                                                      ", 403 ),
            new IntegrationApiErrorMessage( "Missing.Required.Property                                             ", "Property is required by the schema.                                                                                                                                                ", 400 ),
            new IntegrationApiErrorMessage( "Validation.Exception                                                  ", "An error occurred attempting to validate data.                                                                                                                                     ", 400 ),
            new IntegrationApiErrorMessage( "Key.Not.Found                                                         ", "The provided key was not found in the database.                                                                                                                                    ", 404 ),
            new IntegrationApiErrorMessage( "GUID.Not.Found                                                        ", "An error occurred translating the GUID to an ID.                                                                                                                                   ", 404 ),
            new IntegrationApiErrorMessage( "Bad.Data                                                              ", "Data issue must be resolved in the source system.                                                                                                                                  ", 400 ),
            new IntegrationApiErrorMessage( "GUID.Wrong.Type                                                       ", "The provided GUID is the wrong type for this resource.                                                                                                                             ", 400 ),
            new IntegrationApiErrorMessage( "Create.Update.Exception                                               ", "Error occurred in the source system that prevented update or creation of the record.                                                                                               ", 400 ),
            new IntegrationApiErrorMessage( "Create.Update.Success.Exception                                       ", "The Create or Update request was successful but the system was unable to return a proper response.                                                                                 ", 404 ),
            new IntegrationApiErrorMessage( "Delete.Exception                                                      ", "Error occurred in the source system that prevented deletion of the record.                                                                                                         ", 400 ),
            new IntegrationApiErrorMessage( "Missing.Request.ID                                                    ", "Empty request ID.                                                                                                                                                                  ", 400 ),
            new IntegrationApiErrorMessage( "Missing.Request.Body                                                  ", "	Empty request body.                                                                                                                                                               ", 400 ),
            new IntegrationApiErrorMessage( "Course.NotFound                                                       ", "Course does not exist.                                                                                                                                                             ", 404 ),
            new IntegrationApiErrorMessage( "Course.Duplicate                                                      ", "Course already exists in system.                                                                                                                                                   ", 400 ),
            new IntegrationApiErrorMessage( "Course.Locked                                                         ", "Course cannot be updated due to record lock.                                                                                                                                       ", 400 ),
            new IntegrationApiErrorMessage( "Course.Status.OutOfRange                                              ", "Course status is not a valid status.                                                                                                                                               ", 400 ),
            new IntegrationApiErrorMessage( "Section.NotFound                                                      ", "Section does not exist.                                                                                                                                                            ", 404 ),
            new IntegrationApiErrorMessage( "Section.Course.NotFound                                               ", "Course does not exist.                                                                                                                                                             ", 404 ),
            new IntegrationApiErrorMessage( "Section.Course.NotSpecified                                           ", "Course was not specified.                                                                                                                                                          ", 400 ),
            new IntegrationApiErrorMessage( "Section.StartDate.NotSpecified                                        ", "Start date was not specified.                                                                                                                                                      ", 400 ),
            new IntegrationApiErrorMessage( "Section.StartDate.OutOfRange                                          ", "Section start date is not valid for the course.                                                                                                                                    ", 400 ),
            new IntegrationApiErrorMessage( "Section.Course.Status.Invalid                                         ", "Course status is not valid.                                                                                                                                                        ", 400 ),
            new IntegrationApiErrorMessage( "Section.Duplicate                                                     ", "Section already exists in system.                                                                                                                                                  ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.NotFound                                           ", "Instructional event not found.                                                                                                                                                     ", 404 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.Location.RoomNotFound                              ", "Room not found.                                                                                                                                                                    ", 404 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.Location.RoomScheduleConflict                      ", "Unable to use this room due to something else using this room during the same time.                                                                                                ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.Location.InsufficientRoomCapacity                  ", "maxEnrollment is greater than the room capcity.                                                                                                                                    ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.Section.NotFound                                   ", "Section not found.                                                                                                                                                                 ", 404 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.StartTime.OutOfRange                               ", "Start time is out of range.                                                                                                                                                        ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.EndTime.OutOfRange                                 ", "End time is out of range.                                                                                                                                                          ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.StartDate.OutOfRange                               ", "Start date is out of range.                                                                                                                                                        ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.EndDate.OutOfRange                                 ", "End date is out of range.                                                                                                                                                          ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.Recurrence.OutOfRange                              ", "Instructional-Events recurrences by Day includes a day of the week, which is not valid for the start and end date range.                                                           ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.InvalidInstructionalMethod.NotFound                ", "Instructional method not found.                                                                                                                                                    ", 404 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.InstructionalMethod.CourseConstraint               ", "Instructional method is not a valid choice based on the Course.                                                                                                                    ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.ScheduleConflict                                   ", "Instructional event is not valid for this section due to overlapping time and date for times within this section.                                                                  ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.InstructorRoster.InstructorResponsibilityOutOfRange", "Responsibility percentages do not add up to 100.                                                                                                                                   ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.InstructorRoster.Instructor.ScheduleConflict       ", "Instructional event is not valid for this section's instructor roster due to overlapping time and date definitions with another instructional event for the same instructor roster.", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.InstructorRoster.Instructor.WorkloadExceeded       ", "Workload Value causes instructor to exceed limits.                                                                                                                                 ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.InstructorRoster.Instructor.InvalidRole            ", "Not Instructor or does not have the proper role to teach.                                                                                                                          ", 400 ),
            new IntegrationApiErrorMessage( "InstructionalEvent.InstructorRoster.Instructor.NotFound               ", "No person was found with the GUID.                                                                                                                                                 ", 404 ),
            new IntegrationApiErrorMessage( "RoomsAvailabilityRequest.InsufficientRoomCapacity                     ", "maxOccupancy is greater than the room capacity for all rooms.                                                                                                                      ", 400 ),
            new IntegrationApiErrorMessage( "RoomsAvailabilityRequest.NoMeetingDates                               ", "No dates were identified for the start and end dates, start and end times, and recurrence pattern.                                                                                 ", 400 ),
            new IntegrationApiErrorMessage( "RoomsAvailabilityRequest.Recurrences.NotSpecified                     ", "recurrence was not specified.                                                                                                                                                      ", 400 ),
            new IntegrationApiErrorMessage( "RoomsAvailabilityRequest.Recurrences.OutOfRange                       ", "RoomsAvailabilityRequest recurrences by Day includes a day of the week, which is not valid for the start and end date range.                                                       ", 400 ),
            new IntegrationApiErrorMessage( "RoomsAvailabilityRequest.Occupanices.NotSpecified                     ", "occupancies was not specified.                                                                                                                                                     ", 400 ),
            new IntegrationApiErrorMessage( "RoomsAvailabilityRequest.StartDate.OutOfRange                         ", "Start date is out of range.                                                                                                                                                        ", 400 ),
            new IntegrationApiErrorMessage( "RoomsAvailabilityRequest.StartTime.OutOfRange                         ", "Start time is out of range.                                                                                                                                                        ", 400 ),
            new IntegrationApiErrorMessage( "RoomsAvailabilityRequest.Recurrences.Days.NotSpecified                ", "byDay was not specified.                                                                                                                                                           ", 400 ),
            new IntegrationApiErrorMessage( "RoomsAvailabilityRequest.Recurrences.Interval.OutOfRange              ", "interval is out of range.                                                                                                                                                          ", 400 ),
            new IntegrationApiErrorMessage( "RoomsAvailabilityRequest.Occupancies.MaximumOccupancy.OutOfRange      ", "maxOccupancy is out of range.                                                                                                                                                      ", 400 )
        };

        private static Dictionary<string, IntegrationApiErrorMessage> BuildErrorTable()
        {
            return ApiErrorMessages.ToDictionary(x => x.Code, y => y);
        }
    }
}

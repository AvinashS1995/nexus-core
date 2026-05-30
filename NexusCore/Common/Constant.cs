using System;
namespace NexusCore.Common
{
    public static class Constants
    {
        public const string DateTimeFormat = "yyyyMMddHHmmss";

        public const string TempFolder = "TempFolder";

    }


    public enum StatusCodes
    {
        UnAuthorised = 1001,
        NoDataFound = 1002,
        SomethingWentWrong = 1003,
        ErrorHasOccured = 1004,

        LoginSuccessfully = 2001,
        InvalidPassword = 2002,
        MaximumAttemptsYourAccountHaslocked = 2003,
        EmployeeDoesNotExistWithThisEmployeeNo = 2004,
        DetailsDoesNotMatchWithThisEmployeeNo = 2005,
        EmployeeExist = 2006,
        EmployeeInactive = 2007,
        EmployeeLockedSuccessfully = 2008,
        EmployeeUnLockedSuccessfully = 2009,
        EmailAlreadyExist = 2010,
        LogOutSuccessfully = 2011,
        PasswordResetSuccessfully = 2012,
        YouCantReuseYourLastPasswords = 2013,
        PasswordExpired = 2014,
        RefreshTokenExpired = 2015,


        RecordFetchedSuccessfully = 3001,
        RecordSavedSuccessfully = 3002,
        ActionPerformedSuccessfully = 3003,
        RecordUpdatedSuccessfully = 3004,
        RecordDeletedSuccessfully = 3005,

        FileUploadSuccessfully = 4001,
        ErrorHasOccuredInFileUpload = 4002,
        FileDeletedSuccessfully = 4003,
        ErrorHasOccuredInFileDelete = 4004,
    }
}


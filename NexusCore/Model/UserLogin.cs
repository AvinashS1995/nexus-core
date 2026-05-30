using System;
using System.ComponentModel.DataAnnotations;

namespace NexusCore.Model
{
	public class UserLogin : IValidatableObject
    {
        [Required]
        public string EmpNo { get; set; }
        public string Password { get; set; }
        public string FrontendVersion { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (String.IsNullOrEmpty(EmpNo))
            {
                results.Add(new ValidationResult("Emp No Required."));
            }

            return results;
        }
    }

    public class LoginEmployeeViewDataModel
    {
        // Employee Identity
        public string EmpNo { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Pincode { get; set; }
        public string Gender { get; set; }
        public string Division { get; set; }
        public string Role { get; set; }
        public int RoleID { get; set; }
        public string Designation { get; set; }
        public int DesignationID { get; set; }
    }

    public class LoginResultModel
    {
        public bool IsSuccess { get; set; }
        public int ResponseCode { get; set; }
        public string Unlocktime { get; set; }
        public int RemainingAttempt { get; set; }
        public int CityID { get; set; }
        public string LoginEmpID { get; set; }
        public bool IsMasterLogin { get; set; }
        public string RefreshToken { get; set; }
        public string RefreshTokenExpiry { get; set; }
    }

    public class TokenData
    {
        public string AccessToken { get; set; }
        public DateTime ExpiresIn { get; set; }
    }

    public class LoginLogModel
    {
        public string EmpNo { get; set; }
        public string LoginEmpID { get; set; }
        public int? CityID { get; set; }
        public bool IsSuccess { get; set; }
        public int ResponseCode { get; set; }
        public int RemainingAttempt { get; set; }
        public string Unlocktime { get; set; }
        public bool IsMasterLogin { get; set; }
        public string SessionID { get; set; }
        public string RefreshToken { get; set; }
        public string RefreshTokenExpiry { get; set; }
        public string LoginIP { get; set; }
        public string DeviceInfo { get; set; }
        public string UserAgent { get; set; }
        public string Browser { get; set; }
        public string OS { get; set; }
        public string GeoLocation { get; set; }
        public string ISP { get; set; }
        public string CreatedBy { get; set; }
    }

}


using System.Collections.Generic;
using System.Linq;

namespace LegacyOnboarder.Models
{
    public static class EmployeeRequestRepository
    {
        // Pretend this is a database table
        private static readonly List<RequestRecord> _requests = new();

        public static (bool Successful, string[] Errors) Save(int requestId, RequestRecord record)
        {
            if (requestId > 0)
            {
                var existing = _requests.FirstOrDefault(r => r.Id == requestId);
                if (existing != null)
                {
                    // naïve field-by-field copy, no mapping layer
                    existing.EmployeeFirstName = record.EmployeeFirstName;
                    existing.EmployeeLastName  = record.EmployeeLastName;
                    existing.DepartmentId      = record.DepartmentId;
                    existing.EmployeeTypeId    = record.EmployeeTypeId;
                    existing.HiringManagerId   = record.HiringManagerId;
                    existing.StartDate         = record.StartDate;
                    existing.TerminationDate   = record.TerminationDate;
                    existing.TitleId           = record.TitleId;
                    existing.TitleDescription  = record.TitleDescription;
                    existing.Rehire            = record.Rehire;
                    existing.RequestStatus     = record.RequestStatus;
                    existing.IsEditing         = record.IsEditing;
                }
                else
                {
                    _requests.Add(record);
                }
            }
            else
            {
                record.Id = _requests.Count + 1;
                _requests.Add(record);
            }

            // Always "successful" for now, like a fake DAL
            return (true, new string[0]);
        }
    }
}
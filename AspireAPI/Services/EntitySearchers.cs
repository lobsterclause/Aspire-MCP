using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspireAPI.Models; // Assuming models are in AspireAPI.Models

namespace AspireAPI.Services
{
    /// <summary>
    /// Searches TimeEntries entities
    /// </summary>
    public class TimeEntrySearcher : EntitySearcherBase
    {
        public TimeEntrySearcher(AspireApiService aspireApi, ILogger<TimeEntrySearcher> logger) // Use specific logger type
            : base(aspireApi, logger)
        {
        }

        public override async Task<List<SearchResult>> SearchAsync(
            string searchTerm,
            string startDate,
            string endDate,
            int maxResults,
            CancellationToken cancellationToken)
        {
            // Fetch time entries from the API
            var timeEntries = await AspireApi.GetTimeEntriesAsync(
                startDate, endDate, null, null, null, cancellationToken);

            var results = new List<SearchResult>();

            foreach (var entry in timeEntries)
            {
                // Check if the time entry matches the search term
                var matchScore = CalculateTimeEntryMatchScore(entry, searchTerm);

                if (matchScore > 0)
                {
                    results.Add(new SearchResult
                    {
                        Id = entry.Id,
                        EntityType = "TimeEntries",
                        Title = $"{entry.EmployeeName} - {entry.Date:d}",
                        Description = entry.Notes ?? $"{entry.Hours} hours",
                        LastModified = entry.Date,
                        Url = $"/timeentries/{entry.Id}",
                        MatchScore = matchScore,
                        Data = new Dictionary<string, object>
                        {
                            ["EmployeeId"] = entry.EmployeeId,
                            ["EmployeeName"] = entry.EmployeeName,
                            ["Date"] = entry.Date,
                            ["Hours"] = entry.Hours,
                            ["Cost"] = entry.Cost,
                            ["Notes"] = entry.Notes,
                            ["ContactId"] = entry.ContactId,
                            ["ContactName"] = entry.ContactName,
                            ["DivisionId"] = entry.DivisionId,
                            ["DivisionName"] = entry.DivisionName
                        }
                    });

                    // Stop if we've reached the maximum results
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            // Sort results by match score (descending)
            return results.OrderByDescending(r => r.MatchScore).ToList();
        }

        private int CalculateTimeEntryMatchScore(TimeEntryDto entry, string searchTerm)
        {
            // Calculate match scores for different fields
            var employeeNameScore = CalculateMatchScore(entry.EmployeeName, searchTerm);
            var notesScore = CalculateMatchScore(entry.Notes, searchTerm);
            var contactNameScore = CalculateMatchScore(entry.ContactName, searchTerm);
            var divisionNameScore = CalculateMatchScore(entry.DivisionName, searchTerm);

            // Return the highest match score
            return Math.Max(
                Math.Max(employeeNameScore, notesScore),
                Math.Max(contactNameScore, divisionNameScore));
        }
    }

    /// <summary>
    /// Searches Contacts entities
    /// </summary>
    public class ContactSearcher : EntitySearcherBase
    {
        public ContactSearcher(AspireApiService aspireApi, ILogger<ContactSearcher> logger) // Use specific logger type
            : base(aspireApi, logger)
        {
        }

        public override async Task<List<SearchResult>> SearchAsync(
            string searchTerm,
            string startDate,
            string endDate,
            int maxResults,
            CancellationToken cancellationToken)
        {
            // Fetch contacts from the API
            var contacts = await AspireApi.GetContactsAsync(
                "all", searchTerm, 1, maxResults, cancellationToken);

            var results = new List<SearchResult>();

            foreach (var contact in contacts.Data)
            {
                // Check if the contact matches the search term
                var matchScore = CalculateContactMatchScore(contact, searchTerm);

                if (matchScore > 0)
                {
                    results.Add(new SearchResult
                    {
                        Id = contact.Id,
                        EntityType = "Contacts",
                        Title = contact.Name,
                        Description = $"{contact.Type}: {contact.Email ?? contact.Phone ?? "No contact info"}",
                        LastModified = null, // No last modified in the DTO
                        Url = $"/contacts/{contact.Id}",
                        MatchScore = matchScore,
                        Data = new Dictionary<string, object>
                        {
                            ["Name"] = contact.Name,
                            ["Type"] = contact.Type,
                            ["Email"] = contact.Email,
                            ["Phone"] = contact.Phone,
                            ["Address"] = contact.Address,
                            ["City"] = contact.City,
                            ["State"] = contact.State,
                            ["ZipCode"] = contact.ZipCode
                        }
                    });

                    // Stop if we've reached the maximum results
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            // Sort results by match score (descending)
            return results.OrderByDescending(r => r.MatchScore).ToList();
        }

        private int CalculateContactMatchScore(ContactDto contact, string searchTerm)
        {
            // Calculate match scores for different fields
            var nameScore = CalculateMatchScore(contact.Name, searchTerm);
            var emailScore = CalculateMatchScore(contact.Email, searchTerm);
            var phoneScore = CalculateMatchScore(contact.Phone, searchTerm);
            var addressScore = CalculateMatchScore(
                $"{contact.Address} {contact.City} {contact.State} {contact.ZipCode}",
                searchTerm);

            // Return the highest match score
            return Math.Max(
                Math.Max(nameScore, emailScore),
                Math.Max(phoneScore, addressScore));
        }
    }

    /// <summary>
    /// Searches Divisions entities
    /// </summary>
    public class DivisionSearcher : EntitySearcherBase
    {
        public DivisionSearcher(AspireApiService aspireApi, ILogger<DivisionSearcher> logger) // Use specific logger type
            : base(aspireApi, logger)
        {
        }

        public override async Task<List<SearchResult>> SearchAsync(
            string searchTerm,
            string startDate,
            string endDate,
            int maxResults,
            CancellationToken cancellationToken)
        {
            // Fetch divisions from the API
            var divisions = await AspireApi.GetDivisionsAsync(
                null, searchTerm, 1, maxResults, cancellationToken);

            var results = new List<SearchResult>();

            foreach (var division in divisions.Data)
            {
                // Check if the division matches the search term
                var matchScore = CalculateDivisionMatchScore(division, searchTerm);

                if (matchScore > 0)
                {
                    results.Add(new SearchResult
                    {
                        Id = division.Id,
                        EntityType = "Divisions",
                        Title = division.Name,
                        Description = division.Description ?? "No description",
                        LastModified = null, // No last modified in the DTO
                        Url = $"/divisions/{division.Id}",
                        MatchScore = matchScore,
                        Data = new Dictionary<string, object>
                        {
                            ["Name"] = division.Name,
                            ["Description"] = division.Description
                        }
                    });

                    // Stop if we've reached the maximum results
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            // Sort results by match score (descending)
            return results.OrderByDescending(r => r.MatchScore).ToList();
        }

        private int CalculateDivisionMatchScore(DivisionDto division, string searchTerm)
        {
            // Calculate match scores for different fields
            var nameScore = CalculateMatchScore(division.Name, searchTerm);
            var descriptionScore = CalculateMatchScore(division.Description, searchTerm);

            // Return the highest match score
            return Math.Max(nameScore, descriptionScore);
        }
    }

    /// <summary>
    /// Searches Branches entities
    /// </summary>
    public class BranchSearcher : EntitySearcherBase
    {
        public BranchSearcher(AspireApiService aspireApi, ILogger<BranchSearcher> logger) // Use specific logger type
            : base(aspireApi, logger)
        {
        }

        public override async Task<List<SearchResult>> SearchAsync(
            string searchTerm,
            string startDate,
            string endDate,
            int maxResults,
            CancellationToken cancellationToken)
        {
            // Fetch branches from the API
            var branches = await AspireApi.GetBranchesAsync(
                null, searchTerm, 1, maxResults, cancellationToken);

            var results = new List<SearchResult>();

            foreach (var branch in branches.Data)
            {
                // Check if the branch matches the search term
                var matchScore = CalculateBranchMatchScore(branch, searchTerm);

                if (matchScore > 0)
                {
                    results.Add(new SearchResult
                    {
                        Id = branch.Id,
                        EntityType = "Branches",
                        Title = branch.Name,
                        Description = $"{branch.City}, {branch.State}",
                        LastModified = null, // No last modified in the DTO
                        Url = $"/branches/{branch.Id}",
                        MatchScore = matchScore,
                        Data = new Dictionary<string, object>
                        {
                            ["Name"] = branch.Name,
                            ["Address"] = branch.Address,
                            ["City"] = branch.City,
                            ["State"] = branch.State,
                            ["ZipCode"] = branch.ZipCode,
                            ["Phone"] = branch.Phone
                        }
                    });

                    // Stop if we've reached the maximum results
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            // Sort results by match score (descending)
            return results.OrderByDescending(r => r.MatchScore).ToList();
        }

        private int CalculateBranchMatchScore(BranchDto branch, string searchTerm)
        {
            // Calculate match scores for different fields
            var nameScore = CalculateMatchScore(branch.Name, searchTerm);
            var addressScore = CalculateMatchScore(
                $"{branch.Address} {branch.City} {branch.State} {branch.ZipCode}",
                searchTerm);
            var phoneScore = CalculateMatchScore(branch.Phone, searchTerm);

            // Return the highest match score
            return Math.Max(nameScore, Math.Max(addressScore, phoneScore));
        }
    }

    /// <summary>
    /// Placeholder for other entity searchers
    ///
    /// In a full implementation, you would create searchers for:
    /// - InvoiceSearcher
    /// - WorkTicketSearcher
    /// - OpportunitySearcher
    /// - JobSearcher
    /// - InventoryItemSearcher
    ///
    /// Each would follow a similar pattern to the examples above.
    /// </summary>
    public class InvoiceSearcher : EntitySearcherBase
    {
        public InvoiceSearcher(AspireApiService aspireApi, ILogger<InvoiceSearcher> logger) // Use specific logger type
            : base(aspireApi, logger)
        {
        }

        public override async Task<List<SearchResult>> SearchAsync(
            string searchTerm,
            string startDate,
            string endDate,
            int maxResults,
            CancellationToken cancellationToken)
        {
            // Fetch invoices from the API
            var invoices = await AspireApi.GetInvoicesAsync(
                null, null, searchTerm, 1, maxResults, cancellationToken);

            var results = new List<SearchResult>();

            foreach (var invoice in invoices.Data)
            {
                // Check if the invoice matches the search term
                var matchScore = CalculateInvoiceMatchScore(invoice, searchTerm);

                if (matchScore > 0)
                {
                    results.Add(new SearchResult
                    {
                        Id = invoice.Id,
                        EntityType = "Invoices",
                        Title = $"Invoice #{invoice.Number}",
                        Description = $"{invoice.ContactName}: ${invoice.Amount} due {invoice.DueDate:d}",
                        LastModified = invoice.Date,
                        Url = $"/invoices/{invoice.Id}",
                        MatchScore = matchScore,
                        Data = new Dictionary<string, object>
                        {
                            ["Number"] = invoice.Number,
                            ["Date"] = invoice.Date,
                            ["DueDate"] = invoice.DueDate,
                            ["Amount"] = invoice.Amount,
                            ["Status"] = invoice.Status,
                            ["ContactId"] = invoice.ContactId,
                            ["ContactName"] = invoice.ContactName,
                            ["BranchId"] = invoice.BranchId,
                            ["BranchName"] = invoice.BranchName
                        }
                    });

                    // Stop if we've reached the maximum results
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            // Sort results by match score (descending)
            return results.OrderByDescending(r => r.MatchScore).ToList();
        }

        private int CalculateInvoiceMatchScore(InvoiceDto invoice, string searchTerm)
        {
            // Calculate match scores for different fields
            var numberScore = CalculateMatchScore(invoice.Number, searchTerm);
            var contactNameScore = CalculateMatchScore(invoice.ContactName, searchTerm);
            var statusScore = CalculateMatchScore(invoice.Status, searchTerm);
            var branchNameScore = CalculateMatchScore(invoice.BranchName, searchTerm);

            // Check if the search term is a dollar amount that matches
            var amountScore = 0;
            if (decimal.TryParse(searchTerm.Replace("$", ""), out var amount) &&
                Math.Abs(invoice.Amount - amount) < 0.01m)
            {
                amountScore = 100;
            }

            // Return the highest match score
            return Math.Max(
                Math.Max(Math.Max(numberScore, contactNameScore),
                         Math.Max(statusScore, branchNameScore)),
                amountScore);
        }
    }

    public class WorkTicketSearcher : EntitySearcherBase
    {
        public WorkTicketSearcher(AspireApiService aspireApi, ILogger<WorkTicketSearcher> logger) // Use specific logger type
            : base(aspireApi, logger)
        {
        }

        public override async Task<List<SearchResult>> SearchAsync(
            string searchTerm,
            string startDate,
            string endDate,
            int maxResults,
            CancellationToken cancellationToken)
        {
            // Fetch work tickets from the API
            var workTickets = await AspireApi.GetWorkTicketsAsync(
                null, searchTerm, null, 1, maxResults, cancellationToken);

            var results = new List<SearchResult>();

            foreach (var ticket in workTickets.Data)
            {
                // Check if the work ticket matches the search term
                var matchScore = CalculateWorkTicketMatchScore(ticket, searchTerm);

                if (matchScore > 0)
                {
                    results.Add(new SearchResult
                    {
                        Id = ticket.Id,
                        EntityType = "WorkTickets",
                        Title = $"Ticket: {ticket.JobName}",
                        Description = $"Division: {ticket.DivisionName}",
                        LastModified = ticket.ModifiedDate,
                        Url = $"/worktickets/{ticket.Id}",
                        MatchScore = matchScore,
                        Data = new Dictionary<string, object>
                        {
                            ["JobId"] = ticket.JobId,
                            ["JobName"] = ticket.JobName,
                            ["DivisionId"] = ticket.DivisionId,
                            ["DivisionName"] = ticket.DivisionName,
                            ["IsDeleted"] = ticket.IsDeleted,
                            ["CreatedDate"] = ticket.CreatedDate,
                            ["ModifiedDate"] = ticket.ModifiedDate
                        }
                    });

                    // Stop if we've reached the maximum results
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            // Sort results by match score (descending)
            return results.OrderByDescending(r => r.MatchScore).ToList();
        }

        private int CalculateWorkTicketMatchScore(WorkTicketDto ticket, string searchTerm)
        {
            // Calculate match scores for different fields
            var jobNameScore = CalculateMatchScore(ticket.JobName, searchTerm);
            var divisionNameScore = CalculateMatchScore(ticket.DivisionName, searchTerm);
            var idScore = CalculateMatchScore(ticket.Id, searchTerm);

            // Return the highest match score
            return Math.Max(jobNameScore, Math.Max(divisionNameScore, idScore));
        }
    }

    public class OpportunitySearcher : EntitySearcherBase
    {
        public OpportunitySearcher(AspireApiService aspireApi, ILogger<OpportunitySearcher> logger) // Use specific logger type
            : base(aspireApi, logger)
        {
        }

        public override async Task<List<SearchResult>> SearchAsync(
            string searchTerm,
            string startDate,
            string endDate,
            int maxResults,
            CancellationToken cancellationToken)
        {
            // Fetch opportunities from the API
            var opportunities = await AspireApi.GetOpportunitiesAsync(
                null, null, searchTerm, 1, maxResults, cancellationToken);

            var results = new List<SearchResult>();

            foreach (var opportunity in opportunities.Data)
            {
                // Check if the opportunity matches the search term
                var matchScore = CalculateOpportunityMatchScore(opportunity, searchTerm);

                if (matchScore > 0)
                {
                    results.Add(new SearchResult
                    {
                        Id = opportunity.Id,
                        EntityType = "Opportunities",
                        Title = opportunity.Name,
                        Description = $"{opportunity.ContactName}: ${opportunity.Amount} ({opportunity.Status})",
                        LastModified = opportunity.ModifiedDate,
                        Url = $"/opportunities/{opportunity.Id}",
                        MatchScore = matchScore,
                        Data = new Dictionary<string, object>
                        {
                            ["Name"] = opportunity.Name,
                            ["Status"] = opportunity.Status,
                            ["Amount"] = opportunity.Amount,
                            ["ContactId"] = opportunity.ContactId,
                            ["ContactName"] = opportunity.ContactName,
                            ["CreatedDate"] = opportunity.CreatedDate,
                            ["ModifiedDate"] = opportunity.ModifiedDate
                        }
                    });

                    // Stop if we've reached the maximum results
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            // Sort results by match score (descending)
            return results.OrderByDescending(r => r.MatchScore).ToList();
        }

        private int CalculateOpportunityMatchScore(OpportunityDto opportunity, string searchTerm)
        {
            // Calculate match scores for different fields
            var nameScore = CalculateMatchScore(opportunity.Name, searchTerm);
            var contactNameScore = CalculateMatchScore(opportunity.ContactName, searchTerm);
            var statusScore = CalculateMatchScore(opportunity.Status, searchTerm);

            // Check if the search term is a dollar amount that matches
            var amountScore = 0;
            if (decimal.TryParse(searchTerm.Replace("$", ""), out var amount) &&
                Math.Abs(opportunity.Amount - amount) < 0.01m)
            {
                amountScore = 100;
            }

            // Return the highest match score
            return Math.Max(
                Math.Max(nameScore, contactNameScore),
                Math.Max(statusScore, amountScore));
        }
    }

    public class JobSearcher : EntitySearcherBase
    {
        public JobSearcher(AspireApiService aspireApi, ILogger<JobSearcher> logger) // Use specific logger type
            : base(aspireApi, logger)
        {
        }

        public override async Task<List<SearchResult>> SearchAsync(
            string searchTerm,
            string startDate,
            string endDate,
            int maxResults,
            CancellationToken cancellationToken)
        {
            // Fetch jobs from the API
            var jobs = await AspireApi.GetJobsAsync(
                null, null, 1, maxResults, cancellationToken);

            var results = new List<SearchResult>();

            foreach (var job in jobs.Data)
            {
                // Check if the job matches the search term
                var matchScore = CalculateJobMatchScore(job, searchTerm);

                if (matchScore > 0)
                {
                    var endDateString = job.EndDate.HasValue
                        ? $" to {job.EndDate.Value:d}"
                        : " (ongoing)";

                    results.Add(new SearchResult
                    {
                        Id = job.Id,
                        EntityType = "Jobs",
                        Title = job.Name,
                        Description = $"{job.ContactName}: {job.StartDate:d}{endDateString}",
                        LastModified = null, // No last modified in the DTO
                        Url = $"/jobs/{job.Id}",
                        MatchScore = matchScore,
                        Data = new Dictionary<string, object>
                        {
                            ["Name"] = job.Name,
                            ["Status"] = job.Status,
                            ["StartDate"] = job.StartDate,
                            ["EndDate"] = job.EndDate,
                            ["ContactId"] = job.ContactId,
                            ["ContactName"] = job.ContactName,
                            ["BranchId"] = job.BranchId,
                            ["BranchName"] = job.BranchName,
                            ["DivisionId"] = job.DivisionId,
                            ["DivisionName"] = job.DivisionName
                        }
                    });

                    // Stop if we've reached the maximum results
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            // Sort results by match score (descending)
            return results.OrderByDescending(r => r.MatchScore).ToList();
        }

        private int CalculateJobMatchScore(JobDto job, string searchTerm)
        {
            // Calculate match scores for different fields
            var nameScore = CalculateMatchScore(job.Name, searchTerm);
            var contactNameScore = CalculateMatchScore(job.ContactName, searchTerm);
            var statusScore = CalculateMatchScore(job.Status, searchTerm);
            var branchNameScore = CalculateMatchScore(job.BranchName, searchTerm);
            var divisionNameScore = CalculateMatchScore(job.DivisionName, searchTerm);

            // Return the highest match score
            return Math.Max(
                Math.Max(Math.Max(nameScore, contactNameScore),
                         Math.Max(statusScore, branchNameScore)),
                divisionNameScore);
        }
    }

    public class InventoryItemSearcher : EntitySearcherBase
    {
        public InventoryItemSearcher(AspireApiService aspireApi, ILogger<InventoryItemSearcher> logger) // Use specific logger type
            : base(aspireApi, logger)
        {
        }

        public override async Task<List<SearchResult>> SearchAsync(
            string searchTerm,
            string startDate,
            string endDate,
            int maxResults,
            CancellationToken cancellationToken)
        {
            // Fetch inventory items from the API
            var inventoryItems = await AspireApi.GetInventoryItemsAsync(
                null, searchTerm, 1, maxResults, cancellationToken);

            var results = new List<SearchResult>();

            foreach (var item in inventoryItems.Data)
            {
                // Check if the inventory item matches the search term
                var matchScore = CalculateInventoryItemMatchScore(item, searchTerm);

                if (matchScore > 0)
                {
                    results.Add(new SearchResult
                    {
                        Id = item.Id,
                        EntityType = "InventoryItems",
                        Title = item.Name,
                        Description = $"{item.Description} - Qty: {item.Quantity} @ ${item.Price}",
                        LastModified = null, // No last modified in the DTO
                        Url = $"/inventory/{item.Id}",
                        MatchScore = matchScore,
                        Data = new Dictionary<string, object>
                        {
                            ["Name"] = item.Name,
                            ["Description"] = item.Description,
                            ["WarehouseId"] = item.WarehouseId,
                            ["WarehouseName"] = item.WarehouseName,
                            ["Quantity"] = item.Quantity,
                            ["Cost"] = item.Cost,
                            ["Price"] = item.Price
                        }
                    });

                    // Stop if we've reached the maximum results
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }
            }

            // Sort results by match score (descending)
            return results.OrderByDescending(r => r.MatchScore).ToList();
        }

        private int CalculateInventoryItemMatchScore(InventoryItemDto item, string searchTerm)
        {
            // Calculate match scores for different fields
            var nameScore = CalculateMatchScore(item.Name, searchTerm);
            var descriptionScore = CalculateMatchScore(item.Description, searchTerm);
            var warehouseNameScore = CalculateMatchScore(item.WarehouseName, searchTerm);

            // Return the highest match score
            return Math.Max(nameScore, Math.Max(descriptionScore, warehouseNameScore));
        }
    }
}
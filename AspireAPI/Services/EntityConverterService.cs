using System;
using System.Collections.Generic;
using AspireAPI.Models; // Assuming DTOs are in AspireAPI.Models

namespace AspireAPI.Services
{
    /// <summary>
    /// Service for converting entity objects to dictionaries
    /// </summary>
    public class EntityConverterService
    {
        // No logger needed for simple conversions currently

        /// <summary>
        /// Converts a TimeEntryDto to a Dictionary
        /// </summary>
        public Dictionary<string, object> ConvertTimeEntryToDict(TimeEntryDto entity)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = entity.Id,
                ["EmployeeId"] = entity.EmployeeId,
                ["EmployeeName"] = entity.EmployeeName,
                ["Date"] = entity.Date.ToString("yyyy-MM-dd"),
                ["Hours"] = entity.Hours,
                ["Cost"] = entity.Cost,
                ["Notes"] = entity.Notes ?? "",
                ["DivisionId"] = entity.DivisionId,
                ["DivisionName"] = entity.DivisionName,
                ["ContactId"] = entity.ContactId ?? "",
                ["ContactName"] = entity.ContactName ?? "",
                ["BranchId"] = entity.BranchId ?? "",
                ["BranchName"] = entity.BranchName ?? ""
            };
        }

        /// <summary>
        /// Converts a ContactDto to a Dictionary
        /// </summary>
        public Dictionary<string, object> ConvertContactToDict(ContactDto entity)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = entity.Id,
                ["Name"] = entity.Name,
                ["Type"] = entity.Type,
                ["Email"] = entity.Email ?? "",
                ["Phone"] = entity.Phone ?? "",
                ["Address"] = entity.Address ?? "",
                ["City"] = entity.City ?? "",
                ["State"] = entity.State ?? "",
                ["ZipCode"] = entity.ZipCode ?? ""
            };
        }

        /// <summary>
        /// Converts a DivisionDto to a Dictionary
        /// </summary>
        public Dictionary<string, object> ConvertDivisionToDict(DivisionDto entity)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = entity.Id,
                ["Name"] = entity.Name,
                ["Description"] = entity.Description ?? ""
            };
        }

        /// <summary>
        /// Converts a BranchDto to a Dictionary
        /// </summary>
        public Dictionary<string, object> ConvertBranchToDict(BranchDto entity)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = entity.Id,
                ["Name"] = entity.Name,
                ["Address"] = entity.Address ?? "",
                ["City"] = entity.City ?? "",
                ["State"] = entity.State ?? "",
                ["ZipCode"] = entity.ZipCode ?? "",
                ["Phone"] = entity.Phone ?? ""
            };
        }

        /// <summary>
        /// Converts an InvoiceDto to a Dictionary
        /// </summary>
        public Dictionary<string, object> ConvertInvoiceToDict(InvoiceDto entity)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = entity.Id,
                ["Number"] = entity.Number,
                ["BranchId"] = entity.BranchId,
                ["BranchName"] = entity.BranchName,
                ["ContactId"] = entity.ContactId,
                ["ContactName"] = entity.ContactName,
                ["Amount"] = entity.Amount,
                ["Date"] = entity.Date.ToString("yyyy-MM-dd"),
                ["DueDate"] = entity.DueDate.ToString("yyyy-MM-dd"),
                ["Status"] = entity.Status
            };
        }

        /// <summary>
        /// Converts a WorkTicketDto to a Dictionary
        /// </summary>
        public Dictionary<string, object> ConvertWorkTicketToDict(WorkTicketDto entity)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = entity.Id,
                ["JobId"] = entity.JobId,
                ["JobName"] = entity.JobName,
                ["DivisionId"] = entity.DivisionId,
                ["DivisionName"] = entity.DivisionName,
                ["IsDeleted"] = entity.IsDeleted,
                ["CreatedDate"] = entity.CreatedDate.ToString("yyyy-MM-dd"),
                ["ModifiedDate"] = entity.ModifiedDate.ToString("yyyy-MM-dd")
            };
        }

        /// <summary>
        /// Converts an OpportunityDto to a Dictionary
        /// </summary>
        public Dictionary<string, object> ConvertOpportunityToDict(OpportunityDto entity)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = entity.Id,
                ["Name"] = entity.Name,
                ["Status"] = entity.Status,
                ["ContactId"] = entity.ContactId,
                ["ContactName"] = entity.ContactName,
                ["Amount"] = entity.Amount,
                ["CreatedDate"] = entity.CreatedDate.ToString("yyyy-MM-dd"),
                ["ModifiedDate"] = entity.ModifiedDate.ToString("yyyy-MM-dd")
            };
        }

        /// <summary>
        /// Converts a JobDto to a Dictionary
        /// </summary>
        public Dictionary<string, object> ConvertJobToDict(JobDto entity)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = entity.Id,
                ["Name"] = entity.Name,
                ["BranchId"] = entity.BranchId,
                ["BranchName"] = entity.BranchName,
                ["DivisionId"] = entity.DivisionId,
                ["DivisionName"] = entity.DivisionName,
                ["ContactId"] = entity.ContactId,
                ["ContactName"] = entity.ContactName,
                ["Status"] = entity.Status,
                ["StartDate"] = entity.StartDate.ToString("yyyy-MM-dd"),
                ["EndDate"] = entity.EndDate?.ToString("yyyy-MM-dd") ?? ""
            };
        }

        /// <summary>
        /// Converts an InventoryItemDto to a Dictionary
        /// </summary>
        public Dictionary<string, object> ConvertInventoryItemToDict(InventoryItemDto entity)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = entity.Id,
                ["Name"] = entity.Name,
                ["Description"] = entity.Description ?? "",
                ["WarehouseId"] = entity.WarehouseId,
                ["WarehouseName"] = entity.WarehouseName,
                ["Quantity"] = entity.Quantity,
                ["Cost"] = entity.Cost,
                ["Price"] = entity.Price
            };
        }
    }
}
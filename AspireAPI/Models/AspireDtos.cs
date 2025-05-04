using System;
using System.Collections.Generic;

namespace AspireAPI.Models
{
    /// <summary>
    /// Data transfer object for contacts
    /// </summary>
    public class ContactDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Type { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Added missing property (alias for PostalCode)
        public string ZipCode {
            get { return PostalCode; }
            set { PostalCode = value; }
        }
    }

    /// <summary>
    /// Data transfer object for divisions
    /// </summary>
    public class DivisionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object for jobs
    /// </summary>
    public class JobDto
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public string DivisionId { get; set; }
        public string DivisionName { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal ContractAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object for invoices
    /// </summary>
    public class InvoiceDto
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public string JobId { get; set; }
        public string JobName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Added missing properties
        public string BranchId { get; set; }
        public string BranchName { get; set; }
    }

    /// <summary>
    /// Data transfer object for work tickets
    /// </summary>
    public class WorkTicketDto
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string JobId { get; set; }
        public string JobName { get; set; }
        public string AssignedToId { get; set; }
        public string AssignedToName { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Added missing properties
        public string DivisionId { get; set; }
        public string DivisionName { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate {
            get { return CreatedAt; }
            set { CreatedAt = value; }
        }
        public DateTime? ModifiedDate {
            get { return UpdatedAt; }
            set { UpdatedAt = value; }
        }
    }

    /// <summary>
    /// Data transfer object for opportunities
    /// </summary>
    public class OpportunityDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Stage { get; set; }
        public decimal EstimatedValue { get; set; }
        public decimal Probability { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public string DivisionId { get; set; }
        public string DivisionName { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Added missing properties
        public decimal Amount {
            get { return EstimatedValue; }
            set { EstimatedValue = value; }
        }
        public DateTime CreatedDate {
            get { return CreatedAt; }
            set { CreatedAt = value; }
        }
        public DateTime? ModifiedDate {
            get { return UpdatedAt; }
            set { UpdatedAt = value; }
        }
    }

    /// <summary>
    /// Data transfer object for branches
    /// </summary>
    public class BranchDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Added missing properties
        public string ZipCode {
            get { return PostalCode; }
            set { PostalCode = value; }
        }
        public string Phone { get; set; }
    }

    /// <summary>
    /// Data transfer object for equipment
    /// </summary>
    public class EquipmentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string SerialNumber { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object for inventory items
    /// </summary>
    public class InventoryItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string SKU { get; set; }
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public decimal UnitPrice { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Added missing properties
        public string WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int Quantity {
            get { return QuantityOnHand; }
            set { QuantityOnHand = value; }
        }
        public decimal Cost { get; set; }
        public decimal Price {
            get { return UnitPrice; }
            set { UnitPrice = value; }
        }
    }

    /// <summary>
    /// Data transfer object for properties
    /// </summary>
    public class PropertyDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public string Type { get; set; }
        public decimal Size { get; set; }
        public string SizeUnit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object for time entries
    /// </summary>
    public class TimeEntryDto
    {
        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string JobId { get; set; }
        public string JobName { get; set; }
        public string WorkTicketId { get; set; }
        public string TaskDescription { get; set; }
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public string Status { get; set; }
        public bool Billable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Added missing properties
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
        public string DivisionId { get; set; }
        public string DivisionName { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
    }

    /// <summary>
    /// Data transfer object for payments
    /// </summary>
    public class PaymentDto
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
        public string Reference { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
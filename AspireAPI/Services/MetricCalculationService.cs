using System;
using System.Collections.Generic;
using System.Linq;

namespace AspireAPI.Services
{
    /// <summary>
    /// Service for calculating business metrics
    /// </summary>
    public class MetricCalculationService
    {
        /// <summary>
        /// Calculate total hours from time entries
        /// </summary>
        public decimal CalculateTotalHours(List<Dictionary<string, object>> data)
        {
            decimal totalHours = 0;

            foreach (var item in data)
            {
                if (item.TryGetValue("Hours", out var hoursObj) &&
                    decimal.TryParse(hoursObj.ToString(), out var hours))
                {
                    totalHours += hours;
                }
            }

            return Math.Round(totalHours, 2);
        }

        /// <summary>
        /// Calculate total cost from time entries
        /// </summary>
        public decimal CalculateTotalCost(List<Dictionary<string, object>> data)
        {
            decimal totalCost = 0;

            foreach (var item in data)
            {
                if (item.TryGetValue("Cost", out var costObj) &&
                    decimal.TryParse(costObj.ToString(), out var cost))
                {
                    totalCost += cost;
                }
            }

            return Math.Round(totalCost, 2);
        }

        /// <summary>
        /// Calculate average hours per entry
        /// </summary>
        public decimal CalculateAverageHours(List<Dictionary<string, object>> data)
        {
            if (data == null || data.Count == 0)
            {
                return 0;
            }

            decimal totalHours = CalculateTotalHours(data);
            return Math.Round(totalHours / data.Count, 2);
        }

        /// <summary>
        /// Calculate average cost per entry
        /// </summary>
        public decimal CalculateAverageCost(List<Dictionary<string, object>> data)
        {
            if (data == null || data.Count == 0)
            {
                return 0;
            }

            decimal totalCost = CalculateTotalCost(data);
            return Math.Round(totalCost / data.Count, 2);
        }

        /// <summary>
        /// Calculate total revenue
        /// </summary>
        public decimal CalculateTotalRevenue(List<Dictionary<string, object>> data)
        {
            decimal totalRevenue = 0;

            foreach (var item in data)
            {
                // For invoices
                if (item.TryGetValue("Amount", out var amountObj) &&
                    decimal.TryParse(amountObj.ToString(), out var amount))
                {
                    totalRevenue += amount;
                }
                // For time entries (estimated revenue based on billable hours)
                else if (item.TryGetValue("Hours", out var hoursObj) &&
                         decimal.TryParse(hoursObj.ToString(), out var hours))
                {
                    // Assume 90% of hours are billable
                    decimal billableHours = hours * 0.9m;

                    // Use cost as base for revenue calculation
                    if (item.TryGetValue("Cost", out var costObj) &&
                        decimal.TryParse(costObj.ToString(), out var cost))
                    {
                        // Assume a markup of 30% on cost
                        totalRevenue += cost * 1.3m;
                    }
                    else
                    {
                        // Default hourly rate of $150 if cost is not available
                        totalRevenue += billableHours * 150;
                    }
                }
            }

            return Math.Round(totalRevenue, 2);
        }

        /// <summary>
        /// Calculate total profit
        /// </summary>
        public decimal CalculateTotalProfit(List<Dictionary<string, object>> data)
        {
            decimal totalRevenue = CalculateTotalRevenue(data);
            decimal totalCost = CalculateTotalCost(data);

            return Math.Round(totalRevenue - totalCost, 2);
        }

        /// <summary>
        /// Calculate billable hours (assumed to be 90% of total hours)
        /// </summary>
        public decimal CalculateBillableHours(List<Dictionary<string, object>> data)
        {
            decimal totalHours = CalculateTotalHours(data);

            // Assume 90% of hours are billable
            return Math.Round(totalHours * 0.9m, 2);
        }

        /// <summary>
        /// Calculate non-billable hours (assumed to be 10% of total hours)
        /// </summary>
        public decimal CalculateNonBillableHours(List<Dictionary<string, object>> data)
        {
            decimal totalHours = CalculateTotalHours(data);

            // Assume 10% of hours are non-billable
            return Math.Round(totalHours * 0.1m, 2);
        }

        /// <summary>
        /// Calculate utilization rate (billable hours / total hours)
        /// </summary>
        public decimal CalculateUtilizationRate(List<Dictionary<string, object>> data)
        {
            decimal totalHours = CalculateTotalHours(data);

            if (totalHours == 0)
            {
                return 0;
            }

            decimal billableHours = CalculateBillableHours(data);

            return Math.Round(billableHours / totalHours, 2);
        }

        /// <summary>
        /// Calculate average hourly rate (cost / hours)
        /// </summary>
        public decimal CalculateAverageHourlyRate(List<Dictionary<string, object>> data)
        {
            decimal totalHours = CalculateTotalHours(data);

            if (totalHours == 0)
            {
                return 0;
            }

            decimal totalCost = CalculateTotalCost(data);

            return Math.Round(totalCost / totalHours, 2);
        }

        /// <summary>
        /// Calculate profit margin (profit / revenue)
        /// </summary>
        public decimal CalculateProfitMargin(List<Dictionary<string, object>> data)
        {
            decimal totalRevenue = CalculateTotalRevenue(data);

            if (totalRevenue == 0)
            {
                return 0;
            }

            decimal totalProfit = CalculateTotalProfit(data);

            return Math.Round(totalProfit / totalRevenue, 2);
        }
    }
}
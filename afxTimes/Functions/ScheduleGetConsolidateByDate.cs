using afx.EmployesTime.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace afx.EmployesTime.Functions
{
    public static class ScheduleGetConsolidateByDate
    {
        [FunctionName("ScheduleGetConsolidateByDate")]
        public static async Task Run(
            [TimerTrigger("*/20 * * * *")] TimerInfo myTimer,
            [Table("consolidateData", Connection = "AzureWebJobsStorage")] CloudTable consolidateTable,
            string date,
            ILogger log)
        {
            log.LogInformation($"Get all consolidate by date: {date}.");

            string filterConsolidate = TableQuery.GenerateFilterConditionForDate("RegisterTime", QueryComparisons.Equal, DateTime.Parse(date));
            TableQuery<ConsolidateDataEntity> queryConsolidate = new TableQuery<ConsolidateDataEntity>().Where(filterConsolidate);
            TableQuerySegment<ConsolidateDataEntity> completedConsolidate = await consolidateTable.ExecuteQuerySegmentedAsync(queryConsolidate, null);

            var consolidateData = completedConsolidate.Select(d => new
            {
                d.RowKey,
                d.IdEmployee,
                d.WorkMinutes
            })
           .Distinct()
           .OrderBy(d => d.IdEmployee);

            string message = $"Consolidate employees for the date: {date}.";
            log.LogInformation(message);
        }
    }
}

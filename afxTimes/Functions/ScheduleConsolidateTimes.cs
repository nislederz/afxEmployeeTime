using afx.EmployesTime.Entities;
using afxTimes.Entities;
using Common.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace afx.EmployesTime.Functions
{
    public static class ScheduleConsolidateTimes
    {
        [FunctionName("ScheduleConsolidateTimes")]
        public static async Task Run(
            [TimerTrigger("*/20 * * * *")] TimerInfo myTimer,
            [Table("employeeTime", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            [Table("consolidateData", Connection = "AzureWebJobsStorage")] CloudTable consolidateTable,
            ILogger log)
        {
            string utcTime = DateTime.Now.ToString("dd-MM-yyyy");
            int employeeUpdate = 0;
            int employeeCreate = 0;

            log.LogInformation($"Consolidate times at: {utcTime}");

            string filter = TableQuery.GenerateFilterConditionForBool("Consolidate", QueryComparisons.Equal, false);
            TableQuery<EmployeesTimeEntity> query = new TableQuery<EmployeesTimeEntity>().Where(filter);
            TableQuerySegment<EmployeesTimeEntity> completedTimes = await timeTable.ExecuteQuerySegmentedAsync(query, null);

            string filterConsolidate = TableQuery.GenerateFilterConditionForDate("RegisterTime", QueryComparisons.Equal, DateTime.Parse(utcTime));
            TableQuery<ConsolidateDataEntity> queryConsolidate = new TableQuery<ConsolidateDataEntity>().Where(filterConsolidate);
            TableQuerySegment<ConsolidateDataEntity> completedConsolidate = await consolidateTable.ExecuteQuerySegmentedAsync(queryConsolidate, null);

            var data = completedTimes.Select(d => new
            {
                d.RowKey,
                d.IdEmployee,
                d.RegisterTime,
                d.TypeOutput
            })
            .Distinct()
            .OrderBy(d => d.RegisterTime)
            .OrderBy(d => d.IdEmployee);

            var employees = completedTimes.Select(d => new
            {
                d.IdEmployee
            })
            .Distinct()
            .OrderBy(d => d.IdEmployee);

            foreach (var employee in employees)
            {
                var item = data.Where(x => x.IdEmployee == employee.IdEmployee);
                if (item.Count() == 2)
                {
                    string startHour = item.Where(x => x.TypeOutput == 0).First().RegisterTime.ToString();
                    string endHour = item.Where(x => x.TypeOutput == 1).First().RegisterTime.ToString();
                    int workMinutes = (int)(DateTime.Parse(endHour) - DateTime.Parse(startHour)).TotalMinutes;

                    System.Collections.Generic.IEnumerable<ConsolidateDataEntity> existEmployeed = completedConsolidate.Where(x => x.IdEmployee == employee.IdEmployee);
                    if (existEmployeed.Count() == 1)
                    {
                        //UPDATE CONSOLIDATE
                        TableOperation findOperation = TableOperation.Retrieve<ConsolidateDataEntity>("CONSOLIDATE", existEmployeed.First().RowKey);
                        TableResult findResult = await consolidateTable.ExecuteAsync(findOperation);

                        ConsolidateDataEntity timeEntity = (ConsolidateDataEntity)findResult.Result;
                        if (!string.IsNullOrEmpty(timeEntity.RegisterTime.ToString()))
                        {
                            timeEntity.WorkMinutes = timeEntity.WorkMinutes + workMinutes;
                        }

                        TableOperation addOperationUpdate = TableOperation.Replace(timeEntity);
                        await consolidateTable.ExecuteAsync(addOperationUpdate);
                        employeeUpdate++;
                    }
                    else
                    {
                        //INSERT CONSOLIDATE
                        ConsolidateDataEntity consolidateEntity = new ConsolidateDataEntity
                        {
                            IdEmployee = employee.IdEmployee,
                            RegisterTime = DateTime.Parse(utcTime),
                            WorkMinutes = workMinutes,
                            ETag = "*",
                            PartitionKey = "CONSOLIDATE",
                            RowKey = Guid.NewGuid().ToString()
                        };

                        TableOperation addOperationCreate = TableOperation.Insert(consolidateEntity);
                        await consolidateTable.ExecuteAsync(addOperationCreate);
                        employeeCreate++;
                    }

                    foreach (var dt in item)
                    {
                        //UPDATE EMPLOYEE CONSOLIDATE
                        TableOperation findOperationTime = TableOperation.Retrieve<EmployeesTimeEntity>("EMPLOYEETIME", dt.RowKey);
                        TableResult findResultTime = await timeTable.ExecuteAsync(findOperationTime);
                        EmployeesTimeEntity timeEntityTime = (EmployeesTimeEntity)findResultTime.Result;

                        if (!string.IsNullOrEmpty(timeEntityTime.RegisterTime.ToString()))
                        {
                            timeEntityTime.Consolidate = true;
                        }

                        TableOperation addOperationTime = TableOperation.Replace(timeEntityTime);
                        await timeTable.ExecuteAsync(addOperationTime);
                    }
                }
            }

            string responseMessage = $"Consolidate: create {employeeCreate} and update {employeeUpdate} employees.";
            log.LogInformation(responseMessage);
        }
    }
}

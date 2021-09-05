using afx.Common.Models;
using afx.EmployesTime.Entities;
using afxTimes.Entities;
using Common.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace afx.EmployesTime.Functions
{
    public static class RegisterTimes
    {
        [FunctionName(nameof(CreateEmployeeTime))]
        public static async Task<IActionResult> CreateEmployeeTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "employeeTime")] HttpRequest req,
            [Table("employeeTime", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("Time Register.");

            string request = await new StreamReader(req.Body).ReadToEndAsync();
            EmployeeTime time = JsonConvert.DeserializeObject<EmployeeTime>(request);

            if (string.IsNullOrEmpty(time?.IdEmployee.ToString()) || string.IsNullOrEmpty(time?.RegisterTime.ToString()) || string.IsNullOrEmpty(time?.TypeOutput.ToString()))
            {
                return new BadRequestObjectResult(new Responses
                {
                    isSuccess = false,
                    Message = "the request have void filds."
                });
            }

            EmployeesTimeEntity timeEntity = new EmployeesTimeEntity
            {
                IdEmployee = time.IdEmployee,
                RegisterTime = time.RegisterTime,
                TypeOutput = time.TypeOutput,
                Consolidate = false,
                ETag = "*",
                PartitionKey = "EMPLOYEETIME",
                RowKey = Guid.NewGuid().ToString()
            };

            TableOperation addOperation = TableOperation.Insert(timeEntity);
            await timeTable.ExecuteAsync(addOperation);

            string responseMessage = "Register Successful.";
            log.LogInformation(responseMessage);
            return new OkObjectResult(new Responses
            {
                isSuccess = true,
                Message = responseMessage,
                Result = timeEntity
            });
        }

        [FunctionName(nameof(UpdateEmployeeTime))]
        public static async Task<IActionResult> UpdateEmployeeTime(
           [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "employeeTime/{id}")] HttpRequest req,
           [Table("employeeTime", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
           string id,
           ILogger log)
        {
            log.LogInformation($"Update for time the employee: {id}.");

            string request = await new StreamReader(req.Body).ReadToEndAsync();
            EmployeeTime time = JsonConvert.DeserializeObject<EmployeeTime>(request);

            TableOperation findOperation = TableOperation.Retrieve<EmployeesTimeEntity>("EMPLOYEETIME", id);
            TableResult findResult = await timeTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Responses
                {
                    isSuccess = false,
                    Message = "Employee not found."
                });
            }

            EmployeesTimeEntity timeEntity = (EmployeesTimeEntity)findResult.Result;
            if (!string.IsNullOrEmpty(timeEntity.RegisterTime.ToString()))
            {
                timeEntity.RegisterTime = time.RegisterTime;
            }

            TableOperation addOperation = TableOperation.Replace(timeEntity);
            await timeTable.ExecuteAsync(addOperation);

            string message = $"Update Time For Employee: {id}.";
            log.LogInformation(message);

            return new OkObjectResult(new Responses
            {
                isSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }

        [FunctionName(nameof(DeleteEmployeeTime))]
        public static async Task<IActionResult> DeleteEmployeeTime(
           [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "employeeTime/{id}")] HttpRequest req,
           [Table("employeeTime", "EMPLOYEETIME", "{id}", Connection = "AzureWebJobsStorage")] EmployeesTimeEntity timeEntity,
           [Table("employeeTime", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
           string id,
           ILogger log)
        {
            log.LogInformation($"Delete time: {id}, received.");

            if (timeEntity == null)
            {
                return new BadRequestObjectResult(new Responses
                {
                    isSuccess = false,
                    Message = "Employee time not found."
                });
            }

            await timeTable.ExecuteAsync(TableOperation.Delete(timeEntity));

            string message = $"Employee time deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Responses
            {
                isSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }

        [FunctionName(nameof(GetAllEmployeeTime))]
        public static async Task<IActionResult> GetAllEmployeeTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "employeeTime")] HttpRequest req,
            [Table("employeeTime", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("Get all todos received.");

            TableQuery<EmployeesTimeEntity> query = new TableQuery<EmployeesTimeEntity>();
            TableQuerySegment<EmployeesTimeEntity> todos = await timeTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all todos.";
            log.LogInformation(message);

            return new OkObjectResult(new Responses
            {
                isSuccess = true,
                Message = message,
                Result = todos
            });
        }

        [FunctionName(nameof(GetEmployeeTimeById))]
        public static IActionResult GetEmployeeTimeById(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "employeeTime/{id}")] HttpRequest req,
           [Table("employeeTime", "EMPLOYEETIME", "{id}", Connection = "AzureWebJobsStorage")] EmployeesTimeEntity timeEntity,
           string id,
           ILogger log)
        {
            log.LogInformation($"Get employee time by id: {id}, received.");

            if (timeEntity == null)
            {
                return new BadRequestObjectResult(new Responses
                {
                    isSuccess = false,
                    Message = "Employee time not found."
                });
            }

            string message = $"Employee time: {timeEntity.RowKey}, received.";
            log.LogInformation(message);

            return new OkObjectResult(new Responses
            {
                isSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }


        [FunctionName(nameof(ConsolidateTimesAsync))]
        public static async Task<IActionResult> ConsolidateTimesAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "consolidate")] HttpRequest req,
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
            return new OkObjectResult(new Responses
            {
                isSuccess = true,
                Message = responseMessage
            });

        }

        [FunctionName(nameof(GetAllConsolidateByDate))]
        public static async Task<IActionResult> GetAllConsolidateByDate(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidateData/{date}")] HttpRequest req,
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

            return new OkObjectResult(new Responses
            {
                isSuccess = true,
                Message = message,
                Result = consolidateData
            });
        }

    }
}

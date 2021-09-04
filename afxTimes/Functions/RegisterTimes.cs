using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using afx.Common.Models;
using Common.Responses;
using afxTimes.Entities;

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
    }
}

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
    }
}

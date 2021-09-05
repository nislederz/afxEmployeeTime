using afx.Common.Models;
using afxTimes.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace afx.TestingApp.Helpers
{
    public class TestFactory
    {
        public static EmployeesTimeEntity GetTimeEntity()
        {
            return new EmployeesTimeEntity
            {
                IdEmployee = 50,
                RegisterTime = DateTime.Now,
                TypeOutput = 0,
                Consolidate = false,
                ETag = "*",
                PartitionKey = "EMPLOYEETIME",
                RowKey = Guid.NewGuid().ToString()
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid employeeId, EmployeeTime timeRequest)
        {
            string request = JsonConvert.SerializeObject(timeRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromSting(request),
                Path = $"/{employeeId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid employeeId, ConsolidateData consolidateRequest)
        {
            string request = JsonConvert.SerializeObject(consolidateRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromSting(request),
                Path = $"/{employeeId}"
            };
        }
        public static DefaultHttpRequest CreateHttpRequest(string date)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{date}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(string date, ConsolidateData consolidateRequest)
        {
            string request = JsonConvert.SerializeObject(consolidateRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromSting(request),
                Path = $"/{date}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid employeeId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{employeeId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(EmployeeTime timeRequest)
        {
            string request = JsonConvert.SerializeObject(timeRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromSting(request)
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(ConsolidateData consolidateRequest)
        {
            string request = JsonConvert.SerializeObject(consolidateRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromSting(request)
            };
        }

        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static EmployeeTime GetTimeRequest()
        {
            return new EmployeeTime
            {
                IdEmployee = 34,
                RegisterTime = DateTime.Now,
                TypeOutput = 0,
                Consolidate = false
            };
        }

        public static ConsolidateData GetConsolidateRequest()
        {
            return new ConsolidateData
            {
                IdEmployee = 34,
                RegisterTime = DateTime.Now,
                WorkMinutes = 120
            };
        }

        public static Stream GenerateStreamFromSting(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}

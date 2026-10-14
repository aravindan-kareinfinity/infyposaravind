using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfyPOS.Models;
using InfyPOS.Data;
using Newtonsoft.Json;
using WebAPI.Common;
using InfyPOS.Common;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace InfyPOS.Processors
{
    public class WorkflowQueueManager
    {
        private static readonly ConcurrentDictionary<Guid, WorkflowQueueItem> _queueItems = new ConcurrentDictionary<Guid, WorkflowQueueItem>();
        private static readonly ConcurrentDictionary<string, Guid> _hashToQueueId = new ConcurrentDictionary<string, Guid>();
        private static readonly object _lockObject = new object();
        private static WorkflowQueueManager _instance;

        public bool Enabled { get; private set; }

        public static WorkflowQueueManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new WorkflowQueueManager();
                            _instance.Enabled = 
                                !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["QUEUETIMEOUT"])
                                && System.Configuration.ConfigurationManager.AppSettings["QUEUETIMEOUT"].ToLower() == "true";
                        }
                    }
                }
                return _instance;
            }
        }

        public List<WorkflowQueueItem> ActiveList
        {
            get
            {
                return _queueItems.Values.ToList();
            }
        }
        private WorkflowQueueManager()
        {
            // Start cleanup task
            Task.Run(() => CleanupCompletedItems());
        }

        private string GenerateQueueHash(string methodName, object parameters, long organizationId, string userId)
        {
            var hashInput = $"{methodName}|{JsonConvert.SerializeObject(parameters)}|{organizationId}|{userId}";
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public async Task<WorkflowQueueResponse> ExecuteWithTimeout<T>(Func<Task<T>> operation, string methodName, object parameters,
            int timeoutSeconds = 30, 
            long organizationId = 0, string userId = "")
        {
            // Generate unique hash for this operation
            var operationHash = GenerateQueueHash(methodName, parameters, organizationId, userId);
            Guid existingQueueId;
            WorkflowQueueItem existingQueueItem;
            // Check if this operation is already in progress or completed
            if (_hashToQueueId.TryGetValue(operationHash, out  existingQueueId))
            {
                if (_queueItems.TryGetValue(existingQueueId, out existingQueueItem))
                {
                    if (existingQueueItem.Status == QueueStatus.Timeout || 
                        (!existingQueueItem.CompletedOn.HasValue ||
                        DateTime.Now.Subtract(existingQueueItem.CompletedOn.Value).TotalSeconds < 10))
                    {
                        switch (existingQueueItem.Status)
                        {
                            case QueueStatus.Completed:
                                // Return the completed result
                                return new WorkflowQueueResponse
                                {
                                    QueueId = existingQueueId,
                                    Status = QueueStatus.Completed,
                                    Result = existingQueueItem.Result,
                                    IsCompleted = true,
                                    CreatedOn = existingQueueItem.CreatedOn,
                                    CompletedOn = existingQueueItem.CompletedOn
                                };

                            case QueueStatus.Failed:
                                // Return the failed result
                                return new WorkflowQueueResponse
                                {
                                    QueueId = existingQueueId,
                                    Status = QueueStatus.Failed,
                                    IsCompleted = true,
                                    CreatedOn = existingQueueItem.CreatedOn,
                                    CompletedOn = existingQueueItem.CompletedOn,
                                    ErrorMessage = existingQueueItem.ErrorMessage
                                };

                            case QueueStatus.Timeout:
                                // Return timeout response
                                return new WorkflowQueueResponse
                                {
                                    QueueId = existingQueueId,
                                    Status = QueueStatus.Timeout,
                                    IsCompleted = false,
                                    CreatedOn = existingQueueItem.CreatedOn,
                                    ErrorMessage = "Operation Restarted timed out. Use QueueId to poll for results."
                                };

                            case QueueStatus.Pending:
                            case QueueStatus.Running:
                                // Operation is still in progress, return the existing queue ID for polling
                                return new WorkflowQueueResponse
                                {
                                    QueueId = existingQueueId,
                                    Status = existingQueueItem.Status,
                                    IsCompleted = false,
                                    CreatedOn = existingQueueItem.CreatedOn,
                                    ErrorMessage = existingQueueItem.Status == QueueStatus.Running
                                        ? "Operation is already running. Use QueueId to poll for results."
                                        : "Operation is pending. Use QueueId to poll for results."
                                };
                        }
                    }
                    else
                    {
                        WorkflowQueueItem ritem = null;
                        Guid rguid;
                        _queueItems.TryRemove(existingQueueId, out ritem);
                        _hashToQueueId.TryRemove(operationHash, out rguid);
                    }
                }
                else
                {
                    // Hash exists but queue item not found, remove the hash mapping
                    Guid rguid;
                    _hashToQueueId.TryRemove(operationHash, out rguid);
                }
            }

            // Proceed with current flow - create new queue item
            var queueId = Guid.NewGuid();
            var queueItem = new WorkflowQueueItem
            {
                QueueId = queueId,
                MethodName = methodName,
                Parameters = parameters == null ? "": JsonConvert.SerializeObject(parameters),
                Status = QueueStatus.Pending,
                CreatedOn = DateTime.Now,
                TimeoutSeconds = timeoutSeconds,
                OrganizationId = organizationId,
                UserId = userId,
                IsActive = true,
                Version = 1,
                CreatedBy = 0,
                ModifiedBy = 0,
                ModifiedOn = DateTime.Now
            };

            // Add to both dictionaries
            _queueItems.TryAdd(queueId, queueItem);
            _hashToQueueId.TryAdd(operationHash, queueId);

            try
            {
                // Start the operation in a background task
                var operationTask = Task.Run(async () =>
                {
                    try
                    {
                        queueItem.Status = QueueStatus.Running;
                        queueItem.StartedOn = DateTime.Now;
                        
                        var result = await operation();
                        
                        queueItem.Status = QueueStatus.Completed;
                        queueItem.CompletedOn = DateTime.Now;
                        queueItem.Result = result;
                        
                        return result;
                    }
                    catch (Exception ex)
                    {
                        queueItem.Status = QueueStatus.Failed;
                        queueItem.CompletedOn = DateTime.Now;
                        queueItem.ErrorMessage = ex.Message;
                        Logger.Instance(organizationId).Log(ex, Logger.LogType.Error, $"WorkflowQueue Error in {methodName}");
                        return default(T);
                    }
                });

                // Wait for the operation to complete or timeout
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));
                var completedTask = await Task.WhenAny(operationTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    // Operation timed out
                    if (queueItem.Status == QueueStatus.Failed)
                    {
                        return new WorkflowQueueResponse
                        {
                            QueueId = queueId,
                            Status = QueueStatus.Failed,
                            IsCompleted = false,
                            CreatedOn = queueItem.CreatedOn,
                            CompletedOn = DateTime.Now,
                            ErrorMessage = "Failed - " + queueItem.ErrorMessage
                        };

                    }
                    else
                    {
                        queueItem.Status = QueueStatus.Timeout;
                        queueItem.CompletedOn = DateTime.Now;
                        queueItem.ErrorMessage = "Operation timed out";

                        // Return the queue ID for polling
                        return new WorkflowQueueResponse
                        {
                            QueueId = queueId,
                            Status = QueueStatus.Timeout,
                            IsCompleted = false,
                            CreatedOn = queueItem.CreatedOn,
                            ErrorMessage = "Operation timed out. Use QueueId to poll for results."
                        };
                    }

                }
                else
                {
                    // Operation completed successfully
                    var result = await operationTask;
                    if (queueItem.Status == QueueStatus.Failed)
                    {
                        return new WorkflowQueueResponse
                        {
                            QueueId = queueId,
                            Status = QueueStatus.Failed,
                            IsCompleted = false,
                            CreatedOn = queueItem.CreatedOn,
                            CompletedOn = DateTime.Now,
                            ErrorMessage = "Failed - " + queueItem.ErrorMessage
                        };
                    }
                    else
                    {
                        return new WorkflowQueueResponse
                        {
                            QueueId = queueId,
                            Status = QueueStatus.Completed,
                            Result = result,
                            IsCompleted = true,
                            CreatedOn = queueItem.CreatedOn,
                            CompletedOn = queueItem.CompletedOn
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                queueItem.Status = QueueStatus.Failed;
                queueItem.CompletedOn = DateTime.Now;
                queueItem.ErrorMessage = ex.Message;
                
                return new WorkflowQueueResponse
                {
                    QueueId = queueId,
                    Status = QueueStatus.Failed,
                    IsCompleted = true,
                    CreatedOn = queueItem.CreatedOn,
                    CompletedOn = queueItem.CompletedOn,
                    ErrorMessage = ex.Message
                };
            }
        }

        public WorkflowQueueResponse GetQueueResult(Guid queueId)
        {
            WorkflowQueueItem queueItem;
            if (_queueItems.TryGetValue(queueId, out queueItem))
            {
                return new WorkflowQueueResponse
                {
                    QueueId = queueId,
                    Status = queueItem.Status,
                    Result = queueItem.Status == QueueStatus.Completed ? queueItem.Result : null,
                    ErrorMessage = queueItem.ErrorMessage,
                    IsCompleted = queueItem.Status == QueueStatus.Completed || queueItem.Status == QueueStatus.Failed,
                    CreatedOn = queueItem.CreatedOn,
                    CompletedOn = queueItem.CompletedOn
                };
            }

            return new WorkflowQueueResponse
            {
                QueueId = queueId,
                Status = QueueStatus.Failed,
                IsCompleted = true,
                ErrorMessage = "Queue item not found"
            };
        }

        private async Task CleanupCompletedItems()
        {
            while (true)
            {
                try	
                {
                    var itemsToRemove = _queueItems
                        .Where(kvp => kvp.Value.Status == QueueStatus.Completed || 
                                     kvp.Value.Status == QueueStatus.Failed || 
                                     kvp.Value.Status == QueueStatus.Timeout)
                        .Where(kvp => kvp.Value.CompletedOn.HasValue && 
                                     DateTime.Now.Subtract(kvp.Value.CompletedOn.Value).TotalMinutes > 30)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var queueId in itemsToRemove)
                    {
                        WorkflowQueueItem removedItem = null;
                        if (_queueItems.TryRemove(queueId, out removedItem))
                        {
                            // Also remove the corresponding hash mapping
                            var operationHash = GenerateQueueHash(removedItem.MethodName, 
                                JsonConvert.DeserializeObject(removedItem.Parameters), 
                                removedItem.OrganizationId, 
                                removedItem.UserId);
                            Guid rguid;
                            _hashToQueueId.TryRemove(operationHash, out rguid);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance(0).Log(ex, Logger.LogType.Error, "WorkflowQueue Cleanup Error");
                }

                await Task.Delay(TimeSpan.FromMinutes(5)); // Cleanup every 5 minutes
            }
        }

        public List<WorkflowQueueItem> GetQueueItems(WorkflowQueueCriteria criteria)
        {
            var query = _queueItems.Values.AsEnumerable();

            if (criteria.QueueId.HasValue)
                query = query.Where(q => q.QueueId == criteria.QueueId.Value);

            if (criteria.Status.HasValue)
                query = query.Where(q => q.Status == criteria.Status.Value);

            if (criteria.FromDate.HasValue)
                query = query.Where(q => q.CreatedOn >= criteria.FromDate.Value);

            if (criteria.ToDate.HasValue)
                query = query.Where(q => q.CreatedOn <= criteria.ToDate.Value);

            if (!string.IsNullOrEmpty(criteria.MethodName))
                query = query.Where(q => q.MethodName.Contains(criteria.MethodName));

            return query.OrderByDescending(q => q.CreatedOn).ToList();
        }

        internal string ClearQueues()
        {
            int queueitemcount = _queueItems.Count;
            int hashcount = _hashToQueueId.Count;
            _queueItems.Clear();
            _hashToQueueId.Clear();
            return string.Format("Cleared Queue Items {0} Hash Count {1}", queueitemcount, hashcount);
        }
    }

    public class WorkflowQueueItem : BaseEntity
    {
        public Guid QueueId { get; set; }
        public string MethodName { get; set; }
        public string Parameters { get; set; }
        public QueueStatus Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        [JsonIgnore]
        public object Result { get; set; }
        public string ErrorMessage { get; set; }
        public int TimeoutSeconds { get; set; }
        public long OrganizationId { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; }
        public int Version { get; set; }
        public long CreatedBy { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }

    public enum QueueStatus
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3,
        Timeout = 4
    }

    public class WorkflowQueueRequest
    {
        public string MethodName { get; set; }
        public object Parameters { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
    }

    public class WorkflowQueueResponse
    {
        public Guid QueueId { get; set; }
        public QueueStatus Status { get; set; }
        public object Result { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? CompletedOn { get; set; }

        public System.Net.Http.HttpResponseMessage TimoutResponse
        {
            get
            {
                var responseMessage = new HttpResponseMessage((System.Net.HttpStatusCode)600)
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        ReasonPhrase = "BackgroundOperation",
                        QueueId = QueueId,
                        Message = "Operation is running in background. Use QueueId to poll for results.",
                        Status = Status.ToString()
                    }))
                };
                responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                return responseMessage;
            }
        }
    }

    public class WorkflowQueueCriteria : WorkflowQueueItem
    {
        public Guid? QueueId { get; set; }
        public QueueStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string MethodName { get; set; }
    }
} 
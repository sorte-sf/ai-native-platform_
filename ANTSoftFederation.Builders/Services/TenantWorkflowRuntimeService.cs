using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data.Enums;
using Microsoft.EntityFrameworkCore;
namespace SorteFederation.Builders.Services;
public sealed class TenantWorkflowRuntimeService(BuilderDbContext metadata, TenantRuntimeContextFactory factory, BuilderAuthorizationService authorization, IHttpContextAccessor accessor, AuditService audit)
{
 public async Task<BuilderWorkflowInstance> StartScheduled(StartWorkflowRequest r,CancellationToken t)
 {
  var workflow=await metadata.Workflows.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==r.WorkflowId&&x.TenantId==r.TenantId&&x.Status==DefinitionStatus.Published,t)??throw new InvalidOperationException("Only a published workflow can be started.");
  await using var db=await factory.CreateAsync(r.TenantId,t);
  var record=await db.Records.SingleOrDefaultAsync(x=>x.Id==r.RecordId&&x.TenantId==r.TenantId,t)??throw new KeyNotFoundException("Record was not found.");
  if(!await metadata.Objects.AnyAsync(x=>x.Id==record.ObjectId&&x.TenantId==r.TenantId&&x.ApplicationId==workflow.ApplicationId,t))throw new InvalidOperationException("The selected record does not belong to this workflow's application.");
  var systemUser=new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role,BuilderRoleNames.SystemAdmin)],"scheduled-job"));
  await authorization.EnsureRecordAccess(r.TenantId,record.ObjectId,record.Id,"read",systemUser,t);
  var instance=new BuilderWorkflowInstance{TenantId=r.TenantId,WorkflowId=r.WorkflowId,RecordId=r.RecordId};
  foreach(var spec in TaskSpecs(workflow.DefinitionJson))instance.Tasks.Add(new BuilderWorkflowTask{TenantId=r.TenantId,Name=spec.Name,AssignedTo=spec.AssignedTo,DueAt=spec.DueAt,EscalateAt=spec.EscalateAt,EscalateTo=spec.EscalateTo,BranchKey=spec.BranchKey,DependsOn=spec.DependsOn,JoinKey=spec.JoinKey,Activated=string.IsNullOrWhiteSpace(spec.DependsOn),MaxRetries=spec.MaxRetries});
  db.WorkflowInstances.Add(instance);await db.SaveChangesAsync(t);
  await WriteAudit(r.TenantId,"workflow.started",instance.Id,new{r.WorkflowId,r.RecordId,source="scheduled-job"},systemUser,t);
  return instance;
 }
 public async Task<BuilderWorkflowInstance> Start(StartWorkflowRequest r,CancellationToken t){var workflow=await metadata.Workflows.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==r.WorkflowId&&x.TenantId==r.TenantId&&x.Status==DefinitionStatus.Published,t)??throw new InvalidOperationException("Only a published workflow can be started.");await using var db=await factory.CreateAsync(r.TenantId,t);var record=await db.Records.SingleOrDefaultAsync(x=>x.Id==r.RecordId&&x.TenantId==r.TenantId,t)??throw new KeyNotFoundException("Record was not found.");if(!await metadata.Objects.AnyAsync(x=>x.Id==record.ObjectId&&x.TenantId==r.TenantId&&x.ApplicationId==workflow.ApplicationId,t))throw new InvalidOperationException("The selected record does not belong to this workflow's application.");var user=accessor.HttpContext?.User??new System.Security.Claims.ClaimsPrincipal();await authorization.EnsureRecordAccess(r.TenantId,record.ObjectId,record.Id,"read",user,t);var i=new BuilderWorkflowInstance{TenantId=r.TenantId,WorkflowId=r.WorkflowId,RecordId=r.RecordId};foreach(var spec in TaskSpecs(workflow.DefinitionJson)){i.Tasks.Add(new BuilderWorkflowTask{TenantId=r.TenantId,Name=spec.Name,AssignedTo=spec.AssignedTo,DueAt=spec.DueAt,EscalateAt=spec.EscalateAt,EscalateTo=spec.EscalateTo,BranchKey=spec.BranchKey,DependsOn=spec.DependsOn,JoinKey=spec.JoinKey,Activated=string.IsNullOrWhiteSpace(spec.DependsOn),MaxRetries=spec.MaxRetries});}db.WorkflowInstances.Add(i);await db.SaveChangesAsync(t);await WriteAudit(r.TenantId,"workflow.started",i.Id,new { r.WorkflowId,r.RecordId },user,t);return i;}
 public async Task<bool> Complete(CompleteWorkflowTaskRequest r,CancellationToken t){var user=accessor.HttpContext?.User??new System.Security.Claims.ClaimsPrincipal();await using var db=await factory.CreateAsync(r.TenantId,t);var task=await db.WorkflowTasks.Include(x=>x.WorkflowInstance).SingleOrDefaultAsync(x=>x.Id==r.TaskId&&x.TenantId==r.TenantId,t);if(task is null)return false;if(task.Completed)return true;if(task.WorkflowInstance.Status!=WorkflowInstanceStatus.Running)throw new InvalidOperationException("Tasks can only be completed while the workflow is running.");if(!task.Activated)throw new InvalidOperationException("This workflow task is waiting for its branch dependencies.");var record=await db.Records.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==task.WorkflowInstance.RecordId&&x.TenantId==r.TenantId,t)??throw new KeyNotFoundException("Workflow record was not found.");await authorization.EnsureObjectAccess(r.TenantId,record.ObjectId,"read",user,t);if(!string.IsNullOrWhiteSpace(task.AssignedTo)&&!user.IsInRole(BuilderRoleNames.SystemAdmin)&&!user.IsInRole(BuilderRoleNames.TenantAdmin)){var actor=user.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)??user.Identity?.Name;var roles=user.FindAll(System.Security.Claims.ClaimTypes.Role).Concat(user.FindAll("role")).Select(x=>x.Value);if(!string.Equals(task.AssignedTo,actor,StringComparison.OrdinalIgnoreCase)&&!roles.Contains(task.AssignedTo,StringComparer.OrdinalIgnoreCase))throw new UnauthorizedAccessException("The task is assigned to another user or role.");}task.Completed=true;task.CompletedAt=DateTimeOffset.UtcNow;var all=await db.WorkflowTasks.Where(x=>x.WorkflowInstanceId==task.WorkflowInstanceId).ToListAsync(t);foreach(var dependent in all.Where(x=>!x.Activated&&!x.Completed&&!string.IsNullOrWhiteSpace(x.DependsOn))){var dependencies=dependent.DependsOn!.Split(',',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries);if(dependencies.Length>0&&dependencies.All(name=>all.Any(x=>x.Completed&&string.Equals(x.Name,name,StringComparison.OrdinalIgnoreCase))))dependent.Activated=true;}var remaining=all.Any(x=>!x.Completed&&x.Activated);if(!remaining&&all.All(x=>x.Completed))task.WorkflowInstance.Status=WorkflowInstanceStatus.Completed;task.UpdatedAt=DateTimeOffset.UtcNow;await db.SaveChangesAsync(t);await WriteAudit(r.TenantId,"workflow.task.completed",task.WorkflowInstanceId,new { r.TaskId,task.Name,activatedBranches=all.Where(x=>x.Activated&&!x.Completed).Select(x=>x.Name),workflowStatus=task.WorkflowInstance.Status.ToString() },user,t);return true;}
 public async Task<bool> Assign(Guid tenantId, Guid taskId, string assignedTo, CancellationToken t){if(string.IsNullOrWhiteSpace(assignedTo))throw new ArgumentException("AssignedTo is required.");await using var db=await factory.CreateAsync(tenantId,t);var task=await db.WorkflowTasks.Include(x=>x.WorkflowInstance).SingleOrDefaultAsync(x=>x.Id==taskId&&x.TenantId==tenantId,t);if(task is null)return false;var record=await db.Records.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==task.WorkflowInstance.RecordId&&x.TenantId==tenantId,t)??throw new KeyNotFoundException("Workflow record was not found.");var user=accessor.HttpContext?.User??new System.Security.Claims.ClaimsPrincipal();await authorization.EnsureObjectAccess(tenantId,record.ObjectId,"read",user,t);task.AssignedTo=assignedTo.Trim();task.UpdatedAt=DateTimeOffset.UtcNow;await db.SaveChangesAsync(t);await WriteAudit(tenantId,"workflow.task.assigned",task.WorkflowInstanceId,new { taskId,assignedTo=task.AssignedTo },user,t);return true;}
 public async Task<List<BuilderWorkflowInstance>> List(Guid tenantId, CancellationToken t)
 {
  var user=accessor.HttpContext?.User??new System.Security.Claims.ClaimsPrincipal();
  await using var db=await factory.CreateAsync(tenantId,t);
  var instances=await db.WorkflowInstances.AsNoTracking().Include(x=>x.Tasks).Where(x=>x.TenantId==tenantId)
   .OrderByDescending(x=>x.CreatedAt).Take(500).ToListAsync(t);
  var allowed=await AccessibleInstances(tenantId,instances.Select(x=>(x.Id,x.WorkflowId,x.RecordId)),db,user,t);
  return instances.Where(x=>allowed.Contains(x.Id)).ToList();
 }
 public async Task<List<BuilderWorkflowTask>> Inbox(Guid tenantId, CancellationToken t)
 {
  var user=accessor.HttpContext?.User??new System.Security.Claims.ClaimsPrincipal();
  var identity=user.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)??user.Identity?.Name;
  var roles=user.FindAll(System.Security.Claims.ClaimTypes.Role).Concat(user.FindAll("role")).Select(x=>x.Value).ToArray();
  await using var db=await factory.CreateAsync(tenantId,t);
  var tasks=await db.WorkflowTasks.AsNoTracking().Include(x=>x.WorkflowInstance)
   .Where(x=>x.TenantId==tenantId&&!x.Completed&&x.Activated&&x.WorkflowInstance.Status==WorkflowInstanceStatus.Running)
   .OrderBy(x=>x.DueAt??x.CreatedAt).Take(500).ToListAsync(t);
  var privileged=user.IsInRole(BuilderRoleNames.SystemAdmin)||user.IsInRole(BuilderRoleNames.TenantAdmin);
  tasks=tasks.Where(x=>privileged||x.AssignedTo==null||string.Equals(x.AssignedTo,identity,StringComparison.OrdinalIgnoreCase)||roles.Any(role=>string.Equals(role,x.AssignedTo,StringComparison.OrdinalIgnoreCase))).ToList();
  var allowed=await AccessibleInstances(tenantId,tasks.Select(x=>(x.WorkflowInstanceId,x.WorkflowInstance.WorkflowId,x.WorkflowInstance.RecordId)),db,user,t);
  return tasks.Where(x=>allowed.Contains(x.WorkflowInstanceId)).ToList();
 }

 private async Task<HashSet<Guid>> AccessibleInstances(Guid tenantId,IEnumerable<(Guid InstanceId,Guid WorkflowId,Guid RecordId)> source,TenantRuntimeDbContext runtime, System.Security.Claims.ClaimsPrincipal user,CancellationToken token)
 {
  var rows=source.ToList();
  if(rows.Count==0)return [];
  var workflowIds=rows.Select(x=>x.WorkflowId).Distinct().ToArray();
  var recordIds=rows.Select(x=>x.RecordId).Distinct().ToArray();
  var workflowApplications=await metadata.Workflows.AsNoTracking().Where(x=>x.TenantId==tenantId&&workflowIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.ApplicationId,token);
  var records=await runtime.Records.AsNoTracking().Where(x=>x.TenantId==tenantId&&recordIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.ObjectId,token);
  var objectIds=records.Values.Distinct().ToArray();
  var objectApplications=await metadata.Objects.AsNoTracking().Where(x=>x.TenantId==tenantId&&objectIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.ApplicationId,token);
  var allowed=new HashSet<Guid>();
  foreach(var row in rows)
  {
   if(!workflowApplications.TryGetValue(row.WorkflowId,out var workflowApplication)||!records.TryGetValue(row.RecordId,out var objectId)||
      !objectApplications.TryGetValue(objectId,out var recordApplication)||recordApplication!=workflowApplication)continue;
   try{await authorization.EnsureRecordAccess(tenantId,objectId,row.RecordId,"read",user,token);allowed.Add(row.InstanceId);}
   catch(UnauthorizedAccessException){ }
  }
  return allowed;
 }
 public async Task<bool> Cancel(Guid tenantId, Guid instanceId, CancellationToken t){await using var db=await factory.CreateAsync(tenantId,t);var instance=await db.WorkflowInstances.SingleOrDefaultAsync(x=>x.Id==instanceId&&x.TenantId==tenantId,t);if(instance is null)return false;var record=await db.Records.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==instance.RecordId&&x.TenantId==tenantId,t)??throw new KeyNotFoundException("Workflow record was not found.");var user=accessor.HttpContext?.User??new System.Security.Claims.ClaimsPrincipal();await authorization.EnsureObjectAccess(tenantId,record.ObjectId,"read",user,t);if(instance.Status==WorkflowInstanceStatus.Running){instance.Status=WorkflowInstanceStatus.Cancelled;instance.UpdatedAt=DateTimeOffset.UtcNow;await db.SaveChangesAsync(t);await WriteAudit(tenantId,"workflow.cancelled",instance.Id,new { instance.RecordId },user,t);}return true;}
 public async Task<BuilderWorkflowInstance> Transition(TransitionWorkflowRequest r,CancellationToken t){await using var db=await factory.CreateAsync(r.TenantId,t);var instance=await db.WorkflowInstances.SingleOrDefaultAsync(x=>x.Id==r.InstanceId&&x.TenantId==r.TenantId,t)??throw new KeyNotFoundException("Workflow instance was not found.");if(instance.Status!=WorkflowInstanceStatus.Running)throw new InvalidOperationException("Workflow instance is not running.");var record=await db.Records.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==instance.RecordId&&x.TenantId==r.TenantId,t)??throw new KeyNotFoundException("Workflow record was not found.");var user=accessor.HttpContext?.User??new System.Security.Claims.ClaimsPrincipal();await authorization.EnsureObjectAccess(r.TenantId,record.ObjectId,"read",user,t);var transition=await metadata.WorkflowTransitions.AsNoTracking().SingleOrDefaultAsync(x=>x.WorkflowId==instance.WorkflowId&&x.TenantId==r.TenantId&&x.FromState==instance.CurrentState&&x.Action==r.Action,t)??throw new InvalidOperationException("The requested workflow transition is not allowed.");if(!await ConditionMatches(instance.RecordId,transition.ConditionJson,r.TenantId,db,t))throw new InvalidOperationException("The workflow transition condition is not satisfied.");var fromState=instance.CurrentState;instance.CurrentState=transition.ToState;instance.Version++;instance.UpdatedAt=DateTimeOffset.UtcNow;db.WorkflowTasks.Add(new BuilderWorkflowTask{TenantId=r.TenantId,WorkflowInstanceId=instance.Id,Name=$"{transition.ToState} task"});await db.SaveChangesAsync(t);await WriteAudit(r.TenantId,"workflow.transitioned",instance.Id,new { fromState,toState=transition.ToState,r.Action,instance.RecordId },user,t);return instance;}
 private static async Task<bool> ConditionMatches(Guid recordId,string? conditionJson,Guid tenant,TenantRuntimeDbContext db,CancellationToken t){if(string.IsNullOrWhiteSpace(conditionJson))return true;var record=await db.Records.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==recordId&&x.TenantId==tenant,t);if(record is null)return false;using var condition=System.Text.Json.JsonDocument.Parse(conditionJson);using var data=System.Text.Json.JsonDocument.Parse(record.DataJson);return Evaluate(condition.RootElement,data.RootElement);}
 private static IReadOnlyList<string> TaskNames(string definitionJson)
 {
  var names=new List<string>();
  try
  {
   using var document=System.Text.Json.JsonDocument.Parse(definitionJson);
   if(document.RootElement.TryGetProperty("tasks",out var tasks)&&tasks.ValueKind==System.Text.Json.JsonValueKind.Array)
    foreach(var task in tasks.EnumerateArray())
    {
     var name=task.ValueKind==System.Text.Json.JsonValueKind.String?task.GetString():task.ValueKind==System.Text.Json.JsonValueKind.Object&&task.TryGetProperty("name",out var n)?n.GetString():null;
     if(!string.IsNullOrWhiteSpace(name))names.Add(name.Trim());
    }
  }
  catch(System.Text.Json.JsonException) { }
  return names.Count==0?["Initial task"]:names;
 }
 private sealed record TaskSpec(string Name, DateTimeOffset? DueAt, DateTimeOffset? EscalateAt, string? EscalateTo, string? BranchKey, string? DependsOn, string? JoinKey, int MaxRetries, string? AssignedTo);
 private static IReadOnlyList<TaskSpec> TaskSpecs(string definitionJson){var list=new List<TaskSpec>();try{using var d=System.Text.Json.JsonDocument.Parse(definitionJson);if(d.RootElement.TryGetProperty("tasks",out var tasks)&&tasks.ValueKind==System.Text.Json.JsonValueKind.Array)foreach(var x in tasks.EnumerateArray()){if(x.ValueKind==System.Text.Json.JsonValueKind.String){list.Add(new(x.GetString()!,null,null,null,null,null,null,3,null));continue;}if(x.ValueKind!=System.Text.Json.JsonValueKind.Object||!x.TryGetProperty("name",out var n))continue;DateTimeOffset? date=null,escalate=null;if(x.TryGetProperty("dueAt",out var due)&&DateTimeOffset.TryParse(due.GetString(),out var dv))date=dv;if(x.TryGetProperty("escalateAt",out var esc)&&DateTimeOffset.TryParse(esc.GetString(),out var ev))escalate=ev;var max=x.TryGetProperty("maxRetries",out var mr)&&mr.TryGetInt32(out var m)?Math.Clamp(m,0,20):3;list.Add(new(n.GetString()!,date,escalate,x.TryGetProperty("escalateTo",out var et)?et.GetString():null,x.TryGetProperty("branch",out var br)?br.GetString():null,x.TryGetProperty("dependsOn",out var dep)?dep.GetString():null,x.TryGetProperty("join",out var join)?join.GetString():null,max,x.TryGetProperty("assignedTo",out var assigned)?assigned.GetString():null));}}catch(System.Text.Json.JsonException){}return list.Count==0?[new("Initial task",null,null,null,null,null,null,3,null)]:list;}
 private static bool Evaluate(System.Text.Json.JsonElement condition,System.Text.Json.JsonElement data){if(condition.TryGetProperty("all",out var all)&&all.ValueKind==System.Text.Json.JsonValueKind.Array)return all.EnumerateArray().All(x=>Evaluate(x,data));if(condition.TryGetProperty("any",out var any)&&any.ValueKind==System.Text.Json.JsonValueKind.Array)return any.EnumerateArray().Any(x=>Evaluate(x,data));if(!condition.TryGetProperty("field",out var field)||!condition.TryGetProperty("operator",out var op)||!data.TryGetProperty(field.GetString()??string.Empty,out var actual))return false;var expected=condition.TryGetProperty("value",out var value)?value.ToString():null;var text=actual.ToString();return op.GetString()?.ToLowerInvariant() switch{"eq"=>string.Equals(text,expected,StringComparison.OrdinalIgnoreCase),"neq"=>!string.Equals(text,expected,StringComparison.OrdinalIgnoreCase),"contains"=>text.Contains(expected??string.Empty,StringComparison.OrdinalIgnoreCase),"gt"=>decimal.TryParse(text,out var a)&&decimal.TryParse(expected,out var b)&&a>b,"gte"=>decimal.TryParse(text,out var c)&&decimal.TryParse(expected,out var d)&&c>=d,"lt"=>decimal.TryParse(text,out var e)&&decimal.TryParse(expected,out var f)&&e<f,"lte"=>decimal.TryParse(text,out var g)&&decimal.TryParse(expected,out var h)&&g<=h,_=>false};}
 private async Task WriteAudit(Guid tenantId,string eventType,Guid resourceId,object data, System.Security.Claims.ClaimsPrincipal user,CancellationToken token){var json=System.Text.Json.JsonSerializer.Serialize(data);await audit.Record(new CreateAuditEventRequest(tenantId,eventType,"workflow",resourceId,json),user,accessor.HttpContext?.TraceIdentifier,token);}
}

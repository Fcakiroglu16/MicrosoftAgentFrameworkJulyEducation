using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI.Foundry.Hosting;
using Microsoft.Agents.AI.Workflows;
using WorkflowAgentDeploy;

var builder = AgentHost.CreateBuilder(args);

// Foundry projesi ve model deployment'ı — SimpleAgentDeploy/AdvancedAgentDeploy ile aynı proje.
var projectEndpoint = new Uri("https://julyeducation.services.ai.azure.com/api/projects/proj-july-education");
const string deployment = "gpt-5-mini";

var projectClient = new AIProjectClient(projectEndpoint, new DefaultAzureCredential());

// Basit Sequential orkestrasyon: Sipariş Alma -> Stok Kontrolü -> Fatura.
// Her adımın çıktısı bir sonraki agent'a girdi olarak aktarılır.
var orderAgent = SequentialAgents.GetOrderIntakeAgent(projectClient, deployment);
var stockCheckAgent = SequentialAgents.GetStockCheckAgent(projectClient, deployment);
var invoiceAgent = SequentialAgents.GetInvoiceAgent(projectClient, deployment);

var workflow = AgentWorkflowBuilder.BuildSequential(orderAgent, stockCheckAgent, invoiceAgent);

// Workflow'u tek bir AIAgent gibi dışa açıyoruz ki Foundry Responses protokolü üzerinden yayınlanabilsin.
var workflowAgent = workflow.AsAIAgent(
    "sequential-order-workflow",
    "SequentialOrderWorkflowAgent",
    "Sipariş Alma -> Stok Kontrolü -> Fatura adımlarını sırayla çalıştıran orkestrasyon agent'ı.");

builder.Services.AddFoundryResponses(workflowAgent);
builder.RegisterProtocol("responses", endpoints => endpoints.MapFoundryResponses());

var app = builder.Build();
app.Run();
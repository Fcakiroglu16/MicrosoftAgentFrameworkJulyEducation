using AdvancedAgentDeploy;
using AdvancedAgentDeploy.Services;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry.Hosting;
using Microsoft.Extensions.AI;

var builder = AgentHost.CreateBuilder(args);


var projectEndpoint = new Uri("https://julyeducation.services.ai.azure.com/api/projects/proj-july-education");
var deployment = "gpt-5-mini";


var tools = new CustomerAgentTools(new ProductDataStore());
AIAgent agent = new AIProjectClient(projectEndpoint, new DefaultAzureCredential())
    .AsAIAgent(
        deployment,
        """"
        Sen bir e-ticaret müşteri hizmetleri asistanısın. Müşterilerin ürün sorularını yanıtlamak,
        ürün aramalarına yardımcı olmak ve stok bilgisi vermek için tasarlandın.
        Kullanıcılara her zaman Türkçe yanıt ver.
        Fiyat bilgisi verirken TL cinsinden belirt.
        Stokta olmayan ürünler için özür dile ve alternatif öner.
        Yalnızca mağazamızdaki ürünler hakkında bilgi ver.

        """",
        "custer-agent", tools:
        [
            AIFunctionFactory.Create(tools.SearchProductsAsync),
            AIFunctionFactory.Create(tools.GetProductByIdAsync),
            AIFunctionFactory.Create(tools.GetProductsByCategoryAsync),
            AIFunctionFactory.Create(tools.GetProductsByPriceRangeAsync),
            AIFunctionFactory.Create(tools.GetOutOfStockProductsAsync),
            AIFunctionFactory.Create(tools.GetInStockProductsAsync),
            AIFunctionFactory.Create(tools.GetAllCategoriesAsync)
        ]);


builder.Services.AddFoundryResponses(agent);
builder.RegisterProtocol("responses", endpoints => endpoints.MapFoundryResponses());
var app = builder.Build();
app.Run();
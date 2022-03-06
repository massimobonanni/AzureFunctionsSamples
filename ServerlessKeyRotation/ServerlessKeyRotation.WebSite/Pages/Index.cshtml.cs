using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ServerlessKeyRotation.WebSite.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration Configuration;

        public string ConnectionString { get; private set; }

        public IndexModel(ILogger<IndexModel> logger,
            IConfiguration config)
        {
            _logger = logger;
            Configuration = config;
        }

        public void OnGet()
        {
            ConnectionString = Configuration["StorageAccountConnectionString"];
        }
    }
}
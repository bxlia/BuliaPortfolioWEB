using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BuliaPortfolio.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

		[BindProperty]
		public string ClientName { get; set; } = "";

		[BindProperty]
		public string ClientContact { get; set; } = "";

		[BindProperty]
		public string ClientMessage { get; set; } = "";

		public bool IsRequestSent { get; set; }

		public void OnPost()
		{
			IsRequestSent = true;
		}

        public void OnGet()
        {

        }

	}
}

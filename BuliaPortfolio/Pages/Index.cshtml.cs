using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BuliaPortfolio.Pages
{
	// ==================== ÃŒƒ≈À‹ √À¿¬ÕŒ… —“–¿Õ»÷€ ====================
	public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

		// ==================== ƒ¿ÕÕ€≈ »« ‘Œ–Ã€ ====================
		[BindProperty]
		public string ClientName { get; set; } = "";

		[BindProperty]
		public string ClientContact { get; set; } = "";

		[BindProperty]
		public string ClientMessage { get; set; } = "";

		// ==================== —Œ—“ŒﬂÕ»≈ —“–¿Õ»÷€ ====================
		public bool IsRequestSent { get; set; }

		// ==================== Œ¡–¿¡Œ“ ¿ Œ“œ–¿¬ » ‘Œ–Ã€ ====================
		public void OnPost()
		{
			IsRequestSent = true;
		}

        public void OnGet()
        {

        }

	}
}

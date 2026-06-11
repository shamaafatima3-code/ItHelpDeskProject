using Microsoft.AspNetCore.Mvc.Rendering;

namespace ItHelpDesk.Models.ViewModels
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string CurrentRole { get; set; }
        public string SelectedRole { get; set; }

        public List<SelectListItem> Roles { get; set; }
    }
}
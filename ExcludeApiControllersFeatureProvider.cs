using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace ReacodeApp
{
    // Custom feature provider to exclude API controllers from MVC routing
    // This ensures @Url.Action() only generates URLs for MVC controllers (not /api/...)
    public class ExcludeApiControllersFeatureProvider : ControllerFeatureProvider
    {
        protected override bool IsController(TypeInfo typeInfo)
        {
            // Exclude API controllers (in Controllers.Api namespace) from MVC routing
            if (typeInfo.Namespace == "ReacodeApp.Controllers.Api")
            {
                return false;
            }
            
            // Include all other controllers for MVC routing
            return base.IsController(typeInfo);
        }
    }
}





using DNTCommon.Web.Core;
using Project.Web.Admin.Interfaces;
using Project.Web.Admin.Models;
using System.Security.Claims;

namespace Project.Web.Admin.Services
{
    public class SecurityTrimmingService : ISecurityTrimmingService
    {
        private readonly HttpContext _httpContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMvcActionsDiscoveryService _mvcActionsDiscoveryService;

        public SecurityTrimmingService(
            IHttpContextAccessor httpContextAccessor,
            IMvcActionsDiscoveryService mvcActionsDiscoveryService)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _httpContext = _httpContextAccessor.HttpContext;
            _mvcActionsDiscoveryService = mvcActionsDiscoveryService ?? throw new ArgumentNullException(nameof(mvcActionsDiscoveryService));
        }

        public bool CanCurrentUserAccess(string area, string controller, string action)
        {
            return _httpContext != null && CanUserAccess(_httpContext.User, area, controller, action);
        }

        public bool CanUserAccess(ClaimsPrincipal user, string area, string controller, string action)
        {
            var currentClaimValue = $"{area}:{controller}:{action}";
            var securedControllerActions = _mvcActionsDiscoveryService.GetAllSecuredControllerActionsWithPolicy(ConstantPolicies.DynamicPermission);
            if (!securedControllerActions.SelectMany(x => x.MvcActions).Any(x => x.ActionId == currentClaimValue))
            {
                throw new KeyNotFoundException($"The `secured` area={area}/controller={controller}/action={action} with `ConstantPolicies.DynamicPermission` policy not found. Please check you have entered the area/controller/action names correctly and also it's decorated with the correct security policy.");
            }

            if (!user.Identity.IsAuthenticated)
            {
                return false;
            }

            if (user.IsInRole("admin"))
            {
                return true;
            }
            return user.HasClaim(claim => claim.Type == ConstantPolicies.DynamicPermissionClaimType && claim.Value == currentClaimValue);
        }
    }
}
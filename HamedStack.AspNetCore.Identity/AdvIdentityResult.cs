using Microsoft.AspNetCore.Identity;

namespace HamedStack.AspNetCore.Identity
{
    public class AdvIdentityResult : IdentityResult
    {
        new public IEnumerable<IdentityError>? Errors { get; set; }
        new public bool Succeeded { get; set; }
    }
}

using FluentValidation;

namespace ApiSupermercado.Application.Auth;

public sealed class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().MaximumLength(8192);
    }
}

public sealed class MagicLinkSendRequestValidator : AbstractValidator<MagicLinkSendRequest>
{
    public MagicLinkSendRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.ContinueUrl).NotEmpty().Must(BeAbsoluteHttpUrl).WithMessage("ContinueUrl must be an absolute http(s) URL.");
    }

    private static bool BeAbsoluteHttpUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
}

public sealed class MagicLinkVerifyRequestValidator : AbstractValidator<MagicLinkVerifyRequest>
{
    public MagicLinkVerifyRequestValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().MaximumLength(8192);
    }
}

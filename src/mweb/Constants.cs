namespace Minustar.Website;

public static class Constants
{
    public const int TitleLength = 60;
    public const int ShortLength = 30;

    public const string NotApplicable = "n/a";
    public const string BreadcrumbSeparator = "\u00A0» ";
    public const string LongTitle = "ThreeMoons.space";
    public const string ShortTitle = "3Moons";

    public const string ZeroWidthNonJoiner = "\u200C";
    public const string AsciiEndOfText = "\u0003";
    public const char NonbreakingSpace = '\u00A0';

    public static string MakeTitle(params string[] fragments)
        => string.Join(BreadcrumbSeparator, fragments);

    public static string GetTitle<T>(this RazorPage<T> model)
    {
        string? t = model.ViewData["Title"] as string
            ?? model.ViewBag.Title as string;
        return t is null
            ? LongTitle
            : $"{t}{BreadcrumbSeparator}{ShortTitle}";
    }

    public static class ValidationMessages
    {
        public const string FieldIsRequired = "The {0} field is required.";
    }

    public static class Policies
    {
        public const string Editor = nameof(Editor);
    }

    public static class Tables
    {
        public const string Abbreivations = "abbrevs";
        public const string AlternateForms = "alt_forms";
        public const string DictionaryEntries = "dict_entries";
        public const string DictionaryLanguages = "languages";
        public const string ServerEvents = "server_log";
        public const string ReversalEntries = "reversals";
        public const string RoleClaims = "role_claims";
        public const string Roles = "roles";
        public const string TimelineVents = "timeline";
        public const string Users = "users";
        public const string UserClaims = "user_claims";
        public const string UserLogins = "user_logins";
        public const string UserRoles = "user_roles";
        public const string UserTokens = "user_tokens";
    }

    public static class Sections
    {
        public const string Head = nameof(Head);
        public const string Scripts = nameof(Scripts);
    }

    public static class Columns
    {
        public const string Abstract = "abstract";
        public const string AccessFailedCount = "access_fails";
        public const string Category = "category";
        public const string ClaimType = "claim_type";
        public const string ClaimValue = "claim_value";
        public const string CollatorTypeName = "collator_type";
        public const string ConcurrencyStamp = "concurrency_str";
        public const string Contents = "contents";
        public const string Day = "day";
        public const string EntryId = "entry_id";
        public const string EmailConfirmed = "email_confirmed";
        public const string Email = "email";
        public const string EventType = "type";
        public const string FirstLetter = "1st_letter";
        public const string Headword = "headword";
        public const string Hidden = "hidden";
        public const string Id = "id";
        public const string IsApprox = "approx";
        public const string IsErrForm = "err_form";
        public const string Key = "key";
        public const string Kind = "kind";
        public const string LanguageId = "lang_id";
        public const string LockoutEnabled = "lockout_enabled";
        public const string LockoutEnd = "lockout_end";
        public const string LoginProvider = "login_provider";
        public const string Month = "month";
        public const string Name = "name";
        public const string NormalizedEmail = "norm_emiail";
        public const string NativeName = "native_name";
        public const string NormalizedName = "norm_name";
        public const string NormalizedUserName = "norm_uname";
        public const string Order = "order";
        public const string Password = "password";
        public const string PasswordConfirmed = "pwd_confirmed";
        public const string PasswordHash = "pwd_hash";
        public const string Payload = "payload";
        public const string PhoneNumber = "phone";
        public const string PhoneNumberConfirmed = "phone_confirmed";
        public const string Pronunciation = "pronunciation";
        public const string ProviderDisplayName = "provider_display";
        public const string ProviderKey = "provider_key";
        public const string RoleId = "role_id";
        public const string SecurityStamp = "security_stamp";
        public const string ServerVersion = "server_ver";
        public const string SortKey = "sort_key";
        public const string Target = "target";
        public const string TargetHash = "target_hash";
        public const string TargetHeadword = "target_headword";
        public const string Timestamp = "timestamp";
        public const string Title = "title";
        public const string Trigger = "trigger";
        public const string TwoFactorEnabled = "2fa_enabled";
        public const string Type = "type";
        public const string UserId = "user_id";
        public const string UserName = "user_name";
        public const string Value = "value";
        public const string Year = "year";
    }
}

namespace Minustar.Website.Data;

public class AppDbContext : IdentityDbContext
{
    public DbSet<TimelineEvent> Events => Set<TimelineEvent>();
    public DbSet<DictionaryLanguage> Languages => Set<DictionaryLanguage>();
    public DbSet<DictionaryEntry> Entries => Set<DictionaryEntry>();
    public DbSet<Abbreviation> Abbreviations => Set<Abbreviation>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder m)
    {
        base.OnModelCreating(m);

        RenameIdentityEntities(m);
        ConfigureTimelineEventEntities(m);
        ConfigureServerEventsEntityies(m);

        ConfigureDictionaryEntryEntities(m);
    }

    private static void ConfigureDictionaryEntryEntities(ModelBuilder m)
    {
        m.Entity<DictionaryEntry>(ConfigureDictionaryEntryEntity);
        m.Entity<Abbreviation>(ConfigureAbbreviationEntity);
        m.Entity<DictionaryLanguage>(ConfigureDictionaryLanguageEntity);
        m.Entity<ReversalEntry>(ConfigureReversalEntryEntity);
        m.Entity<AlternateForm>(ConfigureAlternateFormEntity);
    }

    private static void ConfigureDictionaryEntryEntity(EntityTypeBuilder<DictionaryEntry> e)
    {
        e.PropertyColumnName(x => x.Id, Columns.Id);
        e.PropertyColumnName(x => x.IsErrForm, Columns.IsErrForm);
        e.PropertyColumnName(x => x.Headword, Columns.Headword);
        e.PropertyColumnName(x => x.SortKey, Columns.SortKey);
        e.PropertyColumnName(x => x.Type, Columns.Type);
        e.PropertyColumnName(x => x.Pronunciation, Columns.Pronunciation);
        e.PropertyColumnName(x => x.FirstLetter, Columns.FirstLetter);
        e.PropertyColumnName(x => x.Contents, Columns.Contents);

        // Setting the bavigation
        e.HasOne(x => x.Language)
         .WithMany(x => x.Entries)
         .HasForeignKey(Columns.LanguageId);

        e.OwnsOne(x => x.Target, z =>
        {
            z.Property(x => x.Headword).HasColumnName(Columns.TargetHeadword);
            z.Property(x => x.TargetHash).HasColumnName(Columns.TargetHash);
        });

        e.Property(x => x.FirstLetter)
         .HasConversion(
            v => v != null ? v.ToStoredString() : null,
            v => LetterInfo.Parse(v)
            );

        e.ToTable(Tables.DictionaryEntries);
    }
    private static void ConfigureAbbreviationEntity(EntityTypeBuilder<Abbreviation> e)
    {
        e.PropertyColumnName(x => x.Id, Columns.Id);
        e.PropertyColumnName(x => x.Kind, Columns.Kind)
            .HasMaxLength(ShortLength)
            .HasConversion<EnumToStringConverter<AbbreviationKind>>();
        e.PropertyColumnName(x => x.Key, Columns.Key);
        e.PropertyColumnName(x => x.Value, Columns.Value);
        e.PropertyColumnName(x => x.Contents, Columns.Contents);

        e.HasOne(x => x.Language)
         .WithMany(x => x.Abbreviations)
         .HasForeignKey(Columns.LanguageId);

        e.ToTable(Tables.Abbreivations);
    }

    private static void ConfigureDictionaryLanguageEntity(EntityTypeBuilder<DictionaryLanguage> e)
    {
        e.PropertyColumnName(x => x.Id, Columns.Id);
        e.PropertyColumnName(x => x.Name, Columns.Name);
        e.PropertyColumnName(x => x.NativeName, Columns.NativeName);
        e.PropertyColumnName(x => x.Abstract, Columns.Abstract);
        e.PropertyColumnName(x => x.CollatorTypeName, Columns.CollatorTypeName);

        e.ToTable(Tables.DictionaryLanguages);
    }

    private static void ConfigureReversalEntryEntity(EntityTypeBuilder<ReversalEntry> e)
    {
        e.PropertyColumnName(x => x.Id, Columns.Id);
        e.PropertyColumnName(x => x.Headword, Columns.Headword);
        e.PropertyColumnName(x => x.SortKey, Columns.SortKey);
        e.PropertyColumnName(x => x.Contents, Columns.Contents);

        e.HasOne(x => x.Entry)
         .WithMany(x => x.Reversaks)
         .HasForeignKey(Columns.EntryId);

        e.ToTable(Tables.ReversalEntries);
    }

    private static void ConfigureAlternateFormEntity(EntityTypeBuilder<AlternateForm> e)
    {
        e.PropertyColumnName(x => x.Id, Columns.Id);
        e.PropertyColumnName(x => x.Trigger, Columns.Trigger);
        e.PropertyColumnName(x => x.Headword, Columns.Headword);
        e.PropertyColumnName(x => x.Order, Columns.Order);
        e.PropertyColumnName(x => x.Contents, Columns.Contents);

        e.HasOne(x => x.Entry)
         .WithMany(x => x.AlternateForms)
         .HasForeignKey(Columns.EntryId);

        e.ToTable(Tables.AlternateForms);
    }

    private static void ConfigureServerEventEntity(EntityTypeBuilder<ServerEvent> e)
    {
        e.PropertyColumnName(x => x.Id, Columns.Id);
        e.PropertyColumnName(x => x.ServerVersion, Columns.ServerVersion);
        e.PropertyColumnName(x => x.Timestamp, Columns.Timestamp);
        e.PropertyColumnName(x => x.Category, Columns.Category);
        e.PropertyColumnName(x => x.EventType, Columns.EventType);
        e.PropertyColumnName(x => x.Payload, Columns.Payload);

        e.ToTable(Tables.ServerEvents);
    }

    private static void ConfigureServerEventsEntityies(ModelBuilder m)
    {
        m.Entity<ServerEvent>(ConfigureServerEventEntity);
    }

    private static void RenameIdentityEntities(ModelBuilder m)
    {
        m.Entity<IdentityRole>(ConfigureIdentityRoleEntity);
        m.Entity<IdentityUserToken<string>>(ConfigureIdentityUserTokenEntity);

        m.Entity<IdentityRoleClaim<string>>(ConfigureIdentityRoleClaimEntity);

        m.Entity<IdentityUserLogin<string>>(ConfigureIdentityUserLoginEntity);
        m.Entity<IdentityUser>(ConfigureIdentityUserEntity);
        m.Entity<IdentityUserClaim<string>>(ConfigureIdentityUserClaimEntity);
        m.Entity<IdentityUserRole<string>>(ConfigureIdentityUserRoleEntity);
    }

    private static void ConfigureIdentityRoleEntity(EntityTypeBuilder<IdentityRole> e)
    {
        e.PropertyColumnName(x => x.Id, Columns.Id);
        e.PropertyColumnName(x => x.Name, Columns.Name);
        e.PropertyColumnName(x => x.NormalizedName, Columns.NormalizedName);
        e.PropertyColumnName(x => x.ConcurrencyStamp, Columns.ConcurrencyStamp);

        e.ToTable(Tables.Roles);
    }

    private static void ConfigureIdentityUserRoleEntity(EntityTypeBuilder<IdentityUserRole<string>> e)
    {
        e.PropertyColumnName(x => x.UserId, Columns.UserId);
        e.PropertyColumnName(x => x.RoleId, Columns.RoleId);

        e.ToTable(Tables.UserRoles);
    }

    private static void ConfigureIdentityUserClaimEntity(EntityTypeBuilder<IdentityUserClaim<string>> e)
    {
        e.PropertyColumnName(x => x.Id, Columns.Id);
        e.PropertyColumnName(x => x.UserId, Columns.UserId);
        e.PropertyColumnName(x => x.ClaimType, Columns.ClaimType);
        e.PropertyColumnName(x => x.ClaimValue, Columns.ClaimValue);

        e.ToTable(Tables.UserClaims);
    }

    private static void ConfigureIdentityUserEntity(EntityTypeBuilder<IdentityUser> e)
    {
        e.PropertyColumnName(x => x.Id, Columns.Id);
        e.PropertyColumnName(x => x.UserName, Columns.UserName);
        e.PropertyColumnName(x => x.NormalizedUserName, Columns.NormalizedUserName);
        e.PropertyColumnName(x => x.Email, Columns.Email);
        e.PropertyColumnName(x => x.NormalizedEmail, Columns.NormalizedEmail);
        e.PropertyColumnName(x => x.EmailConfirmed, Columns.EmailConfirmed);
        e.PropertyColumnName(x => x.PasswordHash, Columns.PasswordHash);
        e.PropertyColumnName(x => x.SecurityStamp, Columns.SecurityStamp);
        e.PropertyColumnName(x => x.ConcurrencyStamp, Columns.ConcurrencyStamp);
        e.PropertyColumnName(x => x.PhoneNumber, Columns.PhoneNumber);
        e.PropertyColumnName(x => x.PhoneNumberConfirmed, Columns.PhoneNumberConfirmed);
        e.PropertyColumnName(x => x.TwoFactorEnabled, Columns.TwoFactorEnabled);
        e.PropertyColumnName(x => x.LockoutEnd, Columns.LockoutEnd);
        e.PropertyColumnName(x => x.LockoutEnabled, Columns.LockoutEnabled);
        e.PropertyColumnName(x => x.AccessFailedCount, Columns.AccessFailedCount);

        e.ToTable(Tables.Users);
    }

    private static void ConfigureIdentityUserLoginEntity(EntityTypeBuilder<IdentityUserLogin<string>> e)
    {
        e.PropertyColumnName(x => x.UserId, Columns.UserId);
        e.PropertyColumnName(x => x.ProviderKey, Columns.ProviderKey);
        e.PropertyColumnName(x => x.ProviderDisplayName, Columns.ProviderDisplayName);
        e.PropertyColumnName(x => x.LoginProvider, Columns.LoginProvider);

        e.ToTable(Tables.UserLogins);
    }
    private static void ConfigureIdentityRoleClaimEntity(EntityTypeBuilder<IdentityRoleClaim<string>> e)
    {
        e.PropertyColumnName(x => x.Id, Columns.Id);
        e.PropertyColumnName(x => x.RoleId, Columns.RoleId);
        e.PropertyColumnName(x => x.ClaimType, Columns.ClaimType);
        e.PropertyColumnName(x => x.ClaimValue, Columns.ClaimValue);

        e.ToTable(Tables.RoleClaims);
    }

    private static void ConfigureIdentityUserTokenEntity(EntityTypeBuilder<IdentityUserToken<string>> e)
    {
        e.PropertyColumnName(x => x.UserId, Columns.UserId);
        e.PropertyColumnName(x => x.Name, Columns.Name);
        e.PropertyColumnName(x => x.Value, Columns.Value);
        e.PropertyColumnName(x => x.LoginProvider, Columns.LoginProvider);

        e.ToTable(Tables.UserTokens);
    }

    private static void ConfigureTimelineEventEntities(ModelBuilder m)
    {
        m.Entity<TimelineEvent>(e =>
        {
            e.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName(Columns.Id);

            e.Property(x => x.IsApprox)
                .HasDefaultValue(false)
                .HasColumnName(Columns.IsApprox);

            e.Property(x => x.Year)
                .IsRequired()
                .HasColumnName(Columns.Year);

            e.Property(x => x.Month)
                .HasColumnName(Columns.Month);

            e.Property(x => x.Day)
                .HasColumnName(Columns.Day);

            e.Property(x => x.Title)
                .HasMaxLength(TitleLength)
                .IsRequired()
                .HasColumnName(Columns.Title);

            e.Property(x => x.Contents)
                .IsRequired()
                .HasColumnName(Columns.Contents);

            e.HasKey(x => x.Id);
            e.ToTable(Tables.TimelineVents);
        });
    }
}
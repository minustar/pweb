IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [languages] (
    [id] int NOT NULL IDENTITY,
    [name] nvarchar(60) NOT NULL,
    [native_name] nvarchar(60) NULL,
    [abstract] nvarchar(max) NULL,
    [collator_type] nvarchar(255) NULL,
    CONSTRAINT [PK_languages] PRIMARY KEY ([id])
);
GO

CREATE TABLE [roles] (
    [id] nvarchar(450) NOT NULL,
    [name] nvarchar(256) NULL,
    [norm_name] nvarchar(256) NULL,
    [concurrency_str] nvarchar(max) NULL,
    CONSTRAINT [PK_roles] PRIMARY KEY ([id])
);
GO

CREATE TABLE [server_log] (
    [id] uniqueidentifier NOT NULL,
    [server_ver] nvarchar(30) NOT NULL,
    [timestamp] datetimeoffset NOT NULL,
    [category] nvarchar(60) NOT NULL,
    [type] nvarchar(60) NOT NULL,
    [payload] nvarchar(max) NULL,
    CONSTRAINT [PK_server_log] PRIMARY KEY ([id])
);
GO

CREATE TABLE [timeline] (
    [id] int NOT NULL IDENTITY,
    [approx] bit NOT NULL DEFAULT CAST(0 AS bit),
    [year] int NOT NULL,
    [month] int NULL,
    [day] int NULL,
    [title] nvarchar(60) NOT NULL,
    [contents] nvarchar(max) NOT NULL,
    [Hidden] bit NOT NULL,
    CONSTRAINT [PK_timeline] PRIMARY KEY ([id])
);
GO

CREATE TABLE [users] (
    [id] nvarchar(450) NOT NULL,
    [user_name] nvarchar(256) NULL,
    [norm_uname] nvarchar(256) NULL,
    [email] nvarchar(256) NULL,
    [norm_emiail] nvarchar(256) NULL,
    [email_confirmed] bit NOT NULL,
    [pwd_hash] nvarchar(max) NULL,
    [security_stamp] nvarchar(max) NULL,
    [concurrency_str] nvarchar(max) NULL,
    [phone] nvarchar(max) NULL,
    [phone_confirmed] bit NOT NULL,
    [2fa_enabled] bit NOT NULL,
    [lockout_end] datetimeoffset NULL,
    [lockout_enabled] bit NOT NULL,
    [access_fails] int NOT NULL,
    CONSTRAINT [PK_users] PRIMARY KEY ([id])
);
GO

CREATE TABLE [abbrevs] (
    [id] int NOT NULL IDENTITY,
    [lang_id] int NOT NULL,
    [kind] int NOT NULL,
    [key] nvarchar(30) NOT NULL,
    [value] nvarchar(60) NOT NULL,
    [contents] nvarchar(max) NULL,
    CONSTRAINT [PK_abbrevs] PRIMARY KEY ([id]),
    CONSTRAINT [FK_abbrevs_languages_lang_id] FOREIGN KEY ([lang_id]) REFERENCES [languages] ([id]) ON DELETE CASCADE
);
GO

CREATE TABLE [dict_entries] (
    [id] int NOT NULL IDENTITY,
    [lang_id] int NOT NULL,
    [err_form] bit NOT NULL,
    [headword] nvarchar(60) NOT NULL,
    [sort_key] nvarchar(60) NULL,
    [type] nvarchar(30) NULL,
    [pronunciation] nvarchar(60) NULL,
    [target_headword] nvarchar(max) NULL,
    [target_hash] nvarchar(max) NULL,
    [1st_letter] nvarchar(max) NULL,
    [contents] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_dict_entries] PRIMARY KEY ([id]),
    CONSTRAINT [FK_dict_entries_languages_lang_id] FOREIGN KEY ([lang_id]) REFERENCES [languages] ([id]) ON DELETE CASCADE
);
GO

CREATE TABLE [role_claims] (
    [id] int NOT NULL IDENTITY,
    [role_id] nvarchar(450) NOT NULL,
    [claim_type] nvarchar(max) NULL,
    [claim_value] nvarchar(max) NULL,
    CONSTRAINT [PK_role_claims] PRIMARY KEY ([id]),
    CONSTRAINT [FK_role_claims_roles_role_id] FOREIGN KEY ([role_id]) REFERENCES [roles] ([id]) ON DELETE CASCADE
);
GO

CREATE TABLE [user_claims] (
    [id] int NOT NULL IDENTITY,
    [user_id] nvarchar(450) NOT NULL,
    [claim_type] nvarchar(max) NULL,
    [claim_value] nvarchar(max) NULL,
    CONSTRAINT [PK_user_claims] PRIMARY KEY ([id]),
    CONSTRAINT [FK_user_claims_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE CASCADE
);
GO

CREATE TABLE [user_logins] (
    [login_provider] nvarchar(128) NOT NULL,
    [provider_key] nvarchar(128) NOT NULL,
    [provider_display] nvarchar(max) NULL,
    [user_id] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_user_logins] PRIMARY KEY ([login_provider], [provider_key]),
    CONSTRAINT [FK_user_logins_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE CASCADE
);
GO

CREATE TABLE [user_roles] (
    [user_id] nvarchar(450) NOT NULL,
    [role_id] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_user_roles] PRIMARY KEY ([user_id], [role_id]),
    CONSTRAINT [FK_user_roles_roles_role_id] FOREIGN KEY ([role_id]) REFERENCES [roles] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_user_roles_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE CASCADE
);
GO

CREATE TABLE [user_tokens] (
    [user_id] nvarchar(450) NOT NULL,
    [login_provider] nvarchar(128) NOT NULL,
    [name] nvarchar(128) NOT NULL,
    [value] nvarchar(max) NULL,
    CONSTRAINT [PK_user_tokens] PRIMARY KEY ([user_id], [login_provider], [name]),
    CONSTRAINT [FK_user_tokens_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([id]) ON DELETE CASCADE
);
GO

CREATE TABLE [alt_forms] (
    [id] int NOT NULL IDENTITY,
    [entry_id] int NOT NULL,
    [trigger] nvarchar(60) NULL,
    [headword] nvarchar(60) NOT NULL,
    [order] int NULL,
    [contents] nvarchar(max) NULL,
    CONSTRAINT [PK_alt_forms] PRIMARY KEY ([id]),
    CONSTRAINT [FK_alt_forms_dict_entries_entry_id] FOREIGN KEY ([entry_id]) REFERENCES [dict_entries] ([id]) ON DELETE CASCADE
);
GO

CREATE TABLE [reversals] (
    [id] int NOT NULL IDENTITY,
    [entry_id] int NOT NULL,
    [headword] nvarchar(60) NOT NULL,
    [sort_key] nvarchar(60) NULL,
    [contents] nvarchar(max) NULL,
    CONSTRAINT [PK_reversals] PRIMARY KEY ([id]),
    CONSTRAINT [FK_reversals_dict_entries_entry_id] FOREIGN KEY ([entry_id]) REFERENCES [dict_entries] ([id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_abbrevs_lang_id] ON [abbrevs] ([lang_id]);
GO

CREATE INDEX [IX_alt_forms_entry_id] ON [alt_forms] ([entry_id]);
GO

CREATE INDEX [IX_dict_entries_lang_id] ON [dict_entries] ([lang_id]);
GO

CREATE INDEX [IX_reversals_entry_id] ON [reversals] ([entry_id]);
GO

CREATE INDEX [IX_role_claims_role_id] ON [role_claims] ([role_id]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [roles] ([norm_name]) WHERE [norm_name] IS NOT NULL;
GO

CREATE INDEX [IX_user_claims_user_id] ON [user_claims] ([user_id]);
GO

CREATE INDEX [IX_user_logins_user_id] ON [user_logins] ([user_id]);
GO

CREATE INDEX [IX_user_roles_role_id] ON [user_roles] ([role_id]);
GO

CREATE INDEX [EmailIndex] ON [users] ([norm_emiail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [users] ([norm_uname]) WHERE [norm_uname] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211125160344_CreateDatabase', N'6.0.0');
GO

COMMIT;
GO


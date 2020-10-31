﻿-- SPDX-License-Identifier: Apache-2.0
-- Licensed to the Ed-Fi Alliance under one or more agreements.
-- The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
-- See the LICENSE and NOTICES files in the project root for more information.

CREATE TABLE adminapp.Users (
    Id text NOT NULL,
    UserName character varying(256) NULL,
    NormalizedUserName character varying(256) NULL,
    Email character varying(256) NULL,
    NormalizedEmail character varying(256) NULL,
    EmailConfirmed boolean NOT NULL,
    PasswordHash text NULL,
    SecurityStamp text NULL,
    ConcurrencyStamp text NULL,
    PhoneNumber text NULL,
    PhoneNumberConfirmed boolean NOT NULL,
    TwoFactorEnabled boolean NOT NULL,
    LockoutEnd timestamp with time zone NULL,
    LockoutEnabled boolean NOT NULL,
    AccessFailedCount integer NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY (Id)
);

CREATE TABLE adminapp.Roles (
    Id text NOT NULL,
    Name character varying(256) NULL,
    NormalizedName character varying(256) NULL,
    ConcurrencyStamp text NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (Id)
);

CREATE TABLE adminapp.UserClaims (
    Id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    UserId text NOT NULL,
    ClaimType text NULL,
    ClaimValue text NULL,
    CONSTRAINT PK_UserClaims PRIMARY KEY (Id),
    CONSTRAINT FK_UserClaims_Users_UserId FOREIGN KEY (UserId) REFERENCES adminapp.Users (Id) ON DELETE CASCADE
);

CREATE TABLE adminapp.RoleClaims (
    Id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    RoleId text NOT NULL,
    ClaimType text NULL,
    ClaimValue text NULL,
    CONSTRAINT PK_RoleClaims PRIMARY KEY (Id),
    CONSTRAINT FK_RoleClaims_Roles_RoleId FOREIGN KEY (RoleId) REFERENCES adminapp.Roles (Id) ON DELETE CASCADE
);

CREATE TABLE adminapp.UserLogins (
    LoginProvider character varying(128) NOT NULL,
    ProviderKey character varying(128) NOT NULL,
    ProviderDisplayName text NULL,
    UserId text NOT NULL,
    CONSTRAINT PK_UserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_UserLogins_Users_UserId FOREIGN KEY (UserId) REFERENCES adminapp.Users (Id) ON DELETE CASCADE
);

CREATE TABLE adminapp.UserRoles (
    UserId text NOT NULL,
    RoleId text NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Roles_RoleId FOREIGN KEY (RoleId) REFERENCES adminapp.Roles (Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Users_UserId FOREIGN KEY (UserId) REFERENCES adminapp.Users (Id) ON DELETE CASCADE
);

CREATE TABLE adminapp.UserTokens (
    UserId text NOT NULL,
    LoginProvider character varying(128) NOT NULL,
    Name character varying(128) NOT NULL,
    Value text NULL,
    CONSTRAINT PK_UserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_UserTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES adminapp.Users (Id) ON DELETE CASCADE
);

CREATE INDEX IX_RoleClaims_RoleId ON adminapp.RoleClaims (RoleId);
CREATE UNIQUE INDEX RoleNameIndex ON adminapp.Roles (NormalizedName);
CREATE INDEX IX_UserClaims_UserId ON adminapp.UserClaims (UserId);
CREATE INDEX IX_UserLogins_UserId ON adminapp.UserLogins (UserId);
CREATE INDEX IX_UserRoles_RoleId ON adminapp.UserRoles (RoleId);
CREATE INDEX EmailIndex ON adminapp.Users (NormalizedEmail);
CREATE UNIQUE INDEX UserNameIndex ON adminapp.Users (NormalizedUserName);

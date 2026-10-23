global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore;

global using System.Security.Claims;
global using System.Text;
global using Microsoft.AspNetCore.Authentication.JwtBearer; 
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Authentication.Google;
global using Microsoft.AspNetCore.Authentication.Facebook;
global using Microsoft.AspNetCore.Authentication.Twitter; 
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Mvc;

global using System.IdentityModel.Tokens.Jwt; 
global using EntityFrameworkCore.AutoHistory.Extensions;
global using EntityFrameworkCore.AutoHistory; 
 
global using System.ComponentModel.DataAnnotations;
 
global using Polly;
 
global using Microsoft.AspNetCore.Authentication.OpenIdConnect;
global using Microsoft.IdentityModel.Protocols.OpenIdConnect;

global using Microsoft.AspNetCore.Http.Features;
global using System.Text.Json;
global using System.Text.Json.Serialization;

global using ANTSoftFederation.IdentityServer;
global using ANTSoftFederation.IdentityServer.Bootstraping;
 
global using Scalar.AspNetCore; 
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using ANTSoftFederation.Logger;
global using ANTSoftFederation.Shared.EndPoints;
global using Asp.Versioning;
global using Asp.Versioning.Builder;
global using Figgle.Fonts;
global using System.Reflection;
global using Microsoft.AspNetCore.ResponseCompression;
global using MailKit;
global using MailKit.Security;
global using MimeKit;

global using Microsoft.Extensions.Caching.Memory;

global using System.Security.Cryptography;
global using System.Net.Http.Headers;
global using Microsoft.Extensions.Options;
global using System.Text.Encodings.Web; 

global using ANTSoftFederation.EncryptEFCoreColumn;

global using ANTSoftFederation.IdentityServer.Models;

global using ANTSoftFederation.IdentityServer.DataDbContext;
global using ANTSoftFederation.IdentityServer.Services.Custom;
global using ANTSoftFederation.Shared;

global using ANTSoftFederation.IdentityServer.Services.Auths;
global using ANTSoftFederation.Shared.Services;
global using ANTSoftFederation.GlobalEnums;
global using ANTSoftFederation.Shared.Models;
global using ANTSoftFederation.IdentityServer.DataDbContext.Entities;
global using ANTSoftFederation.Shared.Helpers;

global using ANTSoftFederation.IdentityServer.Enums;
global using ANTSoftFederation.IdentityServer.DTOs.Base; 
global using ANTSoftFederation.IdentityServer.Services;
global using ANTSoftFederation.IdentityServer.DTOs;
global using ANTSoftFederation.IdentityServer.Services.Email;
global using ANTSoftFederation.IdentityServer.Services.Interfaces;

global using ANTSoftFederation.IdentityServer.DataDbContext.Entities.Base;
global using LoginRespondDto = ANTSoftFederation.IdentityServer.Models.LoginRespondDto;
global using UserOtpVerificationDto = ANTSoftFederation.IdentityServer.Models.UserOtpVerificationDto;
global using MailData = ANTSoftFederation.IdentityServer.Models.MailData;
global using MailDataWithAttachments = ANTSoftFederation.IdentityServer.Models.MailDataWithAttachments;
global using EmailSettings = ANTSoftFederation.IdentityServer.Models.EmailSettings;

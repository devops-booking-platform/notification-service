using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Data;
using NotificationService.Domain.DTOs;

namespace NotificationService.Tests.Integration.Controllers;

[Collection("NotificationDisabledController Collection")]
public class NotificationDisabledControllerTests : IClassFixture<NotificationServiceWebApplicationFactory>, IDisposable
{
    private readonly NotificationServiceWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _testUserId = Guid.NewGuid();

    public NotificationDisabledControllerTests(NotificationServiceWebApplicationFactory factory)
    {
        _factory = factory;
        
        // Clear database before each test
        ClearDatabase();
        
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.AuthenticationScheme, options => { });
            });
        }).CreateClient();
        
        _client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "User");
    }

    private void ClearDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Set<Notification>().RemoveRange(context.Set<Notification>());
        context.Set<Domain.Entities.NotificationDisabled>().RemoveRange(context.Set<Domain.Entities.NotificationDisabled>());
        context.SaveChanges();
    }

    [Fact]
    public async Task DisableNotification_ShouldCreateNewDisabledNotification_WhenNotificationTypeNotDisabled()
    {
        // Arrange
        var request = new EnableDisableNotificationRequest
        {
            NotificationType = NotificationType.ReservationCreated
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/notification-disabled/disable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var disabledNotification = await context.Set<NotificationDisabled>()
            .FirstOrDefaultAsync(n => n.UserId == _testUserId && n.NotificationType == NotificationType.ReservationCreated);
        
        disabledNotification.Should().NotBeNull();
        disabledNotification!.UserId.Should().Be(_testUserId);
        disabledNotification.NotificationType.Should().Be(NotificationType.ReservationCreated);
    }

    [Fact]
    public async Task DisableNotification_ShouldOnlyAffectCurrentUser()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var otherUserId = Guid.NewGuid();
        var otherUserDisabled = NotificationDisabled.Create(otherUserId, NotificationType.ReservationCreated);
        await context.Set<NotificationDisabled>().AddAsync(otherUserDisabled);
        await context.SaveChangesAsync();
        
        var request = new EnableDisableNotificationRequest
        {
            NotificationType = NotificationType.ReservationCreated
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/notification-disabled/disable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify both entries exist
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var allDisabled = await verifyContext.Set<NotificationDisabled>()
            .Where(n => n.NotificationType == NotificationType.ReservationCreated)
            .ToListAsync();
        
        allDisabled.Should().HaveCount(2);
        allDisabled.Should().Contain(n => n.UserId == _testUserId);
        allDisabled.Should().Contain(n => n.UserId == otherUserId);
    }

    [Fact]
    public async Task EnableNotification_ShouldRemoveDisabledNotification_WhenIdProvided()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var disabledNotification = NotificationDisabled.Create(_testUserId, NotificationType.ReservationCreated);
        await context.Set<NotificationDisabled>().AddAsync(disabledNotification);
        await context.SaveChangesAsync();
        
        var request = new EnableDisableNotificationRequest
        {
            Id = disabledNotification.Id,
            NotificationType = NotificationType.ReservationCreated
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/notification-disabled/enable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify it was removed
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var exists = await verifyContext.Set<NotificationDisabled>()
            .AnyAsync(n => n.Id == disabledNotification.Id);
        
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task EnableNotification_ShouldReturnNotFound_WhenDisabledNotificationDoesNotExist()
    {
        // Arrange
        var request = new EnableDisableNotificationRequest
        {
            Id = Guid.NewGuid(),
            NotificationType = NotificationType.ReservationCreated
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/notification-disabled/enable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDisabledNotifications_ShouldReturnAllDisabledNotifications_ForCurrentUser()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var disabledNotifications = new List<NotificationDisabled>
        {
            NotificationDisabled.Create(_testUserId, NotificationType.ReservationCreated),
            NotificationDisabled.Create(_testUserId, NotificationType.ReservationCanceled),
            NotificationDisabled.Create(_testUserId, NotificationType.HostRated)
        };
        
        await context.Set<NotificationDisabled>().AddRangeAsync(disabledNotifications);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/notification-disabled");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<GetDisabledNotificationsResponse>>();
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(3);
        result.Should().Contain(n => n.NotificationType == NotificationType.ReservationCreated);
        result.Should().Contain(n => n.NotificationType == NotificationType.ReservationCanceled);
        result.Should().Contain(n => n.NotificationType == NotificationType.HostRated);
    }

    [Fact]
    public async Task GetDisabledNotifications_ShouldReturnOnlyCurrentUserNotifications()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var otherUserId = Guid.NewGuid();
        var disabledNotifications = new List<NotificationDisabled>
        {
            NotificationDisabled.Create(_testUserId, NotificationType.ReservationCreated),
            NotificationDisabled.Create(otherUserId, NotificationType.ReservationCanceled),
            NotificationDisabled.Create(_testUserId, NotificationType.HostRated)
        };
        
        await context.Set<NotificationDisabled>().AddRangeAsync(disabledNotifications);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/notification-disabled");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<GetDisabledNotificationsResponse>>();
        
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result.Should().NotContain(n => n.NotificationType == NotificationType.ReservationCanceled);
    }

    [Fact]
    public async Task GetDisabledNotifications_ShouldReturnEmptyList_WhenNoDisabledNotificationsExist()
    {
        // Act
        var response = await _client.GetAsync("/api/notification-disabled");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<GetDisabledNotificationsResponse>>();
        
        result.Should().NotBeNull();
        result!.Should().BeEmpty();
    }

    [Fact]
    public async Task DisableNotification_ShouldRequireAuthentication()
    {
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();
        var request = new EnableDisableNotificationRequest
        {
            NotificationType = NotificationType.ReservationCreated
        };

        // Act
        var response = await unauthenticatedClient.PostAsJsonAsync("/api/notification-disabled/disable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EnableNotification_ShouldRequireAuthentication()
    {
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();
        var request = new EnableDisableNotificationRequest
        {
            NotificationType = NotificationType.ReservationCreated
        };

        // Act
        var response = await unauthenticatedClient.PostAsJsonAsync("/api/notification-disabled/enable", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDisabledNotifications_ShouldRequireAuthentication()
    {
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();

        // Act
        var response = await unauthenticatedClient.GetAsync("/api/notification-disabled");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DisableNotification_ShouldHandleMultipleNotificationTypes()
    {
        // Arrange
        var notificationTypes = new[]
        {
            NotificationType.ReservationCreated,
            NotificationType.ReservationCanceled,
            NotificationType.HostRated,
            NotificationType.AccommodationRated
        };

        // Act
        foreach (var notificationType in notificationTypes)
        {
            var request = new EnableDisableNotificationRequest { NotificationType = notificationType };
            var response = await _client.PostAsJsonAsync("/api/notification-disabled/disable", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var disabled = await context.Set<NotificationDisabled>()
            .Where(n => n.UserId == _testUserId)
            .ToListAsync();
        
        disabled.Should().HaveCount(4);
    }

    [Fact]
    public async Task EnableNotification_ShouldHandleMultipleNotificationTypes()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var disabledNotifications = new[]
        {
            NotificationDisabled.Create(_testUserId, NotificationType.ReservationCreated),
            NotificationDisabled.Create(_testUserId, NotificationType.ReservationCanceled),
            NotificationDisabled.Create(_testUserId, NotificationType.HostRated)
        };
        
        await context.Set<NotificationDisabled>().AddRangeAsync(disabledNotifications);
        await context.SaveChangesAsync();

        // Act
        foreach (var disabled in disabledNotifications)
        {
            var request = new EnableDisableNotificationRequest 
            { 
                Id = disabled.Id,
                NotificationType = disabled.NotificationType 
            };
            var response = await _client.PostAsJsonAsync("/api/notification-disabled/enable", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var remaining = await verifyContext.Set<NotificationDisabled>()
            .Where(n => n.UserId == _testUserId)
            .ToListAsync();
        
        remaining.Should().BeEmpty();
    }

    public void Dispose()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
    }
}

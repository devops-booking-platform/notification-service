using NotificationService.Repositories;

namespace NotificationService.Tests.Repositories;

public class NotificationQueryableExtensionsTests
{
    private IQueryable<Notification> CreateTestNotifications()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var notifications = new List<Notification>
        {
            CreateNotification(userId1, NotificationType.ReservationCreated, "Message 1", read: false),
            CreateNotification(userId1, NotificationType.ReservationCanceled, "Message 2", read: true),
            CreateNotification(userId1, NotificationType.ReservationResponded, "Message 3", read: false),
            CreateNotification(userId2, NotificationType.HostRated, "Message 4", read: true),
            CreateNotification(userId2, NotificationType.AccommodationRated, "Message 5", read: false),
            CreateNotification(userId2, NotificationType.ReservationCreated, "Message 6", read: true),
        };

        return notifications.AsQueryable();
    }

    private Notification CreateNotification(Guid userId, NotificationType type, string message, bool read)
    {
        var notification = Notification.Create(userId, type, message);
        if (read)
        {
            notification.MarkRead();
        }
        return notification;
    }

    #region QueryByRead Tests

    [Fact]
    public void QueryByRead_WhenReadIsNull_ShouldReturnAllNotifications()
    {
        // Arrange
        var query = CreateTestNotifications();

        // Act
        var result = query.QueryByRead(null).ToList();

        // Assert
        result.Should().HaveCount(6);
    }

    [Fact]
    public void QueryByRead_WhenReadIsTrue_ShouldReturnOnlyReadNotifications()
    {
        // Arrange
        var query = CreateTestNotifications();

        // Act
        var result = query.QueryByRead(true).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(n => n.Read == true);
    }

    [Fact]
    public void QueryByRead_WhenReadIsFalse_ShouldReturnOnlyUnreadNotifications()
    {
        // Arrange
        var query = CreateTestNotifications();

        // Act
        var result = query.QueryByRead(false).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(n => n.Read == false);
    }

    [Fact]
    public void QueryByRead_WhenNoNotificationsMatchFilter_ShouldReturnEmptyList()
    {
        // Arrange
        var notifications = new List<Notification>
        {
            CreateNotification(Guid.NewGuid(), NotificationType.ReservationCreated, "Message 1", read: true),
            CreateNotification(Guid.NewGuid(), NotificationType.ReservationCanceled, "Message 2", read: true),
        };
        var query = notifications.AsQueryable();

        // Act
        var result = query.QueryByRead(false).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region QueryByNotificationType Tests

    [Fact]
    public void QueryByNotificationType_WhenTypeIsNull_ShouldReturnAllNotifications()
    {
        // Arrange
        var query = CreateTestNotifications();

        // Act
        var result = query.QueryByNotificationType(null).ToList();

        // Assert
        result.Should().HaveCount(6);
    }

    [Theory]
    [InlineData(NotificationType.ReservationCreated, 2)]
    [InlineData(NotificationType.ReservationCanceled, 1)]
    [InlineData(NotificationType.ReservationResponded, 1)]
    [InlineData(NotificationType.HostRated, 1)]
    [InlineData(NotificationType.AccommodationRated, 1)]
    public void QueryByNotificationType_WhenTypeIsSpecified_ShouldReturnNotificationsOfThatType(
        NotificationType type, int expectedCount)
    {
        // Arrange
        var query = CreateTestNotifications();

        // Act
        var result = query.QueryByNotificationType(type).ToList();

        // Assert
        result.Should().HaveCount(expectedCount);
        result.Should().OnlyContain(n => n.NotificationType == type);
    }

    [Fact]
    public void QueryByNotificationType_WhenNoNotificationsMatchType_ShouldReturnEmptyList()
    {
        // Arrange
        var notifications = new List<Notification>
        {
            CreateNotification(Guid.NewGuid(), NotificationType.ReservationCreated, "Message 1", read: false),
            CreateNotification(Guid.NewGuid(), NotificationType.ReservationCreated, "Message 2", read: false),
        };
        var query = notifications.AsQueryable();

        // Act
        var result = query.QueryByNotificationType(NotificationType.HostRated).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region QueryByUserId Tests

    [Fact]
    public void QueryByUserId_ShouldReturnNotificationsForSpecifiedUser()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var notifications = new List<Notification>
        {
            CreateNotification(userId1, NotificationType.ReservationCreated, "Message 1", read: false),
            CreateNotification(userId1, NotificationType.ReservationCanceled, "Message 2", read: true),
            CreateNotification(userId2, NotificationType.HostRated, "Message 3", read: false),
        };
        var query = notifications.AsQueryable();

        // Act
        var result = query.QueryByUserId(userId1).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(n => n.UserId == userId1);
    }

    [Fact]
    public void QueryByUserId_WhenUserHasNoNotifications_ShouldReturnEmptyList()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var notifications = new List<Notification>
        {
            CreateNotification(userId1, NotificationType.ReservationCreated, "Message 1", read: false),
        };
        var query = notifications.AsQueryable();

        // Act
        var result = query.QueryByUserId(userId2).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void QueryByUserId_WithEmptyGuid_ShouldReturnEmptyList()
    {
        // Arrange
        var query = CreateTestNotifications();

        // Act
        var result = query.QueryByUserId(Guid.Empty).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Chaining Tests

    [Fact]
    public void ExtensionMethods_WhenChainedTogether_ShouldApplyAllFilters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            CreateNotification(userId, NotificationType.ReservationCreated, "Message 1", read: false),
            CreateNotification(userId, NotificationType.ReservationCreated, "Message 2", read: true),
            CreateNotification(userId, NotificationType.ReservationCanceled, "Message 3", read: false),
            CreateNotification(Guid.NewGuid(), NotificationType.ReservationCreated, "Message 4", read: false),
        };
        var query = notifications.AsQueryable();

        // Act
        var result = query
            .QueryByUserId(userId)
            .QueryByNotificationType(NotificationType.ReservationCreated)
            .QueryByRead(false)
            .ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(userId);
        result.First().NotificationType.Should().Be(NotificationType.ReservationCreated);
        result.First().Read.Should().BeFalse();
    }

    [Fact]
    public void ExtensionMethods_ChainedWithNullFilters_ShouldOnlyApplyNonNullFilters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            CreateNotification(userId, NotificationType.ReservationCreated, "Message 1", read: false),
            CreateNotification(userId, NotificationType.ReservationCanceled, "Message 2", read: true),
            CreateNotification(Guid.NewGuid(), NotificationType.ReservationCreated, "Message 3", read: false),
        };
        var query = notifications.AsQueryable();

        // Act
        var result = query
            .QueryByUserId(userId)
            .QueryByNotificationType(null)
            .QueryByRead(null)
            .ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(n => n.UserId == userId);
    }

    [Fact]
    public void ExtensionMethods_ChainedInDifferentOrder_ShouldProduceSameResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            CreateNotification(userId, NotificationType.ReservationCreated, "Message 1", read: false),
            CreateNotification(userId, NotificationType.ReservationCreated, "Message 2", read: true),
            CreateNotification(userId, NotificationType.ReservationCanceled, "Message 3", read: false),
            CreateNotification(Guid.NewGuid(), NotificationType.ReservationCreated, "Message 4", read: false),
        };
        var query = notifications.AsQueryable();

        // Act
        var result1 = query
            .QueryByUserId(userId)
            .QueryByNotificationType(NotificationType.ReservationCreated)
            .QueryByRead(false)
            .ToList();

        var result2 = query
            .QueryByRead(false)
            .QueryByNotificationType(NotificationType.ReservationCreated)
            .QueryByUserId(userId)
            .ToList();

        // Assert
        result1.Should().BeEquivalentTo(result2);
        result1.Should().HaveCount(1);
    }

    [Fact]
    public void ExtensionMethods_WithComplexQuery_ShouldFilterCorrectly()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var notifications = new List<Notification>
        {
            // User 1 notifications
            CreateNotification(userId1, NotificationType.ReservationCreated, "U1 RC Unread", read: false),
            CreateNotification(userId1, NotificationType.ReservationCreated, "U1 RC Read", read: true),
            CreateNotification(userId1, NotificationType.ReservationCanceled, "U1 RCa Unread", read: false),
            CreateNotification(userId1, NotificationType.HostRated, "U1 HR Read", read: true),
            
            // User 2 notifications
            CreateNotification(userId2, NotificationType.ReservationCreated, "U2 RC Unread", read: false),
            CreateNotification(userId2, NotificationType.ReservationCanceled, "U2 RCa Read", read: true),
        };
        var query = notifications.AsQueryable();

        // Act
        var user1UnreadReservations = query
            .QueryByUserId(userId1)
            .QueryByRead(false)
            .ToList();

        var allReservationCreated = query
            .QueryByNotificationType(NotificationType.ReservationCreated)
            .ToList();

        var user2AllNotifications = query
            .QueryByUserId(userId2)
            .ToList();

        // Assert
        user1UnreadReservations.Should().HaveCount(2);
        user1UnreadReservations.Should().OnlyContain(n => n.UserId == userId1 && !n.Read);

        allReservationCreated.Should().HaveCount(3);
        allReservationCreated.Should().OnlyContain(n => n.NotificationType == NotificationType.ReservationCreated);

        user2AllNotifications.Should().HaveCount(2);
        user2AllNotifications.Should().OnlyContain(n => n.UserId == userId2);
    }

    [Fact]
    public void ExtensionMethods_OnEmptyQueryable_ShouldReturnEmptyResult()
    {
        // Arrange
        var query = new List<Notification>().AsQueryable();

        // Act
        var result = query
            .QueryByUserId(Guid.NewGuid())
            .QueryByNotificationType(NotificationType.ReservationCreated)
            .QueryByRead(false)
            .ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void QueryByUserId_CalledMultipleTimes_ShouldApplyBothFilters()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var query = CreateTestNotifications();

        // Act
        var result = query
            .QueryByUserId(userId1)
            .QueryByUserId(userId2)
            .ToList();

        // Assert - Should return empty as no notification belongs to both users
        result.Should().BeEmpty();
    }

    #endregion
}

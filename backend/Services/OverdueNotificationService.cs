using Bibliotheque.Api.Data;
using Bibliotheque.Api.Models.DTOs;
using Dapper;

namespace Bibliotheque.Api.Services;

public interface IOverdueNotificationService
{
    Task<IEnumerable<LoanReminderDto>> GetUpcomingDueLoansAsync(int daysBeforeDue);
    Task<IEnumerable<LoanReminderDto>> GetOverdueLoansAsync();
    Task LogReminderSentAsync(int loanId, string reminderType);
    Task<IEnumerable<ReminderHistoryDto>> GetReminderHistoryAsync(int? loanId = null);
}

public class LoanReminderDto
{
    public int LoanId { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int DaysUntilDue { get; set; }
    public int DaysOverdue { get; set; }
}

public class ReminderHistoryDto
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public string ReminderType { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

public class OverdueNotificationService : IOverdueNotificationService
{
    private readonly DbContext _context;
    private readonly ILogger<OverdueNotificationService> _logger;

    public OverdueNotificationService(DbContext context, ILogger<OverdueNotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<LoanReminderDto>> GetUpcomingDueLoansAsync(int daysBeforeDue)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT 
                l.id AS LoanId,
                l.user_id AS UserId,
                u.email AS UserEmail,
                CONCAT(u.first_name, ' ', u.last_name) AS UserName,
                b.title AS BookTitle,
                l.due_date AS DueDate,
                DATEDIFF(l.due_date, CURDATE()) AS DaysUntilDue,
                0 AS DaysOverdue
            FROM loans l
            JOIN users u ON l.user_id = u.id
            JOIN books b ON l.book_id = b.id
            WHERE l.status = 'active'
              AND l.due_date > CURDATE()
              AND DATEDIFF(l.due_date, CURDATE()) <= @DaysBeforeDue
            ORDER BY l.due_date ASC";
        
        return await connection.QueryAsync<LoanReminderDto>(sql, new { DaysBeforeDue = daysBeforeDue });
    }

    public async Task<IEnumerable<LoanReminderDto>> GetOverdueLoansAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT 
                l.id AS LoanId,
                l.user_id AS UserId,
                u.email AS UserEmail,
                CONCAT(u.first_name, ' ', u.last_name) AS UserName,
                b.title AS BookTitle,
                l.due_date AS DueDate,
                0 AS DaysUntilDue,
                DATEDIFF(CURDATE(), l.due_date) AS DaysOverdue
            FROM loans l
            JOIN users u ON l.user_id = u.id
            JOIN books b ON l.book_id = b.id
            WHERE l.status = 'active'
              AND l.due_date < CURDATE()
            ORDER BY l.due_date ASC";
        
        return await connection.QueryAsync<LoanReminderDto>(sql);
    }

    public async Task LogReminderSentAsync(int loanId, string reminderType)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO reminder_history (loan_id, reminder_type, sent_at)
            VALUES (@LoanId, @ReminderType, NOW())";
        
        await connection.ExecuteAsync(sql, new { LoanId = loanId, ReminderType = reminderType });
        _logger.LogInformation("Reminder sent for loan {LoanId}, type: {ReminderType}", loanId, reminderType);
    }

    public async Task<IEnumerable<ReminderHistoryDto>> GetReminderHistoryAsync(int? loanId = null)
    {
        using var connection = _context.CreateConnection();
        var sql = @"
            SELECT 
                rh.id AS Id,
                rh.loan_id AS LoanId,
                rh.reminder_type AS ReminderType,
                rh.sent_at AS SentAt,
                b.title AS BookTitle,
                CONCAT(u.first_name, ' ', u.last_name) AS UserName
            FROM reminder_history rh
            JOIN loans l ON rh.loan_id = l.id
            JOIN books b ON l.book_id = b.id
            JOIN users u ON l.user_id = u.id";
        
        if (loanId.HasValue)
        {
            sql += " WHERE rh.loan_id = @LoanId";
        }
        
        sql += " ORDER BY rh.sent_at DESC LIMIT 100";
        
        return await connection.QueryAsync<ReminderHistoryDto>(sql, new { LoanId = loanId });
    }
}

// Background service pour envoyer les rappels automatiquement
public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Vérification quotidienne

    public ReminderBackgroundService(IServiceProvider serviceProvider, ILogger<ReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder Background Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reminders");
            }
            
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<IOverdueNotificationService>();
        
        // Rappel à J-30
        var loans30Days = await notificationService.GetUpcomingDueLoansAsync(30);
        var loans30 = loans30Days.Where(l => l.DaysUntilDue >= 28 && l.DaysUntilDue <= 30);
        foreach (var loan in loans30)
        {
            _logger.LogInformation("J-30 Reminder: {UserName} - {BookTitle} - Due: {DueDate}", 
                loan.UserName, loan.BookTitle, loan.DueDate.ToShortDateString());
            await notificationService.LogReminderSentAsync(loan.LoanId, "J-30");
        }

        // Rappel à J-5
        var loans5Days = await notificationService.GetUpcomingDueLoansAsync(5);
        var loans5 = loans5Days.Where(l => l.DaysUntilDue >= 3 && l.DaysUntilDue <= 5);
        foreach (var loan in loans5)
        {
            _logger.LogInformation("J-5 Reminder: {UserName} - {BookTitle} - Due: {DueDate}", 
                loan.UserName, loan.BookTitle, loan.DueDate.ToShortDateString());
            await notificationService.LogReminderSentAsync(loan.LoanId, "J-5");
        }

        // Rappel pour les retards
        var overdueLoans = await notificationService.GetOverdueLoansAsync();
        foreach (var loan in overdueLoans)
        {
            _logger.LogWarning("OVERDUE Reminder: {UserName} - {BookTitle} - Overdue by {Days} days", 
                loan.UserName, loan.BookTitle, loan.DaysOverdue);
            await notificationService.LogReminderSentAsync(loan.LoanId, "OVERDUE");
        }
    }
}

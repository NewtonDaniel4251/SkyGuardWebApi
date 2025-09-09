using Microsoft.EntityFrameworkCore;
using SkyGuard.Core.Models;
using SkyGuard.Infrastructure.Data;

namespace SkyGuard.Infrastructure.Respositories
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class SecurityResponseRepository : BaseRepository<SecurityResponse>, ISecurityResponseRepository
    {
        private readonly ILogger<SecurityResponseRepository> _logger;

        public SecurityResponseRepository(AppDbContext context, ILogger<SecurityResponseRepository> logger)
            : base(context)
        {
            _logger = logger;
        }

        public async Task<SecurityResponse> GetByIncidentIdAsync(Guid incidentId)
        {
            try
            {
                return await _context.SecurityResponses
                    .AsNoTracking()
                    .Include(r => r.Incident)
                    .FirstOrDefaultAsync(r => r.IncidentId == incidentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security response for incident: {IncidentId}", incidentId);
                throw;
            }
        }

        public async Task<List<SecurityResponse>> GetRecentResponsesAsync(int count)
        {
            try
            {
                return await _context.SecurityResponses
                    .AsNoTracking()
                    .Include(r => r.Incident)
                    .OrderByDescending(r => r.RespondedAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {Count} recent security responses", count);
                throw;
            }
        }

        public async Task<List<SecurityResponse>> GetByIncidentIdAsync(string incidentId)
        {
            try
            {
                // Parse string to Guid if needed, or handle string-based incident IDs
                if (Guid.TryParse(incidentId, out var guidIncidentId))
                {
                    return await _context.SecurityResponses
                        .AsNoTracking()
                        .Include(r => r.Incident)
                        .Where(r => r.IncidentId == guidIncidentId)
                        .ToListAsync();
                }

                // If incidentId is not a Guid, handle accordingly
                // This might be needed if you have legacy string IDs
                _logger.LogWarning("Invalid GUID format for incidentId: {IncidentId}", incidentId);
                return new List<SecurityResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security responses for incident: {IncidentId}", incidentId);
                throw;
            }
        }

        //public async Task<List<SecurityResponse>> GetByResponderAsync(string responderId)
        //{
        //    try
        //    {
        //        return await _context.SecurityResponses
        //            .AsNoTracking()
        //            .Include(r => r.Incident)
        //            .Where(r => r.RespondedBy == responderId)
        //            .OrderByDescending(r => r.RespondedAt)
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting security responses for responder: {ResponderId}", responderId);
        //        throw;
        //    }
        //}

        public async Task<List<SecurityResponse>> GetByTimeRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.SecurityResponses
                    .AsNoTracking()
                    .Include(r => r.Incident)
                    .Where(r => r.RespondedAt >= startDate && r.RespondedAt <= endDate)
                    .OrderByDescending(r => r.RespondedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security responses from {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }



        //// Additional specialized methods for AI analysis
        //public async Task<List<SecurityResponse>> GetByConfirmationTypeAsync(string confirmationType, int? count = null)
        //{
        //    try
        //    {
        //        var query = _context.SecurityResponses
        //            .AsNoTracking()
        //            .Include(r => r.Incident)
        //            .Where(r => r.Classification == confirmationType)
        //            .OrderByDescending(r => r.RespondedAt);

        //        if (count.HasValue)
        //        {
        //            query = (IOrderedQueryable<SecurityResponse>)query.Take(count.Value);
        //        }

        //        return await query.ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting security responses by confirmation type: {ConfirmationType}", confirmationType);
        //        throw;
        //    }
        //}

        //public async Task<List<SecurityResponse>> GetResponsesWithInterventionImagesAsync()
        //{
        //    try
        //    {
        //        return await _context.SecurityResponses
        //            .AsNoTracking()
        //            .Include(r => r.Incident)
        //            .Where(r => !string.IsNullOrEmpty(r.InterventionImageUrl))
        //            .OrderByDescending(r => r.RespondedAt)
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting security responses with intervention images");
        //        throw;
        //    }
        //}

        //public async Task<Dictionary<string, int>> GetResponseStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        //{
        //    try
        //    {
        //        var query = _context.SecurityResponses.AsQueryable();

        //        if (startDate.HasValue)
        //        {
        //            query = query.Where(r => r.RespondedAt >= startDate.Value);
        //        }

        //        if (endDate.HasValue)
        //        {
        //            query = query.Where(r => r.RespondedAt <= endDate.Value);
        //        }

        //        return await query
        //            .GroupBy(r => r.Confirmation)
        //            .Select(g => new { Confirmation = g.Key, Count = g.Count() })
        //            .ToDictionaryAsync(x => x.Confirmation, x => x.Count);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting response statistics");
        //        throw;
        //    }
        //}
    }
}

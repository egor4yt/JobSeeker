using JobSeeker.Deduplication.Application.Jobs.Base;
using JobSeeker.Deduplication.Application.Services.Lsh;
using JobSeeker.Deduplication.Domain.Entities;
using JobSeeker.Deduplication.ObjectStorage;
using JobSeeker.Deduplication.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSeeker.Deduplication.Application.Jobs.Common.DeduplicateVacancies;

public class DeduplicateVacanciesJob(
    ILogger<DeduplicateVacanciesJob> logger,
    IObjectStorage objectStorage,
    ApplicationDbContext dbContext,
    ILshStrategy<RawVacancy> lshStrategy) : IJob<JobParameters.Common.DeduplicateVacancies>
{
    private CancellationToken _cancellationToken = CancellationToken.None;
    private JobParameters.Common.DeduplicateVacancies _parameter = null!;

    public async Task RunAsync(JobParameters.Common.DeduplicateVacancies parameter, CancellationToken cancellationToken = default)
    {
        _parameter = parameter;
        _cancellationToken = cancellationToken;

        logger.LogInformation("Started deduplication vacancy group {@Parameter}", _parameter);

        await RunAsync();

        logger.LogInformation("Finished deduplication vacancy group {@Parameter}", _parameter);
    }

    private async Task RunAsync()
    {
        var rawVacancies = await dbContext.RawVacancies
            .Where(x => x.OccupationGroup == _parameter.OccupationGroup
                        && x.Occupation == _parameter.Occupation
                        && x.Specialization == _parameter.Specialization
                        && x.SkillTag == _parameter.SkillTag
                        && x.CreatedAt > DateTime.UtcNow.AddDays(-30))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(_cancellationToken);

        foreach (var rawVacancy in rawVacancies)
        {
            rawVacancy.DeduplicationCompleted = false;
        }

        await DeduplicateByLshAsync(rawVacancies);
        DeduplicateByFingerprint(rawVacancies);

        await dbContext.SaveChangesAsync(_cancellationToken);
    }

    private void DeduplicateByFingerprint(IEnumerable<RawVacancy> rawVacancies)
    {
        var groupedVacancies = rawVacancies
            .GroupBy(x => x.Fingerprint)
            .Where(x => x.Count() > 1);

        foreach (var vacancyGroup in groupedVacancies)
        {
            var mainVacancy = vacancyGroup.First();
            var otherVacancies = vacancyGroup.Skip(1);
            var duplicatesCount = 0;

            foreach (var otherVacancy in otherVacancies)
            {
                otherVacancy.OriginalRawVacancyId = mainVacancy.OriginalRawVacancyId ?? mainVacancy.Id;
                otherVacancy.DeduplicationCompleted = true;
                mainVacancy.DeduplicationCompleted = true;

                duplicatesCount += 1;
            }

            if (duplicatesCount > 0)
                logger.LogDebug("Found {DuplicatesCount} duplicates of {OriginalVacancyFingerprint} by fingerprint", duplicatesCount, mainVacancy.Fingerprint);
        }
    }

    private async Task DeduplicateByLshAsync(IEnumerable<RawVacancy> rawVacancies)
    {
        var groupedVacancies = rawVacancies
            .DistinctBy(x => x.Fingerprint)
            .GroupBy(x => x.Company);

        var processedFingerprints = new HashSet<string>();

        foreach (var vacancyGroup in groupedVacancies)
        {
            foreach (var rawVacancy in vacancyGroup)
            {
                if (processedFingerprints.Contains(rawVacancy.Fingerprint)) continue;
                var potentialCandidates = vacancyGroup
                    .Where(x => x.Fingerprint != rawVacancy.Fingerprint)
                    .ToList();

                var candidatesMap = await lshStrategy.GetCandidatesAsync(rawVacancy, potentialCandidates, _cancellationToken);

                var duplicatesCount = 0;
                foreach (var candidateMap in candidatesMap)
                {
                    var candidate = vacancyGroup.First(x => x.Fingerprint == candidateMap.Key);
                    if (processedFingerprints.Contains(candidate.Fingerprint)) continue;

                    candidate.OriginalRawVacancyId = rawVacancy.OriginalRawVacancyId ?? rawVacancy.Id;
                    candidate.DeduplicationCompleted = true;
                    processedFingerprints.Add(candidate.Fingerprint);

                    duplicatesCount += 1;
                }

                rawVacancy.DeduplicationCompleted = true;
                processedFingerprints.Add(rawVacancy.Fingerprint);

                if (duplicatesCount > 0)
                    logger.LogDebug("Found {DuplicatesCount} duplicates of {OriginalVacancyFingerprint} by LSH", duplicatesCount, rawVacancy.Fingerprint);
            }
        }
    }
}
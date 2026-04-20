using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace aqua_api.Modules.System.Infrastructure
{
    public class JobExecutionLogConfiguration : BaseEntityConfiguration<JobExecutionLog>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<JobExecutionLog> builder)
        {
            builder.ToTable("RII_JOB_EXECUTION_LOG");

            builder.Property(x => x.JobId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.RecurringJobId)
                .HasMaxLength(100);

            builder.Property(x => x.JobName)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Queue)
                .HasMaxLength(100);

            builder.Property(x => x.Reason)
                .HasMaxLength(2000);

            builder.Property(x => x.ExceptionType)
                .HasMaxLength(500);

            builder.Property(x => x.ExceptionMessage)
                .HasMaxLength(4000);

            builder.HasIndex(x => x.JobId)
                .HasDatabaseName("IX_JobExecutionLog_JobId");

            builder.HasIndex(x => x.RecurringJobId)
                .HasDatabaseName("IX_JobExecutionLog_RecurringJobId");

            builder.HasIndex(x => x.JobName)
                .HasDatabaseName("IX_JobExecutionLog_JobName");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_JobExecutionLog_Status");

            builder.HasIndex(x => x.FinishedAt)
                .HasDatabaseName("IX_JobExecutionLog_FinishedAt");
        }
    }
}

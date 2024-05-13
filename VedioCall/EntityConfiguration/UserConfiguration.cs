namespace VedioCall;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Name)
               .HasMaxLength(150)
               .IsRequired();

        builder.HasIndex(e => e.Email)
               .IsUnique();
        builder.Property(e => e.Email)
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(e => e.Password)
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(e => e.ImagePath)
               .IsRequired();
    }
}

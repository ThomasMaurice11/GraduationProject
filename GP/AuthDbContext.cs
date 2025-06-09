using GP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GP
{


    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<PetMarriageRequest> PetMarriageRequests { get; set; }
        public DbSet<AnimalMarriageRequest> AnimalMarriageRequests { get; set; }
        public DbSet<AdoptionRequest> AdoptionRequests { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PetSitterRequest> PetSitterRequests { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {
        
        }
    



    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Doctor>().ToTable("Doctors");

            modelBuilder.Entity<PetMarriageRequest>()
         .HasOne(p => p.SenderPet)
         .WithMany()
         .HasForeignKey(p => p.SenderPetId)
         .OnDelete(DeleteBehavior.NoAction);  // Prevent cascade delete

            modelBuilder.Entity<AnimalMarriageRequest>()
      .HasOne(p => p.SenderPet)
      .WithMany()
      .HasForeignKey(p => p.SenderPetId)
      .OnDelete(DeleteBehavior.NoAction);  // Prevent cascade delete

            //     modelBuilder.Entity<Clinic>()
            //.HasOne(c => c.Doctor)
            //.WithMany()
            //.HasForeignKey(c => c.DoctorId)
            //.OnDelete(DeleteBehavior.NoAction); // Optional: Configure delete behavior

            modelBuilder.Entity<PetMarriageRequest>()
                .HasOne(p => p.ReceiverPet)
                .WithMany()
                .HasForeignKey(p => p.ReceiverPetId)
                .OnDelete(DeleteBehavior.NoAction);  // Prevent cascade delete

            modelBuilder.Entity<AnimalMarriageRequest>()
         .HasOne(p => p.ReceiverAnimal)
         .WithMany()
         .HasForeignKey(p => p.ReceiverAnimalId)
         .OnDelete(DeleteBehavior.NoAction);  // Prevent cascade delete

            //modelBuilder.Entity<AdoptionRequest>()
            //    .HasOne(p => p.PetId)
            //    .WithMany()
            //    .HasForeignKey(p => p.PetId)
            //    .OnDelete(DeleteBehavior.NoAction);  // Prevent cascade delete

            modelBuilder.Entity<AdoptionRequest>()
       .HasOne(ar => ar.Pet)
       .WithMany()
       .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.Animal)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AdoptionRequest>()
                .HasOne(ar => ar.Owner)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ChatMessage>()
               .HasOne(m => m.Sender)
               .WithMany(u => u.SentMessages)
               .HasForeignKey(m => m.SenderId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);


            //// Configure Clinic relationships
            //modelBuilder.Entity<Appointment>()
            //    .HasOne(a => a.Clinic)
            //    .WithMany()
            //    .HasForeignKey(a => a.ClinicId)
            //    .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict

            //modelBuilder.Entity<Slot>()
            //    .HasOne(s => s.Clinic)
            //    .WithMany()
            //    .HasForeignKey(s => s.ClinicId)
            //    .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict

            //// Configure other relationships
            //modelBuilder.Entity<Appointment>()
            //    .HasOne(a => a.Slot)
            //    .WithMany()
            //    .HasForeignKey(a => a.SlotId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Appointment>()
            //    .HasOne(a => a.User)
            //    .WithMany()
            //    .HasForeignKey(a => a.UserId)
            //    .OnDelete(DeleteBehavior.Restrict); // Typically OK for user-appointment relationship

            //// Configure Appointment relationships
            //modelBuilder.Entity<Appointment>(entity =>
            //{
            //    // Clinic relationship - NO CASCADE
            //    entity.HasOne(a => a.Clinic)
            //          .WithMany()
            //          .HasForeignKey(a => a.ClinicId)
            //          .OnDelete(DeleteBehavior.NoAction); // Changed to NoAction

            //    // Slot relationship - NO CASCADE
            //    entity.HasOne(a => a.Slot)
            //          .WithMany()
            //          .HasForeignKey(a => a.SlotId)
            //          .OnDelete(DeleteBehavior.NoAction); // Changed to NoAction

            //    // User relationship - can keep CASCADE if appropriate
            //    entity.HasOne(a => a.User)
            //          .WithMany()
            //          .HasForeignKey(a => a.UserId)
            //          .OnDelete(DeleteBehavior.Cascade);
            //});
            // Configure Appointment relationships
            // Configure Appointment relationships
            // Appointment → Clinic
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Clinic)
                .WithMany()
                .HasForeignKey(a => a.ClinicId)
                .OnDelete(DeleteBehavior.NoAction);

            // Appointment → Slot
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Slot)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.SlotId)
                .OnDelete(DeleteBehavior.NoAction);

            // Appointment → User
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Slot → Clinic
            modelBuilder.Entity<Slot>()
                .HasOne(s => s.Clinic)
                .WithMany()
                .HasForeignKey(s => s.ClinicId)
                .OnDelete(DeleteBehavior.NoAction);

            //// Configure Slot relationships
            //modelBuilder.Entity<Slot>(entity =>
            //{
            //    entity.HasOne(s => s.Clinic)
            //          .WithMany()
            //          .HasForeignKey(s => s.ClinicId)
            //          .OnDelete(DeleteBehavior.NoAction); // Changed to NoAction
            //});
            //for TPh Realtionship Doctor Inherit from User
            //     modelBuilder.Entity<ApplicationUser>()
            //.HasDiscriminator<string>("UserType")
            //.HasValue<ApplicationUser>("ApplicationUser")
            //.HasValue<Doctor>("Doctor");


            //for TPT Realtionship Doctor Inherit from User sperate table
            modelBuilder.Entity<Doctor>().ToTable("Doctors");
        }

        internal object FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }

}

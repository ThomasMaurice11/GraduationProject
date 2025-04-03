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
        public DbSet<AdoptionRequest> AdoptionRequests { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
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

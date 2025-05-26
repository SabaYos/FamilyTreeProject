using FamilyTreeAPI.Models;
using System.Linq;

namespace FamilyTreeAPI
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.FamilyTrees.Any())
            {
                context.FamilyTrees.Add(new FamilyTree
                {
                    FamilyTreeName = "Test Family",
                    IsPublic = false,
                    OwnerId = "7474e6bb-c382-4d37-ab1d-c242edc01f20" // Replace with AspNetUsers.Id
                });
                context.SaveChanges();
            }
        }
    }
}

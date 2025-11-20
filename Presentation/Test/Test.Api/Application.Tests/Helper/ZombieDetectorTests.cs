using Application.Strategies.Helper;

namespace Application.Tests
{
    public class ZombieDetectorTests
    {
        [Fact]
        public void IsZombie_ShouldReturnTrue_WhenHorizontalSequencesFound()
        {
            string[] geneticCode =
            {
                "PPPPGA",  
                "APGLGP",
                "LLALGL",
                "APLAPL",
                "PPPPLA",  
                "LAPLGG"
            };

            bool result = geneticCode.IsZombie();

            Assert.True(result);
        }

        [Fact]
        public void IsZombie_ShouldReturnTrue_WhenVerticalSequencesFound()
        {
            string[] geneticCode =
            {
                "PAPGLP",
                "PGLAPL",
                "PALGPA",
                "PAGLPA", 
                "LPGAPL",
                "APGPLG" 
            };

            bool result = geneticCode.IsZombie();

            Assert.True(result);
        }
        [Fact]
        public void IsZombie_ShouldReturnTrue_WhenDiagonalSequencesFound()
        {
            string[] geneticCode =
            {
                "PAGLP",
                "LPAGG",
                "GLPAG",
                "AGLPG",
                "PAGLP"
            };

            bool result = geneticCode.IsZombie();

            Assert.True(result);
        }

        [Fact]
        public void IsZombie_ShouldReturnFalse_WhenNoVirusSequencesArePresent()
        {
            string[] geneticCode =
            {
                "PLAGPL",
                "GAPLLA",
                "ALGPGA",
                "LPAGLP",
                "APLGAP",
                "GLAPLG"
            };

            bool result = geneticCode.IsZombie();

            Assert.False(result);
        }
    }
}

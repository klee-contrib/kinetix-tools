using Microsoft.VisualStudio.TestTools.UnitTesting;

// TODO adapter au projet
namespace MonProjet.CalculImplementationImplementation.Test.DalCalculTest
{
    [TestClass]
    public class SuperCalculTest : DalTest
    {
        [TestMethod]
        public void Check_SuperCalcul_Ok()
        {
            // Act
            this.CheckDalSyntax<DalSuperCalcul>(dal => dal.SuperCalcul(Dum.IdList));
        }
    }
}

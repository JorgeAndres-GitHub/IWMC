using AccesoDatos.Context;
using AccesoDatos.DTOs;
using AccesoDatos.Models;
using AccesoDatos.Operaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Controllers;

namespace ASPUnitTest
{
    public class AutosControllerShould
    {
        private readonly AppCarrosContext _appCarrosContext;
        private readonly AutosController _autosController;
        private readonly AutoDAO _autoDAO;

        public AutosControllerShould()
        {
            var options = new DbContextOptionsBuilder<AppCarrosContext>().UseInMemoryDatabase(databaseName: "TestDatabaseAutosController").Options;
            _appCarrosContext = new AppCarrosContext(options);
            _autoDAO = new AutoDAO(_appCarrosContext);
            _autosController = new AutosController(_appCarrosContext, _autoDAO);

            _appCarrosContext.Autos.AddRange(
                new Auto { Vehiculo = "Mercedes", VersionVehiculo= "AMG E53 4MATIC+", Precio= 402900000.0000m, Tipo = "Carro" },
                new Auto { Vehiculo = "Toyota", VersionVehiculo= "2016 C190", Precio= 850550000.0000m, Tipo = "carro" }
            );
            _appCarrosContext.SaveChanges();
        }

        [Theory]
        [InlineData("carro", 6, true)]
        [InlineData("camioneta", 5, true)]
        [InlineData("Avion", 0, false)]
        public async Task GetCars(string tipo, int expectedCount, bool isOkExpected)
        {
            var result = await _autosController.ObtenerAutosPorTipo(tipo);
            if (isOkExpected)
            {                
                var okResult = Assert.IsType<OkObjectResult>(result);
                var autos = Assert.IsType<string[]>(okResult.Value);
                Assert.Equal(expectedCount, autos.Length);
            }
            else
            {
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async Task GetCarsByBrand()
        {
            //Act
            var result = await _autosController.ObtenerAutosPorMarca("Mercedes", "carro");

            //Assert
            var okResult=Assert.IsType<OkObjectResult>(result);
            var autos = Assert.IsType<List<Auto>>(okResult.Value);
            Assert.NotEmpty(autos);
        }

        [Fact]
        public async Task AddCar()
        {
            //Arrange
            AutoRequestDTO request = new AutoRequestDTO()
            {
                Vehiculo = "Mercedes-AMG Clase G",
                VersionVehiculo = "AMG G63 4MATIC",
                Precio = 1399900000.0000m,
                Tipo = "camioneta"
            };

            //Act
            var result = await _autosController.AgregarAuto(request);

            //Assert
            var isCreated= Assert.IsType<CreatedAtActionResult>(result);
        }
    }
}
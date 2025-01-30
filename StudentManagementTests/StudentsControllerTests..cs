using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Kertaus.Data;
using Kertaus.Models;
using Kertaus.Controllers;
using Microsoft.AspNetCore.Mvc;
using Kertaus.Controllers;
using Kertaus.Data;
using Kertaus.Models;

namespace StudentManagementTests
{
    public class StudentsControllerTests
    {
        [Fact]
        public async Task GetStudents_ReturnsAllStudents()
        {
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "StudentDbTest")
                .Options;

            using (var context = new StudentContext(options))
            {
                context.Students.Add(new Student { Id = 1, firstName = "Maija", LastName = "Meikäläinen", Age = 20 });
                context.Students.Add(new Student { Id = 2, firstName = "Matti", LastName = "Meikäläinen", Age = 22 });
                await context.SaveChangesAsync();
            }

            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);

                var result = await controller.GetStudents();

                var actionResult = Assert.IsType<ActionResult<IEnumerable<Student>>>(result);
                var studentsList = Assert.IsAssignableFrom<IEnumerable<Student>>(actionResult.Value);

                Assert.Equal(2, studentsList.Count());
            }
        }

        [Fact]
        public async Task GetStudent_ReturnsNotFound_WhenStudentDoesNotExist()
        {
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "StudentDbTest_NotFound")
                .Options;

            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);

                var result = await controller.GetStudent(99);

                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async Task PostStudent_AddsStudentSuccessfully()
        {
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "StudentDbTest_Post")
                .Options;

            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);
                var newStudent = new Student { Id = 1, firstName = "John", LastName = "Doe", Age = 25 };

                var result = await controller.PostStudent(newStudent);
                var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                var student = Assert.IsType<Student>(actionResult.Value);

                Assert.Equal("John", student.firstName);
                Assert.Equal(1, await context.Students.CountAsync());
            }
        }

        [Fact]
        public async Task PutStudent_UpdatesStudentSuccessfully()
        {
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "StudentDbTest_Put")
                .Options;

            using (var context = new StudentContext(options))
            {
                context.Students.Add(new Student { Id = 1, firstName = "Alice", LastName = "Smith", Age = 21 });
                await context.SaveChangesAsync();
            }

            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);
                var updatedStudent = new Student { Id = 1, firstName = "Alice", LastName = "Johnson", Age = 22 };

                var result = await controller.PutStudent(1, updatedStudent);

                Assert.IsType<NoContentResult>(result);
                var studentInDb = await context.Students.FindAsync(1);
                Assert.Equal("Johnson", studentInDb.LastName);
            }
        }

        [Fact]
        public async Task DeleteStudent_RemovesStudentSuccessfully()
        {
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "StudentDbTest_Delete")
                .Options;

            using (var context = new StudentContext(options))
            {
                context.Students.Add(new Student { Id = 1, firstName = "Bob", LastName = "Marley", Age = 30 });
                await context.SaveChangesAsync();
            }

            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);

                var result = await controller.DeleteStudent(1);

                Assert.IsType<NoContentResult>(result);
                Assert.Equal(0, await context.Students.CountAsync());
            }
        }
    }
}
